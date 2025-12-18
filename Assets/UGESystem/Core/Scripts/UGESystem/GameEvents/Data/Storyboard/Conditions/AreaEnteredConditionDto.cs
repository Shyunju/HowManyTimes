using Newtonsoft.Json;
using UGESystem; // For AbstractEventCondition

namespace UGESystem
{
    /// <summary>
    /// A data transfer object (DTO) used for serializing and deserializing <see cref="AreaEnteredCondition"/>
    /// for web or file storage.
    /// </summary>
    public class AreaEnteredConditionDto : BaseEventConditionDto
    {
        /// <summary>
        /// Gets or sets the unique ID of the target trigger volume.
        /// </summary>
        [JsonProperty] public string TargetTriggerID { get; set; }

        /// <summary>
        /// Converts this DTO into an <see cref="AreaEnteredCondition"/> instance.
        /// </summary>
        /// <returns>A new <see cref="AreaEnteredCondition"/> instance.</returns>
        public override AbstractEventCondition ToCondition()
        {
            return new AreaEnteredCondition(this);
        }
    }
}