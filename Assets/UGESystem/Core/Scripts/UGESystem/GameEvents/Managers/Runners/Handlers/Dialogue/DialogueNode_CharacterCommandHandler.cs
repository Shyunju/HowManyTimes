using System.Collections;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// Command handler that processes a <see cref="CharacterCommand"/> to control the instantiation,
    /// placement, and animation lifecycle of 2D and 3D characters during events.
    /// </summary>
    public class DialogueNode_CharacterCommandHandler : ICommandHandler
    {
        /// <summary>
        /// Executes the <see cref="CharacterCommand"/>. It delegates the command handling to the
        /// <see cref="UGECharacterManager"/> and proceeds without waiting for character actions to complete.
        /// </summary>
        /// <param name="genericCommand">The command to execute, expected to be a <see cref="CharacterCommand"/>.</param>
        /// <param name="controller">The <see cref="UGEGameEventController"/> managing the current game event flow.</param>
        /// <returns>An IEnumerator for coroutine execution.</returns>
        public IEnumerator Execute(IGameEventCommand genericCommand, UGEGameEventController controller)
        {
            CharacterCommand command = (CharacterCommand)genericCommand; // Explicit cast
            
            controller.CharacterManager.HandleCharacterCommand(command);
            // CharacterCommand는 대기할 필요 없이 바로 다음으로 진행
            // CharacterCommand proceeds directly without waiting
            yield break;
        }
    }
}
