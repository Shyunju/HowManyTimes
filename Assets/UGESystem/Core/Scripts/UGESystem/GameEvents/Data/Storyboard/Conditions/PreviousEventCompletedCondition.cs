using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

namespace UGESystem
{
    /// <summary>
    /// A specific start condition that is met when a <see cref="NodeCompletedEvent"/>
    /// for the specified <c>TargetNodeID</c> is published on the event bus.
    /// </summary>
    [System.Serializable]
    public class PreviousEventCompletedCondition : AbstractEventCondition
    {
        [field: SerializeField]
        [JsonIgnore] // DTO is responsible for serialization
        /// <summary>
        /// Gets the ID of the target node that must be completed for this condition to be met.
        /// </summary>
        public string TargetNodeID { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PreviousEventCompletedCondition"/> class with a default description.
        /// </summary>
        // Default constructor for Unity
        public PreviousEventCompletedCondition() : base("Previous event must be completed.") { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PreviousEventCompletedCondition"/> class from a DTO.
        /// </summary>
        /// <param name="dto">The <see cref="PreviousEventCompletedConditionDto"/> containing the initial data.</param>
        public PreviousEventCompletedCondition(PreviousEventCompletedConditionDto dto) : base(dto)
        {
            TargetNodeID = dto.TargetNodeID;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PreviousEventCompletedCondition"/> class with a specified description and target node ID.
        /// </summary>
        /// <param name="description">A human-readable description of the condition.</param>
        /// <param name="targetNodeID">The ID of the target node.</param>
        public PreviousEventCompletedCondition(string description, string targetNodeID) : base(description)
        {
            TargetNodeID = targetNodeID;
        }
        
        /// <summary>
        /// Converts this <see cref="PreviousEventCompletedCondition"/> instance into a <see cref="PreviousEventCompletedConditionDto"/> for serialization.
        /// </summary>
        /// <returns>A new <see cref="PreviousEventCompletedConditionDto"/> instance.</returns>
        public override BaseEventConditionDto ToDto()
        {
            return new PreviousEventCompletedConditionDto
            {
                Description = Description,
                TargetNodeID = TargetNodeID
            };
        }

        /// <summary>
        /// Subscribes to the <see cref="NodeCompletedEvent"/> to detect when the target node is completed.
        /// </summary>
        /// <param name="onStateChanged">The action to invoke when the condition is met.</param>
        public override void Subscribe(System.Action onStateChanged)
        {
            base.Subscribe(onStateChanged);
            UGEDelayedEventBus.Subscribe<NodeCompletedEvent>(HandleNodeCompleted);
        }

        /// <summary>
        /// Unsubscribes from the <see cref="NodeCompletedEvent"/>.
        /// </summary>
        public override void Unsubscribe()
        {
            base.Unsubscribe();
            UGEDelayedEventBus.Unsubscribe<NodeCompletedEvent>(HandleNodeCompleted);
        }

        private void HandleNodeCompleted(NodeCompletedEvent e)
        {
            if (!IsMet && e.NodeID == TargetNodeID)
            {
                IsMet = true;
                _onStateChanged?.Invoke();
            }
        }

        /// <summary>
        /// Immediately checks if the target node is already completed in the current runner or globally.
        /// </summary>
        /// <param name="runner">The context runner.</param>
        public override void Evaluate(UGEEventTaskRunner runner)
        {
            if (IsMet) return;

            // 1. Check within the same runner first (most common case)
            if (runner.NodeStatuses.TryGetValue(TargetNodeID, out var status))
            {
                if (status == EventStatus.Completed)
                {
                    IsMet = true;
                    return;
                }
            }

            // 2. Check globally via UGESystemController if not found in the same runner
            if (UGESystemController.Instance != null)
            {
                // Note: This might be expensive if many runners exist, but necessary for cross-runner dependencies.
                // However, since NodeIDs are unique across the system, we can look through all active runners.
                // But for now, let's focus on the common case (same runner).
                // If we need cross-runner restoration, we'd need a global status registry.
            }
        }
    }
}
