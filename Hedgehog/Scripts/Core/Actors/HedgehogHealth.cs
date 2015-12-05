using Hedgehog.Core.Moves;
using UnityEngine;
using UnityEngine.Events;

namespace Hedgehog.Core.Actors
{
    /// <summary>
    /// Allows the controller to be harmed and gives it Sonic-style "health", which is 
    /// just whether or not you have rings.
    /// </summary>
    public class HedgehogHealth : MonoBehaviour
    {
        public HedgehogController Controller;
        public HurtRebound HurtReboundMove;
        public Death DeathMove;
        public RingCollector RingCollector;

        public UnityEvent OnHurt;

        /// <summary>
        /// The max number of rings lost when hurt.
        /// </summary>
        [Tooltip("The max number of rings lost when hurt.")]
        public int RingsLost;

        /// <summary>
        /// Duration of invincibility after getting hurt (and after the rebound ends), in seconds.
        /// </summary>
        [Tooltip("Duration of invincibility after getting hurt (and after the rebound ends), in seconds.")]
        public float HurtInvinciblilityTime;

        /// <summary>
        /// Animator bool set to whether invicibility after getting hurt is on
        /// </summary>
        [Tooltip("Animator bool set to whether invicibility after getting hurt is on")]
        public string HurtInvincibleBool;
        protected int HurtInvincibleBoolHash;

        /// <summary>
        /// Whether the controller is invincible.
        /// </summary>
        [HideInInspector]
        public bool Invincible;

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

        public void Reset()
        {
            OnHurt = new UnityEvent();

            Controller = GetComponentInParent<HedgehogController>();
            HurtReboundMove = Controller.GetMove<HurtRebound>();
            DeathMove = Controller.GetMove<Death>();
            RingCollector = GetComponentInChildren<RingCollector>();

            RingsLost = 9001;
            HurtInvinciblilityTime = 2.0f;
        }

        public void Awake()
        {
            OnHurt = OnHurt ?? new UnityEvent();

            Controller = Controller ?? GetComponentInParent<HedgehogController>();
            HurtReboundMove = HurtReboundMove ?? Controller.GetMove<HurtRebound>();
            RingCollector = RingCollector ?? GetComponentInChildren<RingCollector>();

            HurtInvincibilityTimer = 0.0f;

            if (Controller.Animator == null) return;
            HurtInvincibleBoolHash = string.IsNullOrEmpty(HurtInvincibleBool) ?
                0 : Animator.StringToHash(HurtInvincibleBool);
        }

        public void Update()
        {
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

        public void Hurt()
        {
            Hurt(new Vector2(0.0f, 0.0f));
        }

        public void Hurt(Vector2 threatPosition)
        {
            if (Invincible) return;

            HurtReboundMove.ThreatPosition = threatPosition;
            HurtReboundMove.OnEnd.AddListener(OnHurtReboundEnd);
            HurtReboundMove.Perform();

            HurtInvincibilityTimer = HurtInvinciblilityTime;
            HurtInvincible = Invincible = true;

            var shield = Controller.MoveManager.GetMove(MoveGroup.Shield);
            if (shield == null)
            {
                if (RingCollector.Amount <= 0)
                {
                    Kill();
                    return;
                }

                RingCollector.Spill(RingsLost);
            }

            OnHurt.Invoke();
        }

        public void OnHurtReboundEnd()
        {
            HurtInvincible = Invincible = true;
            HurtReboundMove.OnEnd.RemoveListener(OnHurtReboundEnd);
        }

        public void Kill()
        {
            Controller.PerformMove<Death>();
        }
    }
}
