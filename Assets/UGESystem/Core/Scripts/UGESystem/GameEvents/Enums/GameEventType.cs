namespace UGESystem
{
    /// <summary>
    /// Defines the type of <see cref="EventNodeData"/> (e.g., Dialogue, CinematicText),
    /// and an enumeration used by the event runner to select the appropriate command processing strategy.
    /// </summary>
    public enum GameEventType
    {
        /// <summary>A standard dialogue event type.</summary>
        Dialogue, 
        /// <summary>A cinematic text event type.</summary>
        CinematicText
    }
}