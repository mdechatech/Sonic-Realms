using Hedgehog.Core.Actors;
using UnityEngine;

namespace Hedgehog.Core.Moves
{
    public class Move : MonoBehaviour
    {
        /// <summary>
        /// Reference to the attached controller.
        /// </summary>
        protected HedgehogController Controller;

        /// <summary>
        /// Reference to the attached animator.
        /// </summary>
        protected Animator Animator;

        /// <summary>
        /// The move's current state.
        /// </summary>
        public State CurrentState;
        public enum State
        {
            Unavailable,
            Available,
            Active,
        }

        /// <summary>
        /// If the move is active, whether it was activated through player input.
        /// </summary>
        public bool InputActivated;
        #region Animation
        /// <summary>
        /// Name of an Animator trigger set when the move is activated.
        /// </summary>
        public string ActiveTrigger;

        /// <summary>
        /// Name of an Animator bool set to whether the move is active.
        /// </summary>
        public string ActiveBool;

        /// <summary>
        /// Name of an Animator bool set to whether the move is available.
        /// </summary>
        public string AvailableBool;
        #endregion

        public virtual void Reset()
        {
            Controller = GetComponent<HedgehogController>();
            Animator = Animator ?? Controller == null ? null : Controller.Animator;

            ActiveTrigger = AvailableBool = "";
        }

        public virtual void Awake()
        {
            Controller = Controller ?? GetComponent<HedgehogController>();
            Animator = Animator ?? Controller.Animator;

            CurrentState = State.Unavailable;
            InputActivated = false;
        }

        public virtual void Start()
        {
            Animator = Controller.Animator;
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
            if(ActiveBool.Length > 0)
                Animator.SetBool(ActiveBool, CurrentState == State.Active);

            if(AvailableBool.Length > 0)
                Animator.SetBool(AvailableBool, CurrentState == State.Available);
        }

        /// <summary>
        /// Changes the move's state to the one specified.
        /// </summary>
        /// <param name="nextState">The specified state.</param>
        /// <returns>Whether the move's state is different from its previous.</returns>
        public bool ChangeState(State nextState)
        {
            if (nextState == CurrentState) return false;

            var prevState = CurrentState;
            CurrentState = nextState;
            OnStateChanged(prevState);

            if (prevState == State.Active) OnActiveExit();
            else if(CurrentState == State.Active) OnActiveEnter(prevState);

            if (Animator == null) return true;
            if (CurrentState == State.Active && ActiveTrigger.Length > 0)
            {
                Animator.SetTrigger(ActiveTrigger);
            }

            return true;
        }

        /// <summary>
        /// Calls on the controller to perform the move. Works only if the move is available.
        /// </summary>
        /// <returns>Whether the move was performed.</returns>
        public bool Perform()
        {
            return Controller.PerformMove(this);
        }

        /// <summary>
        /// Calls on the controller to force perform the move.
        /// </summary>
        /// <returns>Whether the move was performed. False could be returned because the move was already active.</returns>
        public bool ForcePerform()
        {
            return Controller.ForcePerformMove(this);
        }

        /// <summary>
        /// Calls on the controller to end the move. Only works if the move is active.
        /// </summary>
        /// <returns>Whether the move was ended.</returns>
        public bool End()
        {
            return Controller.EndMove(this);
        }

        /// <summary>
        /// Let the controller know your move can be triggered through input here.
        /// </summary>
        /// <returns></returns>
        public virtual bool Available()
        {
            return false;
        }

        /// <summary>
        /// Let the controller know your move should be activated based on the current input here.
        /// </summary>
        /// <returns></returns>
        public virtual bool InputActivate()
        {
            return false;
        }

        /// <summary>
        /// Let the controller know your move should be deactivated based on the current conditions here.
        /// </summary>
        /// <returns></returns>
        public virtual bool InputDeactivate()
        {
            return false;
        }
        
        /// <summary>
        /// Called when the move's state changes.
        /// </summary>
        /// <param name="previousState">The move's previous state.</param>
        public virtual void OnStateChanged(State previousState)
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
