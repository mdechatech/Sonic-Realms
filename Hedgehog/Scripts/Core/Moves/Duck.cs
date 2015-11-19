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

        public override void Reset()
        {
            base.Reset();
            
            MaxActivateSpeed = 0.61875f;
            ActivateAxis = "Vertical";
            RequireNegative = true;
        }

        public override bool Available()
        {
            var groundControl = Controller.GetMove<GroundControl>();

            return Mathf.Abs(Controller.GroundVelocity) < MaxActivateSpeed &&
                   (groundControl == null || !groundControl.Accelerating);
        }

        public override bool InputActivate()
        {
            return RequireNegative
                ? Input.GetAxis(ActivateAxis) < 0.0f
                : Input.GetAxis(ActivateAxis) > 0.0f;
        }

        public override bool InputDeactivate()
        {
            return !Input.GetButton(ActivateAxis) || !Available();
        }
    }
}
