using System;
using System.Collections;
using SonicRealms.Core.Utils;
using SonicRealms.Level;
using UnityEngine;
using UnityEngine.Events;

namespace SonicRealms.UI
{
    /// <summary>
    /// Generic transition with generic triggers, animations, and sounds.
    /// </summary>
    public class Transition : BaseTransition
    {
        #region Animation
        [Foldout("Animation")]
        public Animator Animator;

        /// <summary>
        /// Name of an Animator bool set to whether the transition is playing.
        /// </summary>
        [Space, Foldout("Animation")]
        [Tooltip("Name of an Animator bool set to whether the transition is playing.")]
        public string PlayingBool;
        protected int PlayingBoolHash;

        /// <summary>
        /// Name of an Animator trigger set when the transition plays.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator trigger set when the transition plays.")]
        public string PlayTrigger;
        protected int PlayTriggerHash;

        /// <summary>
        /// Name of an Animator trigger set when the transition pauses.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator trigger set when the transition pauses.")]
        public string PauseTrigger;
        protected int PauseTriggerHash;

        /// <summary>
        /// Name of an Animator trigger set when the transition stops.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator trigger set when the transition stops.")]
        public string StopTrigger;
        protected int StopTriggerHash;

        /// <summary>
        /// Name of an Animator bool set to whether the transition is entering.
        /// </summary>
        [Space, Foldout("Animation")]
        [Tooltip("Name of an Animator bool set to whether the transition is entering.")]
        public string EnteringBool;
        protected int EnteringBoolHash;

        /// <summary>
        /// Name of an Animator trigger set when the transition starts to enter.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator trigger set when the transition starts to enter.")]
        public string EnterBeginTrigger;
        protected int EnterBeginTriggerHash;

        /// <summary>
        /// Name of an Animator trigger set when the transition finishes entering.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator trigger set when the transition finishes entering.")]
        public string EnterCompleteTrigger;
        protected int EnterCompleteTriggerHash;

        /// <summary>
        /// Name of an Animator bool set to whether the transition is exiting.
        /// </summary>
        [Space, Foldout("Animation")]
        [Tooltip("Name of an Animator bool set to whether the transition is exiting.")]
        public string ExitingBool;
        protected int ExitingBoolHash;

        /// <summary>
        /// Name of an Animator trigger set when the transition starts to exit.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator trigger set when the transition starts to exit.")]
        public string ExitBeginTrigger;
        protected int ExitBeginTriggerHash;

        /// <summary>
        /// Name of an Animator trigger set when the transition finishes exiting.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator trigger set when the transition finishes exiting.")]
        public string ExitCompleteTrigger;
        protected int ExitCompleteTriggerHash;
        #endregion
        #region Events
        /// <summary>
        /// Invoked when the transition begins entering.
        /// </summary>
        [Foldout("Events")]
        public UnityEvent OnEnterBegin;

        /// <summary>
        /// Invoked when the transition finishes entering.
        /// </summary>
        [Foldout("Events")]
        public UnityEvent OnEnterComplete;

        /// <summary>
        /// Invoked when the transition begins exiting.
        /// </summary>
        [Foldout("Events")]
        public UnityEvent OnExitBegin;

        /// <summary>
        /// Invoked when the transition finishes exiting.
        /// </summary>
        [Foldout("Events")]
        public UnityEvent OnExitComplete;

        /// <summary>
        /// Invoked when the transition plays.
        /// </summary>
        [Foldout("Events")]
        public UnityEvent OnPlay;

        /// <summary>
        /// Invoked when the transition pauses.
        /// </summary>
        [Foldout("Events")]
        public UnityEvent OnPause;

        /// <summary>
        /// Invoked when the transition stops.
        /// </summary>
        [Foldout("Events")]
        public UnityEvent OnStop;
        #endregion

        /// <summary>
        /// Triggers that would cause the transition to finish its enter sequence.
        /// </summary>
        [Foldout("Triggers")]
        [Tooltip("Triggers that would cause the transition to finish its enter sequence.")]
        public TransitionWaitModule EnterToEnterComplete;

        /// <summary>
        /// Triggers that would cause the transition to start its exit sequence (after having entered.)
        /// </summary>
        [Foldout("Triggers")]
        [Tooltip("Triggers that would cause the transition to start its exit sequence (after having entered.)")]
        public TransitionWaitModule EnterCompleteToExit;

        /// <summary>
        /// Triggers that would cause the transition to finish its exit sequence.
        /// </summary>
        [Foldout("Triggers")]
        [Tooltip("Triggers that would cause the transition to finish its exit sequence.")]
        public TransitionWaitModule ExitToExitComplete;

        protected Coroutine TriggerCoroutine;

        public override void Reset()
        {
            base.Reset();
            Animator = GetComponent<Animator>();
        }

