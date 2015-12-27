using UnityEngine;

namespace Hedgehog.Core.Utils
{
    [RequireComponent(typeof(AudioSource))]
    public class LoopAudioSource : MonoBehaviour
    {
        private AudioSource _audioSource;

        /// <summary>
        /// The time to loop back to when LoopTime is reached, in seconds.
        /// </summary>
        [Tooltip("The time to loop back to when LoopTime is reached, in seconds.")]
        public float LoopbackTime;

        /// <summary>
        /// The time at which to loop, in seconds.
        /// </summary>
        [Tooltip("The time at which to loop, in seconds.")]
        public float LoopTime;

        public void Reset()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void Awake()
        {
            _audioSource = _audioSource ? _audioSource : GetComponent<AudioSource>();
            if (LoopTime <= 0f) LoopTime = 9001f;
        }

        public void Update()
        {
            if (_audioSource.clip == null || !_audioSource.isPlaying) return;

            if (_audioSource.time > LoopTime)
                _audioSource.time = LoopbackTime + _audioSource.time - LoopTime;
            else if (_audioSource.time + Time.deltaTime >= _audioSource.clip.length)
                _audioSource.time = LoopbackTime + (Time.deltaTime - (_audioSource.clip.length - _audioSource.time));
        }
    }
}
