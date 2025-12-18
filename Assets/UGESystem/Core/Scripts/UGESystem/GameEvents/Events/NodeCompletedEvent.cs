namespace UGESystem
{
    /// <summary>
    /// An event struct published to the <see cref="UGEDelayedEventBus"/> when a <see cref="UGEEventTaskRunner"/>
    /// completes the execution of a storyboard node, passing the ID of the completed node.
    /// </summary>
    public struct NodeCompletedEvent : IGameBusEvent
    {
        /// <summary>
        /// Gets the unique ID of the <see cref="EventNodeData"/> that has been completed.
        /// </summary>
        public string NodeID { get; private set; } // Renamed for consistency

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeCompletedEvent"/> struct.
        /// </summary>
        /// <param name="nodeID">The unique ID of the completed <see cref="EventNodeData"/>.</param>
        public NodeCompletedEvent(string nodeID) // Renamed parameter
        {
            NodeID = nodeID;
        }
    }
}