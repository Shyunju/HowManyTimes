using Newtonsoft.Json;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A data transfer object (DTO) for <see cref="GotoCommand"/>, used for JSON serialization and deserialization.
    /// </summary>
    public class GotoCommandDto : IEventCommandDto
    {
        /// <summary>
        /// Gets or sets the name of the target label to jump to.
        /// </summary>
        [JsonProperty] public string TargetLabel { get; set; }

        /// <summary>
        /// Converts this DTO into a <see cref="GotoCommand"/> instance.
        /// </summary>
        /// <returns>A new <see cref="GotoCommand"/> instance.</returns>
        public EventCommand ToCommand()
        {
            return new GotoCommand(this);
        }
    }

    /// <summary>
    /// A simple command used to change the execution flow within a <see cref="GameEvent"/>
    /// by jumping to a specified <see cref="LabelCommand"/>.
    /// </summary>
    [AvailableIn(GameEventType.Dialogue)]
    [System.Serializable]
    public class GotoCommand : EventCommand
    {
        [SerializeField]
        private string _targetLabel;
        /// <summary>
        /// Gets the name of the target label within the <see cref="GameEvent"/> to jump to.
        /// </summary>
        [JsonIgnore] public string TargetLabel => _targetLabel;

        /// <summary>
        /// Initializes a new instance of the <see cref="GotoCommand"/> class.
        /// </summary>
        public GotoCommand()
        {
            CommandType = CommandType.Goto;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GotoCommand"/> class with a specified target label.
        /// </summary>
        /// <param name="targetLabel">The name of the label to jump to.</param>
        public GotoCommand(string targetLabel)
        {
            CommandType = CommandType.Goto;
            _targetLabel = targetLabel;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GotoCommand"/> class from a DTO.
        /// </summary>
        /// <param name="dto">The <see cref="GotoCommandDto"/> containing the initial data.</param>
        public GotoCommand(GotoCommandDto dto)
        {
            CommandType = CommandType.Goto;
            _targetLabel = dto.TargetLabel;
        }

        /// <summary>
        /// Converts this <see cref="GotoCommand"/> instance into a <see cref="GotoCommandDto"/> for serialization.
        /// </summary>
        /// <returns>A new <see cref="GotoCommandDto"/> instance.</returns>
        public override IEventCommandDto ToDto()
        {
            return new GotoCommandDto
            {
                TargetLabel = _targetLabel
            };
        }
    }
}