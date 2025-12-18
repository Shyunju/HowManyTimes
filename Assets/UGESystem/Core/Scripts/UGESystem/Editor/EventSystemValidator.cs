using UnityEditor;
using UnityEngine;
using System.Linq;

namespace UGESystem
{
    /// <summary>
    /// Provides an editor tool that finds and automatically fixes <c>null</c> command references within all <see cref="GameEvent"/> assets in the project.
    /// </summary>
    public static class EventSystemValidator
    {
        /// <summary>
        /// Finds all <see cref="GameEvent"/> assets in the project, checks for <c>null</c> command references within them,
        /// and offers to automatically remove them. This helps prevent runtime errors caused by corrupted data.
        /// </summary>
        [MenuItem("Tools/UGESystem/Validation/Find and Fix Null Commands")]
        public static void FindAndFixNullCommands()
        {
            string[] guids = AssetDatabase.FindAssets("t:GameEvent");
            int nullCount = 0;
            int fixedCount = 0;
            bool needsFixing = false;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameEvent gameEvent = AssetDatabase.LoadAssetAtPath<GameEvent>(path);

                if (gameEvent == null) continue;

                // Check if the list contains null
                if (gameEvent.Commands.Any(command => command == null))
                {
                    needsFixing = true;
                    int initialCount = gameEvent.Commands.Count;
                    int nullsInThisAsset = gameEvent.Commands.Count(command => command == null);
                    nullCount += nullsInThisAsset;
#if UNITY_EDITOR
                    Debug.LogError($"[Problem Found] GameEvent '{gameEvent.name}' at path '{path}' contains {nullsInThisAsset} null command(s).", gameEvent);
#endif
                }
            }

            if (needsFixing)
            {
                if (EditorUtility.DisplayDialog("Null Command Found",
                    $"Found a total of {nullCount} null command(s) in your GameEvent assets. This is likely causing runtime errors.\n\nWould you like to automatically remove all of them?",
                    "Yes, Fix it!", "No, I'll do it manually."))
                {
                    // Run auto-fix
                    foreach (string guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        GameEvent gameEvent = AssetDatabase.LoadAssetAtPath<GameEvent>(path);
                        if (gameEvent != null && gameEvent.Commands.Any(command => command == null))
                        {
                            int removedCount = gameEvent.Commands.RemoveAll(command => command == null);
                            if (removedCount > 0)
                            {
                                fixedCount++;
                                EditorUtility.SetDirty(gameEvent);
                            }
                        }
                    }
                    AssetDatabase.SaveAssets();
                    EditorUtility.DisplayDialog("Fix Complete", "Successfully fixed {fixedCount} GameEvent asset(s). Please try running the game again.", "OK");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Validation Complete", "No null commands found in any GameEvent assets. Your data looks clean!", "OK");
            }
        }
    }
}