using Newtonsoft.Json;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A specific start condition that is met when the player enters a trigger volume with a matching <c>TargetTriggerID</c>.
    /// </summary>
    [System.Serializable]
    public class AreaEnteredCondition : AbstractEventCondition
    {
        [field: SerializeField] [JsonIgnore] /// <summary>Gets the unique ID of the target trigger volume that the player must enter for this condition to be met.</summary>
        public string TargetTriggerID { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AreaEnteredCondition"/> class with a default description.
        /// </summary>
        // Default constructor for Unity
        public AreaEnteredCondition() : base("Player enters a specific area.") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AreaEnteredCondition"/> class from a DTO.
        /// </summary>
        /// <param name="dto">The <see cref="AreaEnteredConditionDto"/> containing the initial data.</param>
        public AreaEnteredCondition(AreaEnteredConditionDto dto) : base(dto)
        {
            TargetTriggerID = dto.TargetTriggerID;
        }

        /// <summary>
        /// Converts this <see cref="AreaEnteredCondition"/> instance into an <see cref="AreaEnteredConditionDto"/> for serialization.
        /// </summary>
        /// <returns>A new <see cref="AreaEnteredConditionDto"/> instance.</returns>
        public override BaseEventConditionDto ToDto()
        {
            return new AreaEnteredConditionDto
            {
                Description = Description,
                TargetTriggerID = TargetTriggerID
            };
        }

        /// <summary>
        /// Subscribes to the <see cref="AreaEnteredEvent"/> to detect when the player enters the target trigger area.
        /// </summary>
        /// <param name="onStateChanged">The action to invoke when the condition is met.</param>
        public override void Subscribe(System.Action onStateChanged)
        {
            base.Subscribe(onStateChanged);
            UGEDelayedEventBus.Subscribe<AreaEnteredEvent>(HandleAreaEntered);
        }

        /// <summary>
        /// Unsubscribes from the <see cref="AreaEnteredEvent"/>.
        /// </summary>
        public override void Unsubscribe()
        {
            base.Unsubscribe();
            UGEDelayedEventBus.Unsubscribe<AreaEnteredEvent>(HandleAreaEntered);
        }

        private void HandleAreaEntered(AreaEnteredEvent e)
        {
            if (!IsMet && e.TriggerID == TargetTriggerID)
            {
                IsMet = true;
                _onStateChanged?.Invoke();
            }
        }
    }
}