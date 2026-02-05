using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// Command handler that executes a <see cref="RewardCommand"/> by granting the specified rewards
    /// via the active <see cref="UGEEventTaskRunner"/>.
    /// </summary>
    public class RewardCommandHandler : ICommandHandler
    {
        /// <summary>
        /// Executes the <see cref="RewardCommand"/>. It finds the active runner for the current storyboard
        /// and delegates the reward granting logic to it.
        /// </summary>
        /// <param name="genericCommand">The command to execute, expected to be a <see cref="RewardCommand"/>.</param>
        /// <param name="controller">The <see cref="UGEGameEventController"/> managing the current game event flow.</param>
        /// <returns>An IEnumerator for coroutine execution.</returns>
        public IEnumerator Execute(IGameEventCommand genericCommand, UGEGameEventController controller)
        {
            RewardCommand command = (RewardCommand)genericCommand;

            // Find the active runner for the current storyboard context
            // 현재 스토리보드 컨텍스트에 맞는 활성 러너를 찾습니다.
            // UGEGameEventController doesn't explicitly hold the runner ref, but it holds the storyboard.
            // UGEGameEventController는 러너 참조를 명시적으로 가지고 있지 않지만, 스토리보드는 가지고 있습니다.
            
            var runner = UGESystemController.Instance.GetRunnerForStoryboard(controller.CurrentStoryboard);
            
            if (runner != null)
            {
                runner.GrantRewards(command.Rewards);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning("[RewardCommandHandler] Could not find active UGEEventTaskRunner for current storyboard. Rewards not granted.");
#endif
            }
            
            yield break;
        }
    }
}
