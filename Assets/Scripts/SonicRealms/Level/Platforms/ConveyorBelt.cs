using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Platforms
{
    /// <summary>
    /// Moves the player forward or backward on the platform.
    /// </summary>
    [AddComponentMenu("Hedgehog/Platforms/Conveyor Belt")]
    public class ConveyorBelt : ReactivePlatform
    {
        /// <summary>
        /// The player is moved by this amount on the surface, in units per second. Positive means
        /// forward, negative means backward.
        /// </summary>
        [SerializeField] public float Velocity;

        private float _lastSurfaceAngle;

        public override void Reset()
        {
            base.Reset();
            Velocity = 2.5f;
        }

        // Translate the controller by the amount defined in Velocity and the direction defined by its
        // surface angle.
        public override void OnSurfaceStay(TerrainCastHit hit)
        {
            var controller = hit.Controller;
            if (hit.Controller == null) return;

            controller.transform.Translate(DMath.AngleToVector(controller.SurfaceAngle*Mathf.Deg2Rad)*Velocity*
                                 Time.fixedDeltaTime);

            _lastSurfaceAngle = controller.SurfaceAngle;
        }

        // Transfer momentum to the controller when it leaves the conveyor belt.
        public override void OnSurfaceExit(TerrainCastHit hit)
        {
            var controller = hit.Controller;
            if (hit.Controller == null)
                return;

            if (controller.Grounded)
                controller.GroundVelocity += Velocity;
            else
                controller.Velocity += DMath.AngleToVector(_lastSurfaceAngle*Mathf.Deg2Rad)*Velocity;
        }
    }
}
