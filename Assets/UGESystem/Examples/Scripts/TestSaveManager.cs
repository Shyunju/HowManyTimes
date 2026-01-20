using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace UGESystem.Examples
{
    /// <summary>
    /// A simple test script to demonstrate and verify the Save/Load API of UGESystem.
    /// Access these functions via the component context menu (right-click in Inspector).
    /// </summary>
    public class TestSaveManager : MonoBehaviour
    {
        private const string SAVE_KEY = "UGESystem_TestSaveData";

        [Header("Test Controls")]
        [Tooltip("If true, automatically loads the saved state on Start.")]
        public bool loadOnStart = false;

        private void Start()
        {
            if (loadOnStart)
            {
                LoadSavedState();
            }
        }

        /// <summary>
        /// Captures the current UGESystem state and saves it to PlayerPrefs.
        /// </summary>
        [ContextMenu("Save Current State")]
        public void SaveCurrentState()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[TestSaveManager] 'Save Current State' is only available in Play Mode.");
                return;
            }

            if (UGESystemController.Instance == null) return;

            var states = UGESystemController.Instance.CaptureAllStoryboardsState();
            string json = JsonConvert.SerializeObject(states, Formatting.Indented);
            
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();

            Debug.Log($"<color=green>[TestSaveManager] State Saved!</color>\n{json}");
        }

        /// <summary>
        /// Loads the saved state from PlayerPrefs and restores it into UGESystem.
        /// </summary>
        [ContextMenu("Load Saved State")]
        public void LoadSavedState()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[TestSaveManager] 'Load Saved State' is only available in Play Mode.");
                return;
            }

            if (!PlayerPrefs.HasKey(SAVE_KEY))
            {
                Debug.LogWarning("[TestSaveManager] No save data found to load.");
                return;
            }

            string json = PlayerPrefs.GetString(SAVE_KEY);
            var states = JsonConvert.DeserializeObject<List<RunnerStateDto>>(json);

            if (UGESystemController.Instance != null)
            {
                UGESystemController.Instance.RestoreAllStoryboardsState(states);
                Debug.Log($"<color=cyan>[TestSaveManager] State Restored!</color>");
            }
        }

        /// <summary>
        /// Clears the saved data in PlayerPrefs.
        /// </summary>
        [ContextMenu("Clear Save Data")]
        public void ClearSaveData()
        {
            // Clear can be done in Edit Mode, but log might be useful.
            PlayerPrefs.DeleteKey(SAVE_KEY);
            Debug.Log("[TestSaveManager] Save data cleared.");
        }

        /// <summary>
        /// Manually triggers a re-initialization of all runners to simulate a fresh scene load.
        /// </summary>
        [ContextMenu("Reset System (Simulation)")]
        public void ResetSystem()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[TestSaveManager] 'Reset System' is only available in Play Mode.");
                return;
            }

            var runners = FindObjectsByType<UGEEventTaskRunner>(FindObjectsSortMode.None);
            foreach (var runner in runners)
            {
                runner.InitializeStoryboard();
            }
            Debug.Log("[TestSaveManager] All runners re-initialized to 'NotStarted'.");
        }
    }
}