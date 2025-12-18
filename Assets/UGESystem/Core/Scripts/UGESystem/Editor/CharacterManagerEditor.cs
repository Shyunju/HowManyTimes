using UnityEditor;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// Provides a custom inspector for the <see cref="UGECharacterManager"/> component, allowing for character database assignment and 2D/3D character display slot configuration.
    /// </summary>
    [CustomEditor(typeof(UGECharacterManager))]
    public class CharacterManagerEditor : UnityEditor.Editor
    {
        private SerializedProperty _databaseProperty;
        private SerializedProperty _character2DSlotsProperty;
        private SerializedProperty _character3DSlotsProperty;
        private SerializedProperty _isDontDestroyProperty;

        private void OnEnable()
        {
            _databaseProperty = serializedObject.FindProperty("_characterDatabase");
            _character2DSlotsProperty = serializedObject.FindProperty("_character2DSlots");
            _character3DSlotsProperty = serializedObject.FindProperty("_character3DSlots");
            
            _isDontDestroyProperty = serializedObject.FindProperty("_isDontDestroy");
        }

        /// <summary>
        /// Draws the custom inspector GUI for the <see cref="UGECharacterManager"/> component,
        /// allowing character database assignment and display slot configuration.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (_isDontDestroyProperty != null)
            {
                EditorGUILayout.PropertyField(_isDontDestroyProperty, new GUIContent("Don't Destroy On Load"));
            }
            
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_databaseProperty);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("2D Character UI Slots", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_character2DSlotsProperty);
            
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("3D Character UI Slots", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_character3DSlotsProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}