using SonicRealms.Core.Actors;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Platforms
{
    /// <summary>
    /// The surface of a body of water that the player can run on.
    /// </summary>
    public class WaterSurface : ReactivePlatform
    {
        /// <summary>
        /// The minimum speed at which the player must be running to be able to run on top of the water.
        /// </summary>
        [Tooltip("The minimum speed at which the player must be running to be able to run on top of the water.")]
        public float MinFloatSpeed;

        public override void Reset()
        {
            MinFloatSpeed = 5.0f;
        }

        // The water is a surface if the player is upright, on top of it, grounded, not already submerged,
        // and running quickly enough.
        public override bool IsSolid(TerrainCastHit data)
        {
            if (data.Controller == null) return false;
            return base.IsSolid(data) &&
                   (data.Side & ControllerSide.Bottom) > 0 &&
                   data.Raycast.fraction > 0.0f &&
                   data.Controller.Grounded &&
                   Mathf.Abs(data.Controller.GroundVelocity) >= MinFloatSpeed;
        }
    }
}
