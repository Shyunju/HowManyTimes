namespace UGESystem
{
    /// <summary>
    /// An event struct published to the <see cref="UGEDelayedEventBus"/> when a <see cref="UGEEventTaskRunner"/>
    /// starts the execution of a storyboard node, passing the ID of the started node.
    /// </summary>
    public struct NodeStartedEvent : IGameBusEvent
    {
        /// <summary>
        /// Gets the unique ID of the <see cref="EventNodeData"/> that has started execution.
        /// </summary>
        public string NodeID { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeStartedEvent"/> struct.
        /// </summary>
        /// <param name="nodeID">The unique ID of the started <see cref="EventNodeData"/>.</param>
        public NodeStartedEvent(string nodeID)
        {
            NodeID = nodeID;
        }
    }
}