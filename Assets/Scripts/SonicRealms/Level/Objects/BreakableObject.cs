using SonicRealms.Core.Actors;
using SonicRealms.Core.Moves;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Objects
{
    /// <summary>
    /// Object broken by jumping on top of it. Examples include the pillars in Aquatic Ruin and pipe caps
    /// in Chemical Plant.
    /// </summary>
    public class BreakableObject : ReactivePlatform
    {
        /// <summary>
        /// Bounces the controller off at this speed after breaking it.
        /// </summary>
        [Tooltip("Bounces the controller off at this speed after breaking it.")]
        public float BounceSpeed;

        public override void Reset()
        {
            base.Reset();
            BounceSpeed = 1.8f;
        }

        public override void OnPlatformEnter(TerrainCastHit hit)
        {
            // Must be hit from the air
            if (hit.Controller == null || hit.Controller.Grounded) return;

            // Must be hit from the player's bottom side
            if (hit.Side != ControllerSide.Bottom || hit.Controller.RelativeVelocity.y > 0.0f) return;

            // Player must be in a roll
            if (!hit.Controller.IsPerforming<Roll>()) return;

            hit.Controller.IgnoreThisCollision();
            hit.Controller.RelativeVelocity = new Vector2(hit.Controller.RelativeVelocity.x, BounceSpeed);
            ActivateObject(hit.Controller);
            Destroy(gameObject);
        }
    }
}
