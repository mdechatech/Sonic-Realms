using System;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Internal;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// On a call to Allows(), this object returns true or false based on velocity related criteria.
    /// </summary>
    [Serializable]
    public class VelocityLimiter :
        ITriggerLimiter<AreaCollision>,
        ITriggerLimiter<AreaCollision.Contact>,
        ITriggerLimiter<PlatformCollision>,
        ITriggerLimiter<PlatformCollision.Contact>,
        ITriggerLimiter<SurfaceCollision>,
        ITriggerLimiter<SurfaceCollision.Contact>,
        ITriggerLimiter<HedgehogController>
    {
        /// <summary>
        /// If checked, the limiter won't limit the player unless it's airborne.
        /// </summary>
        [Tooltip("If checked, the limiter won't limit the player unless it's airborne.")]
        public bool ApplyWhenAirborne;

        /// <summary>
        /// If checked, the limiter won't limit the player unless it's grounded.
        /// </summary>
        [Tooltip("If checked, the limiter won't limit the player unless it's grounded.")]
        public bool ApplyWhenGrounded;

        /// <summary>
        /// If true, the values below will be compared to the player's velocity accounting for the object's rotation.
        /// </summary>
        [Space]
        [Tooltip("If true, the values below will be compared to the player's velocity accounting for the object's rotation.")]
        public bool RelativeToRotation;

        /// <summary>
        /// If true, the values below will be compared to the player's velocity relative to its gravity.
        /// </summary>
        [Tooltip("If checked, the values below will be compared to the player's velocity relative to its gravity.")]
        public bool RelativeToGravity;

        /// <summary>
        /// If true, the horizontal values below will be compared to the absolute value of the player's horizontal velocity.
        /// </summary>
        [Space]
        [Tooltip("If checked, the horizontal values below will be compared to the absolute value of the player's horizontal velocity.")]
        public bool UseAbsoluteHorizontal;

        /// <summary>
        /// The player's minimum horizontal speed.
        /// </summary>
        [Tooltip("The player's minimum horizontal speed.")]
        public float HorizontalMin = -100;

        /// <summary>
        /// The player's maximum horizontal speed.
        /// </summary>
        [Tooltip("The player's maximum horizontal speed.")]
        public float HorizontalMax = 100;

        /// <summary>
        /// If true, the vertical values below will be compared to the absolute value of the player's vertical velocity.
        /// </summary>
        [Space]
        [Tooltip("If checked, the vertical values below will be compared to the absolute value of the player's vertical velocity.")]
        public bool UseAbsoluteVertical;

        /// <summary>
        /// The player's minimum vertical speed.
        /// </summary>
        [Tooltip("The player's minimum vertical speed.")]
        public float VerticalMin = -100;

        /// <summary>
        /// The player's maximum vertical speed.
        /// </summary>
        [Tooltip("The player's maximum vertical speed.")]
        public float VerticalMax = 100;

        public bool Allows(AreaCollision collision)
        {
            return Allows(collision.Latest);
        }

        public bool Allows(AreaCollision.Contact contact)
        {
            return Allows(contact.Velocity, contact.RelativeVelocity, contact.AreaTrigger.transform.eulerAngles.z,
                contact.Controller.Grounded);
        }

        public bool Allows(PlatformCollision collision)
        {
            return Allows(collision.Latest);
        }

        public bool Allows(PlatformCollision.Contact contact)
        {
            return Allows(contact.Velocity, contact.RelativeVelocity, contact.HitData.Transform.eulerAngles.z,
                contact.Controller.Grounded);
        }

        public bool Allows(SurfaceCollision collision)
        {
            return Allows(collision.Latest);
        }

        public bool Allows(SurfaceCollision.Contact contact)
        {
            return Allows(contact.Velocity, contact.RelativeVelocity, contact.HitData.Transform.eulerAngles.z,
                contact.Controller.Grounded);
        }

        public bool Allows(HedgehogController controller)
        {
            return Allows(controller.Velocity, controller.RelativeVelocity, 0, controller.Grounded);
        }

        public bool Allows(Vector2 velocity, Vector2 relativeVelocity, float rotation, bool grounded)
        {
            if (!(ApplyWhenAirborne && !grounded) && !(ApplyWhenGrounded && grounded))
                return true;

            if (RelativeToGravity)
                velocity = relativeVelocity;

            if (RelativeToRotation)
                velocity = SrMath.RotateBy(velocity, -rotation * Mathf.Deg2Rad);

            var horizontal = UseAbsoluteHorizontal
                ? Mathf.Abs(velocity.x)
                : velocity.x;

            if (horizontal < HorizontalMin || horizontal > HorizontalMax)
                return false;

            var vertical = UseAbsoluteVertical
                ? Mathf.Abs(velocity.y)
                : velocity.y;

            if (vertical < VerticalMin || vertical > VerticalMax)
                return false;

            return true;
        }
    }
}
