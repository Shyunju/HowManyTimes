using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A concrete reward class intended to change a player's specific stat by a certain amount when granted.
    /// </summary>
    [System.Serializable]
    public class ChangeStatReward : AbstractEventReward
    {
        [field: SerializeField]
        /// <summary>
        /// Gets the type of player stat to modify.
        /// </summary>
        public StatType TargetStat { get; private set; }

        [field: SerializeField]
        /// <summary>
        /// Gets the amount by which the target stat should be changed.
        /// </summary>
        public int ChangeAmount { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeStatReward"/> class with a default description.
        /// </summary>
        public ChangeStatReward() : base("Change a player stat.") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeStatReward"/> class with specified parameters.
        /// </summary>
        /// <param name="description">A human-readable description of the reward.</param>
        /// <param name="targetStat">The <see cref="StatType"/> to be modified.</param>
        /// <param name="changeAmount">The amount to change the stat by.</param>
        public ChangeStatReward(string description, StatType targetStat, int changeAmount) : base(description)
        {
            TargetStat = targetStat;
            ChangeAmount = changeAmount;
        }

        /// <summary>
        /// Applies the stat change to the player's target stat.
        /// </summary>
        /// <param name="runner">The <see cref="UGEEventTaskRunner"/> that is currently executing the event.</param>
        public override void GrantReward(UGEEventTaskRunner runner)
        {
            // TODO: Access PlayerStatManager or similar through the runner to actually change the stat
        }
    }
}