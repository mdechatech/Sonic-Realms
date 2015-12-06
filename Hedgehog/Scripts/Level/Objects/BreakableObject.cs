using Hedgehog.Core.Actors;
using Hedgehog.Core.Moves;
using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level.Objects
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
            if (hit.Controller == null || hit.Controller.Grounded) return;
            if (hit.Side != ControllerSide.Bottom || hit.Controller.RelativeVelocity.y > 0.0f) return;
            if (!hit.Controller.MoveManager.IsActive<Roll>()) return;

            hit.Controller.IgnoreNextCollision = true;
            hit.Controller.RelativeVelocity = new Vector2(hit.Controller.RelativeVelocity.x, BounceSpeed);
            ActivateObject(hit.Controller);
            Destroy(gameObject);
        }
    }
}
