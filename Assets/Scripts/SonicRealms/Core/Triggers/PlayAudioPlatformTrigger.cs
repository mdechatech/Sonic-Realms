using SonicRealms.Core.Internal;
using SonicRealms.Core.Utils;
using SonicRealms.Level;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Plays audio based on platform trigger events.
    /// </summary>
    public class PlayAudioPlatformTrigger : ReactivePlatform
    {
        #region Public Fields & Properties

        /// <summary>
        /// Audio clip to play when a player begins colliding with the platform.
        /// </summary>
        public AudioClip PlatformEnterSound { get { return _platformEnterSound; } set { _platformEnterSound = value; } }

        /// <summary>
        /// Audio clip to loop while a player is colliding with the platform.
        /// </summary>
        public AudioClip PlatformStayLoop { get { return _platformStayLoop; } }

        /// <summary>
        /// Audio clip to play when a player stops colliding with the platform.
        /// </summary>
        public AudioClip PlatformExitSound { get { return _platformExitSound; } set { _platformExitSound = value; } }

        /// <summary>
        /// Audio clip to play when a player starts standing on the platform.
        /// </summary>
        public AudioClip SurfaceEnterSound { get { return _surfaceEnterSound; } set { _surfaceEnterSound = value; } }

        /// <summary>
        /// Audio clip to loop while a player is standing on the platform.
        /// </summary>
        public AudioClip SurfaceStayLoop { get { return _surfaceStayLoop; } }

        /// <summary>
        /// Audio clip to play when a player stops standing on the platform.
        /// </summary>
        public AudioClip SurfaceExitSound { get { return _surfaceExitSound; } set { _surfaceExitSound = value; } }

        #endregion

        #region Inspector & Private Fields

        [SerializeField, HideInInspector]
        private AudioSource _baseLoopAudioSource;

        [SerializeField]
        [Tooltip("Audio clip to play when a player begins colliding with the platform.")]
        private AudioClip _platformEnterSound;

        [SerializeField]
        [Tooltip("Audio clip to loop while a player is colliding with the platform.")]
        private AudioClip _platformStayLoop;

        [SerializeField]
        [Tooltip("Audio clip to play when a player stops colliding with the platform.")]
        private AudioClip _platformExitSound;

        [SerializeField]
        [Tooltip("Audio clip to play when a player starts standing on the platform.")]
        private AudioClip _surfaceEnterSound;

        [SerializeField]
        [Tooltip("Audio clip to loop while a player is standing on the platform.")]
        private AudioClip _surfaceStayLoop;

        [SerializeField]
        [Tooltip("Audio clip to play when a player stops standing on the platform.")]
        private AudioClip _surfaceExitSound;

        private AudioSource _platformStayLoopAudioSource;

        private AudioSource _surfaceStayLoopAudioSource;

        #endregion

        #region Lifecycle Functions

        public override void Awake()
        {
            base.Awake();

            CreateAudioSources();
        }

        #endregion

        #region Event Functions

        public override void OnPlatformEnter(PlatformCollision collision)
        {
            if (_platformEnterSound)
            {
                SrSoundManager.PlaySoundEffect(_platformEnterSound);
            }

            if (_platformStayLoopAudioSource && !_platformStayLoopAudioSource.isPlaying)
            {
                _platformStayLoopAudioSource.Play();
            }
        }

        public override void OnPlatformExit(PlatformCollision collision)
        {
            if (_platformExitSound)
            {
                SrSoundManager.PlaySoundEffect(_platformExitSound);
            }

            if (PlatformTrigger.ControllersOnPlatform.Count == 0 && _platformStayLoopAudioSource)
            {
                _platformStayLoopAudioSource.Stop();
            }
        }

        public override void OnSurfaceEnter(SurfaceCollision collision)
        {
            if (_surfaceEnterSound)
            {
                SrSoundManager.PlaySoundEffect(_surfaceEnterSound);
            }

            if (_surfaceStayLoopAudioSource && !_surfaceStayLoopAudioSource.isPlaying)
            {
                _surfaceStayLoopAudioSource.Play();
            }
        }

        public override void OnSurfaceExit(SurfaceCollision collision)
        {
            if (_surfaceExitSound)
            {
                SrSoundManager.PlaySoundEffect(_surfaceExitSound);
            }

            if (PlatformTrigger.ControllersOnSurface.Count == 0 && _surfaceStayLoopAudioSource)
            {
                _surfaceStayLoopAudioSource.Stop();
            }
        }

        private void CreateAudioSources()
        {
            if (_platformStayLoop)
            {
                _platformStayLoopAudioSource = SrSoundManager.CreateSoundEffectSource();
                _platformStayLoopAudioSource.clip = _platformStayLoop;

                _platformStayLoopAudioSource.transform.SetParent(transform);
                _platformStayLoopAudioSource.transform.localPosition = Vector3.zero;
            }

            if (_surfaceStayLoop)
            {
                _surfaceStayLoopAudioSource = SrSoundManager.CreateSoundEffectSource();
                _surfaceStayLoopAudioSource.clip = _surfaceStayLoop;

                _surfaceStayLoopAudioSource.transform.SetParent(transform);
                _surfaceStayLoopAudioSource.transform.localPosition = Vector3.zero;
            }
        }

        #endregion
    }
}
