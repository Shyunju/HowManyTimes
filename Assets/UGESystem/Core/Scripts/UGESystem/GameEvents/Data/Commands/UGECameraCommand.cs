using Newtonsoft.Json;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A data transfer object (DTO) for <see cref="UGECameraCommand"/>, used for JSON serialization and deserialization.
    /// </summary>
    public class UGECameraCommandDto : IEventCommandDto
    {
        /// <summary>
        /// Gets or sets the type of camera action to perform.
        /// </summary>
        [JsonProperty] public UGECameraActionType ActionType { get; set; }
        // [JsonProperty] public string TargetCameraName { get; set; } // REMOVED
        /// <summary>
        /// Gets or sets the duration of the camera action in seconds.
        /// </summary>
        [JsonProperty] public float Duration { get; set; }
        /// <summary>
        /// Gets or sets the target field of view for a zoom action.
        /// </summary>
        [JsonProperty] public float TargetFOV { get; set; }
        /// <summary>
        /// Gets or sets the intensity of camera shake.
        /// </summary>
        [JsonProperty] public float ShakeIntensity { get; set; }

        /// <summary>
        /// Converts this DTO into a <see cref="UGECameraCommand"/> instance.
        /// </summary>
        /// <returns>A new <see cref="UGECameraCommand"/> instance.</returns>
        public EventCommand ToCommand()
        {
            return new UGECameraCommand(this);
        }
    }

    /// <summary>
    /// A command for controlling Cinemachine cameras,
    /// allowing actions such as switching between cameras, zooming, and screen shaking.
    /// </summary>
    [System.Serializable]
    [AvailableIn(GameEventType.Dialogue, GameEventType.CinematicText)]
    public class UGECameraCommand : EventCommand
    {
        [field: SerializeField]
        [JsonIgnore] /// <summary>Gets the type of camera action to perform.</summary>
        public UGECameraActionType ActionType { get; private set; }

        [Header("Target & Duration")]
        [Tooltip("Game object name of the target virtual camera (SwitchTo, Zoom)")]
        [SerializeField, CameraName]
        private string _targetCameraName;
        /// <summary>
        /// Gets the game object name of the target virtual camera for SwitchTo or Zoom actions.
        /// </summary>
        [JsonIgnore] public string TargetCameraName => _targetCameraName;

        [Tooltip("Time taken for camera switch/zoom animation")]
        [SerializeField]
        private float _duration = 1.0f;
        /// <summary>
        /// Gets the time taken for camera switch/zoom animation.
        /// </summary>
        [JsonIgnore] public float Duration => _duration;

        [Header("Zoom Settings")]
        [Tooltip("Target FOV value for zoom action")]
        [SerializeField]
        private float _targetFOV = 40.0f;
        /// <summary>
        /// Gets the target Field of View value for a zoom action.
        /// </summary>
        [JsonIgnore] public float TargetFOV => _targetFOV;

        [Header("Shake Settings")]
        [Tooltip("Shake intensity for shake action")]
        [SerializeField]
        private float _shakeIntensity = 1.0f;
        /// <summary>
        /// Gets the shake intensity for a shake action.
        /// </summary>
        [JsonIgnore] public float ShakeIntensity => _shakeIntensity;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UGECameraCommand"/> class.
        /// </summary>
        public UGECameraCommand()
        {
            CommandType = CommandType.Camera;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UGECameraCommand"/> class from a DTO.
        /// </summary>
        /// <param name="dto">The <see cref="UGECameraCommandDto"/> containing the initial data.</param>
        public UGECameraCommand(UGECameraCommandDto dto)
        {
            CommandType = CommandType.Camera;
            ActionType = dto.ActionType;
            // _targetCameraName = dto.TargetCameraName; // REMOVED
            _duration = dto.Duration;
            _targetFOV = dto.TargetFOV;
            _shakeIntensity = dto.ShakeIntensity;
        }

        /// <summary>
        /// Converts this <see cref="UGECameraCommand"/> instance into a <see cref="UGECameraCommandDto"/> for serialization.
        /// </summary>
        /// <returns>A new <see cref="UGECameraCommandDto"/> instance.</returns>
        public override IEventCommandDto ToDto()
        {
            return new UGECameraCommandDto
            {
                ActionType = ActionType,
                // TargetCameraName = _targetCameraName, // REMOVED
                Duration = _duration,
                TargetFOV = _targetFOV,
                ShakeIntensity = _shakeIntensity
            };
        }
    }
}
