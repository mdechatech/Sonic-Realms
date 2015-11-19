using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Core.Moves
{
    public class LookUp : Move
    {
        /// <summary>
        /// This input must be a positive axis to activate.
        /// </summary>
        [SerializeField]
        [Tooltip("This input must be a positive axis to activate.")]
        public string ActivateAxis;

        private GroundControl groundControl;

        public override void Reset()
        {
            base.Reset();

            ActivateAxis = "Vertical";
        }

        public override void Start()
        {
            base.Start();
            groundControl = Controller.GetMove<GroundControl>();
        }

        public override bool Available()
        {
            return DMath.Equalsf(Controller.GroundVelocity) &&
                   (groundControl == null || 
                   (!groundControl.Braking && !groundControl.Accelerating));
        }

        public override bool InputActivate()
        {
            return Input.GetAxis(ActivateAxis) > 0.0f;
        }

        public override bool InputDeactivate()
        {
            return Input.GetAxis(ActivateAxis) <= 0.0f || !DMath.Equalsf(Controller.GroundVelocity);
        }
    }
}
