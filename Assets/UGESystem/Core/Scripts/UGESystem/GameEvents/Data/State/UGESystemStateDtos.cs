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
        [JsonProperty]
        [field: SerializeField] 
        public string NodeID { get; set; }

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
        [JsonProperty]
        [field: SerializeField] 
        public string RunnerID { get; set; }

        [JsonProperty]
        [field: SerializeField] 
        public string StoryboardName { get; set; }

        [JsonProperty]
        [field: SerializeField] 
        public List<NodeStateDto> NodeStates { get; set; } = new List<NodeStateDto>();
    }

    /// <summary>
    /// Data Transfer Object representing the modified state of a single character in the runtime database.
    /// </summary>
    [Serializable]
    public class CharacterStateDto
    {
        [JsonProperty]
        public string CharacterID { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        
        // 외형 복원을 위해 원본 에셋 중 어떤 캐릭터의 프리팹/표정 데이터를 사용 중인지 기록합니다.
        // Records which original character's prefab/expression data is being used for visual restoration.
        [JsonProperty]
        public string SourceTemplateID { get; set; }
    }

    /// <summary>
    /// The root Data Transfer Object representing the entire system state, including story progress and character data.
    /// </summary>
    [Serializable]
    public class UGESystemStateDto
    {
        [JsonProperty]
        public List<RunnerStateDto> RunnerStates { get; set; } = new List<RunnerStateDto>();
        [JsonProperty]
        public List<CharacterStateDto> CharacterStates { get; set; } = new List<CharacterStateDto>();
    }
}
