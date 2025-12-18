namespace UGESystem
{
    /// <summary>
    /// A simple event struct published to the <see cref="UGEDelayedEventBus"/> when the player enters an <see cref="Triggers.EventTriggerVolume"/>,
    /// passing the volume's <see cref="TriggerID"/>.
    /// </summary>
    public struct AreaEnteredEvent : IGameBusEvent
    {
        /// <summary>
        /// Gets the unique ID of the <see cref="Triggers.EventTriggerVolume"/> that the player entered.
        /// </summary>
        public string TriggerID { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AreaEnteredEvent"/> struct.
        /// </summary>
        /// <param name="triggerID">The unique ID of the <see cref="Triggers.EventTriggerVolume"/> that was entered.</param>
        public AreaEnteredEvent(string triggerID)
        {
            TriggerID = triggerID;
        }
    }
}