using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Generic platform that activates the object trigger when a controller collides with it.
    /// </summary>
    public class ActivatePlatform : ReactivePlatform
    {
        /// <summary>
        /// Whether to activate the platform when collided with.
        /// </summary>
        [Tooltip("Whether to activate the platform when collided with.")]
        public bool WhenColliding;

        /// <summary>
        /// Whether to activate the platform when a controller stands on it.
        /// </summary>
        [Tooltip("Whether to activate the platform when a controller stands on it.")]
        public bool WhenOnSurface;

        /// <summary>
        /// Whether to activate only when the controller hits the surface at a certain angle. If
        /// true, activation happens only from SurfaceAngleMin to SurfaceAngleMax, traveling
        /// counter-clockwise.
        /// </summary>
        [Tooltip("Whether to activate only when the controller hits the surface at a certain angle. " +
                 "If true, activation happens only from SurfaceAngleMin to SurfaceAngleMax, traveling " +
                 "counter-clockwise.")]
        public bool LimitAngle;

        /// <summary>
        /// Whether to add the object's rotation when checking angle.
        /// </summary>
        [Tooltip("Whether to add the object's rotation when checking angle.")]
        public bool RelativeToRotation;

        /// <summary>
        /// The minimum surface angle at which the platform activates when it is hit, in degrees.
        /// </summary>
        [Tooltip("The minimum surface angle at which the platform activates when hit, in degrees.")]
        public float SurfaceAngleMin;

        /// <summary>
        /// The maximum surface angle at which the platform activates when it is hit, in degrees.
        /// </summary>
        [Tooltip("The maximum surface angle at which the platform activates when it is hit, in degrees.")]
        public float SurfaceAngleMax;

        public override void Reset()
        {
            base.Reset();
            if (!GetComponent<ObjectTrigger>()) gameObject.AddComponent<ObjectTrigger>();

            WhenColliding = false;
            WhenOnSurface = true;

            LimitAngle = false;
            RelativeToRotation = true;
            SurfaceAngleMin = -45.0f;
            SurfaceAngleMax = 45.0f;
        }

        public override void OnPlatformEnter(TerrainCastHit hit)
        {
            if (!WhenColliding) return;
            if (!Check(hit)) return;

            ActivateObject(hit.Controller);
        }

        public override void OnPlatformStay(TerrainCastHit hit)
        {
            if (!WhenColliding) return;

            if (Check(hit))
            {
                ActivateObject(hit.Controller);
                return;
            }

            DeactivateObject(hit.Controller);
        }

        public override void OnPlatformExit(TerrainCastHit hit)
        {
            if (!WhenColliding) return;
            DeactivateObject(hit.Controller);
        }

        /// <summary>
        /// The test used when LimitAngle is true to see if the platform should be activated.
        /// </summary>
        /// <param name="angle">The angle, in degrees.</param>
        /// <returns></returns>
        public bool CheckAngle(float angle)
        {
            return DMath.AngleInRange_d(angle - (RelativeToRotation ? transform.eulerAngles.z : 0f)
                , SurfaceAngleMin, SurfaceAngleMax);
        }

        public bool Check(TerrainCastHit hit)
        {
            return !LimitAngle || CheckAngle(hit.SurfaceAngle*Mathf.Rad2Deg);
        }

        public override void OnSurfaceEnter(TerrainCastHit hit)
        {
            if (!WhenOnSurface) return;
            if (!Check(hit)) return;

            ActivateObject(hit.Controller);
        }

        public override void OnSurfaceStay(TerrainCastHit hit)
        {
            if (!WhenOnSurface)
                return;

            if (Check(hit))
            {
                ActivateObject(hit.Controller);
                return;
            }

            DeactivateObject(hit.Controller);
        }

        public override void OnSurfaceExit(TerrainCastHit hit)
        {
            if (!WhenOnSurface) return;
            DeactivateObject(hit.Controller);
        }
    }
}
