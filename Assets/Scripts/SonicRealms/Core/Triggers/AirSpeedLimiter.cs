using System;
using SonicRealms.Core.Actors;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// On a call to Allows(), this object returns true or false based on airspeed related criteria.
    /// </summary>
    [Serializable]
    public class AirSpeedLimiter :
        ITriggerLimiter<AreaCollision>,
        ITriggerLimiter<AreaCollision.Contact>,
        ITriggerLimiter<PlatformCollision>,
        ITriggerLimiter<PlatformCollision.Contact>,
        ITriggerLimiter<SurfaceCollision>,
        ITriggerLimiter<SurfaceCollision.Contact>,
        ITriggerLimiter<HedgehogController>
    {
        /// <summary>
        /// If true, the limiter will always return false when the controller is grounded.
        /// </summary>
        [Tooltip("If true, the limiter will always return false when the controller is grounded.")]
        public bool MustBeAirborne;

        /// <summary>
        /// If true, the values below will be compared to the absolute value of the player's air speed.
        /// </summary>
        [Space]
        [Tooltip("If true, the values below will be compared to the absolute value of the player's air speed.")]
        public bool UseAbsoluteValue;

        /// <summary>
        /// The player's minimum air speed.
        /// </summary>
        [Tooltip("The player's minimum air speed.")]
        public float Min = -100;

        /// <summary>
        /// The player's maximum air speed.
        /// </summary>
        [Tooltip("The player's maximum air speed.")]
        public float Max = 100;

        public bool Allows(AreaCollision collision)
        {
            return Allows(collision.Latest);
        }

        public bool Allows(AreaCollision.Contact contact)
        {
            return Allows(contact.Velocity.magnitude, contact.Controller.Grounded);
        }

        public bool Allows(PlatformCollision collision)
        {
            return Allows(collision.Latest);
        }

        public bool Allows(PlatformCollision.Contact contact)
        {
            return Allows(contact.Velocity.magnitude, contact.Controller.Grounded);
        }

        public bool Allows(SurfaceCollision collision)
        {
            return Allows(collision.Latest);
        }

        public bool Allows(SurfaceCollision.Contact contact)
        {
            return Allows(contact.Velocity.magnitude, contact.Controller.Grounded);
        }

        public bool Allows(HedgehogController controller)
        {
            return Allows(controller.Velocity.magnitude, controller.Grounded);
        }

        public bool Allows(float airSpeed, bool grounded)
        {
            if (MustBeAirborne && grounded)
                return false;

            if (UseAbsoluteValue)
                airSpeed = Mathf.Abs(airSpeed);

            return airSpeed >= Min && airSpeed <= Max;
        }
    }
}
