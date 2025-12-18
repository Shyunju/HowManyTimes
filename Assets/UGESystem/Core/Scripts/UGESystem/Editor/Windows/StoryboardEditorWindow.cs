using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace UGESystem
{
    /// <summary>
    /// A Unity editor window class that hosts a <see cref="StoryboardGraphView"/> to display a storyboard node-based editor.
    /// </summary>
    public class StoryboardEditorWindow : EditorWindow
    {
        private StoryboardGraphView _graphView;
        private Storyboard _currentStoryboard;
        private SerializedObject _storyboardObject;
        private bool _isFocused = false;

        /// <summary>
        /// Gets the currently loaded <see cref="Storyboard"/> asset in this editor window.
        /// </summary>
        public Storyboard CurrentStoryboard => _currentStoryboard;
        /// <summary>
        /// Retrieves the <see cref="StoryboardGraphView"/> instance hosted by this window.
        /// </summary>
        /// <returns>The <see cref="StoryboardGraphView"/> instance.</returns>
        public StoryboardGraphView GetGraphView() => _graphView;
        private const string LAST_OPEN_STORYBOARD_PATH_KEY = "UGESystem.LastOpenStoryboardPath";

        /// <summary>
        /// Opens the Storyboard Editor window.
        /// </summary>
        [MenuItem("Tools/UGESystem/Storyboard Editor")]
        public static void Open()
        {
            StoryboardEditorWindow window = GetWindow<StoryboardEditorWindow>("Storyboard Editor");
            window.minSize = new Vector2(800, 600);
        }

        /// <summary>
        /// Callback for when a Storyboard asset is opened from the Project window.
        /// It opens the editor window and loads the selected Storyboard.
        /// </summary>
        /// <param name="instanceID">The instance ID of the asset.</param>
        /// <param name="line">The line number (not used).</param>
        /// <returns><c>true</c> if the asset is a Storyboard and the window is opened; otherwise <c>false</c>.</returns>
        [OnOpenAsset]
        public static bool OnOpenStoryboard(int instanceID, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceID) as Storyboard;
            if (asset != null)
            {
                StoryboardEditorWindow window = GetWindow<StoryboardEditorWindow>("Storyboard Editor");
                window.SetStoryboard(asset);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Loads and displays the specified <see cref="Storyboard"/> asset in the editor window.
        /// </summary>
        /// <param name="storyboard">The <see cref="Storyboard"/> asset to load.</param>
        public void SetStoryboard(Storyboard storyboard)
        {
            if (storyboard == null) return;
            
            _currentStoryboard = storyboard;
            _storyboardObject = new SerializedObject(storyboard);
            if (_graphView != null)
            {
                _graphView.PopulateGraph(_currentStoryboard);
            }
            Repaint();
            SessionState.SetString(LAST_OPEN_STORYBOARD_PATH_KEY, AssetDatabase.GetAssetPath(storyboard));
            
            // If we are setting a storyboard while the game is running, immediately try to sync the status.
            if (Application.isPlaying)
            {
                SyncNodeStatusFromRunners();
            }
        }

        private void OnEnable()
        {
            _graphView = new StoryboardGraphView
            {
                name = "Storyboard Graph View",
                style = { flexGrow = 1 }
            };
            rootVisualElement.Add(_graphView);

            EditorApplication.update += CheckForChanges;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            UGEDelayedEventBus.Subscribe<NodeStartedEvent>(OnNodeStarted);
            UGEDelayedEventBus.Subscribe<NodeCompletedEvent>(OnNodeCompleted);

            string lastPath = SessionState.GetString(LAST_OPEN_STORYBOARD_PATH_KEY, "");
            if (!string.IsNullOrEmpty(lastPath))
            {
                var storedStoryboard = AssetDatabase.LoadAssetAtPath<Storyboard>(lastPath);
                if (storedStoryboard != null)
                {
                    SetStoryboard(storedStoryboard);
                }
            }
            else
            {
                var selection = Selection.activeObject;
                if (selection is Storyboard storyboard)
                {
                    SetStoryboard(storyboard);
                }
            }
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
            EditorApplication.update -= CheckForChanges;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            UGEDelayedEventBus.Unsubscribe<NodeStartedEvent>(OnNodeStarted);
            UGEDelayedEventBus.Unsubscribe<NodeCompletedEvent>(OnNodeCompleted);
        }

        private void OnFocus()
        {
            _isFocused = true;
            
            if (Application.isPlaying)
            {
                SyncNodeStatusFromRunners();
                return;
            }

            var selection = Selection.activeObject;
            if (selection is Storyboard storyboard)
            {
                SetStoryboard(storyboard);
            }
        }

        private void OnLostFocus()
        {
            _isFocused = false;
        }

        private void CheckForChanges()
        {
            if (Application.isPlaying) return;
            if (_isFocused) return;
            
            if (_storyboardObject != null && _storyboardObject.UpdateIfRequiredOrScript())
            {
                if (_currentStoryboard != null)
                {
                    _graphView.PopulateGraph(_currentStoryboard);
                }
            }
        }

        private void OnNodeStarted(NodeStartedEvent evt)
        {
            if (Application.isPlaying && _graphView != null && _currentStoryboard != null)
            {
                if (_graphView.CreatedNodes.TryGetValue(evt.NodeID, out var node))
                {
                    node.SetStatus(EventStatus.InProgress);
                }
            }
        }

        private void OnNodeCompleted(NodeCompletedEvent evt)
        {
            if (Application.isPlaying && _graphView != null && _currentStoryboard != null)
            {
                if (_graphView.CreatedNodes.TryGetValue(evt.NodeID, out var node))
                {
                    node.SetStatus(EventStatus.Completed);
                }
            }
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if(state == PlayModeStateChange.EnteredPlayMode)
            {
                SyncNodeStatusFromRunners();
            }

            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                if (_graphView != null && _graphView.CreatedNodes != null)
                {
                    foreach (var node in _graphView.CreatedNodes.Values)
                    {
                        node.SetStatus(EventStatus.NotStarted);
                    }
                }
                Repaint();
            }
        }
        
        private void SyncNodeStatusFromRunners()
        {
            if (!Application.isPlaying || UGESystemController.Instance == null || _graphView == null || _currentStoryboard == null) return;
            
            var runner = UGESystemController.Instance.GetRunnerForStoryboard(_currentStoryboard);
            if (runner == null)
            {
                 // If no specific runner is found, maybe reset all nodes to default
                if (_graphView.CreatedNodes != null)
                {
                    foreach (var node in _graphView.CreatedNodes.Values)
                    {
                        node.SetStatus(EventStatus.NotStarted);
                    }
                }
                return;
            }

            var statuses = runner.NodeStatuses;
            if (statuses == null) return;

            foreach(var statusEntry in statuses)
            {
                if (_graphView.CreatedNodes.TryGetValue(statusEntry.Key, out var node))
                {
                    node.SetStatus(statusEntry.Value);
                }
            }
        }

        private void OnSelectionChange()
        {
            if (Application.isPlaying) return;

            var selection = Selection.activeObject;
            if (selection is Storyboard storyboard)
            {
                SetStoryboard(storyboard);
            }
        }
    }
}
