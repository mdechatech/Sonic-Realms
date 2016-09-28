using System;
using SonicRealms.Core.Actors;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// On a call to Allows(), this object returns true or false based on whether the controller is grounded.
    /// </summary>
    [Serializable]
    public class GroundedLimiter : ITriggerLimiter<HedgehogController>
    {
        /// <summary>
        /// If true, the limiter will return false unless the controller is airborne.
        /// </summary>
        [Tooltip("If true, the limiter will return false unless the controller is airborne.")]
        public bool MustBeAirborne;

        /// <summary>
        /// If true, the limiter will return false unless the controller is grounded.
        /// </summary>
        [Tooltip("If true, the limiter will return false unless the controller is grounded.")]
        public bool MustBeGrounded;

        public bool Allows(HedgehogController controller)
        {
            if (MustBeAirborne && controller.Grounded)
                return false;

            if (MustBeGrounded && !controller.Grounded)
                return false;

            return true;
        }
    }
}
