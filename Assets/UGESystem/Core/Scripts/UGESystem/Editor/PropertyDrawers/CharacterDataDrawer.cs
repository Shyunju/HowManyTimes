using UnityEditor;
using UnityEngine;

namespace UGESystem.Editor
{
    /// <summary>
    /// Provides a clean custom inspector layout for the <see cref="CharacterData"/> class,
    /// facilitating editing within lists in <see cref="CharacterDatabase"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(CharacterData))]
    public class CharacterDataDrawer : PropertyDrawer
    {
        // Define the approximate height of HelpBox as a constant (based on 2 lines)
        private const float HelpBoxHeight = 40f; 

        /// <summary>
        /// Draws the custom GUI for a <see cref="CharacterData"/> property,
        /// including validation for missing prefabs, read-only GUID display, and nested editing of expressions.
        /// </summary>
        /// <param name="position">The position and size of the property field.</param>
        /// <param name="property">The <see cref="SerializedProperty"/> to draw.</param>
        /// <param name="label">The label for the property.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            // Find properties used for the foldout label and initial validation
            var nameProp = property.FindPropertyRelative("<Name>k__BackingField");
            var prefabProp = property.FindPropertyRelative("<Prefab>k__BackingField");
            bool isPrefabMissing = prefabProp.objectReferenceValue == null;

            // Use Name for Foldout label
            GUIContent foldoutLabel = new GUIContent(string.IsNullOrEmpty(nameProp.stringValue) ? label.text : nameProp.stringValue);

            // 1. Draw Foldout
            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, foldoutLabel, true);
            
            // Draw inner UI only when Foldout is expanded.
            if (property.isExpanded)
            {
                // Start indent
                EditorGUI.indentLevel++;

                // Set the base Rect so that all UI elements start from below the Foldout.
                Rect contentRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);

                // Draw warning HelpBox
                if (isPrefabMissing)
                {
                    var helpBoxRect = new Rect(contentRect.x, contentRect.y, contentRect.width, HelpBoxHeight);
                    EditorGUI.HelpBox(helpBoxRect, "Character Prefab must be assigned!", MessageType.Error);
                    contentRect.y += HelpBoxHeight; // Move Y position for next UI
                }

                // Define properties to be used internally for drawing each property field.
                var idProp = property.FindPropertyRelative("<CharacterID>k__BackingField");
                var is3dProp = property.FindPropertyRelative("<Is3D>k__BackingField");
                var expressionsProp = property.FindPropertyRelative("<Expressions>k__BackingField");
                
                // CharacterID (read-only)
                GUI.enabled = false;
                EditorGUI.PropertyField(new Rect(contentRect.x, contentRect.y, contentRect.width, EditorGUI.GetPropertyHeight(idProp)), idProp, new GUIContent("Character ID (GUID)"));
                contentRect.y += EditorGUI.GetPropertyHeight(idProp);
                GUI.enabled = true;

                // Name
                EditorGUI.PropertyField(new Rect(contentRect.x, contentRect.y, contentRect.width, EditorGUI.GetPropertyHeight(nameProp)), nameProp);
                contentRect.y += EditorGUI.GetPropertyHeight(nameProp);

                // Is3D
                EditorGUI.PropertyField(new Rect(contentRect.x, contentRect.y, contentRect.width, EditorGUI.GetPropertyHeight(is3dProp)), is3dProp);
                contentRect.y += EditorGUI.GetPropertyHeight(is3dProp);

                // Prefab
                EditorGUI.PropertyField(new Rect(contentRect.x, contentRect.y, contentRect.width, EditorGUI.GetPropertyHeight(prefabProp)), prefabProp);
                contentRect.y += EditorGUI.GetPropertyHeight(prefabProp);

                // Expressions list
                EditorGUI.PropertyField(new Rect(contentRect.x, contentRect.y, contentRect.width, EditorGUI.GetPropertyHeight(expressionsProp)), expressionsProp, true);

                // End indent
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Calculates the total height required for drawing the <see cref="CharacterData"/> property in the inspector,
        /// accounting for expanded state and warning messages.
        /// </summary>
        /// <param name="property">The <see cref="SerializedProperty"/> to measure.</param>
        /// <param name="label">The label for the property.</param>
        /// <returns>The total height in pixels.</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totalHeight = EditorGUIUtility.singleLineHeight; // Foldout default height

            if (property.isExpanded)
            {
                var prefabProp = property.FindPropertyRelative("<Prefab>k__BackingField");
                if (prefabProp.objectReferenceValue == null)
                {
                    totalHeight += HelpBoxHeight; // Add warning box height
                }

                // Accurately calculate and add the height of all internal properties.
                totalHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("<CharacterID>k__BackingField"));
                totalHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("<Name>k__BackingField"));
                totalHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("<Is3D>k__BackingField"));
                totalHeight += EditorGUI.GetPropertyHeight(prefabProp);
                totalHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("<Expressions>k__BackingField"), true);
            }
            
            return totalHeight;
        }
    }
}