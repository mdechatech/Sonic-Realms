using System.Collections.Generic;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level
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
        /// The main level music to play.
        /// </summary>
        [Tooltip("The main level music to play.")]
        public LoopBGMData MainBGM;

        /// <summary>
        /// The background music within the level.
        /// </summary>
        [HideInInspector]
        [Tooltip("The background music within the level.")]
        public AudioSource BGMSource;

        /// <summary>
        /// Used for powerup music, such as the Invincibility music. Takes priority over the BGM.
        /// </summary>
        [HideInInspector]
        [Tooltip("Used for powerup music, such as the Invincibility music. Takes priority over the BGM.")]
        public AudioSource PowerupSource;

        /// <summary>
        /// Used for jingles that take over the background music for a bit, such as the Extra Life jingle.
        /// </summary>
        [HideInInspector]
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

        public void Reset()
        {
            MaxConcurrentAudioClips = DefaultMaxConcurrentAudioClips;
        }

        public void Start()
        {
            Instance = this;
            if(MainBGM) PlayBGM(MainBGM);
        }

        public void Awake()
        {
            _currentAudioSourceIndex = 0;
            _audioSources = new List<AudioSource>();

            if (MaxConcurrentAudioClips <= 0) MaxConcurrentAudioClips = DefaultMaxConcurrentAudioClips;
            for (var i = 0; i < MaxConcurrentAudioClips; ++i)
            {
                var audioSource = new GameObject("Audio Source " + (i + 1)).AddComponent<AudioSource>();
                audioSource.spatialBlend = 0.0f;
                audioSource.transform.SetParent(transform);
                _audioSources.Add(audioSource);
            }

            BGMSource = new GameObject("BGM Source").AddComponent<AudioSource>();
            BGMSource.gameObject.AddComponent<LoopAudioSource>();
            BGMSource.transform.SetParent(transform);
            BGMSource.spatialBlend = 0.0f;

            PowerupSource = new GameObject("Powerup Source").AddComponent<AudioSource>();
            PowerupSource.gameObject.AddComponent<LoopAudioSource>();
            PowerupSource.transform.SetParent(transform);
            PowerupSource.spatialBlend = 0.0f;

            JingleSource = new GameObject("Jingle Source").AddComponent<AudioSource>();
            JingleSource.transform.SetParent(transform);
            JingleSource.spatialBlend = 0.0f;
        }

        public void Update()
        {
            if (CurrentBGMState == BGMState.BGM) return;

            if (CurrentBGMState == BGMState.Powerup)
            {
                if (!PowerupSource.isPlaying)
                {
                    CurrentBGMState = BGMState.BGM;
                    BGMSource.Play();
                }
            }
            else if (CurrentBGMState == BGMState.Jingle)
            {
                if (!JingleSource.isPlaying)
                {
                    CurrentBGMState = BGMState.BGM;
                    BGMSource.Play();
                }
            }
        }

        public static void CreateInstance()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Only one sound manager allowed per scene! Spot taken by " + Instance.name + ".");
                return;
            }

            Instance = new GameObject("Sound Manager").AddComponent<SoundManager>();
        }

        private static void CheckInstance()
        {
            if (Instance != null) return;
            CreateInstance();
            Debug.LogWarning("Tried to play a clip using the SoundManager but it didn't exist! SoundManager created " +
                             "automatically.");
        }

        public AudioSource GetAudioSource()
        {
            var result = _audioSources[_currentAudioSourceIndex];
            _currentAudioSourceIndex = (_currentAudioSourceIndex + 1)%MaxConcurrentAudioClips;
            return result;
        }

        public static AudioSource PlayClipAtPoint(AudioClip clip, Vector3 position = default(Vector3), float volume = 1.0f)
        {
            CheckInstance();

            var audioSource = Instance.GetAudioSource();
            audioSource.clip = clip;
            audioSource.transform.position = position;
            audioSource.volume = volume;

            audioSource.panStereo = 0f;
            audioSource.pitch = 1f;
            audioSource.bypassEffects = audioSource.bypassListenerEffects = audioSource.bypassReverbZones = false;
            audioSource.priority = 128;
            audioSource.spatialBlend = 0f;
            audioSource.mute = false;
            audioSource.outputAudioMixerGroup = null;

            audioSource.Play();
            
            return audioSource;
        }

        public static AudioSource PlayBGM(LoopBGMData data)
        {
            return PlayBGM(data.Clip, data.Volume, data.LoopbackTime, data.LoopTime);
        }

        public static AudioSource PlayBGM(AudioClip clip, float volume = 1.0f)
        {
            return PlayBGM(clip, volume, 0.0f, clip.length + 1f);
        }

        public static AudioSource PlayBGM(AudioClip clip, float volume, float loopbackTime, float loopTime)
        {
            CheckInstance();

            Instance.PowerupSource.Stop();
            Instance.JingleSource.Stop();

            Instance.BGMSource.clip = clip;
            Instance.BGMSource.volume = volume;
            Instance.BGMSource.Play();
            Instance.CurrentBGMState = BGMState.BGM;

            var loop = Instance.BGMSource.gameObject.GetComponent<LoopAudioSource>();
            loop.LoopbackTime = loopbackTime;
            loop.LoopTime = loopTime;

            return Instance.BGMSource;
        }

        public static AudioSource PlaySecondaryBGM(LoopBGMData data)
        {
            return PlaySecondaryBGM(data.Clip, data.Volume, data.LoopbackTime, data.LoopTime);
        }

        public static AudioSource PlaySecondaryBGM(AudioClip clip, float volume = 1.0f)
        {
            return PlaySecondaryBGM(clip, volume, 0f, clip.length + 1f);
        }

        public static AudioSource PlaySecondaryBGM(AudioClip clip, float volume, float loopbackTime, float loopTime)
        {
            CheckInstance();

            Instance.BGMSource.Stop();
            Instance.JingleSource.Stop();

            Instance.PowerupSource.clip = clip;
            Instance.PowerupSource.volume = volume;
            Instance.PowerupSource.Play();
            Instance.CurrentBGMState = BGMState.Powerup;

            var loop = Instance.PowerupSource.gameObject.GetComponent<LoopAudioSource>();
            loop.LoopbackTime = loopbackTime;
            loop.LoopTime = loopTime;

            return Instance.PowerupSource;
        }

        public static AudioSource PlayJingle(AudioClip clip, float volume = 1.0f)
        {
            CheckInstance();

            Instance.BGMSource.Stop();
            Instance.PowerupSource.Stop();

            Instance.JingleSource.clip = clip;
            Instance.JingleSource.volume = volume;
            Instance.JingleSource.Play();
            Instance.CurrentBGMState = BGMState.Jingle;

            return Instance.JingleSource;
        }
    }
}
