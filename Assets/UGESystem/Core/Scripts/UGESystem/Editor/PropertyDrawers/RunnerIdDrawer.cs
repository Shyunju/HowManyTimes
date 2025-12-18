using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace UGESystem.Editor
{
    /// <summary>
    /// A custom property drawer for the <see cref="RunnerIdAttribute"/>,
    /// displaying a dropdown list of all unique <c>RunnerId</c> strings from <see cref="UGEEventTaskRunner"/> components in the current scene.
    /// </summary>
    [CustomPropertyDrawer(typeof(RunnerIdAttribute))]
    public class RunnerIdDrawer : CachedPopupDrawer
    {
        /// <summary>
        /// Overrides the base method to retrieve a list of unique <c>RunnerId</c> values from all active <see cref="UGEEventTaskRunner"/> components in the current scene.
        /// It handles cases where the editor is in Prefab Mode.
        /// </summary>
        /// <param name="property">The <see cref="SerializedProperty"/> that this drawer is for.</param>
        /// <returns>A list of available <c>RunnerId</c> strings in the scene, or a message indicating Prefab Mode.</returns>
        protected override List<string> GetOptions(SerializedProperty property)
        {
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                return new List<string> { "Not available in Prefab Mode" };
            }

            var runnersInScene = Object.FindObjectsByType<UGEEventTaskRunner>(FindObjectsSortMode.None);
            
            return runnersInScene
                .Where(r => !string.IsNullOrEmpty(r.RunnerId))
                .Select(r => r.RunnerId)
                .Distinct()
                .ToList();
        }
    }
}