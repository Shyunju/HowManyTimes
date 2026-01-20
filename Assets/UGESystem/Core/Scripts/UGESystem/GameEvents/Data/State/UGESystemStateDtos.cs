using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace UGESystem
{
    /// <summary>
    /// Data Transfer Object representing the state of a single storyboard node.
    /// </summary>
    [Serializable]
    public class NodeStateDto
    {
        /// <summary>
        /// The unique ID of the node.
        /// </summary>
        [JsonProperty]
        [field: SerializeField] 
        public string NodeID { get; set; }

        /// <summary>
        /// The runtime status of the node.
        /// </summary>
        [JsonProperty]
        [field: SerializeField] 
        public EventStatus Status { get; set; }
    }

    /// <summary>
    /// Data Transfer Object representing the state of all nodes within a single UGEEventTaskRunner.
    /// </summary>
    [Serializable]
    public class RunnerStateDto
    {
        /// <summary>
        /// The unique ID of the runner in the scene.
        /// </summary>
        [JsonProperty]
        [field: SerializeField] 
        public string RunnerID { get; set; }

        /// <summary>
        /// The name of the storyboard currently assigned to the runner.
        /// Used for validation during loading.
        /// </summary>
        [JsonProperty]
        [field: SerializeField] 
        public string StoryboardName { get; set; }

        /// <summary>
        /// A list of states for each node in the storyboard.
        /// </summary>
        [JsonProperty]
        [field: SerializeField] 
        public List<NodeStateDto> NodeStates { get; set; } = new List<NodeStateDto>();
    }
}
