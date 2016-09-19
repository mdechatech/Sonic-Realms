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

        public override void OnPreCollide(PlatformCollision.Contact contact)
        {
            if (contact.Controller == null || !contact.Controller.Grounded)
                return;

            if (contact.HitData.Side == ControllerSide.Bottom || contact.HitData.Side == ControllerSide.Top)
                return;

            if (contact.Controller.IsPerforming<Roll>() ||
                Mathf.Abs(contact.Controller.GroundVelocity) < MinGroundSpeed)
                return;
            
            ActivateObject(contact.Controller);
            Destroy(gameObject);

            if (FreezeTime <= 0.0f)
                return;

            contact.Controller.Interrupt(FreezeTime);
            contact.Controller.IgnoreThisCollision();
        }
    }
}
