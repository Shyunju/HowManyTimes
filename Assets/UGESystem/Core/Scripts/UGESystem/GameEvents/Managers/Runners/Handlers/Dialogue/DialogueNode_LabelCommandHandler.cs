using System.Collections;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A blank command handler for <see cref="LabelCommand"/> which is only used as a jump target
    /// and has no execution logic or waiting time of its own.
    /// </summary>
    public class DialogueNode_LabelCommandHandler : ICommandHandler
    {
        /// <summary>
        /// Executes the <see cref="LabelCommand"/>. As <see cref="LabelCommand"/> has no execution logic or waiting time of its own,
        /// this method simply proceeds immediately.
        /// </summary>
        /// <param name="genericCommand">The command to execute, expected to be a <see cref="LabelCommand"/>.</param>
        /// <param name="controller">The <see cref="UGEGameEventController"/> managing the current game event flow.</param>
        /// <returns>An IEnumerator for coroutine execution.</returns>
        public IEnumerator Execute(IGameEventCommand genericCommand, UGEGameEventController controller)
        {
            LabelCommand command = (LabelCommand)genericCommand; // Explicit cast

            // LabelCommand는 자체적인 실행 로직이나 대기 시간이 없으므로 바로 진행.
            // LabelCommand has no execution logic or waiting time of its own, so it proceeds immediately.
            yield break;
        }
    }
}
