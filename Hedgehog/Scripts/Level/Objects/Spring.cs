using Hedgehog.Core.Actors;
using Hedgehog.Core.Moves;
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

        /// <summary>
        /// Name of an Animator trigger on the controller to set when it triggers the spring.
        /// </summary>
        [Tooltip("Name of an Animator trigger on the controller to set when it triggers the spring.")]
        public string HitTrigger;

        public override bool ActivatesObject
        {
            get { return true; }
        }

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

        public override void OnPlatformEnter(TerrainCastHit hit)
        {
            var hitSide = TerrainUtility.NormalToControllerSide(hit.NormalAngle*Mathf.Rad2Deg - transform.eulerAngles.z);
            if ((BouncySides & hitSide) == 0) return;

            if (LockControl) hit.Controller.GroundControl.Lock(LockDuration);
            if (!KeepOnGround)
            {
                hit.Controller.Detach();
                hit.Controller.EndMove<Roll>();
                hit.Controller.PerformMove<AirControl>(true);
            }

            if (AccurateBounce)
            {
                hit.Controller.Velocity = new Vector2(hit.Controller.Velocity.x*Mathf.Abs(Mathf.Sin(hit.NormalAngle)),
                    hit.Controller.Velocity.y*Mathf.Abs(Mathf.Cos(hit.NormalAngle)));
                hit.Controller.Velocity += DMath.AngleToVector(hit.NormalAngle) * Power;
            }
            else
            {
                hit.Controller.Velocity = DMath.AngleToVector(hit.NormalAngle) * Power;
            }

            if (hit.Controller.Animator != null)
            {
                var logWarnings = hit.Controller.Animator.logWarnings;
                hit.Controller.Animator.logWarnings = false;

                if (!string.IsNullOrEmpty(HitTrigger))
                    hit.Controller.Animator.SetTrigger(HitTrigger);

                hit.Controller.Animator.logWarnings = logWarnings;
            }

            TriggerObject(hit.Controller);
        }
    }
}
