using Newtonsoft.Json;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A marker interface for all command DTOs (Data Transfer Objects),
    /// ensuring they can be converted to <see cref="EventCommand"/> instances.
    /// </summary>
    public interface IEventCommandDto 
    {
        /// <summary>
        /// Converts the DTO into its corresponding <see cref="EventCommand"/> instance.
        /// </summary>
        /// <returns>The <see cref="EventCommand"/> instance.</returns>
        EventCommand ToCommand();
    }

    /// <summary>
    /// An abstract base class for all command types,
    /// defining <see cref="CommandType"/> and providing a shared <c>_isNodeExpanded</c> property for editor UI state.
    /// </summary>
    [System.Serializable]
    public abstract class EventCommand : IGameEventCommand
    {
        [field: SerializeField]
        /// <summary>
        /// Gets the type of this event command.
        /// </summary>
        public CommandType CommandType { get; protected set; }

        [SerializeField, JsonIgnore] // This is editor-only state, should not be in JSON.
        private bool _isNodeExpanded = false;
        /// <summary>
        /// Gets or sets a value indicating whether the command node is expanded in the editor UI.
        /// This is an editor-only state.
        /// </summary>
        public bool IsNodeExpanded
        {
            get { return _isNodeExpanded; }
            set { _isNodeExpanded = value; }
        }

        /// <summary>
        /// Converts this <see cref="EventCommand"/> instance into its corresponding DTO for serialization.
        /// </summary>
        /// <returns>The DTO representation of this command.</returns>
        public abstract IEventCommandDto ToDto();
    }
}