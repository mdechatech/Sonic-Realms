using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level.Platforms
{
    /// <summary>
    /// Turns a platform into a ledge where a controller can only collide with its top side.
    /// </summary>
    [AddComponentMenu("Hedgehog/Platforms/Ledge")]
    public class Ledge : ReactivePlatform
    {
        /// <summary>
        /// Whether the top of the platform is solid.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether the top of the platform is solid.")]
        public bool TopSolid;
        
        /// <summary>
        /// Whether the bottom side of the platform is solid.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether the bottom side of the platform is solid.")]
        public bool BottomSolid;

        /// <summary>
        /// Whether the left side of the platform is solid.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether the left side of the platform is solid.")]
        public bool LeftSolid;

        /// <summary>
        /// Whether the right side of the platform is solid.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether the right side of the platform is solid.")]
        public bool RightSolid;

        public void Reset()
        {
            TopSolid = true;
            BottomSolid = LeftSolid = RightSolid = false;
        }

        // The platform can be collided with if the player is checking its bottom side and
        // the result of the check did not stop where it started.
        public override bool CollidesWith(TerrainCastHit hit)
        {
            // Check must be coming from player's bottom side and be close to the top
            // of the platform
            return hit.Hit.fraction > 0.0f &&
                   (TopSolid && (hit.Side & ControllerSide.Bottom) > 0) ||
                   (BottomSolid && (hit.Side & ControllerSide.Top) > 0) ||
                   (LeftSolid && (hit.Side & ControllerSide.Right) > 0) ||
                   (RightSolid && (hit.Side & ControllerSide.Left) > 0);
        }
    }
}
