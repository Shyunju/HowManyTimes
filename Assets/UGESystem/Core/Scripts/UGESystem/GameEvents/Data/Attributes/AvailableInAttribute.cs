using System;

namespace UGESystem
{
    /// <summary>
    /// A custom attribute for marking <see cref="EventCommand"/> classes,
    /// specifying the <see cref="GameEventArchetype"/>s (e.g., Dialogue, Cinematic) where the command can be used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class AvailableIn : Attribute
    {
        /// <summary>
        /// Gets the array of <see cref="GameEventType"/> where the associated command is supported.
        /// </summary>
        public GameEventType[] SupportedTypes { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AvailableIn"/> attribute with the specified supported <see cref="GameEventType"/>s.
        /// </summary>
        /// <param name="supportedTypes">An array of <see cref="GameEventType"/> values indicating where the command is available.</param>
        public AvailableIn(params GameEventType[] supportedTypes)
        {
            SupportedTypes = supportedTypes;
        }
    }
}