using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level.Objects
{
    [RequireComponent(typeof(BoxCollider2D))]
    [AddComponentMenu("Hedgehog/Objects/Spring")]
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

        /// <summary>
        /// Duration of the control lock, if any.
        /// </summary>
        [SerializeField]
        [Tooltip("Duration of the control lock, if any.")]
        public float LockDuration;

        /// <summary>
        /// Whether to keep the controller on the ground after being hit. Works great for horizontal springs, not
        /// so much for vertical.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to keep the controller on the ground after being hit. Works great for horizontal springs, " +
                 "not so much for vertical.")]
        public bool KeepOnGround;

        public override bool ActivatesObject
        {
            get { return true; }
        }

        public void Reset()
        {
            Power = 10.0f;
            BouncySides = ControllerSide.Right;
            AccurateBounce = false;
            LockControl = false;
            LockDuration = 0.266667f;
            KeepOnGround = false;
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

            if (LockControl) controller.GroundControl.Lock(0.2667f);
            if (!KeepOnGround) controller.Detach();

            if (AccurateBounce)
            {
                controller.Velocity = new Vector2(controller.Velocity.x*Mathf.Abs(Mathf.Sin(hit.NormalAngle)),
                    controller.Velocity.y*Mathf.Abs(Mathf.Cos(hit.NormalAngle)));
                controller.Velocity += DMath.AngleToVector(hit.NormalAngle) * Power;
            }
            else
            {
                controller.Velocity = DMath.AngleToVector(hit.NormalAngle) * Power;
            }

            TriggerObject(controller);
        }
    }
}
