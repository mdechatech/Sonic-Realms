using SonicRealms.Core.Actors;
using SonicRealms.Core.Moves;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Objects
{
    /// <summary>
    /// A wall that can be broken by rolling into it. Imitates the walls in Green Hill Zone.
    /// </summary>
    public class BreakableWall : ReactivePlatform
    {
        /// <summary>
        /// Minimum absolute ground speed to break the wall.
        /// </summary>
        [SerializeField]
        public float MinGroundSpeed;

        /// <summary>
        /// Duration of the freeze frame when the wall is broken, in seconds.
        /// </summary>
        [SerializeField]
        public float FreezeTime;

        public override void Reset()
        {
            base.Reset();
            MinGroundSpeed = 2.7f;
            FreezeTime = 0.03333333f;
        }

        public override void OnPlatformEnter(TerrainCastHit hit)
        {
            if (hit.Controller == null || !hit.Controller.Grounded) return;
            if (hit.Side == ControllerSide.Bottom || hit.Side == ControllerSide.Top) return;

            var moveManager = hit.Controller.GetComponent<MoveManager>();
            if (moveManager == null || moveManager.IsActive<Roll>() || 
                Mathf.Abs(hit.Controller.GroundVelocity) < MinGroundSpeed) return;

            hit.Controller.IgnoreThisCollision();
            ActivateObject(hit.Controller);
            Destroy(gameObject);

            if (FreezeTime <= 0.0f) return;
            hit.Controller.Interrupt(FreezeTime);
        }

        public override void OnPlatformStay(TerrainCastHit hit)
        {
            OnPlatformEnter(hit);
        }
    }
}
