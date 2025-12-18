using System.Collections;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// Command handler for <see cref="PlaySoundCommand"/> that interacts with <see cref="UGESoundManager"/>
    /// to play or stop BGM and SFX.
    /// </summary>
    public class PlaySoundCommandHandler : ICommandHandler
    {
        /// <summary>
        /// Gets the type of command that this handler can process.
        /// </summary>
        public CommandType CommandType => CommandType.PlaySound;

        /// <summary>
        /// Executes the <see cref="PlaySoundCommand"/>. It retrieves the <see cref="UGESoundManager"/>
        /// and performs actions (play/stop) based on the command's parameters for BGM or SFX.
        /// </summary>
        /// <param name="genericCommand">The command to execute, expected to be a <see cref="PlaySoundCommand"/>.</param>
        /// <param name="controller">The <see cref="UGEGameEventController"/> managing the current game event flow.</param>
        /// <returns>An IEnumerator for coroutine execution.</returns>
        public IEnumerator Execute(IGameEventCommand genericCommand, UGEGameEventController controller)
        {
            var playSoundCommand = genericCommand as PlaySoundCommand;
            if (playSoundCommand == null)
            {
                yield break;
            }

            // UGESystemController를 통해 SoundManager 인스턴스에 접근합니다.
            var soundManager = UGESystemController.Instance.SoundManager;
            if (soundManager == null)
            {
#if UNITY_EDITOR
                Debug.LogError("UGESoundManager not found. Please ensure it is part of the UGESystemController hierarchy.");
#endif
                yield break;
            }

            if (playSoundCommand.Action == SoundAction.Play)
            {
                if (playSoundCommand.AudioClip == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning("PlaySoundCommand: AudioClip is null.");
#endif
                    yield break;
                }

                switch (playSoundCommand.SoundType)
                {
                    case SoundType.BGM:
                        soundManager.PlayBGM(playSoundCommand.AudioClip, playSoundCommand.Loop, playSoundCommand.Volume);
                        break;
                    case SoundType.SFX:
                        soundManager.PlaySFX(playSoundCommand.AudioClip, playSoundCommand.Volume);
                        break;
                }
            }
            else // SoundAction.Stop
            {
                switch (playSoundCommand.SoundType)
                {
                    case SoundType.BGM:
                        soundManager.StopBGM();
                        break;
                    case SoundType.SFX:
                        // SFX는 일반적으로 개별적으로 중지시키기보다는,
                        // 전체 오디오를 중지시키는 등의 다른 방법으로 관리됩니다.
                        // 여기서는 Stop 액션을 BGM에만 적용하도록 제한합니다.
                        break;
                }
            }

            yield break;
        }
    }
}