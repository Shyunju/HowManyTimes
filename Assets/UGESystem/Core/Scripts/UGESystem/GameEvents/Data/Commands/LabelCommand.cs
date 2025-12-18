using Newtonsoft.Json;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A data transfer object (DTO) for <see cref="LabelCommand"/>, used for JSON serialization and deserialization.
    /// </summary>
    public class LabelCommandDto : IEventCommandDto
    {
        /// <summary>
        /// Gets or sets the name of the label.
        /// </summary>
        [JsonProperty] public string LabelName { get; set; }

        /// <summary>
        /// Converts this DTO into a <see cref="LabelCommand"/> instance.
        /// </summary>
        /// <returns>A new <see cref="LabelCommand"/> instance.</returns>
        public EventCommand ToCommand()
        {
            return new LabelCommand(this);
        }
    }

    /// <summary>
    /// A marker command that defines a jump destination within a <see cref="GameEvent"/>
    /// for <see cref="GotoCommand"/> and <see cref="ChoiceCommand"/>.
    /// </summary>
    [AvailableIn(GameEventType.Dialogue)]
    [System.Serializable]
    public class LabelCommand : EventCommand
    {
        [SerializeField]
        private string _labelName;
        /// <summary>
        /// Gets the name of the label.
        /// </summary>
        [JsonIgnore] public string LabelName => _labelName;

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelCommand"/> class.
        /// </summary>
        public LabelCommand()
        {
            CommandType = CommandType.Label;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelCommand"/> class with a specified label name.
        /// </summary>
        /// <param name="labelName">The name of the label.</param>
        public LabelCommand(string labelName)
        {
            CommandType = CommandType.Label;
            _labelName = labelName;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="LabelCommand"/> class from a DTO.
        /// </summary>
        /// <param name="dto">The <see cref="LabelCommandDto"/> containing the initial data.</param>
        public LabelCommand(LabelCommandDto dto)
        {
            CommandType = CommandType.Label;
            _labelName = dto.LabelName;
        }

        /// <summary>
        /// Converts this <see cref="LabelCommand"/> instance into a <see cref="LabelCommandDto"/> for serialization.
        /// </summary>
        /// <returns>A new <see cref="LabelCommandDto"/> instance.</returns>
        public override IEventCommandDto ToDto()
        {
            return new LabelCommandDto
            {
                LabelName = _labelName
            };
        }
    }
}