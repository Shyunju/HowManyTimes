using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// An abstract base class for all event rewards,
    /// defining the <see cref="GrantReward"/> method that must be implemented by all concrete reward types.
    /// </summary>
    [System.Serializable]
    public abstract class AbstractEventReward
    {
        [field: SerializeField]
        /// <summary>
        /// Gets the human-readable description of this reward.
        /// </summary>
        public string Description { get; private set; } 

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractEventReward"/> class.
        /// </summary>
        public AbstractEventReward() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractEventReward"/> class with a specified description.
        /// </summary>
        /// <param name="description">A human-readable description of the reward.</param>
        public AbstractEventReward(string description)
        {
            Description = description;
        }

        /// <summary>
        /// Grants the reward to the player or applies its effects within the game environment.
        /// </summary>
        /// <param name="runner">The <see cref="UGEEventTaskRunner"/> that is currently executing the event, providing context for the reward.</param>
        public abstract void GrantReward(UGEEventTaskRunner runner);
    }
}