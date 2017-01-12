using SonicRealms.Core.Actors;
using SonicRealms.Core.Utils;
using SonicRealms.Level;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Plays audio based on effect trigger events.
    /// </summary>
    public class PlayAudioEffectTrigger : ReactiveEffect
    {
        #region Public Fields and Properties

        /// <summary>
        /// Audio clip to play when the effect trigger is activated.
        /// </summary>
        public AudioClip ActivateSound { get { return _activateSound; } set { _activateSound = value; } }

        /// <summary>
        /// Audio clip to loop while the effect trigger is activated.
        /// </summary>
        public AudioClip ActivateStayLoop { get { return _activateStayLoop; } }

        /// <summary>
        /// Audio clip to play when the effect trigger is deactivated.
        /// </summary>
        public AudioClip DeactivateSound { get { return _deactivateSound; } set { _deactivateSound = value; } }

        /// <summary>
        /// Audio clip to play for every player that activates the effect trigger.
        /// </summary>
        public AudioClip ActivatorEnterSound
        {
            get { return _activatorEnterSound; }
            set { _activatorEnterSound = value; }
        }

        // Unsure of whether there should be an ActivatorStayLoop - a sound looping for every
        // player activating the effect trigger

        /// <summary>
        /// Audio clip to play for every player that deactivates the effect trigger.
        /// </summary>
        public AudioClip ActivatorExitSound
        {
            get { return _activatorExitSound; }
            set { _activatorEnterSound = value; }
        }

        #endregion

        #region Private & Inspector Fields

        [SerializeField]
        [Tooltip("Audio clip to play when the effect trigger is activated.")]
        private AudioClip _activateSound;

        [SerializeField]
        [Tooltip("Audio clip to loop while the effect trigger is activated.")]
        private AudioClip _activateStayLoop;

        [SerializeField]
        [Tooltip("Audio clip to play when the effect trigger is deactivated.")]
        private AudioClip _deactivateSound;

        [SerializeField]
        [Tooltip("Audio clip to play for every player that activates the effect trigger.")]
        private AudioClip _activatorEnterSound;

        [SerializeField]
        [Tooltip("Audio clip to play for every player that deactivates the effect trigger.")]
        private AudioClip _activatorExitSound;

        [SerializeField, HideInInspector]
        private AudioSource _baseLoopAudioSource;

        private AudioSource _activateStayLoopAudioSource;

        #endregion

        #region Lifecycle Functions

        public override void Awake()
        {
            base.Awake();

            CreateAudioSources();
        }

        private void CreateAudioSources()
        {
            if (_activateStayLoop)
            {
                _activateStayLoopAudioSource = Instantiate(_baseLoopAudioSource);
                _activateStayLoopAudioSource.clip = _activateStayLoop;

                _activateStayLoopAudioSource.transform.SetParent(transform);
                _activateStayLoopAudioSource.transform.localPosition = Vector3.zero;
            }
        }

        #endregion

        #region Event Functions

        public override void OnActivate(HedgehogController controller)
        {
            if (ActivateSound)
            {
                SoundManager.PlaySoundEffect(ActivateSound);
            }

            if (_activateStayLoopAudioSource && !_activateStayLoopAudioSource.isPlaying)
            {
                _activateStayLoopAudioSource.Play();
            }
        }

        public override void OnDeactivate(HedgehogController controller)
        {
            if (DeactivateSound)
            {
                SoundManager.PlaySoundEffect(DeactivateSound);
            }

            if (_activateStayLoopAudioSource && _activateStayLoopAudioSource.isPlaying)
            {
                _activateStayLoopAudioSource.Stop();
            }
        }

        public override void OnActivatorEnter(HedgehogController controller)
        {
            if (ActivatorEnterSound)
            {
                SoundManager.PlaySoundEffect(ActivatorEnterSound);
            }
        }

        public override void OnActivatorExit(HedgehogController controller)
        {
            if (ActivatorExitSound)
            {
                SoundManager.PlaySoundEffect(ActivatorExitSound);
            }
        }

        #endregion
    }
}
