using Newtonsoft.Json;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A specific start condition that is met when the player interacts with an object having a matching <c>TargetInteractionID</c>.
    /// </summary>
    [System.Serializable]
    public class InteractionTriggeredCondition : AbstractEventCondition
    {
        [field: SerializeField] [JsonIgnore] /// <summary>Gets the unique ID of the target interactable object that the player must interact with for this condition to be met.</summary>
        public string TargetInteractionID { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionTriggeredCondition"/> class with a default description.
        /// </summary>
        // Default constructor for Unity
        public InteractionTriggeredCondition() : base("Player interacts with a specific object.") { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionTriggeredCondition"/> class from a DTO.
        /// </summary>
        /// <param name="dto">The <see cref="InteractionTriggeredConditionDto"/> containing the initial data.</param>
        public InteractionTriggeredCondition(InteractionTriggeredConditionDto dto) : base(dto)
        {
            TargetInteractionID = dto.TargetInteractionID;
        }

        /// <summary>
        /// Converts this <see cref="InteractionTriggeredCondition"/> instance into an <see cref="InteractionTriggeredConditionDto"/> for serialization.
        /// </summary>
        /// <returns>A new <see cref="InteractionTriggeredConditionDto"/> instance.</returns>
        public override BaseEventConditionDto ToDto()
        {
            return new InteractionTriggeredConditionDto
            {
                Description = Description,
                TargetInteractionID = TargetInteractionID
            };
        }

        /// <summary>
        /// Subscribes to the <see cref="InteractionTriggeredEvent"/> to detect when the player interacts with the target object.
        /// </summary>
        /// <param name="onStateChanged">The action to invoke when the condition is met.</param>
        public override void Subscribe(System.Action onStateChanged)
        {
            base.Subscribe(onStateChanged);
            UGEDelayedEventBus.Subscribe<InteractionTriggeredEvent>(HandleInteractionTriggered);
        }

        /// <summary>
        /// Unsubscribes from the <see cref="InteractionTriggeredEvent"/>.
        /// </summary>
        public override void Unsubscribe()
        {
            base.Unsubscribe();
            UGEDelayedEventBus.Unsubscribe<InteractionTriggeredEvent>(HandleInteractionTriggered);
        }

        private void HandleInteractionTriggered(InteractionTriggeredEvent e)
        {
            if (!IsMet && e.InteractionID == TargetInteractionID)
            {
                IsMet = true;
                _onStateChanged?.Invoke();
            }
        }
    }
}