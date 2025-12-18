using UnityEditor;
using UnityEngine;
using System.IO;

namespace UGESystem
{
    /// <summary>
    /// Adds an <c>Assets/Create/UGESystem/Storyboard</c> menu item,
    /// enabling convenient creation of new <see cref="Storyboard"/> ScriptableObject assets in the project.
    /// </summary>
    public static class StoryboardMenuItems
    {
        /// <summary>
        /// Creates a new <see cref="Storyboard"/> ScriptableObject asset at a predefined path
        /// (<c>Assets/Resources/UGESystem/Storyboards/</c>) and selects it in the Unity editor.
        /// It ensures unique asset names and creates the necessary folder if it doesn't exist.
        /// </summary>
        [MenuItem("Assets/Create/UGESystem/Storyboard", false, 0)]
        public static void CreateStoryboardAsset()
        {
            // Create a new Storyboard asset instance.
            Storyboard newStoryboard = ScriptableObject.CreateInstance<Storyboard>();

            // Specify the path to save.
            string path = "Assets/Resources/UGESystem/Storyboards";

            // Check if the folder exists, and if not, create it.
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Create a unique path to avoid duplicate file names.
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/NewStoryboard.asset");

            // Create and save the asset as a file.
            AssetDatabase.CreateAsset(newStoryboard, assetPathAndName);
            AssetDatabase.SaveAssets();

            // Select the created asset and make it visible in the Project window.
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newStoryboard;
        }
    }
}