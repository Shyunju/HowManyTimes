using UnityEngine;
using Newtonsoft.Json;

namespace UGESystem
{
    /// <summary>
    /// A data transfer object (DTO) for <see cref="CharacterCommand"/>, used for JSON serialization and deserialization.
    /// </summary>
    public class CharacterCommandDto : IEventCommandDto
    {
        /// <summary>
        /// Gets or sets the ID of the character.
        /// </summary>
        [JsonProperty] public string CharacterId { get; set; }
        /// <summary>
        /// Gets or sets the action the character should perform.
        /// </summary>
        [JsonProperty] public CharacterAction Action { get; set; }
        /// <summary>
        /// Gets or sets the screen position where the character should appear.
        /// </summary>
        [JsonProperty] public CharacterPosition Position { get; set; }
        /// <summary>
        /// Gets or sets the expression of the character.
        /// </summary>
        [JsonProperty] public string Expression { get; set; }

        /// <summary>
        /// Converts this DTO into a <see cref="CharacterCommand"/> instance.
        /// </summary>
        /// <returns>A new <see cref="CharacterCommand"/> instance.</returns>
        public EventCommand ToCommand()
        {
            return new CharacterCommand(this);
        }
    }

    /// <summary>
    /// A command class used to explicitly control character actions,
    /// such as appearing at a specific screen position, disappearing, or changing expressions.
    /// </summary>
    [AvailableIn(GameEventType.Dialogue, GameEventType.CinematicText)]
    [System.Serializable]
    public class CharacterCommand : EventCommand
    {
        [SerializeField, CharacterId]
        private string _characterId;
        /// <summary>
        /// Gets the unique ID of the character to perform the action.
        /// </summary>
        [JsonIgnore] public string CharacterId => _characterId;
        
        [SerializeField]
        private CharacterAction _action;
        /// <summary>
        /// Gets the action the character should perform (e.g., Show, Hide, ChangeExpression).
        /// </summary>
        [JsonIgnore] public CharacterAction Action => _action;

        [SerializeField]
        private CharacterPosition _position;
        /// <summary>
        /// Gets the screen position where the character should appear. Relevant only for Show action.
        /// </summary>
        [JsonIgnore] public CharacterPosition Position => _position;

        [SerializeField, Expression(nameof(_characterId))]
        private string _expression;
        /// <summary>
        /// Gets the expression to set for the character. Relevant only for ChangeExpression action.
        /// </summary>
        [JsonIgnore] public string Expression => _expression;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CharacterCommand"/> class with default values.
        /// </summary>
        public CharacterCommand()
        {
            CommandType = CommandType.Character;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacterCommand"/> class with specified parameters.
        /// </summary>
        /// <param name="characterId">The unique ID of the character.</param>
        /// <param name="action">The action the character should perform.</param>
        /// <param name="position">The screen position for the character (if applicable).</param>
        /// <param name="expression">The expression to set (if applicable).</param>
        public CharacterCommand(string characterId, CharacterAction action, CharacterPosition position, string expression = "")
        {
            CommandType = CommandType.Character;
            _characterId = characterId;
            _action = action;
            _position = position;
            _expression = expression;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacterCommand"/> class from a DTO.
        /// </summary>
        /// <param name="dto">The <see cref="CharacterCommandDto"/> containing the initial data.</param>
        public CharacterCommand(CharacterCommandDto dto)
        {
            CommandType = CommandType.Character;
            _characterId = dto.CharacterId;
            _action = dto.Action;
            _position = dto.Position;
            _expression = dto.Expression;
        }

        /// <summary>
        /// Converts this <see cref="CharacterCommand"/> instance into a <see cref="CharacterCommandDto"/> for serialization.
        /// </summary>
        /// <returns>A new <see cref="CharacterCommandDto"/> instance.</returns>
        public override IEventCommandDto ToDto()
        {
            return new CharacterCommandDto
            {
                CharacterId = _characterId,
                Action = _action,
                Position = _position,
                Expression = _expression
            };
        }

#if UNITY_EDITOR
        /// <summary>
        /// Sets the default character ID for this command in the editor.
        /// </summary>
        /// <param name="id">The default character ID.</param>
        public void EDITOR_SetDefaultCharacter(string id)
        {
            _characterId = id;
        }
#endif
    }
}