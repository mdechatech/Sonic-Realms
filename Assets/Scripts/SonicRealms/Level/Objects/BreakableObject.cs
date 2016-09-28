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

        public override void OnPreCollide(PlatformCollision.Contact contact)
        {
            // Must be hit from the air
            if (contact.Controller == null || contact.Controller.Grounded)
                return;

            // Must be hit from the player's bottom side
            if (contact.HitData.Side != ControllerSide.Bottom || contact.Controller.RelativeVelocity.y > 0.0f)
                return;

            // Player must be in a roll
            if (!contact.Controller.IsPerforming<Roll>())
                return;

            contact.Controller.RelativeVelocity = new Vector2(contact.Controller.RelativeVelocity.x, BounceSpeed);
            ActivateEffectTrigger(contact.Controller);
            Destroy(gameObject);
            
            contact.Controller.IgnoreThisCollision();
        }
    }
}
