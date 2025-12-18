using System.Collections;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// Command handler that executes a <see cref="ChoiceCommand"/> by displaying choices
    /// via the <see cref="UGEUIManager"/> and waiting for the player's selection.
    /// </summary>
    public class DialogueNode_ChoiceCommandHandler : ICommandHandler
    {
        /// <summary>
        /// Executes the <see cref="ChoiceCommand"/>. It displays the choices to the player
        /// using the <see cref="UGEUIManager"/> and pauses execution until the player makes a choice.
        /// </summary>
        /// <param name="genericCommand">The command to execute, expected to be a <see cref="ChoiceCommand"/>.</param>
        /// <param name="controller">The <see cref="UGEGameEventController"/> managing the current game event flow.</param>
        /// <returns>An IEnumerator for coroutine execution.</returns>
        public IEnumerator Execute(IGameEventCommand genericCommand, UGEGameEventController controller)
        {
            ChoiceCommand command = (ChoiceCommand)genericCommand; // Explicit cast

            controller.IsWaitingForChoice = true;
            controller.UIManager.ShowChoices(command.Choices, controller.OnChoiceSelected);
            
            // 사용자의 선택이 있을 때까지 대기
            // Wait until the user makes a choice
            // GameEventController.OnChoiceSelected가 호출되면 controller.IsWaitingForChoice가 false가 됨
            // When GameEventController.OnChoiceSelected is called, controller.IsWaitingForChoice becomes false
            yield return new WaitUntil(() => !controller.IsWaitingForChoice);
        }
    }
}
