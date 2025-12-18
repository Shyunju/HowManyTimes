using System.Collections;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// Command handler that executes a <see cref="BackgroundCommand"/> by showing or hiding backgrounds (image/video)
    /// via the <see cref="UGEUIManager"/>.
    /// </summary>
    public class DialogueNode_BackgroundCommandHandler : ICommandHandler
    {
        /// <summary>
        /// Executes the <see cref="BackgroundCommand"/>. It controls the display of background images or videos
        /// through the <see cref="UGEUIManager"/> and proceeds without waiting.
        /// </summary>
        /// <param name="genericCommand">The command to execute, expected to be a <see cref="BackgroundCommand"/>.</param>
        /// <param name="controller">The <see cref="UGEGameEventController"/> managing the current game event flow.</param>
        /// <returns>An IEnumerator for coroutine execution.</returns>
        public IEnumerator Execute(IGameEventCommand genericCommand, UGEGameEventController controller)
        {
            BackgroundCommand command = (BackgroundCommand)genericCommand; // Explicit cast

            switch (command.Action)
            {
                case BackgroundAction.Show:
                    if (command.Type == BackgroundType.Image)
                    {
                        if (command.Image != null)
                            controller.UIManager.ShowImageBackground(command.Image);
                    }
                    else if (command.Type == BackgroundType.Video)
                    {
                        if (command.Video != null)
                            controller.UIManager.PlayVideoBackground(command.Video);
                    }
                    break;
                case BackgroundAction.Hide:
                    controller.UIManager.HideBackground();
                    break;
            }
            
            // BackgroundCommand는 대기할 필요 없이 바로 다음으로 진행
            // BackgroundCommand proceeds directly without waiting
            yield break;
        }
    }
}
