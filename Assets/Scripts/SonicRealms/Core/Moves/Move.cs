using System;
using SonicRealms.Core.Actors;
using SonicRealms.Level;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace SonicRealms.Core.Moves
{
    public class Move : MonoBehaviour
    {
        /// <summary>
        /// Reference to the attached controller.
        /// </summary>
        [HideInInspector]
        public HedgehogController Controller;

        /// <summary>
        /// Reference to the associated move manager.
        /// </summary>
        [HideInInspector]
        public MoveManager Manager;

        /// <summary>
        /// Reference to the powerup manager, if any.
        /// </summary>
        [HideInInspector]
        public PowerupManager PowerupManager;

        /// <summary>
        /// Reference to the attached animator.
        /// </summary>
        [AnimationFoldout]
        public Animator Animator;

        /// <summary>
        /// The move's current state.
        /// </summary>
        [DebugFoldout]
        public State CurrentState;
        public enum State   
        {
            Unavailable,    // Can't be performed unless forced to.
            Available,      // Can be performed through player input.
            Active,         // Currently being performed.
        }

        /// <summary>
        /// Whether the move is currently active.
        /// </summary>
        public bool Active
        {
            get { return CurrentState == State.Active; }
            set { ChangeState(value ? State.Active : CurrentState == State.Active ? State.Available : CurrentState); }
        }

        /// <summary>
        /// The move's layer. A layer only has one active move at any time.
        /// </summary>
        public virtual MoveLayer Layer
        {
            get { return MoveLayer.None; }
        }

        /// <summary>
        /// Whether the move can be used through the usual means (ShouldPerform).
        /// </summary>
        [DebugFoldout]
        [Tooltip("Whether the move can be used through the usual means (ShouldPerform).")]
        public bool AllowShouldPerform;

        /// <summary>
        /// Whether the move can be ended through the usual means (ShouldEnd).
        /// </summary>
        [DebugFoldout]
        [Tooltip("Whether the move can be ended through the usual means (ShouldEnd).")]
        public bool AllowShouldEnd;

        #region Events
        /// <summary>
        /// Invoked when the move is performed.
        /// </summary>
        [EventsFoldout]
        public UnityEvent OnActive;

        /// <summary>
        /// Invoked when the move is ended.
        /// </summary>
        [EventsFoldout]
        public UnityEvent OnEnd;

        /// <summary>
        /// Invoked when the move becomes available.
        /// </summary>
        [EventsFoldout]
        public UnityEvent OnAvailable;

        /// <summary>
        /// Invoked when the move becomes unavailable.
        /// </summary>
        [EventsFoldout]
        public UnityEvent OnUnavailable;

        /// <summary>
        /// Invoked when the move is added to a manager.
        /// </summary>
        [EventsFoldout]
        public UnityEvent OnAdd;

        /// <summary>
        /// Invoked when the move is removed from a manager.
        /// </summary>
        [EventsFoldout]
        public UnityEvent OnRemove;
        #endregion
        #region Animation
        /// <summary>
        /// Name of an Animator trigger set when the move is activated.
        /// </summary>
        [AnimationFoldout]
        [Tooltip("Name of an Animator trigger set when the move is activated.")]
        public string ActiveTrigger;
        protected int ActiveTriggerHash;

        /// <summary>
        /// Name of an Animator bool set to whether the move is active.
        /// </summary>
        [AnimationFoldout]
        [Tooltip("Name of an Animator bool set to whether the move is active.")]
        public string ActiveBool;
        protected int ActiveBoolHash;

        /// <summary>
        /// Name of an Animator bool set to whether the move is available.
        /// </summary>
        [AnimationFoldout]
        [Tooltip("Name of an Animator bool set to whether the move is available.")]
        public string AvailableBool;
        protected int AvailableBoolHash;
        #endregion
        #region Sound
        /// <summary>
        /// Whether to mute audio clips the move plays.
        /// </summary>
        [SoundFoldout]
        [Tooltip("Whether to mute audio clips the move plays.")]
        public bool Muted;

        /// <summary>
        /// An audio clip to play when the move is performed.
        /// </summary>
        [SoundFoldout]
        [FormerlySerializedAs("ActiveEnterSound")]
        [Tooltip("An audio clip to play when the move is performed.")]
        public AudioClip PerformSound;

        /// <summary>
        /// An audio clip to play when the move is ended.
        /// </summary>
        [SoundFoldout]
        [FormerlySerializedAs("ActiveExitSound")]
        [Tooltip("An audio clip to play when the move is ended.")]
        public AudioClip EndSound;
        #endregion

        public virtual void Reset()
        {
            Controller = GetComponentInParent<HedgehogController>();
            if(Controller) Animator = Controller.Animator;
            ActiveTrigger = ActiveBool = AvailableBool = "";

            OnActive = new UnityEvent();
            OnEnd = new UnityEvent();
            OnAvailable = new UnityEvent();
            OnAdd = new UnityEvent();
            OnRemove = new UnityEvent();

            PerformSound = EndSound = null;
        }

        public virtual void Awake()
        {
            OnActive = OnActive ?? new UnityEvent();
            OnEnd = OnEnd ?? new UnityEvent();
            OnAvailable = OnAvailable ?? new UnityEvent();
            OnUnavailable = OnUnavailable ?? new UnityEvent();
            OnAdd = OnAdd ?? new UnityEvent();
            OnRemove = OnRemove ?? new UnityEvent();

            ActiveTriggerHash = string.IsNullOrEmpty(ActiveTrigger) ? 0 : Animator.StringToHash(ActiveTrigger);
            ActiveBoolHash = string.IsNullOrEmpty(ActiveBool) ? 0 : Animator.StringToHash(ActiveBool);
            AvailableBoolHash = string.IsNullOrEmpty(AvailableBool) ? 0 : Animator.StringToHash(AvailableBool);

            CurrentState = State.Unavailable;
            AllowShouldPerform = AllowShouldEnd = true;

            if (Manager == null) enabled = false;
        }

        public virtual void OnEnable()
        {
            // I'm here for consistency!
        }

        public virtual void OnDisable()
        {
            End();
        }

        public virtual void Start()
        {
            Controller = Controller ? Controller : GetComponentInParent<HedgehogController>();
            Animator = Animator ? Animator : Controller.Animator;
            Manager = Manager ? Manager : Controller.GetComponent<MoveManager>();
        }

        public virtual void Update()
        {
            if(Animator != null)
                SetAnimatorParameters();

            if (Active && !Controller.Interrupted)
                OnActiveUpdate();
        }

        public virtual void FixedUpdate()
        {
            if (Active && !Controller.Interrupted)
                OnActiveFixedUpdate();
        }

        /// <summary>
        /// Set animator parameters here. Called only if the object has an Animator component.
        /// </summary>
        public virtual void SetAnimatorParameters()
        {
            if(ActiveBoolHash != 0)
                Animator.SetBool(ActiveBoolHash, CurrentState == State.Active);

            if(AvailableBoolHash != 0)
                Animator.SetBool(AvailableBoolHash, CurrentState == State.Available);
        }

        /// <summary>
        /// Changes the move's state to the one specified.
        /// </summary>
        /// <param name="nextState">The specified state.</param>
        /// <param name="mute">Whether to mute sounds during the state change.</param>
        /// <returns>Whether the move's state is different from its previous.</returns>
        public bool ChangeState(State nextState, bool mute = false)
        {
            if (nextState == CurrentState) return false;

            var muted = Muted;
            Muted = mute;

            var prevState = CurrentState;
            CurrentState = nextState;
            OnStateChange(prevState);

            if (prevState == State.Active)
            {
                if(!Muted && EndSound != null)
                    SoundManager.Instance.PlayClipAtPoint(EndSound, transform.position);
                OnActiveExit();
                OnEnd.Invoke();
            }
            else if (CurrentState == State.Active)
            {
                if(!Muted && PerformSound != null)
                    SoundManager.Instance.PlayClipAtPoint(PerformSound, transform.position);
                OnActiveEnter();
                OnActiveEnter(prevState);
                OnActive.Invoke();

                if (Animator == null)
                {
                    Muted = muted;
                    return true;
                }

                if(ActiveTriggerHash != 0)
                    Animator.SetTrigger(ActiveTriggerHash);
            }
            else if (CurrentState == State.Available)
            {
                OnAvailable.Invoke();
            }
            else if (CurrentState == State.Unavailable)
            {
                OnUnavailable.Invoke();
            }

            Muted = muted;
            return true;
        }

        /// <summary>
        /// Tells the manager to perform this move.
        /// </summary>
        /// <param name="force">Whether to perform the move even if it's unavailable.</param>
        /// <param name="mute">Whether to mute sounds the move would normally play.</param>
        /// <returns>Whether the move was performed.</returns>
        public bool Perform(bool force = false, bool mute = false)
        {
            return Manager && Manager.Perform(this, force, mute);
        }

        /// <summary>
        /// Tells the manager to end this move.
        /// </summary>
        /// <returns>Whether the move was ended.</returns>
        public bool End()
        {
            if (Manager == null) return false;
            return Manager.End(this);
        }

        /// <summary>
        /// Adds the move to the specified move manager.
        /// </summary>
        /// <param name="manager">The specified move manager.</param>
        public void Add(MoveManager manager)
        {
            manager.Add(this);
        }

        /// <summary>
        /// Tells the manager to remove this move. WARNING: DESTROYS THE ENTIRE GAMEOBJECT.
        /// </summary>
        public void Remove()
        {
            Manager.Remove(this);
        }

        public void NotifyManagerAdd(MoveManager manager)
        {
            enabled = true;

            Manager = manager;
            PowerupManager = manager.GetComponent<PowerupManager>();
            Controller = manager.Controller;
            if (!Animator && Controller) Animator = Controller.Animator;

            // Pretend the move is available to see if it wants to be performed on init
            CurrentState = State.Available;

            OnManagerAdd();
            OnAdd.Invoke();

            // If it performed itself we can exit here
            if (Active) return;

            if (!Available) CurrentState = State.Unavailable;
        }

        public void NotifyManagerRemove(MoveManager manager)
        {
            OnManagerRemove();

            if (Controller && Animator == Controller.Animator)
                Animator = null;
            Manager = null;
            Controller = null;

            enabled = false;
        }

        /// <summary>
        /// Called when the manager adds this move to its move list.
        /// </summary>
        public virtual void OnManagerAdd()
        {

        }
        
        /// <summary>
        /// Called when the manager removes this move from its move list.
        /// </summary>
        public virtual void OnManagerRemove()
        {

        }

        /// <summary>
        /// Let the controller know your move can be triggered through input here.
        /// </summary>
        /// <value></value>
        public virtual bool Available
        {
            get { return true; }
        }

        /// <summary>
        /// Let the controller know your move should be performed based on the current input here.
        /// </summary>
        /// <value></value>
        public virtual bool ShouldPerform
        {
            get { return false; }
        }

        /// <summary>
        /// Let the controller know your move should be ended based on the current conditions here.
        /// </summary>
        /// <value></value>
        public virtual bool ShouldEnd
        {
            get { return false; }
        }

        /// <summary>
        /// Called when the move's state changes.
        /// </summary>
        /// <param name="previousState">The move's previous state.</param>
        public virtual void OnStateChange(State previousState)
        {

        }

        /// <summary>
        /// Called when the move is activated.
        /// </summary>
        public virtual void OnActiveEnter()
        {

        }

        /// <summary>
        /// Called when the move is activated.
        /// </summary>
        public virtual void OnActiveEnter(State previousState)
        {

        }

        /// <summary>
        /// Called on Update while the move is activated.
        /// </summary>
        public virtual void OnActiveUpdate()
        {

        }

        /// <summary>
        /// Called on FixedUpdate while the move is activated.
        /// </summary>
        public virtual void OnActiveFixedUpdate()
        {

        }

        /// <summary>
        /// Called when the move is deactivated.
        /// </summary>
        public virtual void OnActiveExit()
        {

        }
    }
}
