using UnityEngine;

namespace SonicRealms.Core.Moves
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
            Jump = Manager.Get<Jump>();
            Jump.OnActive.AddListener(OnJump);
            InputName = Jump.ActivateButton;

            Roll = Manager.Get<Roll>();
            if (Roll == null) return;
            Roll.OnEnd.AddListener(OnRollEnd);
        }

        protected void OnRollEnd()
        {
            // Becomes unusable if our roll ends, caused by some outside force such as springs
            if (!Controller.Grounded) Used = true;
        }

        protected void OnJump()
        {
            // Double jump move becomes available again when the jump begins
            Used = false;
        }

        public override bool Available
        {
            get { return !Controller.Grounded && Jump.Used && !Used && Roll.Active; }
        }

        public override bool ShouldPerform
        {
            get { return Input.GetButtonDown(InputName); }
        }

        public override void OnActiveEnter()
        {
            // Double jump moves restore control
            Manager.Perform<AirControl>();
            Used = true;
        }

        public override void OnActiveUpdate()
        {
            if (Controller.Grounded) End();
        }
    }
}