        public override void Awake()
        {
            base.Awake();
            #region Initialization
            OnEnterBegin = OnEnterBegin ?? new UnityEvent();
            OnEnterComplete = OnEnterComplete ?? new UnityEvent();

            OnExitBegin = OnExitBegin ?? new UnityEvent();
            OnExitComplete = OnExitComplete ?? new UnityEvent();

            OnPlay = OnPlay ?? new UnityEvent();
            OnPause = OnPause ?? new UnityEvent();
            OnStop = OnStop ?? new UnityEvent();

            Animator = Animator ?? GetComponent<Animator>();

            PlayingBoolHash = Animator.StringToHash(PlayingBool);
            PlayTriggerHash = Animator.StringToHash(PlayTrigger);
            PauseTriggerHash = Animator.StringToHash(PauseTrigger);
            StopTriggerHash = Animator.StringToHash(StopTrigger);

            EnteringBoolHash = Animator.StringToHash(EnteringBool);
            EnterBeginTriggerHash = Animator.StringToHash(EnterBeginTrigger);
            EnterCompleteTriggerHash = Animator.StringToHash(EnterCompleteTrigger);

            ExitingBoolHash = Animator.StringToHash(ExitingBool);
            ExitBeginTriggerHash = Animator.StringToHash(ExitBeginTrigger);
            ExitCompleteTriggerHash = Animator.StringToHash(ExitCompleteTrigger);
            #endregion
        }

        public override void Enter()
        {
            base.Enter();

            OnEnterBegin.Invoke();
            if (Animator != null && EnterBeginTriggerHash != 0)
            {
                Animator.SetTrigger(EnterBeginTriggerHash);
                Animator.Update(0f);
            }

            StopTriggerCoroutine();
            if (EnterToEnterComplete != default(TransitionWaitModule))
                TriggerCoroutine = StartCoroutine(WaitForTransition(EnterToEnterComplete, EnterComplete));
        }

        public override void EnterComplete()
        {
            base.EnterComplete();

            OnEnterComplete.Invoke();
            if (Animator != null && EnterCompleteTriggerHash != 0)
            {
                Animator.SetTrigger(EnterCompleteTriggerHash);
                Animator.Update(0f);
            }

            StopTriggerCoroutine();
            if (EnterCompleteToExit != default(TransitionWaitModule))
                TriggerCoroutine = StartCoroutine(WaitForTransition(EnterCompleteToExit, Exit));
        }

        public override void Exit()
        {
            base.Exit();

            OnExitBegin.Invoke();
            if (Animator != null && ExitBeginTriggerHash != 0)
                Animator.SetTrigger(ExitBeginTriggerHash);

            StopTriggerCoroutine();
            if (ExitToExitComplete != default(TransitionWaitModule))
                TriggerCoroutine = StartCoroutine(WaitForTransition(ExitToExitComplete, ExitComplete));
        }

        public override void ExitComplete()
        {
            base.ExitComplete();

            OnExitComplete.Invoke();
            if (Animator != null && ExitCompleteTriggerHash != 0)
                Animator.SetTrigger(ExitCompleteTriggerHash);

            if (TriggerCoroutine != null)
            {
                StopCoroutine(TriggerCoroutine);
                TriggerCoroutine = null;
            }
        }

        public override void Play()
        {
            base.Play();

            OnPlay.Invoke();
            if (Animator != null && PlayTriggerHash != 0)
                Animator.SetTrigger(PlayTriggerHash);
        }

        public override void Pause()
        {
            base.Pause();

            OnPause.Invoke();
            if (Animator != null && PauseTriggerHash != 0)
                Animator.SetTrigger(PauseTriggerHash);
        }

        public override void Stop()
        {
            base.Stop();

            OnStop.Invoke();
            if (Animator != null && StopTriggerHash != 0)
                Animator.SetTrigger(StopTriggerHash);
        }

        public void StopTriggerCoroutine()
        {
            if (TriggerCoroutine == null) return;
            StopCoroutine(TriggerCoroutine);
            TriggerCoroutine = null;
        }

        protected virtual IEnumerator WaitForTransition(TransitionWaitModule trigger, Action callback)
        {
            if (trigger.Time > 0f)
            {
                yield return new WaitForSeconds(trigger.Time);
            }
            else if (!string.IsNullOrEmpty(trigger.Animation) && Animator != null)
            {
                yield return new WaitWhile(() => Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f &&
                                                 Animator.GetCurrentAnimatorStateInfo(0).IsName(trigger.Animation));
            }
            else if (trigger.Transition != null)
            {
                yield return new WaitWhile(() => trigger.Transition.IsPlaying);
            }
            else if (trigger.Sound != null)
            {
                SoundManager.Instance.StopBGM();
                SoundManager.Instance.PlayJingle(trigger.Sound);
                yield return new WaitWhile(() => SoundManager.Instance.JingleSource.isPlaying);
            }
            else
            {
                yield break;
            }

            callback();
        }
    }
}
