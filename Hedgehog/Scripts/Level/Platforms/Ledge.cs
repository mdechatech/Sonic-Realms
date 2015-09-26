using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level.Platforms
{
    /// <summary>
    /// Turns a platform into a ledge where a controller can only collide with its top side.
    /// </summary>
    [RequireComponent(typeof(PlatformTrigger))]
    public class Ledge : ReactivePlatform
    {
        // The platform can be collided with if the player is checking its bottom side and
        // the result of the check did not stop where it started.
        public override bool CollidesWith(TerrainCastHit hit)
        {
            if(hit.Source == null) 
                return (hit.Side & TerrainSide.Bottom) > 0;
            
            // Check must be coming from player's bottom side and be close to the top
            // of the platform
            return (hit.Side & TerrainSide.Bottom) > 0 && hit.Hit.fraction > 0.0f;
        }
    }
}
