using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace UGESystem.Editor
{
    /// <summary>
    /// Creates a custom inspector for <see cref="CharacterDatabase"/>, ensuring unique GUIDs for new character entries and providing JSON import/export functionality.
    /// </summary>
    [CustomEditor(typeof(CharacterDatabase))]
    public class CharacterDatabaseEditor : UnityEditor.Editor
    {
        /// <summary>
        /// Draws the custom inspector GUI for the <see cref="CharacterDatabase"/> asset.
        /// It includes functionality for JSON export/import and ensures unique GUIDs for character entries.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // When adding a new character, this resolves GUID duplication and initializes it with default values.
            ValidateAndInitializeCharacters();

            var database = (CharacterDatabase)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("JSON Management", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Export to JSON"))
            {
                string path = EditorUtility.SaveFilePanel("Save Character Database", "", "character_database.json", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    string json = database.ToJson();
                    File.WriteAllText(path, json);
                    EditorUtility.DisplayDialog("Export Successful", $"Database saved to:\n{path}", "OK");
                }
            }

            if (GUILayout.Button("Import from JSON"))
            {
                if (EditorUtility.DisplayDialog("Import Warning",
                    "This will overwrite existing character data (if IDs match) and add new characters from the JSON file. This action cannot be undone.\n\nAre you sure you want to continue?",
                    "Import", "Cancel"))
                {
                    string path = EditorUtility.OpenFilePanel("Load Character Database", "", "json");
                    if (!string.IsNullOrEmpty(path))
                    {
                        Undo.RecordObject(database, "Import Character Database from JSON");
                        string json = File.ReadAllText(path);
                        database.FromJson(json);
                        EditorUtility.SetDirty(database);
                        EditorUtility.DisplayDialog("Import Successful", $"Database loaded from:\n{path}", "OK");
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Character List", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_characters"), true);

            serializedObject.ApplyModifiedProperties();
        }

        private void ValidateAndInitializeCharacters()
        {
            var charactersProp = serializedObject.FindProperty("_characters");
            var seenGuids = new HashSet<string>();

            for (int i = 0; i < charactersProp.arraySize; i++)
            {
                var elementProp = charactersProp.GetArrayElementAtIndex(i);
                var idProp = elementProp.FindPropertyRelative("<CharacterID>k__BackingField");

                // If the ID is empty, or if it's a duplicate (which happens when Unity duplicates an element on pressing '+')
                if (string.IsNullOrEmpty(idProp.stringValue) || !seenGuids.Add(idProp.stringValue))
                {
                    // This is a newly added (or duplicated) element. Reset it to a clean default state.
                    idProp.stringValue = System.Guid.NewGuid().ToString();

                    // Reset other fields to default
                    elementProp.FindPropertyRelative("<Name>k__BackingField").stringValue = "";
                    elementProp.FindPropertyRelative("<Is3D>k__BackingField").boolValue = false;
                    
                    // Reset Expressions list to have one "Default" entry
                    var expressionsProp = elementProp.FindPropertyRelative("<Expressions>k__BackingField");
                    expressionsProp.ClearArray();
                    expressionsProp.InsertArrayElementAtIndex(0);
                    var newExpressionProp = expressionsProp.GetArrayElementAtIndex(0);
                    
                    newExpressionProp.FindPropertyRelative("<ExpressionName>k__BackingField").stringValue = "Default";
                    newExpressionProp.FindPropertyRelative("<AnimationStateName>k__BackingField").stringValue = "Default";
                }
            }
        }
    }
}