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
        /// Whether to make the controller launch straight in the direction of the spring. For example, a spring
        /// facing up with this set to true will launch you straight up, no horizontal movement.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to make the controller launch straight in the direction of the spring. For example, " +
                 "a spring facing up with this checked will launch you straight up, no horizontal movement.")]
        public bool ForceDirection;

        /// <summary>
        /// Whether to lock the controller's horizontal controller after being hit.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to lock the controller's horizontal control after being hit.")]
        public bool LockControl;

        private BoxCollider2D _boxCollider2D;

        public void Reset()
        {
            Power = 10.0f;
            BouncySides = ControllerSide.Right;
            ForceDirection = false;
            LockControl = true;
        }

        public override void Start()
        {
            base.Start();
            _boxCollider2D = GetComponent<BoxCollider2D>();
        }

        public override void OnPlatformEnter(HedgehogController controller, TerrainCastHit hit)
        {
            var hitSide = TerrainUtility.NormalToControllerSide(hit.NormalAngle*Mathf.Rad2Deg - transform.localEulerAngles.z);
            if ((BouncySides & hitSide) == 0) return;

            controller.Detach();
            if (LockControl) controller.LockHorizontal();

            if (ForceDirection)
            {
                controller.Velocity = DMath.AngleToVector(hit.NormalAngle) * Power;
            }
            else
            {
                controller.Velocity = new Vector2(controller.Velocity.x*Mathf.Abs(Mathf.Sin(hit.NormalAngle)),
                    controller.Velocity.y*Mathf.Abs(Mathf.Cos(hit.NormalAngle)));
                controller.Velocity += DMath.AngleToVector(hit.NormalAngle) * Power;
            }

            TriggerObject();
        }
    }
}
