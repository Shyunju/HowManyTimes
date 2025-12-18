using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

namespace UGESystem
{
    /// <summary>
    /// Command handler for <see cref="UGECameraCommand"/> that interacts with <see cref="UGECameraManager"/>
    /// to control Cinemachine cameras by performing actions like switching, zooming, shaking, or resetting.
    /// </summary>
    public class UGECameraCommandHandler : ICommandHandler
    {
        /// <summary>
        /// Executes the <see cref="UGECameraCommand"/>. It delegates camera control operations
        /// to the <see cref="UGECameraManager"/> based on the command's action type.
        /// </summary>
        /// <param name="genericCommand">The command to execute, expected to be a <see cref="UGECameraCommand"/>.</param>
        /// <param name="controller">The <see cref="UGEGameEventController"/> managing the current game event flow.</param>
        /// <returns>An IEnumerator for coroutine execution.</returns>
        public IEnumerator Execute(IGameEventCommand genericCommand, UGEGameEventController controller)
        {
            UGECameraCommand command = (UGECameraCommand)genericCommand;

            switch (command.ActionType)
            {
                case UGECameraActionType.SwitchTo:
                    yield return controller.CameraManager.SwitchTo(command.TargetCameraName, command.Duration);
                    break;
                case UGECameraActionType.Zoom:
                    yield return controller.CameraManager.Zoom(command.TargetCameraName, command.TargetFOV, command.Duration);
                    break;
                case UGECameraActionType.Shake:
                    controller.CameraManager.Shake(command.ShakeIntensity);
                    break; 
                case UGECameraActionType.Reset:
                    controller.CameraManager.ResetCamera();
                    break;
                default:
#if UNITY_EDITOR
                    Debug.LogWarning($"[UGECameraCommandHandler] Unknown ActionType: {command.ActionType}");
#endif
                    break;
            }
        }
    }
}