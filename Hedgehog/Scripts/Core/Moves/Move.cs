using Hedgehog.Core.Actors;
using Hedgehog.Level;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Hedgehog.Core.Moves
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
        /// Default groups a move is in, which is none.
        /// </summary>
        public static readonly MoveGroup[] DefaultGroups = {MoveGroup.All};

        /// <summary>
        /// List of groups the move is in. The move can be found by its move group.
        /// </summary>
        public virtual MoveGroup[] Groups
        {
            get { return DefaultGroups; }
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
            Animator = Controller.Animator;
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
            Manager = Manager ? Manager : Controller.MoveManager;
        }

        public virtual void Update()
        {
            if(Animator != null)
                SetAnimatorParameters();
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
                    SoundManager.PlayClipAtPoint(EndSound, transform.position);
                OnActiveExit();
                OnEnd.Invoke();
            }
            else if (CurrentState == State.Active)
            {
                if(!Muted && PerformSound != null)
                    SoundManager.PlayClipAtPoint(PerformSound, transform.position);
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
            return Manager && Manager.End(this);
        }

        /// <summary>
        /// Tells the manager to remove this move. WARNING: DESTROYS THE ENTIRE GAMEOBJECT.
        /// </summary>
        public void Remove()
        {
            Remove(true);
        }

        /// <summary>
        /// Tells the manager to remove this move.
        /// <param name="destroyGameObject">Whether to destroy the move's object. Great for powerups, not so much
        /// for moves.</param>
        /// </summary>
        public void Remove(bool destroyGameObject)
        {
            if (destroyGameObject) Manager.RemovePowerup(gameObject);
            else Destroy(this);
        }

        /// <summary>
        /// Tells the manager to transfer this move to the specified target. WARNING: THIS TRANSFERS THE ENTIRE
        /// GAMEOBJECT.
        /// </summary>
        /// <param name="target">The target move manager.</param>
        public void Transfer(MoveManager target)
        {
            Manager.TransferPowerup(target.gameObject);
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
