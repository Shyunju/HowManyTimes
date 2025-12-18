using Newtonsoft.Json;
using UGESystem; // For AbstractEventCondition

namespace UGESystem
{
    /// <summary>
    /// A data transfer object (DTO) used for serializing and deserializing <see cref="InteractionTriggeredCondition"/>
    /// for web or file storage.
    /// </summary>
    public class InteractionTriggeredConditionDto : BaseEventConditionDto
    {
        /// <summary>
        /// Gets or sets the unique ID of the target interactable object.
        /// </summary>
        [JsonProperty] public string TargetInteractionID { get; set; }

        /// <summary>
        /// Converts this DTO into an <see cref="InteractionTriggeredCondition"/> instance.
        /// </summary>
        /// <returns>A new <see cref="InteractionTriggeredCondition"/> instance.</returns>
        public override AbstractEventCondition ToCondition()
        {
            return new InteractionTriggeredCondition(this);
        }
    }
}