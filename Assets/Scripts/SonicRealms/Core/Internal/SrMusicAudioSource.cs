using UnityEngine;
using UnityEngine.Audio;

namespace SonicRealms.Core.Internal
{
    /// <summary>
    /// Audio source that handles looping music. It has the option of leading in with an intro and
    /// then playing a looping audio clip afterwards.
    /// </summary>
    public class SrMusicAudioSource : MonoBehaviour
    {
        public bool IsPlaying { get; private set; }

        public AudioMixerGroup MixerGroup
        {
            get { return _loopAudioSource.outputAudioMixerGroup; }
            set
            {
                _introAudioSource.outputAudioMixerGroup = value;
                _loopAudioSource.outputAudioMixerGroup = value;
            }
        }

        public AudioClip IntroClip { get { return _introAudioSource.clip; } }

        public AudioClip LoopClip { get { return _loopAudioSource.clip; } }

        public AudioSource IntroSource { get { return _introAudioSource; } }

        public AudioSource LoopSource { get { return _loopAudioSource; } }

        public bool HasClip { get { return IntroClip || LoopClip; } }

        [SerializeField, HideInInspector]
        private AudioSource _introSourcePrefab;

        [SerializeField, HideInInspector]
        private AudioSource _loopSourcePrefab;
        
        private AudioSource _introAudioSource;
        
        private AudioSource _loopAudioSource;

        protected void Awake()
        {
            CreateAudioSources();
        }

        public void Prepare(AudioClip loopClip)
        {
            Stop();

            _introAudioSource.clip = null;
            _introAudioSource.time = 0;

            _loopAudioSource.clip = loopClip;
            _loopAudioSource.time = 0;
        }

        public void Prepare(AudioClip introClip, AudioClip loopClip)
        {
            Stop();

            _introAudioSource.clip = introClip;
            _introAudioSource.time = 0;

            _loopAudioSource.clip = loopClip;
            _loopAudioSource.time = 0;
        }

        public void Play(AudioClip loopClip)
        {
            Prepare(loopClip);
            Play();
        }

        public void Play(AudioClip introClip, AudioClip loopClip)
        {
            Prepare(introClip, loopClip);
            Play();
        }

        public void Play()
        {
            if (IsPlaying)
                return;

            IsPlaying = true;

            if (!_introAudioSource.clip || (_introAudioSource.time == 0 && _loopAudioSource.time != 0))
            {
                _loopAudioSource.Play();
            }
            else if (_introAudioSource.clip && _loopAudioSource.clip)
            {
                _introAudioSource.Play();

                _loopAudioSource.PlayScheduled(AudioSettings.dspTime +
                                               (_introAudioSource.clip.samples - _introAudioSource.timeSamples) /
                                               (double) AudioSettings.outputSampleRate);
            }
        }

        public void Pause()
        {
            if (!IsPlaying)
                return;

            IsPlaying = false;

            if (_introAudioSource.isPlaying)
            {
                _introAudioSource.Pause();

                _loopAudioSource.SetScheduledStartTime(double.MaxValue);
            }
            else if (_loopAudioSource.isPlaying)
            {
                _loopAudioSource.Pause();
            }
        }

        public void Stop()
        {
            if (!IsPlaying)
                return;

            IsPlaying = false;

            if (_introAudioSource.isPlaying)
            {
                _introAudioSource.Stop();

                _loopAudioSource.SetScheduledStartTime(double.MaxValue);
            }
            else if (_loopAudioSource.isPlaying)
            {
                _loopAudioSource.Stop();
            }
        }

        public void Clear()
        {
            Stop();

            _introAudioSource.clip = null;
            _loopAudioSource.clip = null;
        }

        private void CreateAudioSources()
        {
            _introAudioSource = new GameObject().AddComponent<AudioSource>();
            _introAudioSource.transform.SetParent(transform);
            _introAudioSource.name = "Intro Audio Source";
            
            _loopAudioSource = new GameObject().AddComponent<AudioSource>();
            _loopAudioSource.transform.SetParent(transform);
            _loopAudioSource.name = "Loop Audio Source";
            _loopAudioSource.loop = true;
        }
    }
}
