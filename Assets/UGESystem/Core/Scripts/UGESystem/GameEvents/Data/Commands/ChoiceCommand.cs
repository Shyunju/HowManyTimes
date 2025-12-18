using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A data transfer object (DTO) for a single <see cref="ChoiceOption"/>, used for JSON serialization and deserialization.
    /// </summary>
    public class ChoiceOptionDto
    {
        /// <summary>
        /// Gets or sets the display text for the choice option.
        /// </summary>
        [JsonProperty] public string Text { get; set; }
        /// <summary>
        /// Gets or sets the target label within the <see cref="GameEvent"/> to jump to when this option is selected.
        /// </summary>
        [JsonProperty] public string TargetLabel { get; set; }
    }

    /// <summary>
    /// Represents a single choice option for a <see cref="ChoiceCommand"/>,
    /// linking display text to a target label within the <see cref="GameEvent"/>.
    /// </summary>
    [System.Serializable]
    public class ChoiceOption
    {
        [SerializeField]
        private string _text;
        /// <summary>
        /// Gets the display text for this choice option.
        /// </summary>
        [JsonIgnore] public string Text => _text;
        
        [SerializeField]
        private string _targetLabel;
        /// <summary>
        /// Gets the target label within the <see cref="GameEvent"/> to jump to when this option is selected.
        /// </summary>
        [JsonIgnore] public string TargetLabel => _targetLabel;

        /// <summary>
        /// Initializes a new empty instance of the <see cref="ChoiceOption"/> class.
        /// </summary>
        public ChoiceOption() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChoiceOption"/> class with specified text and target label.
        /// </summary>
        /// <param name="text">The display text for the choice.</param>
        /// <param name="targetLabel">The target label in the <see cref="GameEvent"/> to jump to.</param>
        public ChoiceOption(string text, string targetLabel)
        {
            _text = text;
            _targetLabel = targetLabel;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChoiceOption"/> class from a DTO.
        /// </summary>
        /// <param name="dto">The <see cref="ChoiceOptionDto"/> containing the initial data.</param>
        public ChoiceOption(ChoiceOptionDto dto)
        {
            _text = dto.Text;
            _targetLabel = dto.TargetLabel;
        }

        /// <summary>
        /// Converts this <see cref="ChoiceOption"/> instance into a <see cref="ChoiceOptionDto"/> for serialization.
        /// </summary>
        /// <returns>A new <see cref="ChoiceOptionDto"/> instance.</returns>
        public ChoiceOptionDto ToDto()
        {
            return new ChoiceOptionDto
            {
                Text = _text,
                TargetLabel = _targetLabel
            };
        }
    }

    /// <summary>
    /// A data transfer object (DTO) for <see cref="ChoiceCommand"/>, used for JSON serialization and deserialization.
    /// </summary>
    public class ChoiceCommandDto : IEventCommandDto
    {
        /// <summary>
        /// Gets or sets the list of <see cref="ChoiceOptionDto"/> for this command.
        /// </summary>
        [JsonProperty] public List<ChoiceOptionDto> Choices { get; set; }

        /// <summary>
        /// Converts this DTO into a <see cref="ChoiceCommand"/> instance.
        /// </summary>
        /// <returns>A new <see cref="ChoiceCommand"/> instance.</returns>
        public EventCommand ToCommand()
        {
            return new ChoiceCommand(this);
        }
    }

    /// <summary>
    /// A command containing a list of <see cref="ChoiceOption"/>s,
    /// linking text to be displayed to the player with target labels within the <see cref="GameEvent"/> for branching.
    /// </summary>
    [AvailableIn(GameEventType.Dialogue)]
    [System.Serializable]
    public class ChoiceCommand : EventCommand
    {
        [SerializeField]
        private List<ChoiceOption> _choices = new List<ChoiceOption>();
        /// <summary>
        /// Gets the list of <see cref="ChoiceOption"/>s available to the player.
        /// </summary>
        [JsonIgnore] public List<ChoiceOption> Choices => _choices;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChoiceCommand"/> class with default values.
        /// </summary>
        public ChoiceCommand()
        {
            CommandType = CommandType.Choice;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ChoiceCommand"/> class from a DTO.
        /// </summary>
        /// <param name="dto">The <see cref="ChoiceCommandDto"/> containing the initial data.</param>
        public ChoiceCommand(ChoiceCommandDto dto)
        {
            CommandType = CommandType.Choice;
            _choices = dto.Choices?.Select(c => new ChoiceOption(c)).ToList() ?? new List<ChoiceOption>();
        }

        /// <summary>
        /// Converts this <see cref="ChoiceCommand"/> instance into a <see cref="ChoiceCommandDto"/> for serialization.
        /// </summary>
        /// <returns>A new <see cref="ChoiceCommandDto"/> instance.</returns>
        public override IEventCommandDto ToDto()
        {
            return new ChoiceCommandDto
            {
                Choices = _choices.Select(c => c.ToDto()).ToList()
            };
        }
    }
}