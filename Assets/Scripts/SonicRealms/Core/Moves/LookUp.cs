using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Moves
{
    public class LookUp : Move
    {
        /// <summary>
        /// This input must be a positive axis to activate.
        /// </summary>
        [SerializeField]
        [Tooltip("This input must be a positive axis to activate.")]
        public string ActivateAxis;

        protected GroundControl GroundControl;

        public override int Layer
        {
            get { return (int)MoveLayer.Action; }
        }

        public override bool Available
        {
            get
            {
                return Controller.Grounded && SrMath.Equalsf(Controller.GroundVelocity) && !Manager[MoveLayer.Action] &&
                       (GroundControl == null || (!GroundControl.Braking && !GroundControl.Accelerating)) &&
                       !Manager[MoveLayer.Roll] && !Manager[MoveLayer.Action];
            }
        }

        public override bool ShouldPerform
        {
            get { return Input.GetAxis(ActivateAxis) > 0f; }
        }

        public override bool ShouldEnd
        {
            get
            {
                return !Controller.Grounded || !SrMath.Equalsf(Controller.GroundVelocity) ||
                       (GroundControl == null || !GroundControl.Standing) || Input.GetAxis(ActivateAxis) <= 0f;
            }
        }

        public override void Reset()
        {
            base.Reset();
            ActivateAxis = "Vertical";
        }

        public override void OnManagerAdd()
        {
            GroundControl = Manager.Get<GroundControl>();
        }
    }
}
