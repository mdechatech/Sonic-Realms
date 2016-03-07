using UnityEngine;

namespace SonicRealms.UI
{
    /// <summary>
    /// Base class for things that play, pause, exit, and enter.
    /// </summary>
    public abstract class BaseTransition : MonoBehaviour
    {
        /// <summary>
        /// Whether to automatically exit after finished entering.
        /// </summary>
        [Tooltip("Whether to automatically exit after finished entering.")]
        public bool ExitAfterEnter;
        
        public bool IsPlaying;
        public TransitionState State;

        public virtual float Progress { get; set; }

        public virtual void Reset()
        {
            
        }

        public virtual void Awake()
        {
            IsPlaying = false;
            State = TransitionState.Idle;
        }

        public virtual void Start()
        {
            // for consistency
        }

        public virtual void Enter()
        {
            if (!this) return;
            gameObject.SetActive(true);
            State = TransitionState.Enter;
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
                State = TransitionState.EnterComplete;
                Pause();
            }
        }

        public virtual void Exit()
        {
            if (!this) return;
            gameObject.SetActive(true);
            State = TransitionState.Exit;
            Play();
        }

        public virtual void ExitComplete()
        {
            State = TransitionState.ExitComplete;
            Pause();
        }

        public virtual void Play()
        {
            if (!this) return;
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
            State = TransitionState.Enter;
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
