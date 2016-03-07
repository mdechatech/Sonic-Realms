using SonicRealms.Core.Actors;
using SonicRealms.Core.Moves;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Objects
{
    public class ItemBox : ReactivePlatform
    {
        /// <summary>
        /// How long to wait after being broken to activate, in seconds.
        /// </summary>
        [SerializeField]
        [Tooltip("How long to wait after being broken to activate, in seconds.")]
        public float ActivateDelay;

        /// <summary>
        /// Name of an Animator trigger set when the item box is broken.
        /// </summary>
        [SerializeField]
        [Tooltip("Name of an Animator trigger set when the item box is broken.")]
        public string BrokenTrigger;

        /// <summary>
        /// Stores the controller that broke the item box.
        /// </summary>
        protected HedgehogController Controller;

        /// <summary>
        /// Time until activation. Counts down after being broken.
        /// </summary>
        protected float ActivationCountdown;

        public override void Reset()
        {
            base.Reset();
            ActivateDelay = 0.53333333f;
        }

        public override void Awake()
        {
            base.Awake();
            ActivationCountdown = ActivateDelay;
        }

        public void Update()
        {
            if (Controller == null) return;

            ActivationCountdown -= Time.deltaTime;
            if (ActivationCountdown >= 0.0f) return;

            ActivationCountdown = 0.0f;
            TriggerObject(Controller);
            enabled = false;
        }

        public override void OnPlatformEnter(TerrainCastHit hit)
        {
            // Must be rolling; if in the air, must be traveling down; 
            // if on the ground, must be hitting from the side
            var moveManager = hit.Controller.GetComponent<MoveManager>();
            if (moveManager == null || !moveManager.IsActive<Roll>()) return;
            if (!hit.Controller.Grounded && (hit.Controller.RelativeVelocity.y > 0.0f)) return;
            if (hit.Controller.Grounded && 
                (hit.Side == ControllerSide.Bottom || hit.Side == ControllerSide.Top)) return;

            // Rebound effect
            Bounce(hit.Controller);
            Break(hit.Controller);
        }

        public void NotifyHit(Hitbox hitbox)
        {
            var sonicHitbox = hitbox as SonicHitbox;
            if (sonicHitbox != null && sonicHitbox.Harmful && !sonicHitbox.Vulnerable)
            {
                Bounce(hitbox.Controller);
                Break(hitbox.Controller);
            }
        }

        public void Bounce(HedgehogController controller)
        {
            controller.ApplyGravityOnce();

            if(controller.RelativeVelocity.y > 0f) return;

            var jump = controller.GetMove<Jump>();
            if (jump == null || jump.Active || !jump.Used || controller.IsPerforming<BubbleSpecial>())
            {
                controller.RelativeVelocity = new Vector2(controller.RelativeVelocity.x,
                    -controller.RelativeVelocity.y);
            }
            else
            {
                controller.RelativeVelocity = new Vector2(controller.RelativeVelocity.x,
                    Mathf.Min(jump.ReleaseSpeed, -controller.RelativeVelocity.y +
                    controller.AirGravity * Time.deltaTime));
            }

            // Ignore collision with this since it's destroyed now
            controller.IgnoreThisCollision();
        }

        public void Break(HedgehogController controller)
        {
            GetComponent<Collider2D>().enabled = false;
            Controller = controller;

            if (Animator == null || BrokenTrigger.Length <= 0)
                return;
            Animator.SetTrigger(BrokenTrigger);
        }
    }
}
