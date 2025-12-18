using UnityEditor;
using UnityEngine;
using System.IO;

namespace UGESystem
{
    /// <summary>
    /// Adds an <c>Assets/Create/UGESystem/New Game Event</c> menu item,
    /// enabling convenient creation of new <see cref="GameEvent"/> ScriptableObject assets in the project.
    /// </summary>
    public static class GameEventCreation
    {
        /// <summary>
        /// Creates a new <see cref="GameEvent"/> ScriptableObject asset at a predefined path
        /// (<c>Assets/Resources/UGESystem/EventSO/</c>) and selects it in the Unity editor.
        /// It ensures unique asset names and creates the necessary folder if it doesn't exist.
        /// </summary>
        [MenuItem("Assets/Create/UGESystem/New Game Event", false, 10)]
        public static void CreateGameEventInResources()
        {
            // Create a new GameEvent asset instance.
            GameEvent newEvent = ScriptableObject.CreateInstance<GameEvent>();

            // Specify the path to save.
            string path = "Assets/Resources/UGESystem/EventSO";

            // Check if the folder exists, and if not, create it.
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Create a unique path to avoid duplicate file names.
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/NewGameEvent.asset");

            // Create and save the asset as a file.
            AssetDatabase.CreateAsset(newEvent, assetPathAndName);

            // Select the created asset and make it visible in the Project window.
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newEvent;
        }
    }
}