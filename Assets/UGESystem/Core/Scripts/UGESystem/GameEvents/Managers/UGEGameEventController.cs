using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// The core executor for a single GameEvent. It processes a list of commands sequentially,
    /// using a strategy pattern to delegate execution to different handlers based on the event's context (e.g., Dialogue vs. Cinematic).
    /// </summary>
    public class UGEGameEventController : MonoBehaviour
    {
        /// <summary>
        /// Fired when a GameEvent has finished its execution.
        /// Passes the completed event.
        /// </summary>
        public static event Action<GameEvent> OnEventFinished;

        /// <summary>
        /// Reference to the UI Manager for displaying dialogue, choices, etc.
        /// </summary>
        public UGEUIManager UIManager { get; set; }
        /// <summary>
        /// Reference to the Character Manager for handling character display and animations.
        /// </summary>
        public UGECharacterManager CharacterManager { get; set; }
        /// <summary>
        /// Reference to the Camera Manager for handling camera movements and effects.
        /// </summary>
        public UGECameraManager CameraManager { get; set; }
        /// <summary>
        /// Reference to the Sound Manager for playing BGM and SFX.
        /// </summary>
        public UGESoundManager SoundManager { get; set; }
        /// <summary>
        /// Reference to the Input Manager for handling user input during events.
        /// </summary>
        public UGEInputManager InputManager { get; set; }
        
        /// <summary>
        /// Gets a value indicating whether a GameEvent is currently running.
        /// </summary>
        public bool IsEventRunning { get; private set; } = false;
        private bool _isSkipActive = false;
        /// <summary>
        /// Gets a value indicating whether the cinematic skip has been triggered.
        /// </summary>
        public bool IsSkipActive => _isSkipActive;
        
        private GameEvent _currentEvent;
        private Storyboard _currentStoryboard; // 현재 실행중인 스토리보드 컨텍스트
        /// <summary>
        /// Gets the storyboard currently being executed.
        /// </summary>
        public Storyboard CurrentStoryboard => _currentStoryboard;

        private int _commandIndex;
        private GameEventType _currentEventType;
                
        /// <summary>
        /// Gets or sets a value indicating whether the controller is waiting for user input (e.g., for dialogue continuation or a choice).
        /// </summary>
        public bool IsWaitingForChoice { get; set; } = false;
                
        private Dictionary<string, int> _labelMap;
        private Dictionary<GameEventType, Dictionary<Type, ICommandHandler>> _commandHandlers;
        private float _lastContinueTime = 0f;
        
        private void Awake()
        {
            InitializeCommandHandlers();
        }
        
        private void InitializeCommandHandlers()
        {
            // 모든 핸들러 인스턴스를 단 한 번만 생성하여 재사용합니다.
            var backgroundHandler = new DialogueNode_BackgroundCommandHandler();
            var characterHandler = new DialogueNode_CharacterCommandHandler();
            var choiceHandler = new DialogueNode_ChoiceCommandHandler();
            var dialogueHandler = new DialogueNode_DialogueCommandHandler();
            var endHandler = new DialogueNode_EndCommandHandler();
            var gotoHandler = new DialogueNode_GotoCommandHandler();
            var labelHandler = new DialogueNode_LabelCommandHandler();
            var cameraHandler = new UGECameraCommandHandler();
            var screenEffectHandler = new ScreenEffectCommandHandler();
            var playSoundHandler = new PlaySoundCommandHandler();
            var triggerEventHandler = new TriggerEventCommandHandler();
            var rewardHandler = new RewardCommandHandler();
            var characterUpdateHandler = new CharacterUpdateCommandHandler();
            
            // 시네마틱 전용 다이얼로그 핸들러
            var cinematicDialogueHandler = new CinematicNode_DialogueCommandHandler();


            _commandHandlers = new Dictionary<GameEventType, Dictionary<Type, ICommandHandler>>
            {
                {
                    GameEventType.Dialogue, new Dictionary<Type, ICommandHandler>
                    {
                        { typeof(BackgroundCommand), backgroundHandler },
                        { typeof(CharacterCommand), characterHandler },
                        { typeof(ChoiceCommand), choiceHandler },
                        { typeof(DialogueCommand), dialogueHandler },
                        { typeof(EndCommand), endHandler },
                        { typeof(GotoCommand), gotoHandler },
                        { typeof(LabelCommand), labelHandler },
                        { typeof(UGECameraCommand), cameraHandler },
                        { typeof(ScreenEffectCommand), screenEffectHandler },
                        { typeof(PlaySoundCommand), playSoundHandler },
                        { typeof(TriggerEventCommand), triggerEventHandler },
                        { typeof(RewardCommand), rewardHandler },
                        { typeof(CharacterUpdateCommand), characterUpdateHandler },
                    }
                },
                {
                    GameEventType.CinematicText, new Dictionary<Type, ICommandHandler>
                    {
                        { typeof(BackgroundCommand), backgroundHandler },
                        { typeof(CharacterCommand), characterHandler },
                        { typeof(DialogueCommand), cinematicDialogueHandler },
                        { typeof(EndCommand), endHandler },
                        { typeof(UGECameraCommand), cameraHandler },
                        { typeof(ScreenEffectCommand), screenEffectHandler },
                        { typeof(PlaySoundCommand), playSoundHandler },
                        { typeof(TriggerEventCommand), triggerEventHandler },
                        { typeof(RewardCommand), rewardHandler },
                        { typeof(CharacterUpdateCommand), characterUpdateHandler },
                    }
                },
            };
        }
        
        /// <summary>
        /// Starts processing a given GameEvent.
        /// </summary>
        public void StartEvent(GameEvent gameEvent, GameEventType eventType, Storyboard storyboard)
        {
            StartCoroutine(ProcessEventCoroutine(gameEvent, eventType, storyboard));
        }
        
        private IEnumerator ProcessEventCoroutine(GameEvent gameEvent, GameEventType eventType, Storyboard storyboard)
        {
            if (gameEvent == null || gameEvent.Commands.Count == 0) yield break;
            if (IsEventRunning) yield break;
        
            yield return null;
        
            IsEventRunning = true;
            _currentEvent = gameEvent;
            _currentStoryboard = storyboard;
            _currentEventType = eventType;
            _commandIndex = 0;
            IsWaitingForChoice = false;
            _isSkipActive = false;
        
            if (_currentEventType == GameEventType.CinematicText)
            {
                InputManager.OnSkipCinematic += SkipCinematicEvent;
                InputManager.EnableCinematicSkipListener(true);
            }
        
            BuildLabelMap();
        
            while (_commandIndex < _currentEvent.Commands.Count)
            {
                if (!IsEventRunning) yield break;
        
                IGameEventCommand command = _currentEvent.Commands[_commandIndex];
                if (command == null)
                {
                    _commandIndex++;
                    continue;
                }
        
                Type commandType = command.GetType();
                if (!_commandHandlers.TryGetValue(_currentEventType, out var handlers))
                {
                    _commandIndex++;
                    continue;
                }
        
                if (handlers.TryGetValue(commandType, out var handler))
                {
                    yield return handler.Execute(command, this);
                    if (!IsEventRunning) yield break;
                }
        
                if (IsWaitingForChoice)
                {
                    InputManager.EnableDialogueContinueListener(true);
                    yield return new WaitUntil(() => !IsWaitingForChoice);
                    InputManager.EnableDialogueContinueListener(false);
                }
        
                _commandIndex++;
            }
        
            EndEvent(new EndCommand());
        }
        
        public void ContinueEvent()
        {
            if (Time.time < _lastContinueTime + 0.2f) return;
            _lastContinueTime = Time.time;
            IsWaitingForChoice = false;
        }
        
        public void OnChoiceSelected(int choiceIndex)
        {
            var choiceCommand = _currentEvent.Commands[_commandIndex] as ChoiceCommand;
            if (choiceCommand == null) return;
        
            string targetLabel = choiceCommand.Choices[choiceIndex].TargetLabel;
            JumpToLabel(targetLabel);
            ContinueEvent();
        }
        
        public void JumpToLabel(string label)
        {
            if (_labelMap.TryGetValue(label, out int targetIndex)) _commandIndex = targetIndex;
            else _commandIndex++;
        }
        
        private void BuildLabelMap()
        {
            _labelMap = new Dictionary<string, int>();
            for (int i = 0; i < _currentEvent.Commands.Count; i++)
            {
                if (_currentEvent.Commands[i] is LabelCommand labelCommand)
                {
                    if (!string.IsNullOrEmpty(labelCommand.LabelName) && !_labelMap.ContainsKey(labelCommand.LabelName))
                        _labelMap.Add(labelCommand.LabelName, i);
                }
            }
        }
        
        private void SkipCinematicEvent()
        {
            if (_currentEventType == GameEventType.CinematicText) _isSkipActive = true;
        }
        
        public void EndEvent(EndCommand command)
        {
            if (!IsEventRunning) return;
                        
            InputManager.EnableDialogueContinueListener(false);
            if (_currentEventType == GameEventType.CinematicText)
            {
                InputManager.OnSkipCinematic -= SkipCinematicEvent;
                InputManager.EnableCinematicSkipListener(false);
            }
            _isSkipActive = false;
                        
            var finishedEvent = _currentEvent;
            OnEventFinished?.Invoke(finishedEvent);

            if (command.IsBranching)
                UGEDelayedEventBus.Publish(new JumpToNodeEvent(_currentStoryboard, command.TargetNodeID));
                
            UIManager.HideAllUI();
            CharacterManager.HideAllCharacters();
            CameraManager.ResetCamera();
            if (UGESystemController.Instance.ScreenEffectManager != null)
                UGESystemController.Instance.ScreenEffectManager.ClearEffect();
                        
            IsEventRunning = false;
            _currentEvent = null;
            _currentStoryboard = null; 
        }
    }
}
