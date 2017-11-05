using SonicRealms.Core.Moves;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Objects
{
    /// <summary>
    /// Bumpers like the ones in Spring Yard and Carnival Night.
    /// </summary>
    public class Bumper : ReactivePlatform
    {
        /// <summary>
        /// The velocity at which the controller bounces off.
        /// </summary>
        [SerializeField]
        [Tooltip("The velocity at which the controller bounces off.")]
        public float Velocity;

        /// <summary>
        /// Whether to make the controller bounce accurately (like a ball off a surface) or have its speed set
        /// directly.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to make the controller bounce accurately (like a ball off a surface) or have its speed set " +
                 "directly.")]
        public bool AccurateBounce;

        /// <summary>
        /// Whether to lock the controller's horizontal controller after being hit.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to lock the controller's horizontal control after being hit.")]
        public bool LockControl;

        public override void Reset()
        {
            base.Reset();
            Velocity = 10.0f;
            AccurateBounce = false;
            LockControl = false;
        }

        public override void OnPlatformEnter(PlatformCollision contact)
        {
            // TODO proper bumperes
            /*
            collision.Controller.Detach();

            if(LockControl)
                collision.Controller.GetComponent<MoveManager>().Get<GroundControl>().Lock();
           
            var normal = collision.Latest.Data.NormalAngle;

            if (AccurateBounce)
            {
                collision.Controller.Velocity = new Vector2(collision.Controller.Velocity.x * Mathf.Abs(Mathf.Sin(normal)),
                    collision.Controller.Velocity.y * Mathf.Abs(Mathf.Cos(normal)));

                collision.Controller.Velocity += SrMath.AngleToVector(normal) * Velocity;
            }
            else
            {
                collision.Controller.Velocity = SrMath.AngleToVector(normal) * Velocity;
            }

            TriggerObject();
            */
        }
    }
}
