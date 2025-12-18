using System.Collections;
using UnityEngine; // for Debug and WaitForSeconds

namespace UGESystem
{
    /// <summary>
    /// Command handler that processes a <see cref="DialogueCommand"/> in a cinematic style,
    /// displaying text and characters for a set duration, with the ability to be skipped.
    /// </summary>
    public class CinematicNode_DialogueCommandHandler : ICommandHandler
    {
        /// <summary>
        /// Executes the <see cref="DialogueCommand"/> in a cinematic fashion. It handles character display,
        /// requests the <see cref="UGEUIManager"/> to show cinematic text, waits for a specified duration,
        /// and allows for early skipping by the player.
        /// </summary>
        /// <param name="genericCommand">The command to execute, expected to be a <see cref="DialogueCommand"/>.</param>
        /// <param name="controller">The <see cref="UGEGameEventController"/> managing the current game event flow.</param>
        /// <returns>An IEnumerator for coroutine execution.</returns>
        public IEnumerator Execute(IGameEventCommand genericCommand, UGEGameEventController controller)
        {
            DialogueCommand command = (DialogueCommand)genericCommand; // Explicit cast

            // 1. 캐릭터 관련 로직을 CharacterManager의 헬퍼 메서드로 위임
            controller.CharacterManager.ShowCharacterForDialogue(command);

            // 2. UIManager에 Cinematic Text 출력을 요청하고 애니메이션이 끝날 때까지 대기
            // 2. Request UIManager to display Cinematic Text and wait until animation ends
            yield return controller.UIManager.ShowCinematicText(command.DialogueText, command.CinematicAnimDuration);
            
            // 3. 설정된 시간만큼 대기하되, 스킵 신호가 오면 즉시 중단
            // 3. Wait for the set time, but stop immediately if a skip signal is received
            float timer = 0f;
            while (timer < command.CinematicDisplayDuration)
            {
                if (controller.IsSkipActive)
                {
                    break; // 스킵이 활성화되면 대기 루프를 탈출
                }
                timer += Time.deltaTime;
                yield return null;
            }

            // 4. 텍스트 숨기기
            // 4. Hide text
            controller.UIManager.HideCinematicText();
            
            // Cinematic Text는 자동 진행하므로 GameEventController.ContinueEvent는 호출하지 않음.
            // Cinematic Text proceeds automatically, so GameEventController.ContinueEvent is not called.
            // GameEventController의 ProcessEventCoroutine이 다음 커맨드로 자동 진행할 것임.
            // GameEventController's ProcessEventCoroutine will automatically proceed to the next command.
        }
    }
}
