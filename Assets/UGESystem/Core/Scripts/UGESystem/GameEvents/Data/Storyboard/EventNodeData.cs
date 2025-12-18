using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace UGESystem
{
    /// <summary>
    /// A core data class representing a single node in a storyboard,
    /// including ID, name, position, start conditions, and a reference to the <see cref="GameEvent"/> to execute.
    /// </summary>
    [Serializable]
    public class EventNodeData
    {
        [SerializeField] private string _nodeID;
        /// <summary>
        /// Gets the unique identifier for this node.
        /// </summary>
        public string NodeID => _nodeID;
        [SerializeField] private string _name;
        /// <summary>
        /// Gets the display name of the node.
        /// </summary>
        public string Name => _name;
        
        [SerializeField] private GameEventType _type;
        /// <summary>
        /// Gets the <see cref="GameEventType"/> associated with this node.
        /// </summary>
        public GameEventType Type => _type;
        
        [HideInInspector]
        /// <summary>
        /// Indicates whether this node is the starting point of the storyboard.
        /// </summary>
        public bool IsStartNode = false;

        [SerializeField] 
        private bool _isRepeatable = false;
        /// <summary>
        /// Gets a value indicating whether this node can be triggered multiple times.
        /// </summary>
        public bool IsRepeatable => _isRepeatable;
        
        [SerializeField]
        private Vector2 _position;
        /// <summary>
        /// Gets or sets the position of the node in the editor graph view.
        /// </summary>
        public Vector2 Position
        {
            get => _position;
            set => _position = value;
        }

        [SerializeReference, SerializeField]
        private List<AbstractEventCondition> _startConditions = new List<AbstractEventCondition>();
        /// <summary>
        /// Gets the list of <see cref="AbstractEventCondition"/>s that must be met for this node to start execution.
        /// </summary>
        public List<AbstractEventCondition> StartConditions => _startConditions;

        [SerializeField]
        private List<string> _nextNodeIDs = new List<string>();
        /// <summary>
        /// Gets the list of IDs of the next nodes to execute after this node completes.
        /// </summary>
        public List<string> NextNodeIDs => _nextNodeIDs;

        /// <summary>
        /// The <see cref="GameEvent"/> asset that this node represents and will execute.
        /// </summary>
        public GameEvent GameEventAsset;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventNodeData"/> class with a new unique ID and default name.
        /// </summary>
        public EventNodeData()
        {
            _nodeID = Guid.NewGuid().ToString();
            _name = "New Node";
            _startConditions = new List<AbstractEventCondition>();
            _nextNodeIDs = new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventNodeData"/> class with specified parameters.
        /// </summary>
        /// <param name="nodeID">The unique identifier for the node.</param>
        /// <param name="name">The display name of the node.</param>
        /// <param name="type">The <see cref="GameEventType"/> associated with this node.</param>
        /// <param name="position">The position of the node in the editor graph view.</param>
        public EventNodeData(string nodeID, string name, GameEventType type, Vector2 position)
        {
            _nodeID = nodeID;
            _name = name;
            _type = type;
            _position = position;
            _nextNodeIDs = new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventNodeData"/> class from a DTO.
        /// </summary>
        /// <param name="dto">The <see cref="EventNodeDataDto"/> containing the initial data.</param>
        public EventNodeData(EventNodeDataDto dto)
        {
            _nodeID = dto.NodeID;
            _name = dto.Name;
            _type = dto.Type;
            IsStartNode = dto.IsStartNode;
            _isRepeatable = dto.IsRepeatable;
            _position = dto.Position;

            // Deserialize conditions
            _startConditions = new List<AbstractEventCondition>();
            if (dto.StartConditions != null)
            {
                foreach (var conditionDto in dto.StartConditions)
                {
                    _startConditions.Add(conditionDto.ToCondition());
                }
            }

            // GameEventAsset will be assigned externally after GameEvent lookup
        }

        /// <summary>
        /// Converts this <see cref="EventNodeData"/> instance into an <see cref="EventNodeDataDto"/> for serialization.
        /// </summary>
        /// <returns>A new <see cref="EventNodeDataDto"/> instance.</returns>
        public EventNodeDataDto ToDto()
        {
            return new EventNodeDataDto
            {
                NodeID = NodeID,
                Name = Name,
                Type = Type,
                IsStartNode = IsStartNode,
                IsRepeatable = IsRepeatable,
                Position = Position,
                GameEventGuid = GameEventAsset != null ? GameEventAsset.Guid : null,
                StartConditions = _startConditions.Select(c => c.ToDto()).ToList<IEventConditionDto>()
            };
        }
    }
}