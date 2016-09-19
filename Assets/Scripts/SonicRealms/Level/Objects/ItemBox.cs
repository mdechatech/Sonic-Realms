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

        public override void OnPreCollide(PlatformCollision.Contact contact)
        {
            // Must be rolling; if in the air, must be traveling down; 
            // if on the ground, must be hitting from the side

            if (!contact.Controller.IsPerforming<Roll>())
                return;

            if (!contact.Controller.Grounded && (contact.Controller.RelativeVelocity.y > 0.0f))
                return;

            if (contact.Controller.Grounded &&
                (contact.HitData.Side == ControllerSide.Bottom || contact.HitData.Side == ControllerSide.Top))
                return;

            // Rebound effect
            contact.Controller.IgnoreThisCollision();

            Bounce(contact.Controller, contact.RelativeVelocity);
            Break(contact.Controller);
        }

        public void NotifyHit(Hitbox hitbox)
        {
            var sonicHitbox = hitbox as SonicHitbox;
            if (sonicHitbox != null && sonicHitbox.Harmful && !sonicHitbox.Vulnerable)
            {
                Bounce(hitbox.Controller, hitbox.Controller.RelativeVelocity);
                Break(hitbox.Controller);
            }
        }

        public void Bounce(HedgehogController controller, Vector2 relativeVelocity)
        {
            // Ignore collision with this since it's destroyed now
            controller.ApplyGravityOnce();

            if(relativeVelocity.y > 0f) return;

            var jump = controller.GetMove<Jump>();
            if (jump == null || jump.Active || !jump.Used || controller.IsPerforming<BubbleSpecial>())
            {
                controller.RelativeVelocity = new Vector2(relativeVelocity.x,
                    -relativeVelocity.y);
            }
            else
            {
                controller.RelativeVelocity = new Vector2(relativeVelocity.x,
                    Mathf.Min(jump.ReleaseSpeed, -relativeVelocity.y +
                    controller.AirGravity * Time.deltaTime));
            }
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
