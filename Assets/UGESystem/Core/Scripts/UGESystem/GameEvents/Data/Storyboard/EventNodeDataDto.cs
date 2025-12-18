using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A data transfer object (DTO) for <see cref="EventNodeData"/>,
    /// defining a JSON serialization structure that includes a polymorphic list of condition DTOs.
    /// </summary>
    public class EventNodeDataDto
    {
        /// <summary>
        /// Gets or sets the unique ID of the node.
        /// </summary>
        [JsonProperty] public string NodeID { get; set; }
        /// <summary>
        /// Gets or sets the display name of the node.
        /// </summary>
        [JsonProperty] public string Name { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="GameEventType"/> associated with the node.
        /// </summary>
        [JsonProperty] public GameEventType Type { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this is the start node of the storyboard.
        /// </summary>
        [JsonProperty] public bool IsStartNode { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this node can be triggered multiple times.
        /// </summary>
        [JsonProperty] public bool IsRepeatable { get; set; }
        /// <summary>
        /// Gets or sets the position of the node in the graph view.
        /// </summary>
        [JsonProperty] public Vector2 Position { get; set; }
        
        /// <summary>
        /// Gets or sets a list of <see cref="IEventConditionDto"/> that define when this node can start.
        /// This will be a list of BaseEventConditionDto, requiring TypeNameHandling.Auto for polymorphism.
        /// </summary>
        [JsonProperty] public List<IEventConditionDto> StartConditions { get; set; } // Use object to handle polymorphism
        
        /// <summary>
        /// Gets or sets the GUID of the associated <see cref="GameEvent"/> asset.
        /// </summary>
        [JsonProperty] public string GameEventGuid { get; set; }
    }

    /// <summary>
    /// An abstract base data transfer object (DTO) for <see cref="AbstractEventCondition"/>,
    /// providing common properties for JSON serialization.
    /// Concrete condition DTOs will inherit from this or implement <see cref="IEventConditionDto"/>.
    /// </summary>
    public abstract class BaseEventConditionDto : IEventConditionDto
    {
        /// <summary>
        /// Gets or sets the human-readable description of the condition.
        /// </summary>
        [JsonProperty] public string Description { get; set; }

        /// <summary>
        /// Converts this DTO into its corresponding <see cref="AbstractEventCondition"/> instance.
        /// </summary>
        /// <returns>The <see cref="AbstractEventCondition"/> instance.</returns>
        public abstract AbstractEventCondition ToCondition();
    }
}