using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// Manager that controls audio playback by providing separate <see cref="AudioSource"/> channels for Background Music (BGM) and Sound Effects (SFX).
    /// <br/>
    /// 배경 음악(BGM)과 음향 효과(SFX)를 위한 별도의 <see cref="AudioSource"/> 채널을 제공하여 오디오 재생을 제어하는 관리자입니다.
    /// </summary>
    public class UGESoundManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource _bgmAudioSource;
        [SerializeField] private AudioSource _sfxAudioSource;

        /// <summary>
        /// 배경음악(BGM)을 재생합니다. 이미 재생 중인 BGM은 중단됩니다.
        /// </summary>
        /// <param name="clip">재생할 오디오 클립</param>
        /// <param name="loop">반복 재생 여부</param>
        /// <param name="volume">볼륨</param>
        public void PlayBGM(AudioClip clip, bool loop, float volume)
        {
            if (_bgmAudioSource == null || clip == null) return;

            _bgmAudioSource.Stop();
            _bgmAudioSource.clip = clip;
            _bgmAudioSource.loop = loop;
            _bgmAudioSource.volume = volume;
            _bgmAudioSource.Play();
        }

        /// <summary>
        /// 현재 재생 중인 배경음악(BGM)을 중단합니다.
        /// </summary>
        public void StopBGM()
        {
            if (_bgmAudioSource == null) return;
            _bgmAudioSource.Stop();
        }

        /// <summary>
        /// 효과음(SFX)을 한 번 재생합니다. 기존에 재생 중인 효과음과 겹쳐서 재생됩니다.
        /// </summary>
        /// <param name="clip">재생할 오디오 클립</param>
        /// <param name="volume">볼륨</param>
        public void PlaySFX(AudioClip clip, float volume)
        {
            if (_sfxAudioSource == null || clip == null) return;
            _sfxAudioSource.PlayOneShot(clip, volume);
        }
    }
}
