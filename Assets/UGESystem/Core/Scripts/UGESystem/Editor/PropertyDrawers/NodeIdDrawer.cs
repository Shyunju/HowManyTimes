using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace UGESystem
{
    /// <summary>
    /// A custom property drawer for the <see cref="NodeIdAttribute"/>,
    /// displaying a dropdown of all node names from the currently active <see cref="Storyboard"/>
    /// and storing the unique ID of the selected node.
    /// </summary>
    [CustomPropertyDrawer(typeof(NodeIdAttribute))]
    public class NodeIdDrawer : PropertyDrawer
    {
        /// <summary>
        /// Draws the custom GUI for a string property marked with <see cref="NodeIdAttribute"/>.
        /// It displays a dropdown of node names from the currently active <see cref="Storyboard"/>
        /// and updates the property with the selected node's unique ID.
        /// </summary>
        /// <param name="position">The position and size of the property field.</param>
        /// <param name="property">The <see cref="SerializedProperty"/> to draw.</param>
        /// <param name="label">The label for the property.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 1. BeginProperty establishes the property context for focus and overrides.
            label = EditorGUI.BeginProperty(position, label, property);

            // Apply only to fields of string type
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
#if UNITY_EDITOR
                Debug.LogWarning($"[NodeIdDrawer] {property.name} is not a string type.");
#endif
                EditorGUI.EndProperty();
                return;
            }

            // Find the Storyboard currently being edited in the StoryboardEditorWindow
            Storyboard currentStoryboard = null;
            if (EditorWindow.HasOpenInstances<StoryboardEditorWindow>())
            {
                var window = EditorWindow.GetWindow<StoryboardEditorWindow>();
                currentStoryboard = window.CurrentStoryboard;
            }

            if (currentStoryboard == null)
            {
                EditorGUI.PropertyField(position, property, label); // If there is no Storyboard, draw the default field
                EditorGUI.EndProperty();
                return;
            }

            string currentID = property.stringValue;
            string displayLabel = "(None)";
            bool isInvalid = false;

            // Find name for current ID
            var node = currentStoryboard.EventNodes.Find(n => n.NodeID == currentID);
            if (node != null)
            {
                displayLabel = node.Name;
            }
            else if (!string.IsNullOrEmpty(currentID))
            {
                isInvalid = true;
                displayLabel = $"(Invalid) {currentID}";
            }

            // 2. Draw Label
            // PrefixLabel without manual ID is more stable on macOS
            position = EditorGUI.PrefixLabel(position, label);

            // 3. Prepare Button Style (Tint if invalid)
            Color originalColor = GUI.backgroundColor;
            if (isInvalid)
            {
                GUI.backgroundColor = new Color(1f, 0.6f, 0.6f, 1f);
            }

            // 4. Draw Button
            if (EditorGUI.DropdownButton(position, new GUIContent(displayLabel), FocusType.Keyboard))
            {
                GenericMenu menu = new GenericMenu();

                // Add "None" option
                menu.AddItem(new GUIContent("(None)"), string.IsNullOrEmpty(currentID), () =>
                {
                    property.stringValue = "";
                    property.serializedObject.ApplyModifiedProperties();
                });

                foreach (var sbNode in currentStoryboard.EventNodes)
                {
                    bool isSelected = sbNode.NodeID == currentID;
                    menu.AddItem(new GUIContent(sbNode.Name), isSelected, () =>
                    {
                        property.stringValue = sbNode.NodeID;
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }

                menu.DropDown(position);
            }

            // Restore color
            if (isInvalid)
            {
                GUI.backgroundColor = originalColor;
            }

            EditorGUI.EndProperty();
        }
    }
}