using Newtonsoft.Json;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A data transfer object (DTO) for <see cref="BackgroundCommand"/>, used for JSON serialization and deserialization.
    /// </summary>
    public class BackgroundCommandDto : IEventCommandDto
    {
        /// <summary>
        /// Gets or sets the background action (Show/Hide).
        /// </summary>
        [JsonProperty] public BackgroundAction Action { get; set; }
        /// <summary>
        /// Gets or sets the background type (Image/Video).
        /// </summary>
        [JsonProperty] public BackgroundType Type { get; set; }

        /// <summary>
        /// Converts this DTO into a <see cref="BackgroundCommand"/> instance.
        /// </summary>
        /// <returns>A new <see cref="BackgroundCommand"/> instance.</returns>
        public EventCommand ToCommand()
        {
            return new BackgroundCommand(this);
        }
    }

    /// <summary>
    /// A data-only command class for changing backgrounds, holding action (Show/Hide) and type (Image/Video),
    /// with asset references being editor-only.
    /// </summary>
    [AvailableIn(GameEventType.Dialogue, GameEventType.CinematicText)]
    [System.Serializable]
    public class BackgroundCommand : EventCommand
    {
        [SerializeField]
        private BackgroundAction _action = BackgroundAction.Show;
        /// <summary>
        /// Gets the background action (Show/Hide) to perform.
        /// </summary>
        [JsonIgnore] public BackgroundAction Action => _action;

        [SerializeField]
        private BackgroundType _type = BackgroundType.Image;
        /// <summary>
        /// Gets the type of background (Image/Video) to display.
        /// </summary>
        [JsonIgnore] public BackgroundType Type => _type;

        [SerializeField, JsonIgnore] // Exclude from serialization
        private Texture2D _image;
        /// <summary>
        /// Gets the <see cref="Texture2D"/> asset for an image background.
        /// This reference is editor-only and not serialized to JSON.
        /// </summary>
        [JsonIgnore] public Texture2D Image => _image;

        [SerializeField, JsonIgnore] // Exclude from serialization
        private UnityEngine.Video.VideoClip _video;
        /// <summary>
        /// Gets the <see cref="UnityEngine.Video.VideoClip"/> asset for a video background.
        /// This reference is editor-only and not serialized to JSON.
        /// </summary>
        [JsonIgnore] public UnityEngine.Video.VideoClip Video => _video;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundCommand"/> class with default values.
        /// </summary>
        public BackgroundCommand()
        {
            CommandType = CommandType.Background;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundCommand"/> class from a DTO.
        /// </summary>
        /// <param name="dto">The <see cref="BackgroundCommandDto"/> containing the initial data.</param>
        public BackgroundCommand(BackgroundCommandDto dto)
        {
            CommandType = CommandType.Background;
            _action = dto.Action;
            _type = dto.Type;
        }

        /// <summary>
        /// Converts this <see cref="BackgroundCommand"/> instance into a <see cref="BackgroundCommandDto"/> for serialization.
        /// </summary>
        /// <returns>A new <see cref="BackgroundCommandDto"/> instance.</returns>
        public override IEventCommandDto ToDto()
        {
            return new BackgroundCommandDto
            {
                Action = _action,
                Type = _type
            };
        }
    }
}