using Newtonsoft.Json;
using UGESystem; // For AbstractEventCondition

namespace UGESystem
{
    /// <summary>
    /// A data transfer object (DTO) used for serializing and deserializing <see cref="PreviousEventCompletedCondition"/>
    /// for web or file storage.
    /// </summary>
    public class PreviousEventCompletedConditionDto : BaseEventConditionDto
    {
        /// <summary>
        /// Gets or sets the ID of the target node that must have been completed.
        /// </summary>
        [JsonProperty] public string TargetNodeID { get; set; }

        /// <summary>
        /// Converts this DTO into a <see cref="PreviousEventCompletedCondition"/> instance.
        /// </summary>
        /// <returns>A new <see cref="PreviousEventCompletedCondition"/> instance.</returns>
        public override AbstractEventCondition ToCondition()
        {
            return new PreviousEventCompletedCondition(this);
        }
    }
}