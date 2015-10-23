using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level.Objects
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Spring : ReactivePlatform
    {
        /// <summary>
        /// Which sides of the spring are bouncy. This is relative to rotation.
        /// </summary>
        [SerializeField]
        [BitMask(typeof(ControllerSide))]
        [Tooltip("Which sides of the spring are bouncy. This is relative to rotation.")]
        public ControllerSide BouncySides;

        /// <summary>
        /// Speed at which the controller is launched.
        /// </summary>
        [SerializeField]
        [Tooltip("Speed at which the controller is launched.")]
        public float Power;

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
            Power = 10.0f;
            BouncySides = ControllerSide.Right;
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
            var hitSide = TerrainUtility.NormalToControllerSide(hit.NormalAngle*Mathf.Rad2Deg - transform.eulerAngles.z);
            if ((BouncySides & hitSide) == 0) return;

            controller.Detach();
            if (LockControl) controller.LockHorizontal();

            if (!AccurateBounce)
            {
                controller.Velocity = DMath.AngleToVector(hit.NormalAngle) * Power;
            }
            else
            {
                controller.Velocity = new Vector2(controller.Velocity.x*Mathf.Abs(Mathf.Sin(hit.NormalAngle)),
                    controller.Velocity.y*Mathf.Abs(Mathf.Cos(hit.NormalAngle)));
                controller.Velocity += DMath.AngleToVector(hit.NormalAngle) * Power;
            }

            TriggerObject(controller);
        }
    }
}
