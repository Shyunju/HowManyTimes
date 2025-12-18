namespace UGESystem
{
    /// <summary>
    /// Implemented by EventCommands that need to set default values when created in the GameEventEditor.
    /// </summary>
    public interface IEditorInitializable
    {
        /// <summary>
        /// Called by the GameEventEditor to allow a new command to set its own default values.
        /// </summary>
        void InitializeDefaultValues();
    }
}
