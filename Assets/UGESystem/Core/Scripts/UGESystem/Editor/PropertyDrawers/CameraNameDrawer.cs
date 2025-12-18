using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UGESystem.Editor
{
    /// <summary>
    /// A custom property drawer for the <see cref="CameraNameAttribute"/>,
    /// displaying a dropdown list of all Cinemachine camera names present in the current scene.
    /// </summary>
    [CustomPropertyDrawer(typeof(CameraNameAttribute))]
    public class CameraNameDrawer : CachedPopupDrawer
    {
        /// <summary>
        /// Overrides the base method to retrieve a list of unique Cinemachine camera names from the current scene.
        /// It handles cases where the editor is in Prefab Mode.
        /// </summary>
        /// <param name="property">The <see cref="SerializedProperty"/> that this drawer is for.</param>
        /// <returns>A list of available Cinemachine camera names in the scene, or a message indicating Prefab Mode.</returns>
        protected override List<string> GetOptions(SerializedProperty property)
        {
            // Stop processing if in Prefab Edit Mode, as scene search is meaningless.
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                return new List<string> { "Not available in Prefab Mode" };
            }

            var cameras = Object.FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
            
            return cameras
                .Where(cam => cam != null && !string.IsNullOrEmpty(cam.gameObject.name))
                .Select(cam => cam.gameObject.name)
                .Distinct()
                .ToList();
        }
    }
}