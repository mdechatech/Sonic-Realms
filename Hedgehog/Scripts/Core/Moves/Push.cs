using UnityEngine;

namespace Hedgehog.Core.Moves
{
    public class Push : Move
    {
        #region Control
        /// <summary>
        /// Name of the input axis.
        /// </summary>
        [Tooltip("Name of the input axis.")]
        public string ActivateAxis;

        /// <summary>
        /// Whether to use the input axis for GroundControl and/or AirControl.
        /// </summary>
        [Tooltip("Whether to use the input axis for GroundControl and/or AirControl.")]
        public bool UseControlInput;
        #endregion

        public override void Reset()
        {
            base.Reset();
            UseControlInput = true;
            ActivateAxis = "";
        }

        public override void Start()
        {
            base.Start();
            if (!UseControlInput) return;

            var groundControl = Manager.GetMove<GroundControl>();
            if (groundControl == null)
            {
                var airControl = Manager.GetMove<AirControl>();
                if (airControl == null)
                    return;

                ActivateAxis = airControl.MovementAxis;
                return;
            }

            ActivateAxis = groundControl.MovementAxis;
        }

        public override bool Available()
        {
            return Controller.Grounded && (Controller.LeftWall != null || Controller.RightWall != null);
        }

        public override bool InputActivate()
        {
            return Input.GetButton(ActivateAxis);
        }

        public override bool InputDeactivate()
        {
            return !InputActivate() || !Available();
        }
    }
}
