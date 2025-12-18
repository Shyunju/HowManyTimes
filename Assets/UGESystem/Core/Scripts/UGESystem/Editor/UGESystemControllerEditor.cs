using UnityEditor;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// Creates a master inspector for <see cref="UGESystemController"/>,
    /// integrating the inspectors of all sub-manager components into a single, systematic interface.
    /// </summary>
    [CustomEditor(typeof(UGESystemController))]
    public class UGESystemControllerEditor : UnityEditor.Editor
    {
        private UGESystemController _targetController;

        // Editor instance for each manager component
        private UnityEditor.Editor _uiManagerEditor;
        private UnityEditor.Editor _characterManagerEditor;
        private UnityEditor.Editor _gameEventControllerEditor;
        private UnityEditor.Editor _cameraManagerEditor;
        private UnityEditor.Editor _delayedEventInvokerEditor;
        private UnityEditor.Editor _soundManagerEditor;

        // Foldout state for each manager
        private bool _showUIMgrSettings = false;
        private bool _showCharMgrSettings = false;
        private bool _showCamMgrSettings = false;
        private bool _showDelayedInvokerSettings = false;
        private bool _showSoundMgrSettings = false;

        private void OnEnable()
        {
            _targetController = (UGESystemController)target;

            // Find manager components in child objects and create editors.
            var uiManager = _targetController.GetComponentInChildren<UGEUIManager>(true);
            var charManager = _targetController.GetComponentInChildren<UGECharacterManager>(true);
            var eventController = _targetController.GetComponentInChildren<UGEGameEventController>(true);
            var camManager = _targetController.GetComponentInChildren<UGECameraManager>(true);
            var delayedInvoker = _targetController.GetComponentInChildren<UGEDelayedEventInvoker>(true);
            var soundManager = _targetController.GetComponentInChildren<UGESoundManager>(true);

            // Create an editor for each manager component.
            CreateCachedEditor(uiManager, null, ref _uiManagerEditor);
            CreateCachedEditor(charManager, null, ref _characterManagerEditor);
            CreateCachedEditor(eventController, null, ref _gameEventControllerEditor);
            CreateCachedEditor(camManager, null, ref _cameraManagerEditor);
            CreateCachedEditor(delayedInvoker, null, ref _delayedEventInvokerEditor);
            CreateCachedEditor(soundManager, null, ref _soundManagerEditor);
        }

        private void OnDisable()
        {
            // Remove unnecessary editor instances.
            if (_uiManagerEditor != null) DestroyImmediate(_uiManagerEditor);
            if (_characterManagerEditor != null) DestroyImmediate(_characterManagerEditor);
            if (_gameEventControllerEditor != null) DestroyImmediate(_gameEventControllerEditor);
            if (_cameraManagerEditor != null) DestroyImmediate(_cameraManagerEditor);
            if (_delayedEventInvokerEditor != null) DestroyImmediate(_delayedEventInvokerEditor);
            if (_soundManagerEditor != null) DestroyImmediate(_soundManagerEditor);
        }

        /// <summary>
        /// Draws the custom inspector GUI for the <see cref="UGESystemController"/>,
        /// displaying core settings and a categorized view of its sub-manager components,
        /// each with their own foldable editor.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Controller Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_isDontDestroy"));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("UGESystem Managers", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Draw each manager as a Foldout.
            DrawManagerFoldout("UI Manager Settings", ref _showUIMgrSettings, _uiManagerEditor);
            DrawManagerFoldout("Character Manager Settings", ref _showCharMgrSettings, _characterManagerEditor);
            // UGEGameEventController has no fields to expose in the inspector, so it is not drawn.
            DrawManagerFoldout("Camera Manager Settings", ref _showCamMgrSettings, _cameraManagerEditor);
            DrawManagerFoldout("Sound Manager Settings", ref _showSoundMgrSettings, _soundManagerEditor);
            DrawManagerFoldout("Delayed Event Invoker Settings", ref _showDelayedInvokerSettings, _delayedEventInvokerEditor);
            
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawManagerFoldout(string title, ref bool foldoutState, UnityEditor.Editor managerEditor)
        {
            foldoutState = EditorGUILayout.Foldout(foldoutState, title, true, EditorStyles.foldoutHeader);
            if (foldoutState)
            {
                EditorGUI.indentLevel++;
                DrawManagerProperties(managerEditor);
                EditorGUI.indentLevel--;
            }
        }

        private void DrawManagerProperties(UnityEditor.Editor managerEditor)
        {
            if (managerEditor == null) 
            {
                EditorGUILayout.HelpBox("Required Manager Component is missing or not found in children.", MessageType.Warning);
                return;
            }
            
            managerEditor.serializedObject.Update();
            
            SerializedProperty property = managerEditor.serializedObject.GetIterator();
            property.NextVisible(true); 
            
            while (property.NextVisible(false))
            {
                EditorGUILayout.PropertyField(property, true);
            }
            
            managerEditor.serializedObject.ApplyModifiedProperties();
        }
    }
}
