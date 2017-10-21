using System;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// On a call to Allows(), this object returns true or false based on surface angle related criteria.
    /// </summary>
    [Serializable]
    public class SurfaceAngleLimiter :
        ITriggerLimiter<HedgehogController>,
        ITriggerLimiter<TerrainCastHit>
    {
        /// <summary>
        /// Whether to account for the object's rotation when checking angle.
        /// </summary>
        [Tooltip("Whether to account for the object's rotation when checking angle.")]
        public bool RelativeToRotation;

        /// <summary>
        /// Whether to account for player gravity when checking angle.
        /// </summary>
        [Tooltip("Whether to account for player gravity when checking angle.")]
        public bool RelativeToGravity;

        /// <summary>
        /// The minimum surface angle at which the platform activates when it is hit, in degrees.
        /// </summary>
        [Space]
        [Tooltip("The minimum surface angle at which the platform activates when hit, in degrees.")]
        public float SurfaceAngleMin = -10;

        /// <summary>
        /// The maximum surface angle at which the platform activates when it is hit, in degrees.
        /// </summary>
        [Tooltip("The maximum surface angle at which the platform activates when it is hit, in degrees.")]
        public float SurfaceAngleMax = 10;

        public SurfaceAngleLimiter()
        {
            RelativeToRotation = true;
        }

        public bool Allows(HedgehogController controller)
        {
            return Allows(controller.SurfaceAngle, 0, controller.GravityDirection);
        }

        public bool Allows(TerrainCastHit hit)
        {
            return Allows(hit.SurfaceAngle*Mathf.Rad2Deg, hit.Transform.eulerAngles.z, hit.Controller.GravityDirection);
        }

        public bool Allows(float angle, float rotation, float gravity)
        {
            if (RelativeToRotation)
                angle = SrMath.PositiveAngle_d(angle - rotation);

            if (RelativeToGravity)
                angle = SrMath.PositiveAngle_d(rotation - gravity + 270.0f);

            return SrMath.AngleInRange_d(angle, SurfaceAngleMin, SurfaceAngleMax);
        }
    }
}
