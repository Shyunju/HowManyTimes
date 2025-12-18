using System.Collections;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// Command handler that processes an <see cref="EndCommand"/> to signal the conclusion of a <see cref="GameEvent"/>
    /// and optionally trigger a jump to another storyboard node.
    /// </summary>
    public class DialogueNode_EndCommandHandler : ICommandHandler
    {
        /// <summary>
        /// Executes the <see cref="EndCommand"/>. It calls the <see cref="UGEGameEventController.EndEvent"/> method
        /// to conclude the current event flow. No further progress is needed after this command.
        /// </summary>
        /// <param name="genericCommand">The command to execute, expected to be an <see cref="EndCommand"/>.</param>
        /// <param name="controller">The <see cref="UGEGameEventController"/> managing the current game event flow.</param>
        /// <returns>An IEnumerator for coroutine execution.</returns>
        public IEnumerator Execute(IGameEventCommand genericCommand, UGEGameEventController controller)
        {
            EndCommand command = (EndCommand)genericCommand; // Explicit cast

            

            controller.EndEvent(command);

            // EndCommand는 이벤트 종료를 요청하므로, 더 이상 진행할 필요 없음

            // EndCommand requests to end the event, so no further progress is needed

            yield break;

        }    }
}
