using UnityEngine;

namespace Hedgehog.Core.Moves
{
    public class Duck : Move
    {
        #region Controls
        /// <summary>
        /// Input string used for activation.
        /// </summary>
        [SerializeField]
        [Tooltip("Input string used for activation.")]
        public string ActivateAxis;

        /// <summary>
        /// Whether to activate when the input is in the opposite direction (if ActivateInput is "Vertical" and this
        /// is true, activates when input moves down instead of up).
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to activate when the input is in the opposite direction (if ActivateInput is \"Vertical\" " +
                 "and this is true, activates when input moves down instead of up.")]
        public bool RequireNegative;

        /// <summary>
        /// Maximum speed at which ducking begins.
        /// </summary>
        [SerializeField]
        [Tooltip("Maximum speed at which ducking begins.")]
        public float MaxActivateSpeed;
        #endregion

        protected GroundControl GroundControl;

        public override void Reset()
        {
            base.Reset();
            
            MaxActivateSpeed = 0.61875f;
            ActivateAxis = "Vertical";
            RequireNegative = true;
        }

        public override void OnManagerAdd()
        {
            GroundControl = Manager.GetMove<GroundControl>();
        }

        public override bool Available
        {
            get
            {
                var groundControl = Controller.GroundControl;

                return Controller.Grounded && Mathf.Abs(Controller.GroundVelocity) < MaxActivateSpeed &&
                       (groundControl == null || !groundControl.Accelerating) && !Manager.IsActive<Roll>();
            }
        }

        public override bool ShouldPerform
        {
            get
            {
                return RequireNegative
                    ? Input.GetAxis(ActivateAxis) < 0.0f
                    : Input.GetAxis(ActivateAxis) > 0.0f;
            }
        }

        public override bool ShouldEnd
        {
            get { return !Input.GetButton(ActivateAxis) || !Available; }
        }

        public override void OnActiveEnter()
        {
            GroundControl.ControlLocked = true;
        }

        public override void OnActiveExit()
        {
            GroundControl.ControlLocked = false;
        }
    }
}
