namespace UGESystem
{
    /// <summary>
    /// Defines the runtime status (NotStarted, InProgress, Completed) of a storyboard node,
    /// used for tracking and visualization.
    /// </summary>
    public enum EventStatus
    {
        /// <summary>The event node has not yet started.</summary>
        NotStarted,
        /// <summary>The event node is currently executing.</summary>
        InProgress,
        /// <summary>The event node has finished execution.</summary>
        Completed
    }
}