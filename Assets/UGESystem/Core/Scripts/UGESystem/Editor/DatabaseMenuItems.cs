using UnityEditor;
using UnityEngine;
using System.IO;

namespace UGESystem
{
    /// <summary>
    /// Adds an <c>Assets/Create/UGESystem/Character Database</c> menu item,
    /// allowing the creation or selection of a single <see cref="CharacterDatabase"/> asset for the project.
    /// </summary>
    public static class DatabaseMenuItems
    {
        private const string DATABASE_PATH = "Assets/Resources/UGESystem/CharacterData/CharacterDatabase.asset";

        /// <summary>
        /// Creates or selects the single <see cref="CharacterDatabase"/> asset for the project.
        /// If it doesn't exist, it will be created at a predefined path.
        /// If it exists, it will be selected in the Unity editor.
        /// </summary>
         [MenuItem("Assets/Create/UGESystem/Character Database", false, 0)]
        public static void CreateOrSelectDatabase()
        {
            // Check if the file already exists
            var existingDb = AssetDatabase.LoadAssetAtPath<CharacterDatabase>(DATABASE_PATH);
            if (existingDb != null)
            {
                Selection.activeObject = existingDb;
                return;
            }

            // Create folder
            string folderPath = Path.GetDirectoryName(DATABASE_PATH);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Create asset
            CharacterDatabase newDb = ScriptableObject.CreateInstance<CharacterDatabase>();
            AssetDatabase.CreateAsset(newDb, DATABASE_PATH);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newDb;
        }
    }
}