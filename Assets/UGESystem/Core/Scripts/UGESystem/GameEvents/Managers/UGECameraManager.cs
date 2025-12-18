using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Cinemachine;

namespace UGESystem
{
        /// <summary>
        /// Manager component that controls all Cinemachine-based camera operations during events,
        /// such as camera switching, zooming, and shaking, according to commands.
        /// <br/>
        /// 커맨드에 따라 카메라 전환, 줌, 흔들기 등 이벤트 중 모든 시네머신 기반 카메라 작업을 제어하는 매니저 컴포넌트입니다.
        /// </summary>
        [RequireComponent(typeof(CinemachineImpulseSource))]
        public class UGECameraManager : MonoBehaviour
        {
            [Tooltip("Assign the CinemachineBrain component in the scene. If not found, it will be searched automatically.")]
            [SerializeField] private CinemachineBrain _brain;
    
            private ICinemachineCamera _defaultLiveCamera;
            private List<CinemachineCamera> _eventControlledCameras = new List<CinemachineCamera>();
            private CinemachineImpulseSource _impulseSource;
    
            // 씬에 있는 카메라들을 이름으로 캐싱하여 빠르게 찾기 위함
            // To quickly find cameras in the scene by caching them by name
            private Dictionary<string, CinemachineCamera> _sceneCameraCache = new Dictionary<string, CinemachineCamera>();
    
            private const int EVENT_CAM_PRIORITY_HIGH = 20;
            private const int DEFAULT_GAMEPLAY_CAM_PRIORITY = 10;
            private const int EVENT_CAM_PRIORITY_LOW = 0;
    
            private void Awake()
            {
                _impulseSource = GetComponent<CinemachineImpulseSource>();
    
                if (_brain == null)
                {
                    _brain = FindFirstObjectByType<CinemachineBrain>();
                }
                
                // 씬이 로드될 때 모든 가상 카메라를 찾아 캐시에 저장
                // Cache all virtual cameras in the scene when loaded
                CacheSceneCameras();
            }
    
            private void CacheSceneCameras()
            {
                _sceneCameraCache.Clear();
                var allCameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
                foreach (var cam in allCameras)
                {
                    if (!_sceneCameraCache.ContainsKey(cam.gameObject.name))
                    {
                        _sceneCameraCache.Add(cam.gameObject.name, cam);
                    }
                    else
                    {
#if UNITY_EDITOR
                        Debug.LogWarning($"[UGECameraManager] Duplicate camera name '{cam.gameObject.name}' found. Camera commands may target the wrong camera.");
#endif
                    }
                }
            }
            
            private CinemachineCamera FindCameraByName(string cameraName)
            {
                if (string.IsNullOrEmpty(cameraName)) return null;
    
                if (_sceneCameraCache.TryGetValue(cameraName, out var cam))
                {
                    return cam;
                }
#if UNITY_EDITOR
                Debug.LogWarning($"[UGECameraManager] Camera with name '{cameraName}' not found in cache.");
#endif
                return null;
            }
    
            /// <summary>
            /// Switches the active camera to the one specified by <paramref name="cameraName"/>.
            /// 지정된 <paramref name="cameraName"/>에 해당하는 카메라로 활성 카메라를 전환합니다.
            /// </summary>
            /// <param name="cameraName">The name of the target camera.</param>
            /// <param name="blendDuration">The duration of the blend to the new camera.</param>
            public IEnumerator SwitchTo(string cameraName, float blendDuration)
            {
                CinemachineCamera targetCam = FindCameraByName(cameraName);
    
                if (_brain == null || targetCam == null) yield break;
    
                if (_defaultLiveCamera == null)
                {
                    _defaultLiveCamera = _brain.ActiveVirtualCamera;
                    if (_defaultLiveCamera is CinemachineCamera defaultVCam)
                    {
                        defaultVCam.Priority = DEFAULT_GAMEPLAY_CAM_PRIORITY;
                    }
                }
    
                if (!_eventControlledCameras.Contains(targetCam))
                {
                    _eventControlledCameras.Add(targetCam);
                }
    
                foreach (var cam in _eventControlledCameras)
                {
                    if (cam != null && cam != targetCam)
                    {
                        cam.Priority = EVENT_CAM_PRIORITY_LOW;
                    }
                }
                
                targetCam.Priority = EVENT_CAM_PRIORITY_HIGH;
    
                if (blendDuration > 0)
                {
                    yield return new WaitForSeconds(blendDuration);
                }
            }
    
            /// <summary>
            /// Zooms the specified camera's field of view to a new value over a duration.
            /// 지정된 카메라의 시야(FOV)를 일정 시간 동안 새로운 값으로 줌인/아웃합니다.
            /// </summary>
            /// <param name="cameraName">The name of the camera to zoom. If empty, zooms the currently active camera.</param>
            /// <param name="fov">The target field of view.</param>
            /// <param name="duration">The duration of the zoom in seconds.</param>
            public IEnumerator Zoom(string cameraName, float fov, float duration)
            {
                CinemachineCamera camToZoom = FindCameraByName(cameraName);
                if (camToZoom == null)
                {
                    if (_brain.ActiveVirtualCamera is CinemachineCamera vcam)
                    {
                        camToZoom = vcam;
                    }
                    if (camToZoom == null) yield break;
                }
    
                float startFOV = camToZoom.Lens.FieldOfView;
                float elapsedTime = 0f;
    
                while (elapsedTime < duration)
                {
                    camToZoom.Lens.FieldOfView = Mathf.Lerp(startFOV, fov, elapsedTime / duration);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                camToZoom.Lens.FieldOfView = fov;
            }
    
            /// <summary>
            /// Shakes the active camera with a given intensity using Cinemachine Impulse Source.
            /// Cinemachine Impulse Source를 사용하여 지정된 강도로 활성 카메라를 흔듭니다.
            /// </summary>
            /// <param name="intensity">The intensity of the camera shake.</param>
            public void Shake(float intensity)
            {
                if (_impulseSource == null) return;
                // GenerateImpulse()는 Impulse Source에 미리 정의된 Signal을 사용합니다.
                // GenerateImpulse() uses a pre-defined Signal in the Impulse Source.
                // 강도를 런타임에 동적으로 적용하려면 GenerateImpulse(force)를 사용합니다.
                // To dynamically apply intensity at runtime, use GenerateImpulse(force).
                _impulseSource.GenerateImpulse(Vector3.one * intensity);
            }
    
            /// <summary>
            /// Resets the camera system to the default gameplay camera, clearing any event-controlled cameras.
            /// 카메라 시스템을 기본 게임플레이 카메라로 재설정하고, 이벤트에 의해 제어되던 카메라를 모두 지웁니다.
            /// </summary>
            public void ResetCamera()
            {
                if (_brain == null) return;
    
                foreach (var cam in _eventControlledCameras)
                {
                    if (cam != null)
                    {
                        cam.Priority = EVENT_CAM_PRIORITY_LOW;
                    }
                }
                _eventControlledCameras.Clear();
                
                if (_defaultLiveCamera != null && _defaultLiveCamera is CinemachineCamera defaultVCam)
                {
                    defaultVCam.Priority = DEFAULT_GAMEPLAY_CAM_PRIORITY;
                }
                _defaultLiveCamera = null;
            }
        }
    }