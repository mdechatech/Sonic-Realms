using SonicRealms.Core.Actors;
using SonicRealms.Core.Moves;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.Level.Objects
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

        /// <summary>
        /// Name of an Animator trigger on the controller to set when it triggers the spring.
        /// </summary>
        [Tooltip("Name of an Animator trigger on the controller to set when it triggers the spring.")]
        public string HitTrigger;

        public override void Reset()
        {
            base.Reset();
            Power = 10.0f;
            BouncySides = ControllerSide.Right;
            AccurateBounce = false;
            LockControl = false;
            LockDuration = 0.266667f;
            KeepOnGround = false;
            HitTrigger = "";
        }

        public override void OnPreCollide(PlatformCollision.Contact contact)
        {
            var data = contact.HitData;
            var controller = contact.Controller;

            var hitSide = TerrainUtility.NormalToControllerSide(data.NormalAngle*Mathf.Rad2Deg - transform.eulerAngles.z);
            if ((BouncySides & hitSide) == 0)
                return;

            if (LockControl)
            {
                var groundControl = controller.GetMove<GroundControl>();

                if (groundControl)
                    groundControl.Lock(LockDuration);
            }

            if (!KeepOnGround)
            {
                controller.Detach();

                controller.EndMove<Roll>();

                contact.Controller.IgnoreThisCollision();
            }

            if (AccurateBounce)
            {
                controller.Velocity = new Vector2(controller.Velocity.x*Mathf.Abs(Mathf.Sin(data.NormalAngle)),
                    controller.Velocity.y*Mathf.Abs(Mathf.Cos(data.NormalAngle)));
                controller.Velocity += DMath.UnitVector(data.NormalAngle) * Power;
            }
            else
            {
                controller.Velocity = DMath.UnitVector(data.NormalAngle) * Power;
            }

            if (controller.Animator != null)
            {
                var logWarnings = controller.Animator.logWarnings;
                controller.Animator.logWarnings = false;

                if (!string.IsNullOrEmpty(HitTrigger))
                    controller.Animator.SetTrigger(HitTrigger);

                controller.Animator.logWarnings = logWarnings;
            }

            BlinkEffectTrigger(data.Controller);
        }
    }
}
