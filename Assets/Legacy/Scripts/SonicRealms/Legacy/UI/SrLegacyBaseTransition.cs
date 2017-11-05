using UnityEngine;

namespace SonicRealms.Legacy.UI
{
    /// <summary>
    /// Base class for things that play, pause, exit, and enter.
    /// </summary>
    public abstract class SrLegacyBaseTransition : MonoBehaviour
    {
        /// <summary>
        /// Whether to automatically exit after finished entering.
        /// </summary>
        [Tooltip("Whether to automatically exit after finished entering.")]
        public bool ExitAfterEnter;
        
        public bool IsPlaying;
        public SrLegacyTransitionState State;

        public virtual float Progress { get; set; }

        public virtual void Reset()
        {
            
        }

        public virtual void Awake()
        {
            IsPlaying = false;
            State = SrLegacyTransitionState.Idle;
        }

        public virtual void Start()
        {
            // for consistency
        }

        public virtual void Enter()
        {
            if (!this) return;
            gameObject.SetActive(true);
            State = SrLegacyTransitionState.Enter;
            Play();
        }

        public virtual void EnterComplete()
        {
            if (ExitAfterEnter)
            {
                Exit();
            }
            else
            {
                State = SrLegacyTransitionState.EnterComplete;
                Pause();
            }
        }

        public virtual void Exit()
        {
            if (!this) return;
            gameObject.SetActive(true);
            State = SrLegacyTransitionState.Exit;
            Play();
        }

        public virtual void ExitComplete()
        {
            State = SrLegacyTransitionState.ExitComplete;
            Pause();
        }

        public virtual void Play()
        {
            if (!this || State == SrLegacyTransitionState.Idle) return;
            gameObject.SetActive(true);
            IsPlaying = true;
        }

        public virtual void Pause()
        {
            IsPlaying = false;
        }

        public virtual void Stop()
        {
            IsPlaying = false;
            State = SrLegacyTransitionState.Enter;
        }

        public virtual void Update()
        {
            if(IsPlaying) OnPlayingUpdate();
        }

        public virtual void OnPlayingUpdate()
        {

        }
    }
}
