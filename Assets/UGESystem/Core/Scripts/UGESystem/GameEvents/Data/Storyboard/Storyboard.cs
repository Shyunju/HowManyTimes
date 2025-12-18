using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace UGESystem
{
    /// <summary>
    /// A ScriptableObject that contains a network of event nodes and their connections to create complex narrative sequences.
    /// </summary>
    //[CreateAssetMenu(fileName = "NewStoryboard", menuName = "UGESystem/Storyboard")]
    public class Storyboard : ScriptableObject
    {
        /// <summary>
        /// Represents a connection between two nodes in a storyboard for JSON serialization.
        /// </summary>
        public class ConnectionData
        {
            /// <summary>
            /// Gets or sets the GUID of the output node.
            /// </summary>
            [JsonProperty] public string OutputNodeGuid { get; set; }
            /// <summary>
            /// Gets or sets the GUID of the input node.
            /// </summary>
            [JsonProperty] public string InputNodeGuid { get; set; }
        }

        /// <summary>
        /// A data transfer object (DTO) for <see cref="Storyboard"/>, used for JSON serialization and deserialization.
        /// </summary>
        public class StoryboardDto
        {
            /// <summary>
            /// Gets or sets the name of the storyboard.
            /// </summary>
            [JsonProperty] public string Name { get; set; }
            /// <summary>
            /// Gets or sets the list of <see cref="EventNodeDataDto"/> representing the nodes in the storyboard.
            /// </summary>
            [JsonProperty] public List<EventNodeDataDto> Nodes { get; set; }
            /// <summary>
            /// Gets or sets the list of <see cref="ConnectionData"/> representing the connections between nodes.
            /// </summary>
            [JsonProperty] public List<ConnectionData> Connections { get; set; }
        }

        /// <summary>
        /// The user-defined name of the storyboard.
        /// </summary>
        [field: SerializeField]
        public string Name { get; private set; }

        [SerializeReference]
        [SerializeField]
        private List<EventNodeData> _eventNodes = new List<EventNodeData>();
        
        /// <summary>
        /// The list of all event nodes contained within this storyboard.
        /// </summary>
        public List<EventNodeData> EventNodes { get { return _eventNodes; } }

        /// <summary>
        /// Serializes the entire storyboard, including its nodes and connections, into a JSON string.
        /// </summary>
        /// <returns>A JSON string representing the storyboard.</returns>
        public string ToJson()
        {
            var dto = new StoryboardDto
            {
                Name = this.Name,
                Nodes = _eventNodes.Select(node => node.ToDto()).ToList(),
                Connections = new List<ConnectionData>()
            };

            foreach (var node in _eventNodes)
            {
                foreach (string nextNodeID in node.NextNodeIDs)
                {
                    dto.Connections.Add(new ConnectionData
                    {
                        OutputNodeGuid = node.NodeID,
                        InputNodeGuid = nextNodeID
                    });
                }
            }

            return JsonConvert.SerializeObject(dto, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter> 
                { 
                    new Newtonsoft.Json.Converters.StringEnumConverter(),
                    new Vector2Converter(),
                    new Vector3Converter(),
                    new ColorConverter()
                }
            });
        }

        /// <summary>
        /// Deserializes a JSON string to populate the storyboard's data, reconstructing nodes and connections.
        /// </summary>
        /// <param name="json">The JSON string representing the storyboard.</param>
        /// <param name="gameEventAssetMap">A dictionary mapping <see cref="GameEvent"/> GUIDs to their asset references, used to link nodes to their events.</param>
        public void FromJson(string json, Dictionary<string, GameEvent> gameEventAssetMap)
        {
            var dto = JsonConvert.DeserializeObject<StoryboardDto>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = new List<JsonConverter>
                {
                    new Vector2Converter(),
                    new Vector3Converter(),
                    new ColorConverter()
                }
            });

            if (dto == null)
            {
#if UNITY_EDITOR
                Debug.LogError("[Storyboard.FromJson] Failed to deserialize StoryboardDto.");
#endif
                return;
            }

            this.Name = dto.Name;
            _eventNodes.Clear();
            var nodeDtoMap = dto.Nodes.ToDictionary(n => n.NodeID, n => n);

            // 1. Create all nodes
            foreach (var nodeDto in dto.Nodes)
            {
                if (!gameEventAssetMap.TryGetValue(nodeDto.GameEventGuid, out var gameEventAsset))
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"[Storyboard.FromJson] Could not find GameEvent asset with GUID: {nodeDto.GameEventGuid} for node '{nodeDto.Name}'. This node will be skipped.");
#endif
                    continue;
                }

                // Use the DTO constructor and then assign the looked-up asset
                var newNode = new EventNodeData(nodeDto)
                {
                    GameEventAsset = gameEventAsset
                };
                
                _eventNodes.Add(newNode);
            }
            
            // 2. Link connections
            var eventNodeMap = _eventNodes.ToDictionary(n => n.NodeID, n => n);
            foreach (var connection in dto.Connections)
            {
                if (eventNodeMap.TryGetValue(connection.OutputNodeGuid, out var outputNode) &&
                    eventNodeMap.TryGetValue(connection.InputNodeGuid, out var inputNode))
                {
                    if (!outputNode.NextNodeIDs.Contains(inputNode.NodeID))
                    {
                        outputNode.NextNodeIDs.Add(inputNode.NodeID);
                    }
                }
            }
        }
    }
}