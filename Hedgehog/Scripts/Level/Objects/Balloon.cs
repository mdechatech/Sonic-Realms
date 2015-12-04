using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level.Objects
{
    /// <summary>
    /// Bounces the player into the air when touched.
    /// </summary>
    public class Balloon : ReactivePlatform
    {
        public float Velocity;

        public override bool ActivatesObject
        {
            get { return true; }
        }

        public override void Reset()
        {
            base.Reset();
            Velocity = 3.6f;
        }

        public override void OnPlatformEnter(TerrainCastHit hit)
        {
            if (hit.Controller == null) return;

            hit.Controller.Detach();
            hit.Controller.IgnoreNextCollision = true;
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
