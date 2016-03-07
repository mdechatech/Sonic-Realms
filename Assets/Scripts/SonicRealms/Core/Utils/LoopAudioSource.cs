using UnityEngine;

namespace SonicRealms.Core.Utils
{
    [RequireComponent(typeof(AudioSource))]
    public class LoopAudioSource : MonoBehaviour
    {
        private AudioSource _audioSource;

        /// <summary>
        /// Where the loop starts, in seconds.
        /// </summary>
        public float LoopStart;

        /// <summary>
        /// When the audio reaches this point, in seconds, it loops back to LoopStart.
        /// </summary>
        public float LoopEnd;


        public void Reset()
        {
            _audioSource = GetComponent<AudioSource>();
            LoopStart = 2f;
            LoopEnd = 5f;
        }

        public void Awake()
        {
            _audioSource = _audioSource ? _audioSource : GetComponent<AudioSource>();
            if (LoopStart < 0f) LoopStart = 0f;
            if (LoopEnd <= 0f) LoopEnd = 999f;
        }

        public void FixedUpdate()
        {
            if (_audioSource.clip == null || !_audioSource.isPlaying) return;
            if (_audioSource.time >= LoopEnd)
                _audioSource.time = LoopStart + (_audioSource.time - LoopEnd);
        }

        public void SetFrom(BGMLoopData data)
        {
            _audioSource.clip = data.Clip;
            LoopStart = data.LoopStart;
            LoopEnd = data.LoopEnd;

            if (LoopEnd <= 0f) LoopEnd = 999f;
            LoopStart = Mathf.Clamp(LoopStart, 0f, LoopEnd);
        }

        public void ResetLoopPoints()
        {
            LoopEnd = 999f;
            LoopStart = 0f;
        }
    }
}
