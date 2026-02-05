using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A data transfer object (DTO) for <see cref="RewardCommand"/>, used for JSON serialization and deserialization.
    /// </summary>
    public class RewardCommandDto : IEventCommandDto
    {
        /// <summary>
        /// Gets or sets the list of <see cref="AbstractEventReward"/>s to grant.
        /// </summary>
        [JsonProperty] public List<AbstractEventReward> Rewards { get; set; }

        /// <summary>
        /// Converts this DTO into a <see cref="RewardCommand"/> instance.
        /// </summary>
        /// <returns>A new <see cref="RewardCommand"/> instance.</returns>
        public EventCommand ToCommand()
        {
            return new RewardCommand(this);
        }
    }

    /// <summary>
    /// A command responsible for granting a list of rewards to the player or system.
    /// This separates the reward logic from the <see cref="EndCommand"/>.
    /// </summary>
    [AvailableIn(GameEventType.Dialogue, GameEventType.CinematicText)]
    [System.Serializable]
    public class RewardCommand : EventCommand
    {
        [SerializeReference, SerializeField]
        private List<AbstractEventReward> _rewards = new List<AbstractEventReward>();
        /// <summary>
        /// Gets the list of <see cref="AbstractEventReward"/>s to be granted.
        /// </summary>
        [JsonIgnore] public List<AbstractEventReward> Rewards => _rewards;

        /// <summary>
        /// Initializes a new instance of the <see cref="RewardCommand"/> class.
        /// </summary>
        public RewardCommand()
        {
            CommandType = CommandType.Reward;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RewardCommand"/> class from a DTO.
        /// </summary>
        /// <param name="dto">The <see cref="RewardCommandDto"/> containing the initial data.</param>
        public RewardCommand(RewardCommandDto dto)
        {
            CommandType = CommandType.Reward;
            _rewards = dto.Rewards ?? new List<AbstractEventReward>();
        }

        /// <summary>
        /// Converts this <see cref="RewardCommand"/> instance into a <see cref="RewardCommandDto"/> for serialization.
        /// </summary>
        /// <returns>A new <see cref="RewardCommandDto"/> instance.</returns>
        public override IEventCommandDto ToDto()
        {
            return new RewardCommandDto
            {
                Rewards = _rewards
            };
        }
    }
}
