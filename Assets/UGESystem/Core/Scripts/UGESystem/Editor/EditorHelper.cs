using System;
using UnityEditor;
using UnityEngine;
using System.Linq; // Added for LastOrDefault

namespace UGESystem
{
    /// <summary>
    /// Provides static helper methods for building custom editor interfaces,
    /// such as rendering lists of various types managed by <see cref="SerializeReferenceAttribute"/>.
    /// </summary>
    public static class EditorHelper
    {
        /// <summary>
        /// Builds a polymorphic list view in the Unity editor for a given <see cref="SerializedProperty"/>.
        /// It allows adding new elements of types derived from a specified base type and handles the rendering and deletion of elements.
        /// </summary>
        /// <param name="serializedObject">The <see cref="SerializedObject"/> the property belongs to.</param>
        /// <param name="listProperty">The <see cref="SerializedProperty"/> representing the list to be displayed.</param>
        /// <param name="baseType">The base type for polymorphic elements in the list.</param>
        public static void BuildPolymorphicListView(SerializedObject serializedObject, SerializedProperty listProperty, Type baseType)
        {
            EditorGUILayout.LabelField(listProperty.displayName, EditorStyles.boldLabel);

            for (int i = 0; i < listProperty.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                var element = listProperty.GetArrayElementAtIndex(i);
                // Display type name using managedReferenceFullTypename
                EditorGUILayout.PropertyField(element, new GUIContent(element.managedReferenceFullTypename.Split('.').LastOrDefault()), true);

                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    // It is safe to use the index directly when deleting elements from an array.
                    listProperty.DeleteArrayElementAtIndex(i);
                    break; 
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button($"Add {baseType.Name}"))
            {
                var menu = new GenericMenu();
                var types = TypeCache.GetTypesDerivedFrom(baseType);
                
                // Capture the path so as not to use an old SerializedProperty in the lambda
                string propertyPath = listProperty.propertyPath;

                foreach (var type in types)
                {
                    if (type.IsAbstract) continue;
                    menu.AddItem(new GUIContent(type.Name), false, () =>
                    {
                        // Find the property again using the path at runtime.
                        var freshListProperty = serializedObject.FindProperty(propertyPath);
                        int newIndex = freshListProperty.arraySize;
                        freshListProperty.InsertArrayElementAtIndex(newIndex);
                        var newElement = freshListProperty.GetArrayElementAtIndex(newIndex);
                        newElement.managedReferenceValue = Activator.CreateInstance(type);
                        serializedObject.ApplyModifiedProperties();
                    });
                }
                menu.ShowAsContext();
            }
        }
    }
}