using UnityEngine;

namespace Hedgehog.Core.Moves
{
    /// <summary>
    /// Sonic 3K style double jump move. Is usable only after leaving the ground by jumping.
    /// </summary>
    public class DoubleJumpMove : Move
    {
        protected Jump Jump;
        protected string InputName;

        [HideInInspector]
        public bool Used;

        public override void Awake()
        {
            base.Awake();
            Used = false;
        }

        public override void OnManagerAdd()
        {
            Jump = Manager.GetMove<Jump>();
            InputName = Jump.ActivateButton;
        }

        public override bool Available()
        {
            return !Controller.Grounded && Jump.Used && !Used;
        }

        public override bool InputActivate()
        {
            return Input.GetButtonDown(InputName);
        }

        public override void OnActiveEnter(State previousState)
        {
            Used = true;
            Manager.Perform<AirControl>();
            Controller.OnAttach.AddListener(OnAttach);
        }

        private void OnAttach()
        {
            Used = false;
            Controller.OnAttach.RemoveListener(OnAttach);
        }
    }
}
