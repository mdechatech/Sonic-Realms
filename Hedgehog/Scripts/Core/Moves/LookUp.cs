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
        public string InputAxis;

        public override void Reset()
        {
            base.Reset();

            InputAxis = "Vertical";
        }

        public override bool Available()
        {
            return DMath.Equalsf(Controller.GroundVelocity) && !Controller.GroundControl.Accelerating &&
                   !Controller.GroundControl.Braking;
        }

        public override bool InputActivate()
        {
            return Input.GetAxis(InputAxis) > 0.0f;
        }

        public override bool InputDeactivate()
        {
            return Input.GetAxis(InputAxis) <= 0.0f || !DMath.Equalsf(Controller.GroundVelocity);
        }
    }
}
