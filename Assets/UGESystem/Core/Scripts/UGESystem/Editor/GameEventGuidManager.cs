using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UGESystem
{
    /// <summary>
    /// A static editor class that maintains a project-wide dictionary mapping unique GUIDs to their corresponding <see cref="GameEvent"/> assets for quick lookup.
    /// </summary>
    [InitializeOnLoad]
    public static class GameEventGuidManager
    {
        private static Dictionary<string, GameEvent> _guidToGameEventMap;
        private static bool _isInitialized = false;

        static GameEventGuidManager()
        {
            Initialize();
        }

        private static void Initialize()
        {
            if (_isInitialized) return;

            _guidToGameEventMap = new Dictionary<string, GameEvent>();
            Refresh();
            _isInitialized = true;
            
            // Re-initialize will be handled by GameEventAssetPostprocessor
        }

        /// <summary>
        /// Refreshes the internal lookup dictionary by finding all <see cref="GameEvent"/> assets in the project
        /// and mapping their GUIDs to the asset instances. This method should be called when assets are created, deleted, or moved.
        /// </summary>
        public static void Refresh()
        {
            _guidToGameEventMap.Clear();

            string[] guids = AssetDatabase.FindAssets("t:GameEvent"); // Find all GameEvent ScriptableObjects
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GameEvent gameEvent = AssetDatabase.LoadAssetAtPath<GameEvent>(assetPath);

                if (gameEvent != null && !string.IsNullOrEmpty(gameEvent.Guid))
                {
                    if (_guidToGameEventMap.ContainsKey(gameEvent.Guid))
                    {
#if UNITY_EDITOR
                        Debug.LogWarning($"[UGESystem] Duplicate GameEvent GUID found: {gameEvent.Guid} for asset '{gameEvent.name}' at '{assetPath}'. Skipping duplicate.");
#endif
                    }
                    else
                    {
                        _guidToGameEventMap.Add(gameEvent.Guid, gameEvent);
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves a <see cref="GameEvent"/> asset by its unique GUID from the cached lookup dictionary.
        /// </summary>
        /// <param name="guid">The unique identifier of the <see cref="GameEvent"/> to retrieve.</param>
        /// <returns>The <see cref="GameEvent"/> asset if found, otherwise <c>null</c>.</returns>
        public static GameEvent GetGameEvent(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            if (!_isInitialized) Initialize(); // Ensure initialized if accessed directly before editor finishes loading

            if (_guidToGameEventMap.TryGetValue(guid, out GameEvent gameEvent))
            {
                return gameEvent;
            }
#if UNITY_EDITOR
            Debug.LogWarning($"[UGESystem] GameEvent with GUID '{guid}' not found in the project.");
#endif
            return null;
        }
    }
}