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
        /// <summary>
        /// Manages all UI elements related to events (dialogue boxes, choices, etc.).
        /// </summary>
        public UGEUIManager UIManager { get; private set; }
        /// <summary>
        /// Manages character instantiation, placement, and animations.
        /// </summary>
        public UGECharacterManager CharacterManager { get; private set; }
        /// <summary>
        /// Executes the command sequence within a single GameEvent.
        /// </summary>
        public UGEGameEventController GameEventController { get; private set; }
        /// <summary>
        /// Manages all Cinemachine-based camera operations during events.
        /// </summary>
        public UGECameraManager CameraManager { get; private set; }
        /// <summary>
        /// Manages the delayed invocation of events from the event bus to prevent race conditions.
        /// </summary>
        public UGEDelayedEventInvoker DelayedEventInvoker { get; private set; }
        /// <summary>
        /// Manages background music (BGM) and sound effects (SFX).
        /// </summary>
        public UGESoundManager SoundManager { get; private set; }
        /// <summary>
        /// Manages event-specific user inputs (e.g., continue, skip).
        /// </summary>
        public UGEInputManager InputManager { get; private set; }
        /// <summary>
        /// Manages full-screen effects like fades and tints.
        /// </summary>
        public UGEScreenEffectManager ScreenEffectManager { get; private set; }

        // Runner and Queue Management
        private List<UGEEventTaskRunner> _activeRunners = new List<UGEEventTaskRunner>();
        private List<(UGEEventTaskRunner runner, EventNodeData node, int insertionOrder)> _globalPendingNodes = new List<(UGEEventTaskRunner, EventNodeData, int)>();
        private int _insertionCounter = 0;

        private bool _initialEventsKickedOff = false; 

        protected override void OnAwake()
        {
            base.OnAwake();

            // Find or create manager components as children.
            UIManager = FindOrCreateManager<UGEUIManager>();
            CharacterManager = FindOrCreateManager<UGECharacterManager>();
            GameEventController = FindOrCreateManager<UGEGameEventController>();
            CameraManager = FindOrCreateManager<UGECameraManager>();
            DelayedEventInvoker = FindOrCreateManager<UGEDelayedEventInvoker>();
            SoundManager = FindOrCreateManager<UGESoundManager>();
            InputManager = FindOrCreateManager<UGEInputManager>();
            ScreenEffectManager = FindOrCreateManager<UGEScreenEffectManager>();

            // Inject dependencies.
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
        /// </summary>
        /// <param name="runner">The runner to register.</param>
        public void RegisterRunner(UGEEventTaskRunner runner)
        {
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
    }
}
