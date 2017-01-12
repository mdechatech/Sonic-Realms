using System;
using UnityEngine;
using UnityEngine.Audio;

namespace SonicRealms.Core.Utils
{
    /// <summary>
    /// Manages music and sound effects. See <see cref="MusicSource"/> for how music is handled.
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        /// <summary>
        /// <para>
        /// Sometimes you need to stop the level music for a bit to play something special, such as
        /// invincibility music or an extra life sound. Here's a summary of what kind of music to
        /// play where:
        /// </para>
        /// <para><see cref="PlayJingle"/>: Extra life sound and other one-shots</para>
        /// <para><see cref="PlayPowerupMusic"/>: Drowning, super sonic music, and other temp music</para>
        /// <para><see cref="PlayMainMusic"/>: Level and boss music</para>
        /// <para>
        /// Jingles will play above all other sources, followed by powerup music and lastly main music.
        /// </para>
        /// </summary>
        public enum MusicSource
        {
            /// <summary>
            /// For when nothing is playing, usually at the results screen at the end of a level.
            /// </summary>
            None,

            /// <summary>
            /// Highest priority. For non-looping jingles that replace the current music for a short moment,
            /// such as the extra life sound.
            /// </summary>
            Jingle,

            /// <summary>
            /// Has priority over the main music. For music that plays when the player is in a special situation,
            /// such as the Super Sonic and Drowning music.
            /// </summary>
            PowerupMusic,

            /// <summary>
            /// Lowest priority. For normal level music.
            /// </summary>
            MainMusic,
        }

        /// <summary>
        /// Source of the music that is currently playing.
        /// </summary>
        public static MusicSource ActiveMusicSource { get { return Instance._activeMusicSource; } }

        /// <summary>
        /// 0-1 value that controls the volume of everything the SoundManager plays.
        /// </summary>
        public static float MasterVolume
        {
            get { return Instance._masterVolume; }
            set
            {
                value = Mathf.Clamp01(value);

                Instance._masterVolume = value;
                Instance._mixer.SetFloat("Master Volume", GetDecibels(value));
            }
        }

        /// <summary>
        /// 0-1 value that controls the volume of sound effects.
        /// </summary>
        public static float FxVolume
        {
            get { return Instance._fxVolume; }
            set
            {
                value = Mathf.Clamp01(value);

                Instance._fxVolume = value;
                Instance._mixer.SetFloat("FX Volume", GetDecibels(value));
            }
        }

        /// <summary>
        /// 0-1 value that controls the volume of all music.
        /// </summary>
        public static float MusicVolume
        {
            get { return Instance._musicVolume; }
            set
            {
                value = Mathf.Clamp01(value);

                Instance._musicVolume = value;
                Instance._mixer.SetFloat("Music Volume", GetDecibels(value));
            }
        }

        public static AudioSource JingleSource { get { return Instance._jingleSource; } }

        public static MusicAudioSource PowerupMusicSource { get { return Instance._powerupMusicSource; } }

        public static MusicAudioSource MainMusicSource { get { return Instance._mainMusicSource; } }

        private static SoundManager Instance
        {
            get
            {
                if (!_instance)
                    CreateInstance();

                return _instance;
            }
        }

        private static SoundManager _instance;

        private static string PrefabPath = "Sound Manager Prefab";
        private static string MixerPath = "Sound Manager Mixer";

        [SerializeField, HideInInspector]
        private MusicAudioSource _mainMusicPrefab;

        [SerializeField, HideInInspector]
        private MusicAudioSource _powerupMusicPrefab;

        [SerializeField, HideInInspector]
        private AudioSource _jinglePrefab;

        [SerializeField, HideInInspector]
        private AudioSource _effectPrefab;

        [SerializeField, Range(1, 32)]
        private int _maxSoundEffects;

        [SerializeField]
        private AudioMixerGroup _fxGroup;

        [SerializeField]
        private AudioMixerGroup _musicGroup;

        [SerializeField]
        private AudioMixerGroup _mainMusicGroup;

        [SerializeField]
        private AudioMixerGroup _powerupMusicGroup;

        [SerializeField]
        private AudioMixerGroup _jingleGroup;

        private MusicAudioSource _mainMusicSource;

        private MusicAudioSource _powerupMusicSource;

