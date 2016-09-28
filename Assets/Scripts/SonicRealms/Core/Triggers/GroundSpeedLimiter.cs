using System;
using SonicRealms.Core.Actors;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// On a call to Allows(), this object returns true or false based on groundspeed related criteria.
    /// </summary>
    [Serializable]
    public class GroundSpeedLimiter :
        ITriggerLimiter<AreaCollision>,
        ITriggerLimiter<AreaCollision.Contact>,
        ITriggerLimiter<PlatformCollision>,
        ITriggerLimiter<PlatformCollision.Contact>,
        ITriggerLimiter<SurfaceCollision>,
        ITriggerLimiter<SurfaceCollision.Contact>,
        ITriggerLimiter<HedgehogController>
    {
        /// <summary>
        /// If true, the limiter will always return false when the controller is airborne.
        /// </summary>
        [Tooltip("If true, the limiter will always return false when the controller is airborne.")]
        public bool MustBeGrounded;

        /// <summary>
        /// If true, the values below will be compared to the absolute value of the player's ground velocity.
        /// </summary>
        [Space]
        [Tooltip("If true, the values below will be compared to the absolute value of the player's ground velocity.")]
        public bool UseAbsoluteValue;

        /// <summary>
        /// The player's minimum ground speed.
        /// </summary>
        [Tooltip("The player's minimum ground speed.")]
        public float Min = -100;

        /// <summary>
        /// The player's maximum ground speed.
        /// </summary>
        [Tooltip("The player's maximum ground speed.")]
        public float Max = 100;

        public bool Allows(AreaCollision collision)
        {
            return Allows(collision.Latest);
        }

        public bool Allows(AreaCollision.Contact contact)
        {
            return Allows(contact.GroundVelocity, contact.Controller.Grounded);
        }

        public bool Allows(PlatformCollision collision)
        {
            return Allows(collision.Latest);
        }

        public bool Allows(PlatformCollision.Contact contact)
        {
            return Allows(contact.GroundVelocity, contact.Controller.Grounded);
        }

        public bool Allows(SurfaceCollision collision)
        {
            return Allows(collision.Latest);
        }

        public bool Allows(SurfaceCollision.Contact contact)
        {
            return Allows(contact.GroundVelocity, contact.Controller.Grounded);
        }

        public bool Allows(HedgehogController controller)
        {
            return Allows(controller.GroundVelocity, controller.Grounded);
        }

        public bool Allows(float groundSpeed, bool grounded)
        {
            if (MustBeGrounded && !grounded)
                return false;

            if (UseAbsoluteValue)
                groundSpeed = Mathf.Abs(groundSpeed);

            return groundSpeed >= Min && groundSpeed <= Max;
        }
    }
}
