using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UGESystem
{
    /// <summary>
    /// The central singleton controller for the UGESystem.
    /// It owns and manages all core manager components and controls the execution order
    /// of event nodes through a global event queue. Only one instance should exist in the scene.
    /// </summary>
    public class UGESystemController : Singleton<UGESystemController>
    {
        private UGEUIManager _uiManager;
        /// <summary>
        /// Manages all UI elements related to events (dialogue boxes, choices, etc.).
        /// </summary>
        public UGEUIManager UIManager
        {
            get
            {
                if (_uiManager == null) _uiManager = FindOrCreateManager<UGEUIManager>();
                return _uiManager;
            }
        }

        private UGECharacterManager _characterManager;
        /// <summary>
        /// Manages character instantiation, placement, and animations.
        /// </summary>
        public UGECharacterManager CharacterManager
        {
            get
            {
                if (_characterManager == null) _characterManager = FindOrCreateManager<UGECharacterManager>();
                return _characterManager;
            }
        }

        private UGEGameEventController _gameEventController;
        /// <summary>
        /// Executes the command sequence within a single GameEvent.
        /// </summary>
        public UGEGameEventController GameEventController
        {
            get
            {
                if (_gameEventController == null) _gameEventController = FindOrCreateManager<UGEGameEventController>();
                return _gameEventController;
            }
        }

        private UGECameraManager _cameraManager;
        /// <summary>
        /// Manages all Cinemachine-based camera operations during events.
        /// </summary>
        public UGECameraManager CameraManager
        {
            get
            {
                if (_cameraManager == null) _cameraManager = FindOrCreateManager<UGECameraManager>();
                return _cameraManager;
            }
        }

        private UGEDelayedEventInvoker _delayedEventInvoker;
        /// <summary>
        /// Manages the delayed invocation of events from the event bus to prevent race conditions.
        /// </summary>
        public UGEDelayedEventInvoker DelayedEventInvoker
        {
            get
            {
                if (_delayedEventInvoker == null) _delayedEventInvoker = FindOrCreateManager<UGEDelayedEventInvoker>();
                return _delayedEventInvoker;
            }
        }

        private UGESoundManager _soundManager;
        /// <summary>
        /// Manages background music (BGM) and sound effects (SFX).
        /// </summary>
        public UGESoundManager SoundManager
        {
            get
            {
                if (_soundManager == null) _soundManager = FindOrCreateManager<UGESoundManager>();
                return _soundManager;
            }
        }

        private UGEInputManager _inputManager;
        /// <summary>
        /// Manages event-specific user inputs (e.g., continue, skip).
        /// </summary>
        public UGEInputManager InputManager
        {
            get
            {
                if (_inputManager == null) _inputManager = FindOrCreateManager<UGEInputManager>();
                return _inputManager;
            }
        }

        private UGEScreenEffectManager _screenEffectManager;
        /// <summary>
        /// Manages full-screen effects like fades and tints.
        /// </summary>
        public UGEScreenEffectManager ScreenEffectManager
        {
            get
            {
                if (_screenEffectManager == null) _screenEffectManager = FindOrCreateManager<UGEScreenEffectManager>();
                return _screenEffectManager;
            }
        }

        // Runner and Queue Management
        private List<UGEEventTaskRunner> _activeRunners = new List<UGEEventTaskRunner>();
        private List<(UGEEventTaskRunner runner, EventNodeData node, int insertionOrder)> _globalPendingNodes = new List<(UGEEventTaskRunner, EventNodeData, int)>();
        private int _insertionCounter = 0;

        private bool _initialEventsKickedOff = false;

        protected override void OnAwake()
        {
            base.OnAwake();

            // Inject dependencies.
            // Accessing the properties ensures they are initialized via FindOrCreateManager.
            if (GameEventController != null)
            {
                GameEventController.UIManager = UIManager;
                GameEventController.CharacterManager = CharacterManager;
                GameEventController.CameraManager = CameraManager;
                GameEventController.SoundManager = SoundManager;
                GameEventController.InputManager = InputManager;
            }
        }

        private T FindOrCreateManager<T>() where T : Component
        {
            // 먼저 이미 생성된 필드에 있는지 확인 (Lazy Init 중복 호출 방지용)
            // First check if it's already in the created field (to prevent redundant Lazy Init calls)
            // (이 메서드는 프로퍼티 내부에서 호출되므로 이 체크는 사실상 프로퍼티의 null 체크와 동일하지만, 안전을 위해 유지)

            T manager = GetComponentInChildren<T>(true);
            if (manager == null)
            {
                var go = new GameObject(typeof(T).Name);
                go.transform.SetParent(transform);
                manager = go.AddComponent<T>();
            }
            return manager;
        }

        private void LateUpdate()
        {
            // Execute initial event kickstart logic only once after game start.
            if (!_initialEventsKickedOff)
            {
                KickstartInitialEvents();
                _initialEventsKickedOff = true;
            }
        }

        private void KickstartInitialEvents()
        {
            if (_activeRunners.Count == 0) return;

            // Find the highest priority (lowest number).
            int highestPriority = _activeRunners.Min(r => r.Priority);

            // Find all runners with that priority.
            var highestPriorityRunners = _activeRunners.Where(r => r.Priority == highestPriority).ToList();

            if (highestPriority == 0)
            {
                // If priority is 0, attempt to start nodes for all priority 0 runners (they will enter the queue sequentially).
                // Order by name to ensure deterministic execution order.
                foreach (var runner in highestPriorityRunners.OrderBy(r => r.name))
                {
                    if (runner.Storyboard != null)
                    {
                        var startNode = runner.Storyboard.EventNodes.FirstOrDefault(n => n.IsStartNode);
                        if (startNode != null)
                        {
                            runner.TryStartNode(startNode);
                        }
                    }
                }
            }
            else // highestPriority > 0
            {
                // If priority is greater than 0, only one of the highest priority runners will execute.
                // Select the first runner by name to ensure deterministic execution order.
                var runnerToStart = highestPriorityRunners.OrderBy(r => r.name).FirstOrDefault();
                if (runnerToStart != null && runnerToStart.Storyboard != null)
                {
                    var startNode = runnerToStart.Storyboard.EventNodes.FirstOrDefault(n => n.IsStartNode);
                    if (startNode != null)
                    {
                        runnerToStart.TryStartNode(startNode);
                    }
                }
            }
        }


        #region Runner and Queue Logic
        /// <summary>
        /// Registers a UGEEventTaskRunner with the controller when it becomes active.
        /// Checks for duplicate RunnerIds.
        /// </summary>
        /// <param name="runner">The runner to register.</param>
        public void RegisterRunner(UGEEventTaskRunner runner)
        {
            if (string.IsNullOrEmpty(runner.RunnerId))
            {
                Debug.LogError($"[UGESystemController] Runner '{runner.name}' has no RunnerId assigned!", runner);
                return;
            }

            // Check for duplicates
            var duplicate = _activeRunners.FirstOrDefault(r => r.RunnerId == runner.RunnerId);
            if (duplicate != null && duplicate != runner)
            {
                Debug.LogError($"[UGESystemController] Duplicate RunnerId detected: '{runner.RunnerId}'! " +
                    $"Conflict between '{runner.name}' and '{duplicate.name}'. " +
                    $"Please regenerate RunnerId for one of them via the Inspector Context Menu.", runner);
                return;
            }

            if (!_activeRunners.Contains(runner))
            {
                _activeRunners.Add(runner);
            }
        }

        /// <summary>
        /// Unregisters a UGEEventTaskRunner from the controller when it becomes inactive.
        /// </summary>
        /// <param name="runner">The runner to unregister.</param>
        public void UnregisterRunner(UGEEventTaskRunner runner)
        {
            if (_activeRunners.Contains(runner))
            {
                _activeRunners.Remove(runner);
            }
        }

        /// <summary>
        /// Gets the first active runner associated with a specific Storyboard asset.
        /// </summary>
        /// <param name="storyboard">The storyboard to find the runner for.</param>
        /// <returns>The found UGEEventTaskRunner, or null if not found.</returns>
        public UGEEventTaskRunner GetRunnerForStoryboard(Storyboard storyboard)
        {
            if (storyboard == null) return null;
            return _activeRunners.FirstOrDefault(r => r.Storyboard == storyboard);
        }

        /// <summary>
        /// Finds an active UGEEventTaskRunner in the scene by its unique ID.
        /// </summary>
        /// <param name="runnerId">The unique ID of the runner.</param>
        /// <returns>The found UGEEventTaskRunner, or null if not found.</returns>
        public UGEEventTaskRunner GetRunnerById(string runnerId)
        {
            if (string.IsNullOrEmpty(runnerId)) return null;
            return _activeRunners.FirstOrDefault(r => r.RunnerId == runnerId);
        }

        /// <summary>
        /// Adds a node to the global pending queue to be executed when the system is ready.
        /// </summary>
        /// <param name="runner">The runner responsible for the node.</param>
        /// <param name="node">The event node to enqueue.</param>
        public void EnqueueNode(UGEEventTaskRunner runner, EventNodeData node)
        {
            _globalPendingNodes.Add((runner, node, _insertionCounter++));
        }

        /// <summary>
        /// Attempts to execute the next highest-priority node from the global pending queue if no other event is running.
        /// </summary>
        public void TryStartNextPendingNode()
        {
            if (GameEventController.IsEventRunning || _globalPendingNodes.Count == 0)
            {
                return;
            }

            // Use List.Sort for in-place, stable sorting.
            _globalPendingNodes.Sort((item1, item2) =>
            {
                int priorityComparison = item1.runner.Priority.CompareTo(item2.runner.Priority);
                if (priorityComparison != 0)
                {
                    return priorityComparison; // Sort by priority (ascending, lower number is higher priority)
                }
                // If priorities are equal, sort by insertion order to maintain FIFO
                return item1.insertionOrder.CompareTo(item2.insertionOrder);
            });

            var nextItem = _globalPendingNodes[0];
            _globalPendingNodes.RemoveAt(0);

            nextItem.runner.StartNode(nextItem.node);
        }
        #endregion

        #region Save / Load Global API
        /// <summary>
        /// Captures the runtime status of all active storyboards in the scene.
        /// This can be called by an external save system to snapshot the story progress.
        /// </summary>
        /// <returns>A list of state DTOs for each active runner.</returns>
        public List<RunnerStateDto> CaptureAllStoryboardsState()
        {
            var states = new List<RunnerStateDto>();
            foreach (var runner in _activeRunners)
            {
                states.Add(runner.CaptureState());
            }
            return states;
        }

        /// <summary>
        /// Restores the runtime status of storyboards from a previously captured state.
        /// This will re-initialize node statuses and resume any InProgress events.
        /// </summary>
        /// <param name="savedStates">The list of saved runner states to restore.</param>
        public void RestoreAllStoryboardsState(List<RunnerStateDto> savedStates)
        {
            if (savedStates == null) return;

            foreach (var state in savedStates)
            {
                var runner = GetRunnerById(state.RunnerID);
                if (runner != null)
                {
                    runner.RestoreState(state);
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"[UGESystemController] Could not find runner with ID '{state.RunnerID}' for restoration. Skipping.");
#endif
                }
            }

            // After all restorations, if an event was resumed (InProgress), it might have entered the queue.
            // But RestoreAllStoryboardsState is likely called before/during initialization, 
            // so we set a flag to ensure KickstartInitialEvents doesn't conflict.
            _initialEventsKickedOff = true; 
            
            // Try starting the first queued node if any were resumed.
            TryStartNextPendingNode();
        }
        #endregion
    }
}