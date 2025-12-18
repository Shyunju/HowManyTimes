using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A data transfer object (DTO) for <see cref="EndCommand"/>, used for JSON serialization and deserialization.
    /// </summary>
    public class EndCommandDto : IEventCommandDto
    {
        /// <summary>
        /// Gets or sets the list of <see cref="AbstractEventReward"/>s to grant when the event ends.
        /// </summary>
        [JsonProperty] public List<AbstractEventReward> Rewards { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the command should branch to another node.
        /// </summary>
        [JsonProperty] public bool IsBranching { get; set; }
        /// <summary>
        /// Gets or sets the ID of the target node to branch to if <see cref="IsBranching"/> is true.
        /// </summary>
        [JsonProperty] public string TargetNodeID { get; set; }

        /// <summary>
        /// Converts this DTO into an <see cref="EndCommand"/> instance.
        /// </summary>
        /// <returns>A new <see cref="EndCommand"/> instance.</returns>
        public EventCommand ToCommand()
        {
            return new EndCommand(this);
        }
    }

    /// <summary>
    /// A command marking the end of a <see cref="GameEvent"/>,
    /// including an optional list of rewards to grant and a target for branching to another node in the storyboard.
    /// </summary>
    [AvailableIn(GameEventType.Dialogue, GameEventType.CinematicText)]
    [Serializable]
    public class EndCommand : EventCommand
    {
        [SerializeReference, SerializeField]
        private List<AbstractEventReward> _rewards = new List<AbstractEventReward>();
        /// <summary>
        /// Gets the list of <see cref="AbstractEventReward"/>s to grant when the event ends.
        /// </summary>
        [JsonIgnore] public List<AbstractEventReward> Rewards => _rewards;

        [Header("Branching")]
        [SerializeField]
        private bool _isBranching = false;
        /// <summary>
        /// Gets a value indicating whether this command initiates a branch to another node.
        /// </summary>
        [JsonIgnore] public bool IsBranching => _isBranching;

        [SerializeField, NodeId]
        private string _targetNodeID;
        /// <summary>
        /// Gets the ID of the target node to branch to if <see cref="IsBranching"/> is true.
        /// </summary>
        [JsonIgnore] public string TargetNodeID => _targetNodeID;

        /// <summary>
        /// Initializes a new instance of the <see cref="EndCommand"/> class.
        /// </summary>
        public EndCommand()
        {
            CommandType = CommandType.End;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EndCommand"/> class from a DTO.
        /// </summary>
        /// <param name="dto">The <see cref="EndCommandDto"/> containing the initial data.</param>
        public EndCommand(EndCommandDto dto)
        {
            CommandType = CommandType.End;
            _rewards = dto.Rewards ?? new List<AbstractEventReward>();
            _isBranching = dto.IsBranching;
            _targetNodeID = dto.TargetNodeID;
        }

        /// <summary>
        /// Converts this <see cref="EndCommand"/> instance into an <see cref="EndCommandDto"/> for serialization.
        /// </summary>
        /// <returns>A new <see cref="EndCommandDto"/> instance.</returns>
        public override IEventCommandDto ToDto()
        {
            return new EndCommandDto
            {
                Rewards = _rewards,
                IsBranching = _isBranching,
                TargetNodeID = _targetNodeID
            };
        }
    }
}