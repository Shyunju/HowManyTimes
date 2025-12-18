using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic; // Added for List

namespace UGESystem
{
    /// <summary>
    /// A custom property drawer for the <see cref="NodeNameAttribute"/>,
    /// displaying a dropdown of all node names from the currently active <see cref="Storyboard"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(NodeNameAttribute))]
    public class NodeNameDrawer : PropertyDrawer
    {
        /// <summary>
        /// Draws the custom GUI for a string property marked with <see cref="NodeNameAttribute"/>.
        /// It displays a dropdown of node names from the currently active <see cref="Storyboard"/>
        /// and updates the property with the selected node's name.
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
                Debug.LogWarning($"[NodeNameDrawer] {property.name} is not a string type.");
#endif
                return;
            }

            // Find the currently open storyboard editor window.
            var window = EditorWindow.GetWindow<StoryboardEditorWindow>(false, null, false);
            if (window == null)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            Storyboard storyboard = window.CurrentStoryboard;

            if (storyboard == null || storyboard.EventNodes.Count == 0)
            {
                // If no storyboard is selected or there are no nodes, display as a normal text field
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            // Get the list of node names.
            var nodeNames = storyboard.EventNodes.Select(n => n.Name).ToArray();
            string currentValue = property.stringValue;
            int selectedIndex = -1;

            for (int i = 0; i < nodeNames.Length; i++)
            {
                if (nodeNames[i] == currentValue)
                {
                    selectedIndex = i;
                    break;
                }
            }
            
            // If the current value is not in the list, add "None" to the beginning and select it as number 0
            if (selectedIndex == -1)
            {
                var namesWithNone = new string[nodeNames.Length + 1];
                namesWithNone[0] = "None";
                System.Array.Copy(nodeNames, 0, namesWithNone, 1, nodeNames.Length);
                nodeNames = namesWithNone;
                selectedIndex = 0;
            }

            // Draw a dropdown menu (Popup).
            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, nodeNames);
            if (EditorGUI.EndChangeCheck())
            {
                // Save the name selected by the user to the property.
                property.stringValue = (selectedIndex > 0 && selectedIndex < nodeNames.Length) ? nodeNames[selectedIndex] : string.Empty;
            }
        }
    }
}