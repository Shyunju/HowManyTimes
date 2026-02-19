using System.Collections;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// Command handler that executes a <see cref="CharacterUpdateCommand"/> by updating character data
    /// in the runtime database and performing a visual hot-swap if necessary.
    /// </summary>
    public class CharacterUpdateCommandHandler : ICommandHandler
    {
        /// <summary>
        /// Executes the <see cref="CharacterUpdateCommand"/>. It delegates the data update and
        /// hot-swap logic to the <see cref="UGECharacterManager"/>.
        /// </summary>
        /// <param name="genericCommand">The command to execute, expected to be a <see cref="CharacterUpdateCommand"/>.</param>
        /// <param name="controller">The <see cref="UGEGameEventController"/> managing the current game event flow.</param>
        /// <returns>An IEnumerator for coroutine execution.</returns>
        public IEnumerator Execute(IGameEventCommand genericCommand, UGEGameEventController controller)
        {
            CharacterUpdateCommand command = (CharacterUpdateCommand)genericCommand;

            if (controller.CharacterManager != null)
            {
                controller.CharacterManager.UpdateCharacterData(command);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning("[CharacterUpdateCommandHandler] UGECharacterManager is not assigned to the controller.");
#endif
            }

            // This command completes instantly.
            yield break;
        }
    }
}
