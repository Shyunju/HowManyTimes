using System;
using UnityEngine;
using Newtonsoft.Json;

namespace UGESystem
{
    /// <summary>
    /// An abstract base class for all event start conditions,
    /// defining shared properties like <see cref="IsMet"/> and methods for event bus subscription.
    /// </summary>
    [Serializable]
    public abstract class AbstractEventCondition
    {
        [field: SerializeField]
        /// <summary>
        /// Gets the human-readable description of this condition.
        /// </summary>
        public string Description { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether this condition has been met.
        /// </summary>
        public bool IsMet { get; protected set; }
        
        protected Action _onStateChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractEventCondition"/> class with a default description and unmet status.
        /// </summary>
        // Default constructor for Unity serialization/creation
        protected AbstractEventCondition()
        {
            Description = "New Condition";
            IsMet = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractEventCondition"/> class with a specified description and unmet status.
        /// </summary>
        /// <param name="description">A human-readable description of the condition.</param>
        protected AbstractEventCondition(string description)
        {
            Description = description;
            IsMet = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractEventCondition"/> class from a DTO.
        /// </summary>
        /// <param name="dto">The <see cref="BaseEventConditionDto"/> containing initial data for the condition.</param>
        protected AbstractEventCondition(BaseEventConditionDto dto)
        {
            Description = dto.Description;
            IsMet = false; // Always reset state on load
        }

        /// <summary>
        /// Converts this <see cref="AbstractEventCondition"/> instance into its corresponding DTO for serialization.
        /// </summary>
        /// <returns>The DTO representation of this condition.</returns>
        public abstract BaseEventConditionDto ToDto();

        /// <summary>
        /// Subscribes a callback to be invoked when the condition's state changes.
        /// This method is typically called by the <see cref="UGEEventTaskRunner"/>.
        /// </summary>
        /// <param name="onStateChanged">The action to invoke when the condition's state changes.</param>
        public virtual void Subscribe(Action onStateChanged)
        {
            _onStateChanged = onStateChanged;
        }

        /// <summary>
        /// Unsubscribes all registered callbacks from this condition, preventing memory leaks.
        /// </summary>
        public virtual void Unsubscribe()
        {
            _onStateChanged = null;
        }

        /// <summary>
        /// Resets the condition's state to unmet.
        /// </summary>
        public virtual void Reset()
        {
            IsMet = false;
        }
    }
}