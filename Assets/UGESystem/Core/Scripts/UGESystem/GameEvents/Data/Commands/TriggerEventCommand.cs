using Newtonsoft.Json;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A data transfer object (DTO) for <see cref="TriggerEventCommand"/>, used for JSON serialization and deserialization.
    /// </summary>
    public class TriggerEventCommandDto : IEventCommandDto
    {
        // [JsonProperty] public string TargetRunnerId { get; set; } // REMOVED

        /// <summary>
        /// Converts this DTO into a <see cref="TriggerEventCommand"/> instance.
        /// </summary>
        /// <returns>A new <see cref="TriggerEventCommand"/> instance.</returns>
        public EventCommand ToCommand()
        {
            return new TriggerEventCommand(this);
        }
    }

    /// <summary>
    /// A command used to start the execution of another <see cref="UGEEventTaskRunner"/> within the same scene
    /// by referencing its unique <c>RunnerId</c>.
    /// </summary>
    [System.Serializable]
    [AvailableIn(GameEventType.Dialogue, GameEventType.CinematicText)]
    public class TriggerEventCommand : EventCommand
    {
        [Tooltip("The ID of the UGEEventTaskRunner in the scene to trigger.")]
        [SerializeField, RunnerId] private string _targetRunnerId;
        /// <summary>
        /// Gets the unique ID of the <see cref="UGEEventTaskRunner"/> to be triggered in the scene.
        /// </summary>
        public string TargetRunnerId => _targetRunnerId;

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerEventCommand"/> class.
        /// </summary>
        public TriggerEventCommand()
        {
            CommandType = CommandType.TriggerEvent;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerEventCommand"/> class from a DTO.
        /// </summary>
        /// <param name="dto">The <see cref="TriggerEventCommandDto"/> containing the initial data.</param>
        public TriggerEventCommand(TriggerEventCommandDto dto)
        {
            CommandType = CommandType.TriggerEvent;
            // _targetRunnerId = dto.TargetRunnerId; // REMOVED
        }

        /// <summary>
        /// Converts this <see cref="TriggerEventCommand"/> instance into a <see cref="TriggerEventCommandDto"/> for serialization.
        /// </summary>
        /// <returns>A new <see cref="TriggerEventCommandDto"/> instance.</returns>
        public override IEventCommandDto ToDto()
        {
            return new TriggerEventCommandDto
            {
                // TargetRunnerId = _targetRunnerId // REMOVED
            };
        }
    }
}
