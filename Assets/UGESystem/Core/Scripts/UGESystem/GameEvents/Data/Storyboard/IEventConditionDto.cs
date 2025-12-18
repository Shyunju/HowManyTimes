using UGESystem; // For AbstractEventCondition

namespace UGESystem
{
    /// <summary>
    /// A common interface for all event condition DTOs,
    /// ensuring they have a <see cref="ToCondition"/> method to convert back to their runtime data type.
    /// </summary>
    public interface IEventConditionDto
    {
        /// <summary>
        /// Converts the DTO into its corresponding <see cref="AbstractEventCondition"/> instance.
        /// </summary>
        /// <returns>The <see cref="AbstractEventCondition"/> instance.</returns>
        AbstractEventCondition ToCondition();
    }
}