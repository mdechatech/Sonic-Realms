using System.Collections;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Objects
{
    /// <summary>
    /// Bounces the player into the air when touched.
    /// </summary>
    public class Balloon : ReactivePlatform
    {
        /// <summary>
        /// Speed at which the player bounces, in units per second.
        /// </summary>
        [Tooltip("Speed at which the player bounces, in units per second.")]
        public float Velocity;

        public override void Reset()
        {
            base.Reset();
            Velocity = 3.6f;
        }

        public override void OnPlatformEnter(TerrainCastHit hit)
        {
            if (hit.Controller == null) return;

            hit.Controller.Detach();
            hit.Controller.IgnoreThisCollision();
            hit.Controller.RelativeVelocity = new Vector2(hit.Controller.RelativeVelocity.x, Velocity);
            GetComponent<Collider2D>().enabled = false;

            ActivateObject(hit.Controller);
        }

        public override void OnPlatformStay(TerrainCastHit hit)
        {
            if(!ObjectTrigger.Activated) OnPlatformEnter(hit);
        }
    }
}
