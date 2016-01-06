using Hedgehog.Core.Moves;
using Hedgehog.Core.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Hedgehog.Core.Actors
{
    /// <summary>
    /// Allows the controller to be harmed and gives it Sonic-style "health", which is 
    /// just whether or not you have rings.
    /// </summary>
    public class HedgehogHealth : HealthSystem
    {
        public HedgehogController Controller;

        public override float Health
        {
            get { return 1f; }
            set { if (value < 0f) Kill(); }
        }

        /// <summary>
        /// Move to perform when hurt.
        /// </summary>
        [Foldout("Injury")]
        [Tooltip("Move to perform when hurt.")]
        public HurtRebound HurtReboundMove;

        /// <summary>
        /// Move to perform when killed.
        /// </summary>
        [Foldout("Injury")]
        [Tooltip("Move to perform when killed.")]
        public Death DeathMove;

        /// <summary>
        /// The controller's ring collector.
        /// </summary>
        [Foldout("Injury"), Space]
        [Tooltip("The controller's ring collector.")]
        public RingCollector RingCollector;

        /// <summary>
        /// The max number of rings lost when hurt.
        /// </summary>
        [Foldout("Injury")]
        [Tooltip("The max number of rings lost when hurt.")]
        public int RingsLost;

        /// <summary>
        /// Duration of invincibility after getting hurt (and after the rebound ends), in seconds.
        /// </summary>
        [Foldout("Injury")]
        [Tooltip("Duration of invincibility after getting hurt (and after the rebound ends), in seconds.")]
        public float HurtInvinciblilityTime;

        /// <summary>
        /// Called when the controller's death animation is complete.
        /// </summary>
        [Foldout("Events")]
        public UnityEvent OnDeathComplete;

        /// <summary>
        /// Animator bool set to whether invicibility after getting hurt is on
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Animator bool set to whether invicibility after getting hurt is on")]
        public string HurtInvincibleBool;
        protected int HurtInvincibleBoolHash;

        /// <summary>
        /// Whether the controller is invincible due to having been hurt.
        /// </summary>
        [HideInInspector]
        public bool HurtInvincible;

        /// <summary>
        /// Countdown until the controller is no longer invincible after being hurt.
        /// </summary>
        [HideInInspector]
        public float HurtInvincibilityTimer;

        public override void Reset()
        {
            base.Reset();

            OnDeathComplete = new UnityEvent();;

            Controller = GetComponentInParent<HedgehogController>();
            HurtReboundMove = Controller.GetMove<HurtRebound>();
            DeathMove = Controller.GetMove<Death>();
            RingCollector = GetComponentInChildren<RingCollector>();

            RingsLost = 9001;
            HurtInvinciblilityTime = 2.0f;
        }

        public override void Awake()
        {
            base.Awake();

            OnDeathComplete = OnDeathComplete ?? new UnityEvent();

            Controller = Controller ?? GetComponentInParent<HedgehogController>();
            HurtReboundMove = HurtReboundMove ?? Controller.GetMove<HurtRebound>();
            RingCollector = RingCollector ?? GetComponentInChildren<RingCollector>();

            HurtInvincibilityTimer = 0.0f;

            if (Controller.Animator == null) return;
            HurtInvincibleBoolHash = string.IsNullOrEmpty(HurtInvincibleBool) ?
                0 : Animator.StringToHash(HurtInvincibleBool);
        }

        public override void Start()
        {
            base.Start();
            Animator = Controller.Animator;
        }

        public override void Update()
        {
            base.Update();
            if (HurtInvincible)
            {
                HurtInvincibilityTimer -= Time.deltaTime;
                if (HurtInvincibilityTimer < 0.0f)
                {
                    HurtInvincible = Invincible = false;
                    HurtInvincibilityTimer = 0.0f;
                }
            }

            if (Controller.Animator == null)
                return;

            if(HurtInvincibleBoolHash != 0)
                Controller.Animator.SetBool(HurtInvincibleBoolHash, HurtInvincible);
        }

        public override void TakeDamage(float damage, Transform source)
        {
            if (Invincible) return;
            
            HurtReboundMove.ThreatPosition = source ? (Vector2)source.position : default(Vector2);
            HurtReboundMove.OnEnd.AddListener(OnHurtReboundEnd);
            HurtReboundMove.Perform();

            HurtInvincibilityTimer = HurtInvinciblilityTime;
            HurtInvincible = Invincible = true;

            var shield = Controller.MoveManager.GetMove<Shield>();
            if (shield == null)
            {
                if (RingCollector.Amount <= 0)
                {
                    Kill();
                    return;
                }

                RingCollector.Spill(RingsLost);
            }

            Controller.EndMove<Roll>();
            OnHurt.Invoke(null);
        }

        public void OnHurtReboundEnd()
        {
            HurtInvincible = Invincible = true;
            HurtReboundMove.OnEnd.RemoveListener(OnHurtReboundEnd);
        }

        public void OnDeathEnd()
        {
            OnDeathComplete.Invoke();
            DeathMove.OnEnd.RemoveListener(OnDeathEnd);
        }

        public override void Kill(Transform source)
        {
            if (DeathMove.Active) return;

            DeathMove.Perform();
            DeathMove.OnEnd.AddListener(OnDeathEnd);

            base.Kill();
        }
    }
}