        private AudioSource _jingleSource;

        private AudioSource[] _effectSources;
        private Action<AudioSource>[] _effectModifiers;
        private Action<AudioSource>[] _effectReverters;
        private int _effectSourceIndex;

        private MusicSource _activeMusicSource;

        private AudioMixer _mixer;

        private float _masterVolume;
        private float _musicVolume;
        private float _fxVolume;

        protected void Reset()
        {
            _maxSoundEffects = 32;
        }

        protected void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            DontDestroyOnLoad(gameObject);

            if (_maxSoundEffects <= 0)
                _maxSoundEffects = 32;

            CreateAudioSources();

            _mixer = Resources.Load<AudioMixer>(MixerPath);
            if (!_mixer)
            {
                Debug.LogError(string.Format("Expected an Audio Mixer asset at Resources/{0}.", MixerPath));
                gameObject.SetActive(false);
                return;
            }

            _mixer.GetFloat("Master Volume", out _masterVolume);
            _mixer.GetFloat("FX Volume", out _fxVolume);
            _mixer.GetFloat("Music Volume", out _musicVolume);

            _masterVolume = GetLinear(_masterVolume);
            _fxVolume = GetLinear(_fxVolume);
            _musicVolume = GetLinear(_musicVolume);
        }

        protected void Update()
        {
            if (_activeMusicSource == MusicSource.Jingle)
            {
                if (!_jingleSource.isPlaying)
                {
                    _jingleSource.clip = null;
                    PlayNextMusic();
                }
            }
            else if (_activeMusicSource == MusicSource.PowerupMusic)
            {
                if (!_powerupMusicSource.IsPlaying)
                {
                    _powerupMusicSource.Clear();
                    PlayNextMusic();
                }
            } else if (_activeMusicSource == MusicSource.MainMusic)
            {
                if (!_mainMusicSource.IsPlaying)
                {
                    _mainMusicSource.Clear();
                    _activeMusicSource = MusicSource.None;
                }
            }
        }

        /// <summary>
        /// Stops the currently playing music source.
        /// </summary>
        public static void StopActiveMusic()
        {
            switch (Instance._activeMusicSource)
            {
                case MusicSource.Jingle:
                    StopJingle();
                    break;

                case MusicSource.PowerupMusic:
                    StopPowerupMusic();
                    break;

                case MusicSource.MainMusic:
                    StopMainMusic();
                    break;
            }
        }

        /// <summary>
        /// Whether the audio clip is playing or is queued to play under the <see cref="MusicSource.Jingle"/> source.
        /// </summary>
        public static bool JingleIs(AudioClip jingle)
        {
            return Instance._jingleSource.clip == jingle;
        }

        /// <summary>
        /// Whether the audio clip is playing or is queued to play under the <see cref="MusicSource.PowerupMusic"/> source.
        /// </summary>
        public static bool PowerupMusicIs(AudioClip loopClip)
        {
            return Instance._powerupMusicSource.LoopClip == loopClip;
        }

        /// <summary>
        /// Whether the audio clips playing or are queued to play under the <see cref="MusicSource.PowerupMusic"/> source.
        /// </summary>
        public static bool PowerupMusicIs(AudioClip introClip, AudioClip loopClip)
        {
            return Instance._powerupMusicSource.LoopClip == loopClip &&
                   Instance._powerupMusicSource.IntroClip == introClip;
        }

        /// <summary>
        /// Whether the audio clip is playing or is queued to play under the <see cref="MusicSource.MainMusic"/> source.
        /// </summary>
        public static bool MainMusicIs(AudioClip loopClip)
        {
            return Instance._mainMusicSource.LoopClip == loopClip;
        }

        /// <summary>
        /// Whether the audio clips are playing or are queued to play under the <see cref="MusicSource.MainMusic"/> source.
        /// </summary>
        public static bool MainMusicIs(AudioClip introClip, AudioClip loopClip)
        {
            return Instance._mainMusicSource.LoopClip == loopClip &&
                   Instance._mainMusicSource.IntroClip == introClip;
        }

        /// <summary>
        /// Plays music under the <see cref="MusicSource.Jingle"/> source.
        /// </summary>
        public static void PlayJingle(AudioClip jingle)
        {
            PauseAllMusic();

            Instance._activeMusicSource = MusicSource.Jingle;
            Instance._jingleSource.clip = jingle;
            Instance._jingleSource.time = 0;
            Instance._jingleSource.Play();
        }

        /// <summary>
        /// Stops the current jingle.
        /// </summary>
        public static void StopJingle()
        {
            Instance._jingleSource.Stop();
            Instance._jingleSource.clip = null;
        }

        /// <summary>
        /// Loops music under the <see cref="MusicSource.PowerupMusic"/> source.
        /// </summary>
        public static void PlayPowerupMusic(AudioClip loopClip)
        {
            PlayPowerupMusic(null, loopClip);
        }

        /// <summary>
        /// Loops music under the <see cref="MusicSource.PowerupMusic"/> source.
        /// </summary>
        /// <param name="introClip">An audio clip to lead in with. Can be null.</param>
        /// <param name="loopClip">The audio clip that follows the intro clip and then loops until stopped.</param>
        public static void PlayPowerupMusic(AudioClip introClip, AudioClip loopClip)
        {
            PauseAllMusic();
            StopJingle();

            Instance._activeMusicSource = MusicSource.PowerupMusic;

            if (!introClip)
                Instance._powerupMusicSource.Play(loopClip);
            else
                Instance._powerupMusicSource.Play(introClip, loopClip);
        }

        /// <summary>
        /// Stops the current powerup music.
        /// </summary>
        public static void StopPowerupMusic()
        {
            Instance._powerupMusicSource.Clear();
        }
        /// <summary>
        /// Loops music under the <see cref="MusicSource.MainMusic"/> source.
        /// </summary>
        public static void PlayMainMusic(AudioClip loopClip)
        {
            PlayMainMusic(null, loopClip);
        }

        /// <summary>
        /// Loops music under the <see cref="MusicSource.PowerupMusic"/> source.
        /// </summary>
        /// <param name="introClip">An audio clip to lead in with. Can be null.</param>
        /// <param name="loopClip">The audio clip that follows the intro clip and then loops until stopped.</param>
        public static void PlayMainMusic(AudioClip introClip, AudioClip loopClip)
        {
            StopAllMusic();

            Instance._activeMusicSource = MusicSource.MainMusic;

            if (!introClip)
                Instance._mainMusicSource.Play(loopClip);
            else
                Instance._mainMusicSource.Play(introClip, loopClip);
        }

        /// <summary>
        /// Stops the current main music.
        /// </summary>
        public static void StopMainMusic()
        {
            Instance._mainMusicSource.Clear();
        }

        /// <summary>
        /// Plays the given audio clip.
        /// </summary>
        public static AudioSource PlaySoundEffect(AudioClip soundEffect)
        {
            return PlaySoundEffect(soundEffect, null, null);
        }

        /// <summary>
        /// Plays the given audio clip at the given volume.
        /// </summary>
        public static AudioSource PlaySoundEffect(AudioClip soundEffect, float volume)
        {
            var oldVolume = 1f;

            return PlaySoundEffect(soundEffect,

                source =>
                {
                    oldVolume = source.volume;
                    source.volume = volume;
                },

                source => source.volume = oldVolume);
        }

        /// <summary>
        /// Plays the given sound effect.
        /// </summary>
        /// <param name="soundEffect"></param>
        /// <param name="modifier">A function that modifies the audio source that will play the sound effect. Can be null.</param>
        /// <param name="reverter">A function that reverts the modifications to the audio source. Can be null.</param>
        public static AudioSource PlaySoundEffect(AudioClip soundEffect, Action<AudioSource> modifier, Action<AudioSource> reverter)
        {
            var source = Instance._effectSources[Instance._effectSourceIndex];

            var oldReverter = Instance._effectReverters[Instance._effectSourceIndex];
            if (oldReverter != null)
                oldReverter(source);

            Instance._effectReverters[Instance._effectSourceIndex] = reverter;
            Instance._effectModifiers[Instance._effectSourceIndex] = modifier;

            source.clip = soundEffect;
            source.time = 0;
            source.Play();

            if (modifier != null)
                modifier(source);

            Instance._effectSourceIndex = (Instance._effectSourceIndex + 1) % Instance._effectSources.Length;

            return source;
        }

        /// <summary>
        /// Pauses all sources of music.
        /// </summary>
        public static void PauseAllMusic()
        {
            Instance._jingleSource.Pause();
            Instance._powerupMusicSource.Pause();
            Instance._mainMusicSource.Pause();

            Instance._activeMusicSource = MusicSource.None;
        }

        /// <summary>
        /// Stops all sources of music.
        /// </summary>
        public static void StopAllMusic()
        {
            Instance._jingleSource.Stop();
            Instance._powerupMusicSource.Stop();
            Instance._mainMusicSource.Stop();

            Instance._activeMusicSource = MusicSource.None;
        }

        private static void CreateInstance()
        {
            var prefab = Resources.Load<SoundManager>(PrefabPath);
            if (!prefab)
            {
                Debug.LogError(string.Format("Expected a Sound Manager prefab at Resources/{0}.", PrefabPath));
                return;
            }

            var instance = Instantiate(prefab);
            instance.name = "Sound Manager";

            // SoundManager will set itself to Instance in Awake()
        }

        /// <summary>
        /// Creates an audio source for sound effects that is affected by the Sound Manager's settings.
        /// </summary>
        public static AudioSource CreateSoundEffectSource()
        {
            var source = new GameObject().AddComponent<AudioSource>();
            source.outputAudioMixerGroup = Instance._fxGroup;

            return source;
        }

        /// <summary>
        /// Converts the given 0-1 value to the decibels used internally for the Sound Manager's
        /// audio mixer.
        /// </summary>
        public static float GetDecibels(float linear)
        {
            linear = Mathf.Lerp(0.1f, 1, Mathf.Clamp01(linear));

            return linear == 0 ? -80 : Mathf.Log10(linear) * 80;
        }

        /// <summary>
        /// Converts the given decibel value to a 0-1 value used for the Sound Manager's volume properties.
        /// </summary>
        public static float GetLinear(float decibels)
        {
            return Mathf.Lerp(0, 1, Mathf.Pow(10, decibels / 80));
        }

        /// <summary>
        /// Plays the highest priority music source.
        /// </summary>
        public static void PlayNextMusic()
        {
            if (Instance._jingleSource.clip)
            {
                Instance._jingleSource.Play();
                Instance._activeMusicSource = MusicSource.Jingle;
            }
            else if (Instance._powerupMusicSource.HasClip)
            {
                Instance._powerupMusicSource.Play();
                Instance._activeMusicSource = MusicSource.PowerupMusic;
            }
            else if (Instance._mainMusicSource.HasClip)
            {
                Instance._mainMusicSource.Play();
                Instance._activeMusicSource = MusicSource.MainMusic;
            }
            else
            {
                Instance._activeMusicSource = MusicSource.None;
            }
        }

        private void CreateAudioSources()
        {
            _mainMusicSource = new GameObject().AddComponent<MusicAudioSource>();
            _mainMusicSource.name = "Main Music Audio Source";
            _mainMusicSource.transform.SetParent(transform);
            _mainMusicSource.MixerGroup = _mainMusicGroup;

            _powerupMusicSource = new GameObject().AddComponent<MusicAudioSource>();
            _powerupMusicSource.name = "Powerup Music Audio Source";
            _powerupMusicSource.transform.SetParent(transform);
            _powerupMusicSource.MixerGroup = _powerupMusicGroup;

            _jingleSource = new GameObject().AddComponent<AudioSource>();
            _jingleSource.name = "Jingle Audio Source";
            _jingleSource.transform.SetParent(transform);
            _jingleSource.outputAudioMixerGroup = _jingleGroup;

            CreateEffectSources();
        }

        private void CreateEffectSources()
        {
            _effectSources = new AudioSource[_maxSoundEffects];
            _effectModifiers = new Action<AudioSource>[_maxSoundEffects];
            _effectReverters = new Action<AudioSource>[_maxSoundEffects];

            var parent = new GameObject("Effect Audio Sources").transform;
            parent.SetParent(transform);

            for (var i = 0; i < _maxSoundEffects; ++i)
            {
                var effectSource = CreateSoundEffectSource();
                effectSource.name = i.ToString();
                effectSource.transform.SetParent(parent);

                _effectSources[i] = effectSource;
            }
        }
    }
}
