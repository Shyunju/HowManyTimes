using System.Collections;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// Command handler that executes a <see cref="GotoCommand"/> by instructing the <see cref="UGEGameEventController"/>
    /// to jump to a specified label within the <see cref="GameEvent"/>.
    /// </summary>
    public class DialogueNode_GotoCommandHandler : ICommandHandler
    {
        /// <summary>
        /// Executes the <see cref="GotoCommand"/>. It calls <see cref="UGEGameEventController.JumpToLabel"/>
        /// to change the execution flow to the command's target label.
        /// </summary>
        /// <param name="genericCommand">The command to execute, expected to be a <see cref="GotoCommand"/>.</param>
        /// <param name="controller">The <see cref="UGEGameEventController"/> managing the current game event flow.</param>
        /// <returns>An IEnumerator for coroutine execution.</returns>
        public IEnumerator Execute(IGameEventCommand genericCommand, UGEGameEventController controller)
        {
            GotoCommand command = (GotoCommand)genericCommand; // Explicit cast

            controller.JumpToLabel(command.TargetLabel);
            // GotoCommand는 점프 후 바로 다음 커맨드로 진행하므로 대기할 필요 없음.
            // GotoCommand proceeds directly to the next command after jumping, so no waiting is required.
            // GameEventController의 JumpToLabel 내부에서 ProcessCommand를 호출하여 다음 커맨드로 이동함.
            // GameEventController's JumpToLabel calls ProcessCommand internally to move to the next command.
            yield break;
        }
    }
}
