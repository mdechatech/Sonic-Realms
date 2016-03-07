using System.Collections.Generic;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level
{
    public class SoundManager : MonoBehaviour
    {
        public const int DefaultMaxConcurrentAudioClips = 16;

        public static SoundManager Instance;

        /// <summary>
        /// Maximum number of audio clips that the sound manager can play concurrently.
        /// </summary>
        [Tooltip("Maximum number of audio clips that the sound manager can play concurrently.")]
        public int MaxConcurrentAudioClips;

        private List<AudioSource> _audioSources;
        private int _currentAudioSourceIndex;

        /// <summary>
        /// The base settings to use for audio sources created by PlayClipAtPoint.
        /// </summary>
        [Tooltip("The base settings to use for audio sources created by PlayClipAtPoint.")]
        public AudioSource BaseClipAudioSource;

        /// <summary>
        /// The background music within the level.
        /// </summary>
        [Space]
        [Tooltip("The background music within the level.")]
        public AudioSource BGMSource;
        [HideInInspector] public LoopAudioSource BGMLooper;

        /// <summary>
        /// Used for powerup music, such as the Invincibility music. Takes priority over the BGM.
        /// </summary>
        [Tooltip("Used for powerup music, such as the Invincibility music. Takes priority over the BGM.")]
        public AudioSource PowerupSource;
        [HideInInspector] public LoopAudioSource PowerupLooper;

        /// <summary>
        /// Used for jingles that take over the background music for a bit, such as the Extra Life jingle.
        /// </summary>
        [Tooltip("Used for jingles that take over the background music for a bit, such as the Extra Life jingle.")]
        public AudioSource JingleSource;

        /// <summary>
        /// Represents the source of the currently playing BGM.
        /// </summary>
        public enum BGMState
        {
            None,
            BGM,
            Powerup,
            Jingle
        }

        /// <summary>
        /// The BGM currently being played.
        /// </summary>
        [HideInInspector]
        public BGMState CurrentBGMState;

        [HideInInspector]
        public bool AutoplayBGM;

        [HideInInspector]
        public bool AutoplaySecondaryBGM;

        public void Reset()
        {
            MaxConcurrentAudioClips = DefaultMaxConcurrentAudioClips;
            BaseClipAudioSource = null;
        }

        public void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                Instance = this;
            }

            _currentAudioSourceIndex = 0;
            CreateAudioClipSources();
        }

        protected void CreateAudioClipSources()
        {
            _audioSources = new List<AudioSource>();

            if (MaxConcurrentAudioClips <= 0) MaxConcurrentAudioClips = DefaultMaxConcurrentAudioClips;
            for (var i = 0; i < MaxConcurrentAudioClips; ++i)
            {
                var audioSource = new GameObject("Audio Source " + (i + 1)).AddComponent<AudioSource>();
                audioSource.spatialBlend = 0.0f;
                audioSource.transform.SetParent(transform);
                _audioSources.Add(audioSource);
            }
        }

        public void Start()
        {
            DontDestroyOnLoad(gameObject);

            BGMLooper = BGMSource.GetComponent<LoopAudioSource>() ??
                        BGMSource.gameObject.AddComponent<LoopAudioSource>();
            PowerupLooper = PowerupSource.GetComponent<LoopAudioSource>() ??
                            PowerupSource.gameObject.AddComponent<LoopAudioSource>();
        }

        public void Update()
        {
            if (CurrentBGMState == BGMState.BGM) return;
            if (CurrentBGMState == BGMState.Jingle)
            {
                if (!JingleSource.isPlaying && AutoplayBGM)
                {
                    CurrentBGMState = BGMState.BGM;
                    BGMSource.Play();
                }
            }
        }

        public void ResetAudio()
        {
            BGMSource.Stop();
            BGMSource.clip = null;
            BGMLooper.ResetLoopPoints();

            PowerupSource.Stop();
            PowerupSource.clip = null;
            PowerupLooper.ResetLoopPoints();

            JingleSource.Stop();
            JingleSource.clip = null;
        }

        public AudioSource GetAudioSource()
        {
            var result = _audioSources[_currentAudioSourceIndex];
            _currentAudioSourceIndex = (_currentAudioSourceIndex + 1)%MaxConcurrentAudioClips;
            return result;
        }

        public AudioSource PlayClipAtPoint(AudioClip clip, Vector3 position = default(Vector3), float volume = 1.0f)
        {
            var audioSource = Instance.GetAudioSource();
            AssignValuesTo(audioSource);

            audioSource.clip = clip;
            audioSource.transform.position = position;
            audioSource.volume = volume;

            audioSource.Play();
            return audioSource;
        }

        public AudioSource PlayBGM(AudioClip clip, float volume = 1.0f)
        {
            PowerupSource.Stop();
            JingleSource.Stop();

            BGMSource.clip = clip;
            BGMSource.volume = volume;
            BGMSource.Play();
            CurrentBGMState = BGMState.BGM;

            BGMLooper.LoopStart = 0f;
            BGMLooper.LoopEnd = 999f;

            AutoplayBGM = true;

            return BGMSource;
        }

        public AudioSource PlayBGM(BGMLoopData data)
        {
            var audioSource = PlayBGM(data.Clip, data.Volume);
            BGMLooper.SetFrom(data);

            AutoplayBGM = true;

            return audioSource;
        }

        public void StopBGM(AudioClip clip)
        {
            if (BGMSource.clip == clip) StopBGM();
        }

        public void StopBGM()
        {
            if(BGMSource.isPlaying) BGMSource.Stop();
            AutoplayBGM = false;
        }

        public AudioSource PlaySecondaryBGM(AudioClip clip, float volume = 1.0f)
        {
            BGMSource.Stop();
            JingleSource.Stop();

            PowerupSource.clip = clip;
            PowerupSource.volume = volume;
            PowerupSource.Play();
            CurrentBGMState = BGMState.Powerup;

            PowerupLooper.LoopStart = 0f;
            PowerupLooper.LoopEnd = 999f;

            AutoplaySecondaryBGM = true;

            return PowerupSource;
        }

        public AudioSource PlaySecondaryBGM(BGMLoopData data)
        {
            var audioSource = PlaySecondaryBGM(data.Clip, data.Volume);
            PowerupLooper.SetFrom(data);

            AutoplaySecondaryBGM = true;

            return audioSource;
        }

        public void StopSecondaryBGM(AudioClip clip)
        {
            AutoplaySecondaryBGM = false;
            if (PowerupSource.clip == clip) StopSecondaryBGM();
        }

        public void StopSecondaryBGM()
        {
            AutoplaySecondaryBGM = false;
            if (PowerupSource.isPlaying) PowerupSource.Stop();
            if (!BGMSource.isPlaying && AutoplayBGM) BGMSource.Play();
        }

        public AudioSource PlayJingle(AudioClip clip, float volume = 1.0f)
        {
            BGMSource.Stop();
            PowerupSource.Stop();

            JingleSource.clip = clip;
            JingleSource.volume = volume;
            JingleSource.Play();
            CurrentBGMState = BGMState.Jingle;

            return JingleSource;
        }

        public AudioSource CreateAudioSource()
        {
            var audioSource = new GameObject().AddComponent<AudioSource>();
            AssignValuesTo(audioSource);
            return audioSource;
        }

        public void AssignValuesTo(AudioSource audioSource)
        {
            if (!BaseClipAudioSource)
            {
                AssignDefaultValuesTo(audioSource);
                return;
            }

            audioSource.transform.position = BaseClipAudioSource.transform.position;
            audioSource.outputAudioMixerGroup = BaseClipAudioSource.outputAudioMixerGroup;
            audioSource.spatialBlend = BaseClipAudioSource.spatialBlend;
            audioSource.panStereo = BaseClipAudioSource.panStereo;
            audioSource.pitch = BaseClipAudioSource.pitch;
            audioSource.bypassEffects = BaseClipAudioSource.bypassEffects;
            audioSource.bypassListenerEffects = BaseClipAudioSource.bypassListenerEffects;
            audioSource.bypassReverbZones = BaseClipAudioSource.bypassReverbZones;
            audioSource.priority = BaseClipAudioSource.priority;
            audioSource.mute = BaseClipAudioSource.mute;
            audioSource.outputAudioMixerGroup = BaseClipAudioSource.outputAudioMixerGroup;
        }

        public void AssignDefaultValuesTo(AudioSource audioSource)
        {
            audioSource.transform.position = default(Vector3);
            audioSource.volume = 1f;
            audioSource.panStereo = 0f;
            audioSource.pitch = 1f;
            audioSource.bypassEffects = audioSource.bypassListenerEffects = audioSource.bypassReverbZones = false;
            audioSource.priority = 128;
            audioSource.spatialBlend = 0f;
            audioSource.mute = false;
            audioSource.outputAudioMixerGroup = null;
        }
    }
}
