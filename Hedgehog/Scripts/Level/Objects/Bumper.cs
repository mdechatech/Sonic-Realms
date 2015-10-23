using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level.Objects
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

        public void Reset()
        {
            Velocity = 10.0f;
            AccurateBounce = false;
            LockControl = false;
        }

        public override void Start()
        {
            base.Start();
            AutoActivate = false;
        }

        public override void OnPlatformEnter(HedgehogController controller, TerrainCastHit hit)
        {
            controller.Detach();
            if(LockControl) controller.LockHorizontal();
           
            var normal = hit.NormalAngle;

            if (AccurateBounce)
            {
                controller.Velocity = new Vector2(controller.Velocity.x * Mathf.Abs(Mathf.Sin(normal)),
                    controller.Velocity.y * Mathf.Abs(Mathf.Cos(normal)));
                controller.Velocity += DMath.AngleToVector(normal) * Velocity;
            }
            else
            {
                controller.Velocity = DMath.AngleToVector(normal) * Velocity;
            }

            TriggerObject();
        }
    }
}
