using Newtonsoft.Json;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// Defines how the character data should be updated.
    /// </summary>
    public enum CharacterUpdateType
    {
        /// <summary>Fully replace data (Name, Prefab, Expressions) with another character's template.</summary>
        Full,
        /// <summary>Only update specific fields like Name.</summary>
        Partial,
        /// <summary>Restore the character's data to its original asset state.</summary>
        ResetToOriginal
    }

    /// <summary>
    /// A data transfer object (DTO) for <see cref="CharacterUpdateCommand"/>, used for JSON serialization and deserialization.
    /// </summary>
    public class CharacterUpdateCommandDto : IEventCommandDto
    {
        [JsonProperty] public string TargetCharacterId { get; set; }
        [JsonProperty] public CharacterUpdateType UpdateType { get; set; }
        [JsonProperty] public string SourceTemplateId { get; set; }
        [JsonProperty] public string OverrideName { get; set; }

        public EventCommand ToCommand()
        {
            return new CharacterUpdateCommand(this);
        }
    }

    /// <summary>
    /// A command that updates a character's data in the runtime database,
    /// allowing for outfit changes, name changes, or complete data replacement.
    /// </summary>
    [AvailableIn(GameEventType.Dialogue, GameEventType.CinematicText)]
    [System.Serializable]
    public class CharacterUpdateCommand : EventCommand
    {
        [SerializeField, CharacterId]
        private string _targetCharacterId;
        /// <summary>
        /// Gets the ID of the character to be updated.
        /// </summary>
        [JsonIgnore] public string TargetCharacterId => _targetCharacterId;

        [SerializeField]
        private CharacterUpdateType _updateType = CharacterUpdateType.Full;
        /// <summary>
        /// Gets the type of update to perform (Full, Partial, or Reset).
        /// </summary>
        [JsonIgnore] public CharacterUpdateType UpdateType => _updateType;

        [SerializeField, CharacterId]
        private string _sourceTemplateId;
        /// <summary>
        /// Gets the ID of the character whose data will be used as a template for a Full update.
        /// </summary>
        [JsonIgnore] public string SourceTemplateId => _sourceTemplateId;

        [SerializeField]
        private string _overrideName;
        /// <summary>
        /// Gets the new name to apply for a Partial update.
        /// </summary>
        [JsonIgnore] public string OverrideName => _overrideName;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacterUpdateCommand"/> class.
        /// </summary>
        public CharacterUpdateCommand()
        {
            CommandType = CommandType.CharacterUpdate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacterUpdateCommand"/> class from a DTO.
        /// </summary>
        /// <param name="dto">The DTO containing initial data.</param>
        public CharacterUpdateCommand(CharacterUpdateCommandDto dto)
        {
            CommandType = CommandType.CharacterUpdate;
            _targetCharacterId = dto.TargetCharacterId;
            _updateType = dto.UpdateType;
            _sourceTemplateId = dto.SourceTemplateId;
            _overrideName = dto.OverrideName;
        }

        /// <summary>
        /// Converts this command into a DTO for serialization.
        /// </summary>
        /// <returns>A new <see cref="CharacterUpdateCommandDto"/>.</returns>
        public override IEventCommandDto ToDto()
        {
            return new CharacterUpdateCommandDto
            {
                TargetCharacterId = _targetCharacterId,
                UpdateType = _updateType,
                SourceTemplateId = _sourceTemplateId,
                OverrideName = _overrideName
            };
        }
    }
}
