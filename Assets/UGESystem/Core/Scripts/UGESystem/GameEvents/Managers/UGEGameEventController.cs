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
        /// Passes the completed event and a list of rewards to be granted.
        /// </summary>
        public static event Action<GameEvent, List<AbstractEventReward>> OnEventFinished;

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
        private int _commandIndex;
        private GameEventType _currentEventType;
                
        /// <summary>
        /// Gets or sets a value indicating whether the controller is waiting for user input (e.g., for dialogue continuation or a choice).
        /// </summary>
        public bool IsWaitingForChoice { get; set; } = false;
                
        private Dictionary<string, int> _labelMap;
        private Dictionary<GameEventType, Dictionary<Type, ICommandHandler>> _commandHandlers;
        
        private void Awake()
        {
            InitializeCommandHandlers();
        }
        
        private void InitializeCommandHandlers()
        {
            // 모든 핸들러 인스턴스를 단 한 번만 생성하여 재사용합니다.
            // Create all handler instances only once for reuse.
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
                    }
                },
                {
                    GameEventType.CinematicText, new Dictionary<Type, ICommandHandler>
                    {
                        { typeof(BackgroundCommand), backgroundHandler },
                        { typeof(CharacterCommand), characterHandler },
                        { typeof(DialogueCommand), cinematicDialogueHandler }, // 시네마틱 전용 핸들러 사용
                        { typeof(EndCommand), endHandler },
                        { typeof(UGECameraCommand), cameraHandler },
                        { typeof(ScreenEffectCommand), screenEffectHandler },
                        { typeof(PlaySoundCommand), playSoundHandler },
                        { typeof(TriggerEventCommand), triggerEventHandler },
                    }
                },
            };
        }
        
        /// <summary>
        /// Starts processing a given GameEvent.
        /// </summary>
        /// <param name="gameEvent">The GameEvent asset to execute.</param>
        /// <param name="eventType">The context in which the event is running (e.g., Dialogue, CinematicText).</param>
        /// <param name="storyboard">The parent storyboard this event belongs to.</param>
        public void StartEvent(GameEvent gameEvent, GameEventType eventType, Storyboard storyboard)
        {
            StartCoroutine(ProcessEventCoroutine(gameEvent, eventType, storyboard));
        }
        
        private IEnumerator ProcessEventCoroutine(GameEvent gameEvent, GameEventType eventType, Storyboard storyboard)
        {
            if (gameEvent == null || gameEvent.Commands.Count == 0)
            {
                yield break;
            }
        
            if (IsEventRunning)
            {
                yield break;
            }
        
            yield return null;
        
            IsEventRunning = true;
            _currentEvent = gameEvent;
            _currentStoryboard = storyboard;
            _currentEventType = eventType;
            _commandIndex = 0;
            IsWaitingForChoice = false;
            _isSkipActive = false; // 이벤트 시작 시 스킵 플래그 초기화
        
            // 시네마틱 스킵 리스너 구독
            // Subscribe to cinematic skip listener
            if (_currentEventType == GameEventType.CinematicText)
            {
                InputManager.OnSkipCinematic += SkipCinematicEvent;
                InputManager.EnableCinematicSkipListener(true);
            }
        
            BuildLabelMap();
        
            while (_commandIndex < _currentEvent.Commands.Count)
            {
                if (!IsEventRunning)
                {
                    yield break;
                }
        
                IGameEventCommand command = _currentEvent.Commands[_commandIndex];
        
                if (command == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"Null command found at index {_commandIndex} in GameEvent '{_currentEvent.name}'. Skipping.");
#endif
                    _commandIndex++;
                    continue;
                }
        
                Type commandType = command.GetType();
        
                if (!_commandHandlers.TryGetValue(_currentEventType, out var handlers))
                {
#if UNITY_EDITOR
                    Debug.LogError($"[GameEventController] No handler dictionary found for GameEventType '{_currentEventType}'. Please register it in InitializeCommandHandlers.");
#endif
                    _commandIndex++;
                    continue;
                }
        
                if (handlers.TryGetValue(commandType, out var handler))
                {
                    yield return handler.Execute(command, this);
                    if (!IsEventRunning)
                    {
                        yield break;
                    }
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"No handler found for command '{commandType.Name}' in context '{_currentEventType}'. Skipping command.");
#endif
                }
        
                if (IsWaitingForChoice)
                {
                    // The UIManager is now responsible for handling the continue input,
                    // so this controller no longer needs to subscribe to the InputManager directly.
                    // 이제 UIManager가 계속 입력을 처리하므로, 이 컨트롤러는 더 이상 InputManager를 직접 구독할 필요가 없습니다.
                    InputManager.EnableDialogueContinueListener(true);
                    yield return new WaitUntil(() => !IsWaitingForChoice);
                    InputManager.EnableDialogueContinueListener(false);
                }
        
                _commandIndex++;
            }
        
            EndEvent(new EndCommand());
        }
        
        /// <summary>
        /// Signals the controller to stop waiting and proceed to the next command.
        /// Typically called by user input.
        /// </summary>
        public void ContinueEvent()
        {
            IsWaitingForChoice = false;
        }
        
        /// <summary>
        /// Handles the selection of a choice, jumping to the corresponding label in the event.
        /// </summary>
        /// <param name="choiceIndex">The index of the selected choice.</param>
        public void OnChoiceSelected(int choiceIndex)
        {
            var choiceCommand = _currentEvent.Commands[_commandIndex] as ChoiceCommand;
            if (choiceCommand == null) return;
        
            string targetLabel = choiceCommand.Choices[choiceIndex].TargetLabel;
        
            JumpToLabel(targetLabel);
            ContinueEvent();
        }
        
        /// <summary>
        /// Jumps the command execution to the index of a specified label.
        /// </summary>
        /// <param name="label">The target label name to jump to.</param>
        public void JumpToLabel(string label)
        {
            if (_labelMap.TryGetValue(label, out int targetIndex))
            {
                _commandIndex = targetIndex;
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Label '{label}' not found. Continuing to next command.");
#endif
                _commandIndex++;
            }
        }
        
        private void BuildLabelMap()
        {
            _labelMap = new Dictionary<string, int>();
            for (int i = 0; i < _currentEvent.Commands.Count; i++)
            {
                if (_currentEvent.Commands[i] is LabelCommand labelCommand)
                {
                    if (!string.IsNullOrEmpty(labelCommand.LabelName) && !_labelMap.ContainsKey(labelCommand.LabelName))
                    {
                        _labelMap.Add(labelCommand.LabelName, i);
                    }
                }
            }
        }
        
        private void SkipCinematicEvent()
        {
            if (_currentEventType == GameEventType.CinematicText)
            {
                _isSkipActive = true;
            }
        }
        
        /// <summary>
        /// Ends the current event, cleans up UI and listeners, and fires the OnEventFinished event.
        /// </summary>
        /// <param name="command">The EndCommand that triggered the event termination, containing rewards and branching info.</param>
        public void EndEvent(EndCommand command)
        {
            if (!IsEventRunning) return;
                        
            // 리스너 정리
            // Clean up listeners
            InputManager.EnableDialogueContinueListener(false);
            if (_currentEventType == GameEventType.CinematicText)
            {
                InputManager.OnSkipCinematic -= SkipCinematicEvent;
                InputManager.EnableCinematicSkipListener(false);
            }
            _isSkipActive = false; // 스킵 플래그 초기화
                        
            var finishedEvent = _currentEvent;
            var rewards = command.Rewards;
                            
            // 분기 또는 이벤트 종료 처리를 먼저 수행
            // Handle branching or event finishing first
            if (command.IsBranching)
            {
                // 상태를 초기화하기 전에 컨텍스트를 사용하여 이벤트를 발행
                // Publish the event using the context before resetting the state
                UGEDelayedEventBus.Publish(new JumpToNodeEvent(_currentStoryboard, command.TargetNodeID));
            }
            else
            {
                OnEventFinished?.Invoke(finishedEvent, rewards);
            }
                
            // 모든 처리가 끝난 후, UI 및 상태를 정리
            // After all processing is done, clean up UI and state
            UIManager.HideAllUI();
            CharacterManager.HideAllCharacters();
            CameraManager.ResetCamera();
            if (UGESystemController.Instance.ScreenEffectManager != null)
            {
                UGESystemController.Instance.ScreenEffectManager.ClearEffect();
            }
                        
            IsEventRunning = false;
            _currentEvent = null;
            _currentStoryboard = null; 
        }
    }
}
