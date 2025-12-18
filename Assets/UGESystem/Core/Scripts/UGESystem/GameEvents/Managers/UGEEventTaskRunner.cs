using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A MonoBehaviour responsible for managing the execution of a single Storyboard asset.
    /// It subscribes to all node start conditions and initiates node execution when conditions are met.
    /// </summary>
    public class UGEEventTaskRunner : MonoBehaviour
    {
        [Tooltip("The Storyboard asset that this Task Runner will execute.")]
        [SerializeField] private Storyboard _storyboard;
        [Tooltip("The lower the number, the higher the priority to execute first.")]
        [SerializeField] private int _priority = 100;

        /// <summary>
        /// The execution priority of this runner. Runners with lower numbers are executed first.
        /// </summary>
        public int Priority => _priority;
        /// <summary>
        /// The Storyboard asset managed by this runner.
        /// </summary>
        public Storyboard Storyboard => _storyboard;

        [Tooltip("A unique ID to find and trigger this runner from a command.")]
        [SerializeField] private string _runnerId = System.Guid.NewGuid().ToString(); // 기본값으로 GUID 할당
        /// <summary>
        /// A unique identifier for this runner, used by TriggerEventCommand.
        /// </summary>
        public string RunnerId => _runnerId;

#if UNITY_EDITOR
        private void Reset()
        {
            var allRunners = FindObjectsByType<UGEEventTaskRunner>(FindObjectsSortMode.None);
            if (allRunners.Length > 1)
            {
                int maxPriority = -1;
                foreach(var runner in allRunners)
                {
                    if (runner != this)
                    {
                        if (runner.Priority > maxPriority)
                        {
                            maxPriority = runner.Priority;
                        }
                    }
                }
                _priority = maxPriority + 1;
            }
            else
            {
                _priority = 0;
            }
        }
#endif

        private Dictionary<string, EventStatus> _nodeStatus = new Dictionary<string, EventStatus>();
        /// <summary>
        /// A read-only dictionary tracking the current execution status of each node in the storyboard.
        /// </summary>
        public IReadOnlyDictionary<string, EventStatus> NodeStatuses => _nodeStatus; // Public accessor for editor sync
        private Dictionary<string, EventNodeData> _nodeLookup = new Dictionary<string, EventNodeData>();
        private Dictionary<GameEventType, IEventNodeRunner> _nodeRunners;

        private void Awake()
        {
            _nodeRunners = new Dictionary<GameEventType, IEventNodeRunner>
            {
                { GameEventType.Dialogue, new GameEventNodeRunner() },
                { GameEventType.CinematicText, new GameEventNodeRunner() }
            };
        }

        private void OnEnable()
        {
            if (UGESystemController.Instance != null)
            {
                UGESystemController.Instance.RegisterRunner(this);
            }
            UGEDelayedEventBus.Subscribe<JumpToNodeEvent>(OnJumpToNodeRequested);
        }

        private void OnDisable()
        {
            if (UGESystemController.Instance != null)
            {
                UGESystemController.Instance.UnregisterRunner(this);
            }
            UGEDelayedEventBus.Unsubscribe<JumpToNodeEvent>(OnJumpToNodeRequested);

            if (_storyboard == null) return;
            
            foreach (var node in _storyboard.EventNodes)
            {
                if (node == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"A null EventNodeData was found in Storyboard '{_storyboard.name}'. Skipping it.", _storyboard);
#endif
                    continue;
                }

                foreach (var condition in node.StartConditions)
                {
                    condition.Unsubscribe();
                }
            }
        }

        private void Start()
        {
            // UGESystemController가 시작 노드를 실행하기 전에,
            // 이 러너가 담당하는 스토리보드의 모든 조건들을 활성화(구독)합니다.
            // Before UGESystemController runs the start node,
            // activate (subscribe to) all conditions of the storyboard this runner is responsible for.
            InitializeStoryboard();
        }

        /// <summary>
        /// Initializes the storyboard by caching all nodes and subscribing to their start conditions.
        /// </summary>
        public void InitializeStoryboard()
        {
            if (_storyboard == null) return;

            _nodeStatus.Clear();
            _nodeLookup.Clear();

            foreach (var node in _storyboard.EventNodes)
            {
                if (node == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"A null EventNodeData was found in Storyboard '{_storyboard.name}'. Skipping it.", _storyboard);
#endif
                    continue;
                }
                
                _nodeStatus.Add(node.NodeID, EventStatus.NotStarted);
                _nodeLookup.Add(node.NodeID, node);

                foreach (var condition in node.StartConditions)
                {
                    condition.Reset();
                    condition.Subscribe(() => OnConditionStateChanged(node.NodeID));
                }
            }
        }

        private void OnConditionStateChanged(string nodeID)
        {
            if (_nodeStatus[nodeID] != EventStatus.NotStarted) return;
            var node = _nodeLookup[nodeID];
            if (node.StartConditions.All(c => c.IsMet))
            {
                TryStartNode(node);
            }
        }

        /// <summary>
        /// Attempts to start a node. If another event is already running, it enqueues the node in the global controller.
        /// </summary>
        /// <param name="node">The event node to start.</param>
        public void TryStartNode(EventNodeData node)
        {
            if (UGESystemController.Instance.GameEventController.IsEventRunning)
            {
                // 다른 이벤트가 실행 중이면 글로벌 대기열에 추가
                // If another event is running, add to the global queue
                UGESystemController.Instance.EnqueueNode(this, node);
            }
            else
            {
                StartNode(node);
            }
        }
        
        private void OnJumpToNodeRequested(JumpToNodeEvent evt)
        {
            // 이 점프 이벤트가 현재 Runner의 Storyboard를 위한 것이 아니면 무시합니다.
            // If this jump event is not for the current Runner's Storyboard, ignore it.
            if (evt.TargetStoryboard != _storyboard)
            {
                return;
            }

            string currentRunningNodeID = _nodeStatus.FirstOrDefault(s => s.Value == EventStatus.InProgress).Key;
            if (!string.IsNullOrEmpty(currentRunningNodeID))
            {
                _nodeStatus[currentRunningNodeID] = EventStatus.Completed;
                UGEDelayedEventBus.Publish(new NodeCompletedEvent(currentRunningNodeID));
            }
            
            if (_nodeLookup.TryGetValue(evt.TargetNodeID, out var nextNode))
            {
                TryStartNode(nextNode);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError($"Jump failed: Node with ID '{evt.TargetNodeID}' not found in storyboard '{_storyboard.name}'.");
#endif
            }
        }

        private void GrantRewards(List<AbstractEventReward> rewards)
        {
            foreach (var reward in rewards)
            {
                reward.GrantReward(this);
            }
        }

        /// <summary>
        /// Immediately starts the execution of a specific node, bypassing the global queue.
        /// This should generally be called by the UGESystemController.
        /// </summary>
        /// <param name="node">The event node to execute.</param>
        public void StartNode(EventNodeData node) // Now public so Controller can call it
        {
            // If called from outside Start() (like in tests), _nodeStatus might not be initialized yet.
            // In that case, run initialization.
            if (!_nodeStatus.ContainsKey(node.NodeID))
            {
                InitializeStoryboard();
            }

            if (_nodeStatus.ContainsKey(node.NodeID) && _nodeStatus[node.NodeID] != EventStatus.NotStarted) return;
            
            _nodeStatus[node.NodeID] = EventStatus.InProgress;
            UGEDelayedEventBus.Publish(new NodeStartedEvent(node.NodeID));
            StartCoroutine(RunNodeCoroutine(node));
        }

        private System.Collections.IEnumerator RunNodeCoroutine(EventNodeData node)
        {
            if (_nodeRunners.TryGetValue(node.Type, out IEventNodeRunner runner))
            {
                NodeRunResult result = null;
                // 'this'를 전달하여 runner가 어떤 TaskRunner에 의해 실행되었는지 알 수 있도록 함
                // Pass 'this' so the runner knows which TaskRunner it was executed by
                yield return runner.Run(node, this, (runResult) => { result = runResult; });
                
                _nodeStatus[node.NodeID] = EventStatus.Completed;
                UGEDelayedEventBus.Publish(new NodeCompletedEvent(node.NodeID));

                if (result != null && result.Success)
                {
                    GrantRewards(result.Rewards);
                }

                if (node.IsRepeatable)
                {
                    _nodeStatus[node.NodeID] = EventStatus.NotStarted;
                    foreach (var condition in node.StartConditions)
                    {
                        condition.Reset();
                    }
                }
                
                // 실행이 끝났으므로, 글로벌 큐의 다음 노드를 실행하도록 컨트롤러에 알림
                // Since execution is finished, notify the controller to run the next node in the global queue
                UGESystemController.Instance.TryStartNextPendingNode();
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError($"No runner found for GameEventType: {node.Type} on node '{node.Name}'");
#endif
            }
        }
    }
}