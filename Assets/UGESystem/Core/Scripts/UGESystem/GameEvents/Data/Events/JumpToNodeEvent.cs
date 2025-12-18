namespace UGESystem
{
    /// <summary>
    /// An event struct published to the <see cref="UGEDelayedEventBus"/>,
    /// instructing a <see cref="UGEEventTaskRunner"/> to immediately jump to a specific node within a storyboard,
    /// and used by <see cref="EndCommand"/> for branching.
    /// </summary>
    public struct JumpToNodeEvent : IGameBusEvent
    {
        /// <summary>
        /// Gets the <see cref="Storyboard"/> to jump to.
        /// </summary>
        public Storyboard TargetStoryboard { get; private set; }
        /// <summary>
        /// Gets the ID of the target node within the storyboard to jump to.
        /// </summary>
        public string TargetNodeID { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JumpToNodeEvent"/> struct.
        /// </summary>
        /// <param name="targetStoryboard">The <see cref="Storyboard"/> to jump to.</param>
        /// <param name="targetNodeID">The ID of the target node within the storyboard.</param>
        public JumpToNodeEvent(Storyboard targetStoryboard, string targetNodeID)
        {
            TargetStoryboard = targetStoryboard;
            TargetNodeID = targetNodeID;
        }
    }
}