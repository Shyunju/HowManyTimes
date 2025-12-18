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
    /// </summary>
    public class UGECharacterManager : MonoBehaviour
    {
        [SerializeField] private CharacterDatabase _characterDatabase;
        /// <summary>
        /// Gets the reference to the project's central character database.
        /// </summary>
        public CharacterDatabase CharacterDB => _characterDatabase;
        
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
        /// A dictionary tracking active 2D character instances, keyed by their screen position.
        /// /// (Korean) 현재 활성화된 2D 캐릭터 인스턴스를 화면 위치를 키로 하여 추적하는 딕셔너리입니다.
        /// </summary>
        private Dictionary<CharacterPosition, GameObject> _active2DCharacters = new Dictionary<CharacterPosition, GameObject>();
        
        /// <summary>
        /// A dictionary tracking active 3D character instances, keyed by their screen position.
        /// /// (Korean) 현재 활성화된 3D 캐릭터 인스턴스를 화면 위치를 키로 하여 추적하는 딕셔너리입니다.
        /// </summary>
        private Dictionary<CharacterPosition, GameObject> _active3DCharacters = new Dictionary<CharacterPosition, GameObject>();
        
        /// <summary>
        /// The layer index for 3D characters, used to isolate them for rendering.
        /// /// (Korean) 3D 캐릭터를 렌더링을 위해 격리하는 데 사용되는 레이어 인덱스입니다.
        /// </summary>
        private int _character3DLayer;

        private void Awake()
        {
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

        /// <summary>
        /// Destroys all active character GameObjects and clears the display.
        /// </summary>
        public void HideAllCharacters()
        {
            foreach (var character in _active2DCharacters.Values)
            {
                Destroy(character);
            }
            _active2DCharacters.Clear();

            foreach (var slot in _character3DSlots)
            {
                if(slot.displayImage) slot.displayImage.gameObject.SetActive(false);
            }
            
            foreach (var character in _active3DCharacters.Values)
            {
                Destroy(character);
            }
            _active3DCharacters.Clear();
        }

        /// <summary>
        /// Processes a CharacterCommand, handling the logic for showing, hiding, or changing expressions for both 2D and 3D characters.
        /// </summary>
        /// <param name="command">The CharacterCommand to execute.</param>
        public void HandleCharacterCommand(CharacterCommand command)
        {
            if(CharacterDB == null) return;
            CharacterData characterData = CharacterDB.GetCharacterData(command.CharacterId);
            if (characterData == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Character with ID '{command.CharacterId}' not found in database.");
#endif
                return;
            }

            if (characterData.Is3D)
            {
                Handle3DCharacter(command, characterData);
            }
            else
            {
                Handle2DCharacter(command, characterData);
            }
        }

        /// <summary>
        /// A helper method to handle character display logic from within a DialogueCommand.
        /// </summary>
        public void ShowCharacterForDialogue(DialogueCommand dialogueCommand)
        {
            if (dialogueCommand.ClearAllCharacters)
            {
                HideAllCharacters();
            }

            if (dialogueCommand.ShowCharacter)
            {
                var tempCharCommand = new CharacterCommand(
                    dialogueCommand.CharacterName, 
                    CharacterAction.Show, 
                    dialogueCommand.CharacterPosition, 
                    dialogueCommand.Expression
                );
                HandleCharacterCommand(tempCharCommand);
            }
        }

        /// <summary>
        /// Manages the lifecycle of a 2D character based on the received command.
        /// Handles showing, hiding, and changing expressions by instantiating prefabs and controlling their animators.
        /// /// (Korean) 수신된 명령에 따라 2D 캐릭터의 생명주기를 관리합니다.
        /// /// 프리팹을 인스턴스화하고 애니메이터를 제어하여 표시, 숨기기, 표정 변경을 처리합니다.
        /// </summary>
        /// <param name="command">The character command to process. /// (Korean) 처리할 캐릭터 명령입니다.</param>
        /// <param name="characterData">The database entry for the character. /// (Korean) 캐릭터의 데이터베이스 항목입니다.</param>
        private void Handle2DCharacter(CharacterCommand command, CharacterData characterData)
        {
            Character2DPositionSlot slot = _character2DSlots.FirstOrDefault(s => s.position == command.Position);
            if (slot == null || slot.anchor == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"2D slot for position '{command.Position}' is not configured in CharacterManager.");
#endif
                return;
            }
            
            if (command.Action == CharacterAction.Show || command.Action == CharacterAction.ChangeExpression)
            {
                GameObject characterInstance;
                if (!_active2DCharacters.TryGetValue(command.Position, out characterInstance) || characterInstance.name != characterData.Prefab.name + "(Clone)")
                {
                    if (characterInstance != null)
                    {
                        Destroy(characterInstance);
                    }
                    characterInstance = Instantiate(characterData.Prefab, slot.anchor);
                    _active2DCharacters[command.Position] = characterInstance;
                }

                Animator animator = characterInstance.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = characterInstance.GetComponentInChildren<Animator>();
                }
                if (animator == null)
                {
#if UNITY_EDITOR
                    Debug.LogError($"Animator component is missing on the 2D character prefab for: {characterData.CharacterID}");
#endif
                    return;
                }

                string expressionName = string.IsNullOrEmpty(command.Expression) ? "default" : command.Expression;
                CharacterExpression expression = characterData.Expressions.FirstOrDefault(e => e.ExpressionName == expressionName);
                if (expression != null && !string.IsNullOrEmpty(expression.AnimationStateName))
                {
                    animator.Play(expression.AnimationStateName);
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"Expression '{expressionName}' or its AnimationStateName not found for character '{command.CharacterId}'.");
#endif
                }
            }
            else if (command.Action == CharacterAction.Hide)
            {
                if (_active2DCharacters.TryGetValue(command.Position, out GameObject characterToHide))
                {
                    Destroy(characterToHide);
                    _active2DCharacters.Remove(command.Position);
                }
            }
        }

        /// <summary>
        /// Manages the lifecycle of a 3D character based on the received command.
        /// Handles showing, hiding, and changing expressions by instantiating prefabs, controlling their animators, and managing their display via RawImage.
        /// /// (Korean) 수신된 명령에 따라 3D 캐릭터의 생명주기를 관리합니다.
        /// /// 프리팹 인스턴스화, 애니메이터 제어, RawImage를 통한 표시 관리를 통해 표시, 숨기기, 표정 변경을 처리합니다.
        /// </summary>
        /// <param name="command">The character command to process. /// (Korean) 처리할 캐릭터 명령입니다.</param>
        /// <param name="characterData">The database entry for the character. /// (Korean) 캐릭터의 데이터베이스 항목입니다.</param>
        private void Handle3DCharacter(CharacterCommand command, CharacterData characterData)
        {
            Character3DPositionSlot slot = _character3DSlots.FirstOrDefault(s => s.position == command.Position);
            if (slot == null || slot.anchor == null || slot.displayImage == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"3D slot for position '{command.Position}' is not configured in CharacterManager.");
#endif
                return;
            }

            switch (command.Action)
            {
                case CharacterAction.Show:
                    if (_active3DCharacters.TryGetValue(command.Position, out GameObject existingCharacter))
                    {
                        Destroy(existingCharacter);
                    }

                    GameObject newCharacter = Instantiate(characterData.Prefab, slot.anchor.position, slot.anchor.rotation);
                    SetLayerRecursively(newCharacter, _character3DLayer);
                    _active3DCharacters[command.Position] = newCharacter;
                    slot.displayImage.gameObject.SetActive(true);
                    
                    goto case CharacterAction.ChangeExpression;

                case CharacterAction.ChangeExpression:
                    if (_active3DCharacters.TryGetValue(command.Position, out GameObject activeCharacter))
                    {
                        Animator animator = activeCharacter.GetComponent<Animator>();
                        if (animator == null)
                        {
                            animator = activeCharacter.GetComponentInChildren<Animator>();
                        }
                        if (animator == null)
                        {
#if UNITY_EDITOR
                            Debug.LogError($"Animator component is missing on the 3D character prefab for: {characterData.CharacterID}");
#endif
                            return;
                        }
                        
                        string expressionName = string.IsNullOrEmpty(command.Expression) ? "default" : command.Expression;
                        CharacterExpression expression = characterData.Expressions.FirstOrDefault(e => e.ExpressionName == expressionName);
                        if (expression != null && !string.IsNullOrEmpty(expression.AnimationStateName))
                        {
                            animator.Play(expression.AnimationStateName);
                        }
                        else
                        {
#if UNITY_EDITOR
                            Debug.LogWarning($"Expression '{expressionName}' or its AnimationStateName not found for character '{command.CharacterId}'.");
#endif
                        }
                    }
                    break;

                case CharacterAction.Hide:
                    if (_active3DCharacters.TryGetValue(command.Position, out GameObject characterToHide))
                    {
                        Destroy(characterToHide);
                        _active3DCharacters.Remove(command.Position);
                        slot.displayImage.gameObject.SetActive(false);
                    }
                    break;
            }
        }

        /// <summary>
        /// Recursively sets the layer for a GameObject and all of its children.
        /// /// (Korean) GameObject와 모든 자식 객체의 레이어를 재귀적으로 설정합니다.
        /// </summary>
        /// <param name="obj">The root GameObject. /// (Korean) 루트 GameObject입니다.</param>
        /// <param name="layer">The layer index to set. /// (Korean) 설정할 레이어 인덱스입니다.</param>
        private void SetLayerRecursively(GameObject obj, int layer)
        {
            if(layer == -1) return;
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }
}
