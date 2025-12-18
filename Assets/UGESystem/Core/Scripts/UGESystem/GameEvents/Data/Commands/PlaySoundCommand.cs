using Newtonsoft.Json;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A data transfer object (DTO) for <see cref="PlaySoundCommand"/>, used for JSON serialization and deserialization.
    /// </summary>
    public class PlaySoundCommandDto : IEventCommandDto
    {
        /// <summary>
        /// Gets or sets the action to perform (Play/Stop).
        /// </summary>
        [JsonProperty] public SoundAction Action { get; set; }
        /// <summary>
        /// Gets or sets the type of sound (BGM/SFX).
        /// </summary>
        [JsonProperty] public SoundType SoundType { get; set; }
        /// <summary>
        /// Gets or sets the volume of the sound.
        /// </summary>
        [JsonProperty] public float Volume { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the sound should loop.
        /// </summary>
        [JsonProperty] public bool Loop { get; set; }

        /// <summary>
        /// Converts this DTO into a <see cref="PlaySoundCommand"/> instance.
        /// </summary>
        /// <returns>A new <see cref="PlaySoundCommand"/> instance.</returns>
        public EventCommand ToCommand()
        {
            return new PlaySoundCommand(this);
        }
    }

    /// <summary>
    /// A command for controlling audio, such as playing one-shot or looping sounds,
    /// or stopping existing sounds, via <see cref="UGESoundManager"/>.
    /// </summary>
    [System.Serializable]
    [AvailableIn(GameEventType.Dialogue, GameEventType.CinematicText)]
    public class PlaySoundCommand : EventCommand
    {
        [field: SerializeField] /// <summary>Gets the action to perform (Play/Stop).</summary>
        public SoundAction Action { get; private set; } = SoundAction.Play;
        [field: SerializeField] /// <summary>Gets the type of sound (BGM/SFX) to play or stop.</summary>
        public SoundType SoundType { get; private set; } = SoundType.SFX;
        
        [field: SerializeField] 
        [JsonIgnore] /// <summary>Gets the <see cref="AudioClip"/> to play. This reference is editor-only and not serialized to JSON.</summary>
        public AudioClip AudioClip { get; private set; }

        [Header("Options")]
        [field: SerializeField, Range(0f, 1f)] /// <summary>Gets the volume at which the sound should be played (0.0 to 1.0).</summary>
        public float Volume { get; private set; } = 1.0f;
        [field: SerializeField] /// <summary>Gets a value indicating whether the sound should loop. Primarily used for BGM.</summary>
        public bool Loop { get; private set; } = true; // Primarily used for BGM

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaySoundCommand"/> class.
        /// </summary>
        public PlaySoundCommand()
        {
            CommandType = CommandType.PlaySound;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaySoundCommand"/> class from a DTO.
        /// </summary>
        /// <param name="dto">The <see cref="PlaySoundCommandDto"/> containing the initial data.</param>
        public PlaySoundCommand(PlaySoundCommandDto dto)
        {
            CommandType = CommandType.PlaySound;
            Action = dto.Action;
            SoundType = dto.SoundType;
            Volume = dto.Volume;
            Loop = dto.Loop;
        }

        /// <summary>
        /// Converts this <see cref="PlaySoundCommand"/> instance into a <see cref="PlaySoundCommandDto"/> for serialization.
        /// </summary>
        /// <returns>A new <see cref="PlaySoundCommandDto"/> instance.</returns>
        public override IEventCommandDto ToDto()
        {
            return new PlaySoundCommandDto
            {
                Action = Action,
                SoundType = SoundType,
                Volume = Volume,
                Loop = Loop
            };
        }
    }
}