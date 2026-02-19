using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UGESystem
{
    /// <summary>
    /// Defines a mapping between a screen position and a UI anchor for spawning 2D characters.
    /// </summary>
    [System.Serializable]
    public class Character2DPositionSlot
    {
        public CharacterPosition position;
        public RectTransform anchor; // 2D 프리팹이 생성될 UI 앵커 // UI anchor where 2D prefab will be created
    }
    
    /// <summary>
    /// Defines a mapping between a screen position, a 3D world anchor, and a UI RawImage for displaying 3D characters.
    /// </summary>
    [System.Serializable]
    public class Character3DPositionSlot
    {
        public CharacterPosition position;
        public Transform anchor; // 3D 모델이 생성될 위치 // Location where 3D model will be created
        public RawImage displayImage; // 3D 모델을 렌더링할 RawImage // RawImage to render the 3D model
    }

    /// <summary>
    /// Manages the lifecycle of 2D and 3D characters during events.
    /// Handles instantiation, placement, animation, and cleanup of character GameObjects based on commands.
    /// It maintains a runtime clone of the character database to allow safe modifications.
    /// </summary>
    public class UGECharacterManager : MonoBehaviour
    {
        [SerializeField] private CharacterDatabase _characterDatabase;
        
        /// <summary>
        /// Gets the runtime clone of the character database. All modifications should be made here.
        /// /// (Korean) 캐릭터 데이터베이스의 런타임 복제본입니다. 모든 수정사항은 여기서 이루어져야 합니다.
        /// </summary>
        public CharacterDatabase RuntimeCharacterDB { get; private set; }

        /// <summary>
        /// Gets the reference to the active character database (either runtime clone or original asset).
        /// </summary>
        public CharacterDatabase CharacterDB => RuntimeCharacterDB != null ? RuntimeCharacterDB : _characterDatabase;
        
        /// <summary>
        /// A list of UI slots for positioning 2D characters.
        /// /// (Korean) 2D 캐릭터를 배치하기 위한 UI 슬롯 리스트입니다.
        /// </summary>
        [Header("2D Character UI Slots")]
        [SerializeField] private List<Character2DPositionSlot> _character2DSlots = new List<Character2DPositionSlot>();

        /// <summary>
        /// A list of UI and world slots for positioning and displaying 3D characters.
        /// /// (Korean) 3D 캐릭터를 배치하고 표시하기 위한 UI 및 월드 슬롯 리스트입니다.
        /// </summary>
        [Header("3D Character UI Slots")]
        [SerializeField] private List<Character3DPositionSlot> _character3DSlots = new List<Character3DPositionSlot>();
        
        /// <summary>
        /// Internal class to track the state of an active character instance.
        /// /// (Korean) 활성화된 캐릭터 인스턴스의 상태를 추적하기 위한 내부 클래스입니다.
        /// </summary>
        private class ActiveCharacterInfo
        {
            public string CharacterId;
            public CharacterPosition Position;
            public GameObject Instance;
            public bool Is3D;
        }

        private Dictionary<string, ActiveCharacterInfo> _idToCharacter = new Dictionary<string, ActiveCharacterInfo>();
        private Dictionary<CharacterPosition, ActiveCharacterInfo> _posToCharacter = new Dictionary<CharacterPosition, ActiveCharacterInfo>();
        
        /// <summary>
        /// The layer index for 3D characters, used to isolate them for rendering.
        /// </summary>
        private int _character3DLayer;

        private void Awake()
        {
            // Create a runtime clone of the database to prevent accidental asset modification in editor.
            if (_characterDatabase != null)
            {
                RuntimeCharacterDB = Instantiate(_characterDatabase);
            }

            _character3DLayer = LayerMask.NameToLayer("Character3D");
            if (_character3DLayer == -1)
            {
#if UNITY_EDITOR
                Debug.LogError("Error: 'Character3D' layer is not defined in Project Settings -> Tags and Layers. Please add it to a User Layer slot to proceed.");
#endif
            }
            
            HideAllCharacters();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_characterDatabase == null)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:CharacterDatabase");
                if (guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    _characterDatabase = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterDatabase>(path);
                    UnityEditor.EditorUtility.SetDirty(this);
                }
            }
        }
