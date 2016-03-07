using UnityEngine;

namespace SonicRealms.Core.Moves
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

        public override MoveLayer Layer
        {
            get { return MoveLayer.Action; }
        }

        public override void Reset()
        {
            base.Reset();
            
            MaxActivateSpeed = 0.61875f;
            ActivateAxis = "Vertical";
            RequireNegative = true;
        }

        public override void OnManagerAdd()
        {
            GroundControl = Manager.Get<GroundControl>();
        }

        public override bool Available
        {
            get
            {
                return Controller.Grounded && Mathf.Abs(Controller.GroundVelocity) < MaxActivateSpeed &&
                       (GroundControl == null || !GroundControl.Accelerating) && 
                       Manager[MoveLayer.Action] == null && Manager[MoveLayer.Roll] == null;
            }
        }

        public override bool ShouldPerform
        {
            get { return CheckAxis(); }
        }

        public override bool ShouldEnd
        {
            get
            {
                return !CheckAxis() ||
                       (!Controller.Grounded || Mathf.Abs(Controller.GroundVelocity) >= MaxActivateSpeed ||
                        (GroundControl != null && GroundControl.Accelerating));
            }
        }

        public override void OnActiveEnter()
        {
            GroundControl.ControlLockTimerOn = true;
        }

        public override void OnActiveExit()
        {
            GroundControl.ControlLockTimerOn = false;
        }

        public bool CheckAxis()
        {
            return RequireNegative
                    ? Input.GetAxis(ActivateAxis) < 0.0f
                    : Input.GetAxis(ActivateAxis) > 0.0f;
        }
    }
}
