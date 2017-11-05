using SonicRealms.Core.Internal;
using SonicRealms.Core.Utils;
using SonicRealms.Level;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Plays audio based on area trigger events.
    /// </summary>
    public class PlayAudioAreaTrigger : ReactiveArea
    {
        #region Public Fields & Properties

        /// <summary>
        /// Audio clip to play when a player enters the area.
        /// </summary>
        public AudioClip AreaEnterSound { get { return _areaEnterSound; } set { _areaEnterSound = value; } }

        /// <summary>
        /// Audio clip to loop while a player is inside the area.
        /// </summary>
        public AudioClip AreaStayLoop { get { return _areaStayLoop; } }

        /// <summary>
        /// Audio clip to play when a player exits the area.
        /// </summary>
        public AudioClip AreaExitSound { get { return _areaExitSound; } set { _areaExitSound = value; } }

        #endregion

        #region Private & Inspector Fields

        [SerializeField]
        [Tooltip("Audio clip to play when a player enters the area.")]
        private AudioClip _areaEnterSound;

        [SerializeField]
        [Tooltip("Audio clip to loop while a player is inside the area.")]
        private AudioClip _areaStayLoop;

        [SerializeField]
        [Tooltip("Audio clip to loop while a player is inside the area.")]
        private AudioClip _areaExitSound;

        [SerializeField, HideInInspector]
        private AudioSource _baseLoopAudioSource;

        private AudioSource _areaStayLoopAudioSource;

        #endregion

        #region Lifecycle Functions

        public override void Awake()
        {
            base.Awake();

            CreateAudioSources();
        }

        private void CreateAudioSources()
        {
            if (_areaStayLoop)
            {
                _areaStayLoopAudioSource = Instantiate(_baseLoopAudioSource);
                _areaStayLoopAudioSource.clip = _areaStayLoop;

                _areaStayLoopAudioSource.transform.SetParent(transform);
                _areaStayLoopAudioSource.transform.localPosition = Vector3.zero;
            }
        }

        #endregion

        #region Event Functions

        public override void OnAreaEnter(AreaCollision collision)
        {
            if (AreaEnterSound)
            {
                SrSoundManager.PlaySoundEffect(AreaEnterSound);
            }

            if (_areaStayLoopAudioSource && !_areaStayLoopAudioSource.isPlaying)
            {
                _areaStayLoopAudioSource.Play();
            }
        }

        public override void OnAreaExit(AreaCollision collision)
        {
            if (AreaExitSound)
            {
                SrSoundManager.PlaySoundEffect(AreaExitSound);
            }

            if (_areaStayLoopAudioSource && AreaTrigger.ControllersInArea.Count == 0)
            {
                _areaStayLoopAudioSource.Stop();
            }
        }

        #endregion
    }
}
