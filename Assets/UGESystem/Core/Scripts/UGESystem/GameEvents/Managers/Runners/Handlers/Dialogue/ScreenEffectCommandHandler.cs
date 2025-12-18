using System.Collections;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// Command handler for <see cref="ScreenEffectCommand"/> that interacts with <see cref="UGEScreenEffectManager"/>
    /// to execute full-screen visual effects such as fades, flashes, and tints.
    /// </summary>
    public class ScreenEffectCommandHandler : ICommandHandler
    {
        /// <summary>
        /// Executes the <see cref="ScreenEffectCommand"/>. It delegates the screen effect operations
        /// to the <see cref="UGESystemController.Instance.ScreenEffectManager"/> based on the command's effect type.
        /// </summary>
        /// <param name="genericCommand">The command to execute, expected to be a <see cref="ScreenEffectCommand"/>.</param>
        /// <param name="controller">The <see cref="UGEGameEventController"/> managing the current game event flow.</param>
        /// <returns>An IEnumerator for coroutine execution.</returns>
        public IEnumerator Execute(IGameEventCommand genericCommand, UGEGameEventController controller)
        {
            var command = (ScreenEffectCommand)genericCommand;
            var screenEffectManager = UGESystemController.Instance.ScreenEffectManager;

            if (screenEffectManager == null)
            {
#if UNITY_EDITOR
                Debug.LogError("[ScreenEffectCommandHandler] UGEScreenEffectManager is not available on UGESystemController.");
#endif
                yield break;
            }

            switch (command.EffectType)
            {
                case ScreenEffectType.FadeOut:
                    // Fade from current (likely transparent) to TargetColor
                    yield return screenEffectManager.Fade(command.TargetColor, command.Duration);
                    break;

                case ScreenEffectType.FadeIn:
                    yield return screenEffectManager.FadeIn(command.TargetColor, command.Duration);
                    break;
                
                case ScreenEffectType.Flash:
                    float fadeTime = command.Duration - command.FlashHoldDuration;
                    if (fadeTime < 0) fadeTime = 0;
                    float fadeInTime = fadeTime * 0.2f; // 20% of remaining time for fade-in
                    float fadeOutTime = fadeTime * 0.8f; // 80% of remaining time for fade-out
                    yield return screenEffectManager.Flash(command.TargetColor, fadeInTime, command.FlashHoldDuration, fadeOutTime);
                    break;

                case ScreenEffectType.Tint:
                    // Fade from current to a specific tint color
                    yield return screenEffectManager.Tint(command.TargetColor, command.Duration);
                    break;

                default:
#if UNITY_EDITOR
                    Debug.LogWarning($"[ScreenEffectCommandHandler] Unknown ScreenEffectType: {command.EffectType}");
#endif
                    break;
            }
        }
    }
}