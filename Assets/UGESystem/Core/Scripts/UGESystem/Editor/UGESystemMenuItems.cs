using UnityEditor;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// Adds a <c>Tools/UGESystem/Create System Object</c> menu item,
    /// automatically creating a <c>UGESystem</c> GameObject hierarchy in the scene with all necessary manager components.
    /// </summary>
    public static class UGESystemMenuItems
    {
        /// <summary>
        /// Creates a <c>UGESystem</c> GameObject in the current scene and populates it with all necessary manager components,
        /// including <see cref="UGESystemController"/>, <see cref="UGEUIManager"/>, <see cref="UGECharacterManager"/>,
        /// <see cref="UGEGameEventController"/>, <see cref="UGECameraManager"/>, <see cref="UGEDelayedEventInvoker"/>,
        /// <see cref="UGEInputManager"/>, and <see cref="UGESoundManager"/>.
        /// It also sets up AudioSources for BGM and SFX within the <see cref="UGESoundManager"/>.
        /// </summary>
        [MenuItem("Tools/UGESystem/Create System Object")]
        private static void CreateSystemObject()
        {
            // Create root object and add controller
            GameObject root = new GameObject("UGESystem");
            root.AddComponent<UGESystemController>();

            // Create child objects and components for each manager
            CreateManagerObject<UGEUIManager>("UGEUIManager", root.transform);
            CreateManagerObject<UGECharacterManager>("UGECharacterManager", root.transform);
            CreateManagerObject<UGEGameEventController>("UGEGameEventController", root.transform);
            CreateManagerObject<UGECameraManager>("UGECameraManager", root.transform);
            CreateManagerObject<UGEDelayedEventInvoker>("UGEDelayedEventInvoker", root.transform);
            CreateManagerObject<UGEInputManager>("UGEInputManager", root.transform);

            // Create UGESoundManager and configure AudioSources
            var soundManagerGo = new GameObject("UGESoundManager");
            soundManagerGo.transform.SetParent(root.transform);
            soundManagerGo.AddComponent<UGESoundManager>();
            
            // Add and configure AudioSource for BGM
            var bgmSource = soundManagerGo.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;

            // Add and configure AudioSource for SFX
            var sfxSource = soundManagerGo.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            
            // Select the created root object
            Selection.activeGameObject = root;
        }

        private static void CreateManagerObject<T>(string name, Transform parent) where T : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.AddComponent<T>();
        }
    }
}