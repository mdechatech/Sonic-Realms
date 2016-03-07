using UnityEngine;

namespace SonicRealms.Core.Moves
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

        public override MoveLayer Layer
        {
            get { return MoveLayer.Action; }
        }

        public override bool Available
        {
            get { return Controller.Grounded && (Controller.LeftWall != null || Controller.RightWall != null); }
        }

        public override bool ShouldPerform
        {
            get { return Input.GetButton(ActivateAxis); }
        }

        public override bool ShouldEnd
        {
            get { return !ShouldPerform || !Available; }
        }

        public override void Reset()
        {
            base.Reset();
            UseControlInput = true;
            ActivateAxis = "";
        }

        public override void Start()
        {
            if (!UseControlInput) return;

            var groundControl = Manager.Get<GroundControl>();
            if (groundControl == null)
            {
                var airControl = Manager.Get<AirControl>();
                if (airControl == null)
                    return;

                ActivateAxis = airControl.MovementAxis;
                return;
            }

            ActivateAxis = groundControl.MovementAxis;
        }
    }
}
