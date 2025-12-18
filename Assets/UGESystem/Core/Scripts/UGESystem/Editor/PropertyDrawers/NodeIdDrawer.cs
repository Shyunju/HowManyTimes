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
            // Apply only to fields of string type
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
#if UNITY_EDITOR
                Debug.LogWarning($"[NodeIdDrawer] {property.name} is not a string type.");
#endif
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
                return;
            }

            List<string> nodeNames = new List<string> { "(None)" }; // Index 0 is "(None)"
            List<string> nodeIDs = new List<string> { "" }; // Index 0 is an empty string

            foreach (var node in currentStoryboard.EventNodes)
            {
                nodeNames.Add(node.Name);
                nodeIDs.Add(node.NodeID);
            }

            // Find the index corresponding to the currently saved NodeID
            int currentIndex = nodeIDs.IndexOf(property.stringValue);
            if (currentIndex == -1)
            {
                currentIndex = 0; // If not found, "(None)"
            }

            // Draw popup field
            int selectedIndex = EditorGUI.Popup(position, label.text, currentIndex, nodeNames.ToArray());

            // When the selection changes
            if (selectedIndex != currentIndex)
            {
                property.stringValue = nodeIDs[selectedIndex]; // Save the selected NodeID
                property.serializedObject.ApplyModifiedProperties(); // Apply changes
            }
        }
    }
}