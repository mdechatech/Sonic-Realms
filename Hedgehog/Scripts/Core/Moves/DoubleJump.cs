using UnityEngine;

namespace Hedgehog.Core.Moves
{
    /// <summary>
    /// Sonic 3K style double jump move. Is usable only after leaving the ground by jumping.
    /// </summary>
    public abstract class DoubleJump : Move
    {
        protected Jump Jump;
        protected Roll Roll;
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
            Jump.OnActive.AddListener(OnJump);
            InputName = Jump.ActivateButton;

            Roll = Manager.GetMove<Roll>();
            if (Roll == null) return;
            Roll.OnEnd.AddListener(OnRollEnd);
        }

        protected void OnRollEnd()
        {
            if (!Controller.Grounded) Used = true;
        }

        protected void OnJump()
        {
            // Double jump move becomes available again when the jump begins
            Used = false;
        }

        public override bool Available()
        {
            return !Controller.Grounded && Jump.Used && !Used && Roll.Active;
        }

        public override bool InputActivate()
        {
            return Input.GetButtonDown(InputName);
        }

        public override void OnActiveEnter()
        {
            // Become unavailable once used
            Used = true;
            Manager.Perform<AirControl>();
        }
    }
}
