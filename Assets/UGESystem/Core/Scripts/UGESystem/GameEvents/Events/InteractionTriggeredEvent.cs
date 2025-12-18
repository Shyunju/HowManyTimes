namespace UGESystem
{
    /// <summary>
    /// An event struct published to the <see cref="UGEDelayedEventBus"/> when the player interacts with an <see cref="Triggers.InteractableObject"/>,
    /// passing the object's <see cref="InteractionID"/>.
    /// </summary>
    public struct InteractionTriggeredEvent : IGameBusEvent
    {
        /// <summary>
        /// Gets the unique ID of the <see cref="Triggers.InteractableObject"/> that was interacted with.
        /// </summary>
        public string InteractionID { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionTriggeredEvent"/> struct.
        /// </summary>
        /// <param name="interactionID">The unique ID of the <see cref="Triggers.InteractableObject"/> that was interacted with.</param>
        public InteractionTriggeredEvent(string interactionID)
        {
            InteractionID = interactionID;
        }
    }
}