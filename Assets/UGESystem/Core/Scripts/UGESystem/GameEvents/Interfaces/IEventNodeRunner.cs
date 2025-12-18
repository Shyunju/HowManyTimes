using System.Collections;
using System.Collections.Generic;

namespace UGESystem
{
    /// <summary>
    /// Represents the result of an event node execution, indicating success and any rewards granted.
    /// </summary>
    public class NodeRunResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the node execution was successful.
        /// </summary>
        public bool Success { get; set; } = true;
        /// <summary>
        /// Gets or sets a list of rewards granted during the node execution.
        /// </summary>
        public List<AbstractEventReward> Rewards { get; set; } = new List<AbstractEventReward>();
        // 추후, 분기 결과 등 다른 데이터를 추가할 수 있음
        // Later, other data such as branch results can be added
    }

    /// <summary>
    /// Interface that all node runners must implement to define how an event node is executed.
    /// </summary>
    public interface IEventNodeRunner
    {
        /// <summary>
        /// Responsible for executing the logic of an event node.
        /// Calls the <paramref name="onComplete"/> callback when execution is finished.
        /// </summary>
        /// <param name="node">The <see cref="EventNodeData"/> to be run.</param>
        /// <param name="runner">The <see cref="UGEEventTaskRunner"/> managing the node's execution.</param>
        /// <param name="onComplete">A callback action to be invoked upon completion, providing the <see cref="NodeRunResult"/>.</param>
        /// <returns>An <see cref="IEnumerator"/> to support coroutine execution.</returns>
        IEnumerator Run(EventNodeData node, UGEEventTaskRunner runner, System.Action<NodeRunResult> onComplete);
    }
}
