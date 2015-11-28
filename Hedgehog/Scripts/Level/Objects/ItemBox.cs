using Hedgehog.Core.Actors;
using Hedgehog.Core.Moves;
using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level.Objects
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

        protected Animator Animator;

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

            Animator = GetComponent<Animator>();
            ActivationCountdown = ActivateDelay;
        }

        public override bool ActivatesObject
        {
            get { return true; }
        }

        public void Update()
        {
            if (Controller == null) return;

            ActivationCountdown -= Time.deltaTime;
            if (ActivationCountdown <= 0.0f)
            {
                ActivationCountdown = 0.0f;
                ActivateObject(Controller);
                Controller = null;
            }
        }

        public override void OnPlatformEnter(TerrainCastHit hit)
        {
            // Must be rolling; if in the air, mut be traveling down; 
            // if on the ground, must be hitting from the side
            if (!hit.Controller.MoveManager.IsActive<Roll>()) return;
            if (!hit.Controller.Grounded && (hit.Controller.RelativeVelocity.y > 0.0f)) return;
            if (hit.Controller.Grounded && 
                (hit.Side == ControllerSide.Bottom || hit.Side == ControllerSide.Top)) return;

            // Rebound effect
            var jump = hit.Controller.MoveManager.Get<Jump>();
            if (jump == null || jump.CurrentState == Move.State.Active || !jump.Used)
            {
                hit.Controller.RelativeVelocity = new Vector2(hit.Controller.RelativeVelocity.x,
                    -hit.Controller.RelativeVelocity.y);
            }
            else
            {
                hit.Controller.RelativeVelocity = new Vector2(hit.Controller.RelativeVelocity.x,
                    Mathf.Min(jump.ReleaseSpeed, -hit.Controller.RelativeVelocity.y));
            }

            // Ignore collision with this since it's destroyed now
            hit.Controller.IgnoreNextCollision = true;
            GetComponent<Collider2D>().enabled = false;
            Controller = hit.Controller;

            if (Animator == null || BrokenTrigger.Length <= 0) return;
            Animator.SetTrigger(BrokenTrigger);
        }
    }
}
