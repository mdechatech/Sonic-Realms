using Hedgehog.Core.Actors;
using Hedgehog.Core.Moves;
using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level.Objects
{
    public class BreakableWall : ReactivePlatform
    {
        /// <summary>
        /// Minimum absolute ground speed to break the wall.
        /// </summary>
        [SerializeField]
        public float MinGroundSpeed;

        public override bool ActivatesObject
        {
            get { return true; }
        }

        public void Reset()
        {
            MinGroundSpeed = 2.7f;
        }

        public override void OnPlatformEnter(TerrainCastHit hit)
        {
            if (hit.Controller == null || !hit.Controller.Grounded) return;
            if (hit.Side == ControllerSide.Bottom || hit.Side == ControllerSide.Top) return;
            if (!hit.Controller.IsActive<Roll>() || Mathf.Abs(hit.Controller.GroundVelocity) < MinGroundSpeed) return;

            hit.Controller.IgnoreNextCollision = true;
            ActivateObject(hit.Controller);
            Destroy(gameObject);
        }
    }
}
