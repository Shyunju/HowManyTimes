using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A ScriptableObject that holds a list of commands for a specific event sequence,
    /// managing its own GUID and JSON serialization/deserialization.
    /// </summary>
    //[CreateAssetMenu(fileName = "NewGameEvent", menuName = "UGESystem/Game Event")]
    public class GameEvent : ScriptableObject
    {
        // --- DTO for pure data serialization ---
        public class GameEventDto
        {
            public string Guid;
            public GameEventArchetype Archetype;
            // Using List<object> to hold various command DTOs, which will be identified by the $type property.
            public List<object> Commands;
        }
        
        /// <summary>
        /// A unique identifier for this <see cref="GameEvent"/>, used for linking from storyboards and other systems.
        /// </summary>
        [field: SerializeField] public string Guid { get; private set; }

        [SerializeField] private GameEventArchetype _archetype = GameEventArchetype.Generic;
        /// <summary>
        /// The intended use-context for this event (e.g., Dialogue, CinematicText),
        /// which can affect which commands are available or how they are handled.
        /// </summary>
        public GameEventArchetype Archetype => _archetype;

        [SerializeReference, SerializeField]
        private List<IGameEventCommand> _commands = new List<IGameEventCommand>();
        
        /// <summary>
        /// The list of all commands that will be executed in sequence when this event is run.
        /// </summary>
        public List<IGameEventCommand> Commands 
        { 
            get { return _commands; } 
        }

#if UNITY_EDITOR
        private void OnEnable()
        {
            // If the current GUID is empty
            if (string.IsNullOrEmpty(this.Guid))
            {
                // Assign a new GUID
                this.Guid = System.Guid.NewGuid().ToString();
                // Notify Unity editor to save changes.
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif

        /// <summary>
        /// Serializes the entire <see cref="GameEvent"/> content, including its list of commands, into a JSON string.
        /// </summary>
        /// <returns>A JSON string representing the <see cref="GameEvent"/>.</returns>
        public string ToJson()
        {
            var dto = new GameEventDto
            {
                Guid = this.Guid,
                Archetype = this.Archetype,
                Commands = _commands.Select(c => (c as EventCommand)?.ToDto()).Where(d => d != null).ToList<object>()
            };

            return JsonConvert.SerializeObject(dto, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter> 
                { 
                    new Newtonsoft.Json.Converters.StringEnumConverter(),
                    new Vector2Converter(),
                    new Vector3Converter(),
                    new ColorConverter()
                }
            });
        }

        /// <summary>
        /// Deserializes a JSON string to populate this <see cref="GameEvent"/>'s data, reconstructing its command list.
        /// </summary>
        /// <param name="json">The JSON string representing the <see cref="GameEvent"/>.</param>
        public void FromJson(string json)
        {
            var dto = JsonConvert.DeserializeObject<GameEventDto>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = new List<JsonConverter>
                {
                    new Vector2Converter(),
                    new Vector3Converter(),
                    new ColorConverter()
                }
            });

            if (dto == null) return;
            
            this.Guid = dto.Guid;
            _archetype = dto.Archetype;
            _commands.Clear();

            if (dto.Commands != null)
            {
                foreach (var commandObj in dto.Commands)
                {
                    IEventCommandDto commandDto = commandObj as IEventCommandDto;

                    // Fallback: If it came in as a JObject (which happens if TypeNameHandling fails or type is ambiguous)
                    if (commandDto == null && commandObj is Newtonsoft.Json.Linq.JObject jObj)
                    {
                        // Try to find the $type property
                        var typeToken = jObj["$type"];
                        if (typeToken != null)
                        {
                            string typeName = typeToken.ToString();
                            // Try to get the type directly
                            System.Type targetType = System.Type.GetType(typeName);
                            
                            if (targetType != null)
                            {
                                commandDto = jObj.ToObject(targetType) as IEventCommandDto;
                            }
                        }
                    }

                    if (commandDto != null)
                    {
                        EventCommand newCommand = commandDto.ToCommand();
                        if (newCommand != null)
                        {
                            _commands.Add(newCommand);
                        }
                    }
                }
            }
            
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}