using SonicRealms.Core.Actors;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Platforms
{
    /// <summary>
    /// Platform that can't be landed on from the air, like the corkscrews from Emerald Hill.
    /// Can also require a minimum speed of entry and minimum speed to stay.
    /// </summary>
    public class Corkscrew : ReactivePlatform
    {
        /// <summary>
        /// Minimum speed to be able to step onto the platform, in units per second.
        /// </summary>
        [Tooltip("Minimum speed to be able to step onto the platform, in units per second.")]
        public float MinEntrySpeed;

        /// <summary>
        /// Minimum speed to be able to remain on the platform, in units per second.
        /// </summary>
        [Tooltip("Minimum speed to be able to remain on the platform, in units per second.")]
        public float MinStaySpeed;

        public override void Reset()
        {
            base.Reset();
            MinEntrySpeed = 2.4f;
            MinStaySpeed = 2.4f;
            PlayerSurfaceBool = "On Corkscrew";
        }

        public override bool IsSolid(TerrainCastHit data)
        {
            if (data == null || data.Controller == null) return false;

            // Similar to a ledge - only solid to the controller's bottom sensors
            return data.Side == ControllerSide.Bottom &&
                   data.Raycast.fraction > 0.0f &&
                   data.Controller.Grounded &&

                   // Use stay speed if controller is already standing on it, entry speed otherwise
                   (data.Controller.IsStandingOn(transform)
                       ? Mathf.Abs(data.Controller.GroundVelocity) > MinStaySpeed
                       : Mathf.Abs(data.Controller.GroundVelocity) > MinEntrySpeed);
        }
    }
}
