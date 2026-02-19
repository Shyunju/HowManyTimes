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
        [SerializeField]
        private GameObject _descriptionBox;
        public GameObject DescriptionBox{get{return _descriptionBox;}}
        private UGEUIManager _uiManager;
        public UGEUIManager UIManager
        {
            get
            {
                if (_uiManager == null) _uiManager = FindOrCreateManager<UGEUIManager>();
                return _uiManager;
            }
        }

        private UGECharacterManager _characterManager;
        public UGECharacterManager CharacterManager
        {
            get
            {
                if (_characterManager == null) _characterManager = FindOrCreateManager<UGECharacterManager>();
                return _characterManager;
            }
        }

        private UGEGameEventController _gameEventController;
        public UGEGameEventController GameEventController
        {
            get
            {
                if (_gameEventController == null) _gameEventController = FindOrCreateManager<UGEGameEventController>();
                return _gameEventController;
            }
        }

        private UGECameraManager _cameraManager;
        public UGECameraManager CameraManager
        {
            get
            {
                if (_cameraManager == null) _cameraManager = FindOrCreateManager<UGECameraManager>();
                return _cameraManager;
            }
        }

        private UGEDelayedEventInvoker _delayedEventInvoker;
        public UGEDelayedEventInvoker DelayedEventInvoker
        {
            get
            {
                if (_delayedEventInvoker == null) _delayedEventInvoker = FindOrCreateManager<UGEDelayedEventInvoker>();
                return _delayedEventInvoker;
            }
        }

        private UGESoundManager _soundManager;
        public UGESoundManager SoundManager
        {
            get
            {
                if (_soundManager == null) _soundManager = FindOrCreateManager<UGESoundManager>();
                return _soundManager;
            }
        }

        private UGEInputManager _inputManager;
        public UGEInputManager InputManager
        {
            get
            {
                if (_inputManager == null) _inputManager = FindOrCreateManager<UGEInputManager>();
                return _inputManager;
            }
        }

        private UGEScreenEffectManager _screenEffectManager;
        public UGEScreenEffectManager ScreenEffectManager
        {
            get
            {
                if (_screenEffectManager == null) _screenEffectManager = FindOrCreateManager<UGEScreenEffectManager>();
                return _screenEffectManager;
            }
        }

        private List<UGEEventTaskRunner> _activeRunners = new List<UGEEventTaskRunner>();
        private List<(UGEEventTaskRunner runner, EventNodeData node, int insertionOrder)> _globalPendingNodes = new List<(UGEEventTaskRunner, EventNodeData, int)>();
        private int _insertionCounter = 0;

        private bool _initialEventsKickedOff = false;

        protected override void OnAwake()
        {
            base.OnAwake();

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
            if (!_initialEventsKickedOff)
            {
                KickstartInitialEvents();
                _initialEventsKickedOff = true;
            }
        }

        private void KickstartInitialEvents()
        {
            if (_activeRunners.Count == 0) return;

            int highestPriority = _activeRunners.Min(r => r.Priority);
            var highestPriorityRunners = _activeRunners.Where(r => r.Priority == highestPriority).ToList();

            if (highestPriority == 0)
            {
                foreach (var runner in highestPriorityRunners.OrderBy(r => r.name))
                {
                    if (runner.Storyboard != null)
                    {
                        var startNode = runner.Storyboard.EventNodes.FirstOrDefault(n => n.IsStartNode);
                        if (startNode != null) runner.TryStartNode(startNode);
                    }
                }
            }
            else
            {
                var runnerToStart = highestPriorityRunners.OrderBy(r => r.name).FirstOrDefault();
                if (runnerToStart != null && runnerToStart.Storyboard != null)
                {
                    var startNode = runnerToStart.Storyboard.EventNodes.FirstOrDefault(n => n.IsStartNode);
                    if (startNode != null) runnerToStart.TryStartNode(startNode);
                }
            }
        }


        #region Runner and Queue Logic
        public void RegisterRunner(UGEEventTaskRunner runner)
        {
            if (string.IsNullOrEmpty(runner.RunnerId)) return;

            var duplicate = _activeRunners.FirstOrDefault(r => r.RunnerId == runner.RunnerId);
            if (duplicate != null && duplicate != runner) return;

            if (!_activeRunners.Contains(runner)) _activeRunners.Add(runner);
        }

        public void UnregisterRunner(UGEEventTaskRunner runner)
        {
            if (_activeRunners.Contains(runner)) _activeRunners.Remove(runner);
        }

        public UGEEventTaskRunner GetRunnerForStoryboard(Storyboard storyboard)
        {
            if (storyboard == null) return null;
            return _activeRunners.FirstOrDefault(r => r.Storyboard == storyboard);
        }

        public UGEEventTaskRunner GetRunnerById(string runnerId)
        {
            if (string.IsNullOrEmpty(runnerId)) return null;
            return _activeRunners.FirstOrDefault(r => r.RunnerId == runnerId);
        }

        public void EnqueueNode(UGEEventTaskRunner runner, EventNodeData node)
        {
            _globalPendingNodes.Add((runner, node, _insertionCounter++));
        }

        public void TryStartNextPendingNode()
        {
            if (GameEventController.IsEventRunning || _globalPendingNodes.Count == 0) return;

            _globalPendingNodes.Sort((item1, item2) =>
            {
                int priorityComparison = item1.runner.Priority.CompareTo(item2.runner.Priority);
                if (priorityComparison != 0) return priorityComparison;
                return item1.insertionOrder.CompareTo(item2.insertionOrder);
            });

            var nextItem = _globalPendingNodes[0];
            _globalPendingNodes.RemoveAt(0);
            nextItem.runner.StartNode(nextItem.node);
        }
        #endregion

        #region Save / Load Global API
        /// <summary>
        /// Captures the entire system state, including character data and story progress.
        /// </summary>
        public UGESystemStateDto CaptureSystemState()
        {
            var systemState = new UGESystemStateDto();

            // 1. Capture Story Progress (Runners)
            foreach (var runner in _activeRunners)
            {
                systemState.RunnerStates.Add(runner.CaptureState());
            }

            // 2. Capture Modified Character Data
            if (CharacterManager != null && CharacterManager.RuntimeCharacterDB != null)
            {
                foreach (var charData in CharacterManager.RuntimeCharacterDB.Characters)
                {
                    systemState.CharacterStates.Add(new CharacterStateDto
                    {
                        CharacterID = charData.CharacterID,
                        Name = charData.Name,
                        // Visual/Template restoration could be added here later if needed.
                    });
                }
            }

            return systemState;
        }

        /// <summary>
        /// Restores the entire system state. 
        /// Crucially, it restores character data FIRST before resuming storyboards.
        /// </summary>
        public void RestoreSystemState(UGESystemStateDto savedState)
        {
            if (savedState == null) return;

            // 1. Restore Character Data FIRST
            if (CharacterManager != null && CharacterManager.RuntimeCharacterDB != null)
            {
                foreach (var charState in savedState.CharacterStates)
                {
                    var target = CharacterManager.RuntimeCharacterDB.GetCharacterData(charState.CharacterID);
                    if (target != null)
                    {
                        target.UpdateData(charState.Name, target.Is3D, target.Prefab, target.Expressions);
                    }
                }
            }

            // 2. Restore Storyboard Progress
            foreach (var runnerState in savedState.RunnerStates)
            {
                var runner = GetRunnerById(runnerState.RunnerID);
                if (runner != null)
                {
                    runner.RestoreState(runnerState);
                }
            }

            _initialEventsKickedOff = true; 
            TryStartNextPendingNode();
        }

        // Backward compatibility wrappers
        public List<RunnerStateDto> CaptureAllStoryboardsState() => CaptureSystemState().RunnerStates;
        public void RestoreAllStoryboardsState(List<RunnerStateDto> savedStates) 
        {
            var dto = new UGESystemStateDto { RunnerStates = savedStates };
            RestoreSystemState(dto);
        }
        #endregion
    }
}
