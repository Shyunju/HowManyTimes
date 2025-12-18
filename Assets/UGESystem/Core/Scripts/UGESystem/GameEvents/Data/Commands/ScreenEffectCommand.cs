using Newtonsoft.Json;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// Defines the types of screen effects that can be applied.
    /// </summary>
    public enum ScreenEffectType
    {
        /// <summary>Screen fades in from a color.</summary>
        FadeIn,
        /// <summary>Screen fades out to a color.</summary>
        FadeOut,
        /// <summary>Screen flashes with a color.</summary>
        Flash,
        /// <summary>Screen is tinted with a persistent color.</summary>
        Tint
    }

    /// <summary>
    /// A data transfer object (DTO) for <see cref="ScreenEffectCommand"/>, used for JSON serialization and deserialization.
    /// </summary>
    public class ScreenEffectCommandDto : IEventCommandDto
    {
        /// <summary>
        /// Gets or sets the type of screen effect.
        /// </summary>
        [JsonProperty] public ScreenEffectType EffectType { get; set; }
        /// <summary>
        /// Gets or sets the duration of the effect in seconds.
        /// </summary>
        [JsonProperty] public float Duration { get; set; }
        /// <summary>
        /// Gets or sets the target color for the effect. Alpha is used for Tint and FadeIn.
        /// </summary>
        [JsonProperty] public Color TargetColor { get; set; }
        /// <summary>
        /// Gets or sets how long the flash stays at full color before fading out.
        /// </summary>
        [JsonProperty] public float FlashHoldDuration { get; set; }

        /// <summary>
        /// Converts this DTO into a <see cref="ScreenEffectCommand"/> instance.
        /// </summary>
        /// <returns>A new <see cref="ScreenEffectCommand"/> instance.</returns>
        public EventCommand ToCommand()
        {
            return new ScreenEffectCommand(this);
        }
    }

    /// <summary>
    /// A command for controlling full-screen visual effects such as fades, flashes, and tints,
    /// via <see cref="UGEScreenEffectManager"/>.
    /// </summary>
    [System.Serializable]
    [AvailableIn(GameEventType.Dialogue, GameEventType.CinematicText)]
    public class ScreenEffectCommand : EventCommand
    {
        [field: Header("Effect Settings")]
        [field: SerializeField]
        [JsonIgnore] /// <summary>Gets the type of screen effect to apply.</summary>
        public ScreenEffectType EffectType { get; private set; }

        [field: SerializeField]
        [field: Tooltip("Duration of the effect in seconds.")]
        [JsonIgnore] /// <summary>Gets the duration of the effect in seconds.</summary>
        public float Duration { get; private set; } = 1.0f;
        
        [field: SerializeField]
        [field: Tooltip("Target color for the effect. Alpha is used for Tint and FadeIn.")]
        [JsonIgnore] /// <summary>Gets the target color for the effect. Alpha is used for Tint and FadeIn.</summary>
        public Color TargetColor { get; private set; } = Color.black;

        [Header("Flash Specific Settings")]
        [SerializeField]
        [Tooltip("How long the flash stays at full color before fading out.")]
        private float _flashHoldDuration = 0.1f;
        /// <summary>
        /// Gets how long the flash stays at full color before fading out.
        /// </summary>
        [JsonIgnore] public float FlashHoldDuration => _flashHoldDuration;


        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenEffectCommand"/> class.
        /// </summary>
        public ScreenEffectCommand()
        {
            CommandType = CommandType.ScreenEffect;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenEffectCommand"/> class from a DTO.
        /// </summary>
        /// <param name="dto">The <see cref="ScreenEffectCommandDto"/> containing the initial data.</param>
        public ScreenEffectCommand(ScreenEffectCommandDto dto)
        {
            CommandType = CommandType.ScreenEffect;
            EffectType = dto.EffectType;
            Duration = dto.Duration;
            TargetColor = dto.TargetColor;
            _flashHoldDuration = dto.FlashHoldDuration;
        }

        /// <summary>
        /// Converts this <see cref="ScreenEffectCommand"/> instance into a <see cref="ScreenEffectCommandDto"/> for serialization.
        /// </summary>
        /// <returns>A new <see cref="ScreenEffectCommandDto"/> instance.</returns>
        public override IEventCommandDto ToDto()
        {
            return new ScreenEffectCommandDto
            {
                EffectType = EffectType,
                Duration = Duration,
                TargetColor = TargetColor,
                FlashHoldDuration = _flashHoldDuration
            };
        }
    }
}