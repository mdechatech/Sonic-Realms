using Hedgehog.Core.Actors;
using UnityEngine;

namespace Hedgehog.Core.Moves
{
    public class ControlState : MonoBehaviour
    {
        public bool IsCurrent;

        protected HedgehogController Controller;
        protected Animator Animator;

        public virtual void Awake()
        {
            IsCurrent = false;
            Controller = GetComponent<HedgehogController>();
            Animator = Controller.Animator;
        }

        public virtual void Update()
        {
            GetInput();

            if(Animator != null)
                SetAnimatorParameters();
        }

        /// <summary>
        /// Get and store input for the next FixedUpdate here.
        /// </summary>
        public virtual void GetInput()
        {

        }

        /// <summary>
        /// Set animator parameters here. Called only if the object has an Animator component.
        /// </summary>
        public virtual void SetAnimatorParameters()
        {

        }

        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        /// <param name="previousState">The control state before </param>
        public virtual void OnStateEnter(ControlState previousState)
        {

        }

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        /// <param name="nextState">The control state that will be entered next.</param>
        public virtual void OnStateExit(ControlState nextState)
        {

        }

        /// <summary>
        /// Called on Update while this is the current state.
        /// </summary>
        public virtual void OnStateUpdate()
        {

        }

        /// <summary>
        /// Called on FixedUpdate while this is current state.
        /// </summary>
        public virtual void OnStateFixedUpdate()
        {

        }
    }
}