#endif

        public void HideAllCharacters()
        {
            foreach (var info in _idToCharacter.Values)
            {
                if (info.Instance != null) Destroy(info.Instance);
            }
            _idToCharacter.Clear();
            _posToCharacter.Clear();

            foreach (var slot in _character3DSlots)
            {
                if(slot.displayImage) slot.displayImage.gameObject.SetActive(false);
            }
        }

        private void RegisterActiveCharacter(string characterId, CharacterPosition position, GameObject instance, bool is3D)
        {
            var info = new ActiveCharacterInfo
            {
                CharacterId = characterId,
                Position = position,
                Instance = instance,
                Is3D = is3D
            };
            _idToCharacter[characterId] = info;
            _posToCharacter[position] = info;
        }

        private void UnregisterByPosition(CharacterPosition position)
        {
            if (_posToCharacter.TryGetValue(position, out var info))
            {
                _idToCharacter.Remove(info.CharacterId);
                _posToCharacter.Remove(position);
                if (info.Instance != null) Destroy(info.Instance);
            }
        }

        private void UnregisterById(string characterId)
        {
            if (_idToCharacter.TryGetValue(characterId, out var info))
            {
                _idToCharacter.Remove(characterId);
                _posToCharacter.Remove(info.Position);
                if (info.Instance != null) Destroy(info.Instance);
            }
        }

        /// <summary>
        /// Updates a character's data in the runtime database and performs a hot-swap if the character is active.
        /// /// (Korean) 런타임 데이터베이스에서 캐릭터 데이터를 업데이트하고, 활성화된 캐릭터인 경우 핫스왑을 수행합니다.
        /// </summary>
        public void UpdateCharacterData(CharacterUpdateCommand command)
        {
            if (RuntimeCharacterDB == null) return;

            var targetData = RuntimeCharacterDB.GetCharacterData(command.TargetCharacterId);
            if (targetData == null) return;

            // 1. Update the database
            switch (command.UpdateType)
            {
                case CharacterUpdateType.Full:
                    var templateData = RuntimeCharacterDB.GetCharacterData(command.SourceTemplateId);
                    if (templateData != null)
                    {
                        RuntimeCharacterDB.UpdateCharacter(command.TargetCharacterId, templateData);
                    }
                    break;
                case CharacterUpdateType.Partial:
                    if (!string.IsNullOrEmpty(GameManager.Instance.UserName))
                    {
                        //targetData.UpdateData(command.OverrideName, targetData.Is3D, targetData.Prefab, targetData.Expressions);
                        targetData.UpdateData(GameManager.Instance.UserName, targetData.Is3D, targetData.Prefab, targetData.Expressions);

                    }
                    break;
                case CharacterUpdateType.ResetToOriginal:
                    var originalData = _characterDatabase.GetCharacterData(command.TargetCharacterId);
                    if (originalData != null)
                    {
                        RuntimeCharacterDB.UpdateCharacter(command.TargetCharacterId, originalData);
                    }
                    break;
            }

            // 2. Hot-swap if the character is currently active in the scene
            if (_idToCharacter.TryGetValue(command.TargetCharacterId, out var info))
            {
                SwapCharacterVisuals(info);
            }
        }

        private void SwapCharacterVisuals(ActiveCharacterInfo info)
        {
            var updatedData = CharacterDB.GetCharacterData(info.CharacterId);
            if (updatedData == null || info.Instance == null) return;

            // Capture current state
            Animator oldAnimator = info.Instance.GetComponent<Animator>();
            if (oldAnimator == null) oldAnimator = info.Instance.GetComponentInChildren<Animator>();
            
            string currentStateName = "";
            float currentNormalizedTime = 0f;
            if (oldAnimator != null)
            {
                var stateInfo = oldAnimator.GetCurrentAnimatorStateInfo(0);
                currentStateName = stateInfo.fullPathHash != 0 ? "" : ""; // fullPathHash fallback is tricky, use short name if possible
                // For simplicity in this version, we will re-apply the last expression later.
                currentNormalizedTime = stateInfo.normalizedTime;
            }

            CharacterPosition pos = info.Position;
            bool was3D = info.Is3D;

            // Destroy old instance
            if (info.Instance != null) Destroy(info.Instance);

            // Re-instantiate based on new data
            GameObject newInstance = null;
            if (updatedData.Is3D)
            {
                Character3DPositionSlot slot = _character3DSlots.FirstOrDefault(s => s.position == pos);
                if (slot != null && slot.anchor != null)
                {
                    newInstance = Instantiate(updatedData.Prefab, slot.anchor.position, slot.anchor.rotation);
                    SetLayerRecursively(newInstance, _character3DLayer);
                }
            }
            else
            {
                Character2DPositionSlot slot = _character2DSlots.FirstOrDefault(s => s.position == pos);
                if (slot != null && slot.anchor != null)
                {
                    newInstance = Instantiate(updatedData.Prefab, slot.anchor);
                }
            }

            if (newInstance != null)
            {
                info.Instance = newInstance;
                info.Is3D = updatedData.Is3D;
                // Re-apply "default" or last known expression if possible
                ApplyExpression(newInstance, updatedData, "default");
            }
        }

        public void HandleCharacterCommand(CharacterCommand command)
        {
            if(CharacterDB == null) return;
            CharacterData characterData = CharacterDB.GetCharacterData(command.CharacterId);
            if (characterData == null) return;

            if (characterData.Is3D) Handle3DCharacter(command, characterData);
            else Handle2DCharacter(command, characterData);
        }

        public void ShowCharacterForDialogue(DialogueCommand dialogueCommand)
        {
            if (dialogueCommand.ClearAllCharacters) HideAllCharacters();
            if (dialogueCommand.ShowCharacter)
            {
                var tempCharCommand = new CharacterCommand(dialogueCommand.CharacterName, CharacterAction.Show, dialogueCommand.CharacterPosition, dialogueCommand.Expression);
                HandleCharacterCommand(tempCharCommand);
            }
        }

        private void Handle2DCharacter(CharacterCommand command, CharacterData characterData)
        {
            Character2DPositionSlot slot = _character2DSlots.FirstOrDefault(s => s.position == command.Position);
            if (slot == null || slot.anchor == null) return;
            
            if (command.Action == CharacterAction.Show || command.Action == CharacterAction.ChangeExpression)
            {
                if (_idToCharacter.TryGetValue(command.CharacterId, out var existingById) && existingById.Position != command.Position)
                    UnregisterById(command.CharacterId);

                if (_posToCharacter.TryGetValue(command.Position, out var existingAtPos) && existingAtPos.CharacterId != command.CharacterId)
                    UnregisterByPosition(command.Position);

                GameObject characterInstance = null;
                if (_idToCharacter.TryGetValue(command.CharacterId, out var currentInfo)) characterInstance = currentInfo.Instance;

                if (characterInstance == null || characterInstance.name != characterData.Prefab.name + "(Clone)")
                {
                    if (characterInstance != null) UnregisterById(command.CharacterId);
                    characterInstance = Instantiate(characterData.Prefab, slot.anchor);
                    RegisterActiveCharacter(command.CharacterId, command.Position, characterInstance, false);
                }
                ApplyExpression(characterInstance, characterData, command.Expression);
            }
            else if (command.Action == CharacterAction.Hide) UnregisterByPosition(command.Position);
        }

        private void Handle3DCharacter(CharacterCommand command, CharacterData characterData)
        {
            Character3DPositionSlot slot = _character3DSlots.FirstOrDefault(s => s.position == command.Position);
            if (slot == null || slot.anchor == null || slot.displayImage == null) return;

            switch (command.Action)
            {
                case CharacterAction.Show:
                    if (_idToCharacter.TryGetValue(command.CharacterId, out var existingById) && existingById.Position != command.Position)
                        UnregisterById(command.CharacterId);
                    
                    if (_posToCharacter.TryGetValue(command.Position, out var existingAtPos))
                        UnregisterByPosition(command.Position);

                    GameObject newCharacter = Instantiate(characterData.Prefab, slot.anchor.position, slot.anchor.rotation);
                    SetLayerRecursively(newCharacter, _character3DLayer);
                    RegisterActiveCharacter(command.CharacterId, command.Position, newCharacter, true);
                    slot.displayImage.gameObject.SetActive(true);
                    goto case CharacterAction.ChangeExpression;

                case CharacterAction.ChangeExpression:
                    if (_idToCharacter.TryGetValue(command.CharacterId, out var info))
                        ApplyExpression(info.Instance, characterData, command.Expression);
                    break;

                case CharacterAction.Hide:
                    UnregisterByPosition(command.Position);
                    slot.displayImage.gameObject.SetActive(false);
                    break;
            }
        }

        private void ApplyExpression(GameObject instance, CharacterData data, string expressionName)
        {
            if (instance == null) return;
            Animator animator = instance.GetComponent<Animator>();
            if (animator == null) animator = instance.GetComponentInChildren<Animator>();
            if (animator == null) return;

            string targetExp = string.IsNullOrEmpty(expressionName) ? "default" : expressionName;
            CharacterExpression expression = data.Expressions.FirstOrDefault(e => e.ExpressionName == targetExp);
            if (expression != null && !string.IsNullOrEmpty(expression.AnimationStateName))
                animator.Play(expression.AnimationStateName);
        }

        private void SetLayerRecursively(GameObject obj, int layer)
        {
            if(layer == -1) return;
            obj.layer = layer;
            foreach (Transform child in obj.transform) SetLayerRecursively(child.gameObject, layer);
        }
    }
}
