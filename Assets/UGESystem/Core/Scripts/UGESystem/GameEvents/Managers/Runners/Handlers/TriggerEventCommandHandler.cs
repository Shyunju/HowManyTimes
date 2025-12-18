using System.Collections;
using System.Linq; // For FirstOrDefault
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// Command handler that executes a <see cref="TriggerEventCommand"/> by finding a <see cref="UGEEventTaskRunner"/> in the scene
    /// by its ID and initiating its storyboard.
    /// <br/>
    /// ID로 씬에서 <see cref="UGEEventTaskRunner"/>를 찾아 해당 스토리보드를 시작함으로써 <see cref="TriggerEventCommand"/>를 실행하는 커맨드 핸들러입니다.
    /// </summary>
    public class TriggerEventCommandHandler : ICommandHandler
    {
        /// <summary>
        /// Gets the type of command that this handler can process.
        /// 이 핸들러가 처리할 수 있는 커맨드의 타입을 가져옵니다.
        /// </summary>
        public CommandType CommandType => CommandType.TriggerEvent;

        /// <summary>
        /// Executes the <see cref="TriggerEventCommand"/>. It finds the target <see cref="UGEEventTaskRunner"/>
        /// by its ID and requests to start its storyboard's initial node.
        /// <br/>
        /// <see cref="TriggerEventCommand"/>를 실행합니다. 대상 <see cref="UGEEventTaskRunner"/>를 ID로 찾아
        /// 해당 스토리보드의 시작 노드를 실행하도록 요청합니다.
        /// </summary>
        /// <param name="genericCommand">The command to execute, expected to be a <see cref="TriggerEventCommand"/>.</param>
        /// <param name="controller">The <see cref="UGEGameEventController"/> managing the current game event flow.</param>
        /// <returns>An IEnumerator for coroutine execution.</returns>
        public IEnumerator Execute(IGameEventCommand genericCommand, UGEGameEventController controller)
        {
            var command = genericCommand as TriggerEventCommand;
            if (command != null && !string.IsNullOrEmpty(command.TargetRunnerId))
            {
                // UGESystemController를 통해 ID로 러너를 찾습니다.
                var targetRunner = UGESystemController.Instance.GetRunnerById(command.TargetRunnerId);

                if (targetRunner != null)
                {
                    // 해당 러너의 시작 노드를 찾아 실행 요청합니다.
                    // Runner는 이미 Start()에서 InitializeStoryboard()를 호출했으므로,
                    // 조건을 구독하고 있는 상태입니다.
                    var startNode = targetRunner.Storyboard?.EventNodes.FirstOrDefault(n => n.IsStartNode);
                    if (startNode != null)
                    {
                        targetRunner.TryStartNode(startNode);
                    }
                    else
                    {
#if UNITY_EDITOR
                        Debug.LogWarning($"TriggerEventCommand: Runner with ID '{command.TargetRunnerId}' was found, but its storyboard '{targetRunner.Storyboard.name}' has no Start Node.");
#endif
                    }
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogError($"TriggerEventCommand: Could not find an active UGEEventTaskRunner with ID '{command.TargetRunnerId}' in the scene. Please ensure the runner exists and is active.");
#endif
                }
            }
            yield break;
        }
    }
}