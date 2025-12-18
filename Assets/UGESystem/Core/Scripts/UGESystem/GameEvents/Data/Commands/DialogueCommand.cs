using Newtonsoft.Json;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A data transfer object (DTO) for <see cref="DialogueCommand"/>, used for JSON serialization and deserialization.
    /// </summary>
    public class DialogueCommandDto : IEventCommandDto
    {
        /// <summary>
        /// Gets or sets the dialogue text to display.
        /// </summary>
        [JsonProperty] public string DialogueText { get; set; }
        /// <summary>
        /// Gets or sets the name of the character speaking.
        /// </summary>
        [JsonProperty] public string CharacterName { get; set; }
        /// <summary>
        /// Gets or sets the expression of the character.
        /// </summary>
        [JsonProperty] public string Expression { get; set; }
        /// <summary>
        /// Gets or sets the position of the character on screen.
        /// </summary>
        [JsonProperty] public CharacterPosition CharacterPosition { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether all characters should be cleared before displaying this dialogue.
        /// </summary>
        [JsonProperty] public bool ClearAllCharacters { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the speaking character should be shown.
        /// </summary>
        [JsonProperty] public bool ShowCharacter { get; set; }
        /// <summary>
        /// Gets or sets the animation duration for cinematic text.
        /// </summary>
        [JsonProperty] public float CinematicAnimDuration { get; set; }
        /// <summary>
        /// Gets or sets the display duration for cinematic text.
        /// </summary>
        [JsonProperty] public float CinematicDisplayDuration { get; set; }

        /// <summary>
        /// Converts this DTO into a <see cref="DialogueCommand"/> instance.
        /// </summary>
        /// <returns>A new <see cref="DialogueCommand"/> instance.</returns>
        public EventCommand ToCommand()
        {
            return new DialogueCommand(this);
        }
    }

    /// <summary>
    /// A comprehensive command for displaying dialogue,
    /// including convenience options for simultaneously managing character visibility and position.
    /// </summary>
    [AvailableIn(GameEventType.Dialogue, GameEventType.CinematicText)]
    [System.Serializable]
    public class DialogueCommand : EventCommand
    {
        [SerializeField, TextArea(3, 5)]
        private string _dialogueText;
        /// <summary>
        /// Gets the text content of the dialogue.
        /// </summary>
        [JsonIgnore] public string DialogueText => _dialogueText;
        
        [SerializeField, CharacterId]
        private string _characterName;
        /// <summary>
        /// Gets the name of the character speaking this dialogue.
        /// </summary>
        [JsonIgnore] public string CharacterName => _characterName;

        [SerializeField, Expression(nameof(_characterName))]
        private string _expression;
        /// <summary>
        /// Gets the expression of the character during this dialogue.
        /// </summary>
        [JsonIgnore] public string Expression => _expression;

        [SerializeField]
        private CharacterPosition _characterPosition;
        /// <summary>
        /// Gets the screen position where the character should be placed during this dialogue.
        /// </summary>
        [JsonIgnore] public CharacterPosition CharacterPosition => _characterPosition;

        [SerializeField]
        private bool _clearAllCharacters = false;
        /// <summary>
        /// Gets a value indicating whether all characters on screen should be cleared before this dialogue appears.
        /// </summary>
        [JsonIgnore] public bool ClearAllCharacters => _clearAllCharacters;

        [SerializeField]
        private bool _showCharacter = true;
        /// <summary>
        /// Gets a value indicating whether the speaking character should be shown on screen.
        /// </summary>
        [JsonIgnore] public bool ShowCharacter => _showCharacter;

        [Header("Cinematic Settings")]
        [SerializeField]
        private float _cinematicAnimDuration = 1.0f;
        /// <summary>
        /// Gets the duration of the animation for cinematic dialogue.
        /// </summary>
        [JsonIgnore] public float CinematicAnimDuration => _cinematicAnimDuration;

        [SerializeField]
        private float _cinematicDisplayDuration = 3.0f;
        /// <summary>
        /// Gets the display duration for cinematic dialogue before auto-advancing.
        /// </summary>
        [JsonIgnore] public float CinematicDisplayDuration => _cinematicDisplayDuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogueCommand"/> class with default values.
        /// </summary>
        public DialogueCommand()
        {
            CommandType = CommandType.Dialogue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogueCommand"/> class with specified character name, dialogue text, and optional expression.
        /// </summary>
        /// <param name="characterName">The name of the character speaking.</param>
        /// <param name="dialogueText">The text content of the dialogue.</param>
        /// <param name="expression">The expression of the character (optional).</param>
        public DialogueCommand(string characterName, string dialogueText, string expression = "")
        {
            CommandType = CommandType.Dialogue;
            _characterName = characterName;
            _dialogueText = dialogueText;
            _expression = expression;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogueCommand"/> class from a DTO.
        /// </summary>
        /// <param name="dto">The <see cref="DialogueCommandDto"/> containing the initial data.</param>
        public DialogueCommand(DialogueCommandDto dto)
        {
            CommandType = CommandType.Dialogue;
            _dialogueText = dto.DialogueText;
            _characterName = dto.CharacterName;
            _expression = dto.Expression;
            _characterPosition = dto.CharacterPosition;
            _clearAllCharacters = dto.ClearAllCharacters;
            _showCharacter = dto.ShowCharacter;
            _cinematicAnimDuration = dto.CinematicAnimDuration;
            _cinematicDisplayDuration = dto.CinematicDisplayDuration;
        }

        /// <summary>
        /// Converts this <see cref="DialogueCommand"/> instance into a <see cref="DialogueCommandDto"/> for serialization.
        /// </summary>
        /// <returns>A new <see cref="DialogueCommandDto"/> instance.</returns>
        public override IEventCommandDto ToDto()
        {
            return new DialogueCommandDto
            {
                DialogueText = _dialogueText,
                CharacterName = _characterName,
                Expression = _expression,
                CharacterPosition = _characterPosition,
                ClearAllCharacters = _clearAllCharacters,
                ShowCharacter = _showCharacter,
                CinematicAnimDuration = _cinematicAnimDuration,
                CinematicDisplayDuration = _cinematicDisplayDuration
            };
        }

#if UNITY_EDITOR
        /// <summary>
        /// Sets the default character name for this command in the editor.
        /// </summary>
        /// <param name="name">The default character name.</param>
        public void EDITOR_SetDefaultCharacter(string name)
        {
            _characterName = name;
        }
#endif
    }
}
