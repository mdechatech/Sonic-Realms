using SonicRealms.Core.Moves;
using SonicRealms.Core.Utils;
using SonicRealms.Level;
using UnityEngine;
using UnityEngine.Events;

namespace SonicRealms.Core.Actors
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
        [Tooltip("Move to perform when hurt.")]
        public HurtRebound HurtReboundMove;

        /// <summary>
        /// Move to perform when killed.
        /// </summary>
        [Tooltip("Move to perform when killed.")]
        public Death DeathMove;

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

        #region Injury
        /// <summary>
        /// The controller's ring collector.
        /// </summary>
        [Foldout("Injury")]
        [Tooltip("The controller's ring collector.")]
        public RingCounter RingCounter;

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
        #endregion
        #region Animation
        /// <summary>
        /// Animator bool set to whether invicibility after getting hurt is on
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Animator bool set to whether invicibility after getting hurt is on")]
        public string HurtInvincibleBool;
        protected int HurtInvincibleBoolHash;
        #endregion
        #region Sound
        /// <summary>
        /// Audio clip to play when losing rings as the result of getting hurt.
        /// </summary>
        [Foldout("Sound")]
        [Tooltip("Audio clip to play when losing rings as the result of getting hurt.")]
        public AudioClip RingLossSound;

        /// <summary>
        /// Audio clip to play when losing a shield or rebounding.
        /// </summary>
        [Foldout("Sound")]
        [Tooltip("Audio clip to play when losing a shield or rebounding.")]
        public AudioClip ReboundSound;

        /// <summary>
        /// Audio clip to play on death.
        /// </summary>
        [Foldout("Sound")]
        [Tooltip("Audio clip to play on death.")]
        public AudioClip DeathSound;

        /// <summary>
        /// Any hit object that has this tag will be considered spikes.
        /// </summary>
        [Foldout("Sound"), Space, Tag]
        [Tooltip("Any hit object that has this tag will be considered spikes.")]
        public string SpikeTag;

        /// <summary>
        /// Audio clip to play when hitting spikes with a shield on.
        /// </summary>
        [Foldout("Sound")]
        [Tooltip("Audio clip to play when hitting spikes with a shield on.")]
        public AudioClip SpikeSound;
        #endregion
        #region Events
        /// <summary>
        /// Called when the controller's death animation is complete.
        /// </summary>
        [Foldout("Events")]
        public UnityEvent OnDeathComplete;

        /// <summary>
        /// Called when the controller kills a badnik.
        /// </summary>
        [Foldout("Events")]
        public EnemyEvent OnEnemyKilled;
        #endregion

        [Foldout("Debug")]
        public bool DeathComplete;

        public override void Reset()
        {
            base.Reset();

            RingLossSound = null;
            ReboundSound = null;
            DeathSound = null;
            SpikeTag = "";
            SpikeSound = null;

            OnDeathComplete = new UnityEvent();
            OnEnemyKilled = new EnemyEvent();

            Controller = GetComponentInParent<HedgehogController>();

            HurtReboundMove = Controller.GetMove<HurtRebound>();
            DeathMove = Controller.GetMove<Death>();

            RingCounter = GetComponentInChildren<RingCounter>();

            RingsLost = 9001;
            HurtInvinciblilityTime = 2.0f;
        }

        public override void Awake()
        {
            base.Awake();

            OnDeathComplete = OnDeathComplete ?? new UnityEvent();
            OnEnemyKilled = OnEnemyKilled ?? new EnemyEvent();

            Controller = Controller ?? GetComponentInParent<HedgehogController>();
            HurtReboundMove = HurtReboundMove ?? Controller.GetComponent<MoveManager>().Get<HurtRebound>();
            RingCounter = RingCounter ?? GetComponentInChildren<RingCounter>();

            HurtInvincibilityTimer = 0.0f;

            HurtInvincibleBoolHash = Animator.StringToHash(HurtInvincibleBool);
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
                Controller.Animator.SetBool(HurtInvincibleBoolHash, HurtInvincible && !IsDead);
        }

        public override void TakeDamage(float damage, Transform source)
        {
            if (Invincible) return;

            var spikes = source.CompareTag(SpikeTag);

            HurtReboundMove.ThreatPosition = source ? (Vector2)source.position : default(Vector2);
            HurtReboundMove.OnEnd.AddListener(OnHurtReboundEnd);
            HurtReboundMove.Perform();

            HurtInvincibilityTimer = HurtInvinciblilityTime;
            HurtInvincible = Invincible = true;

            var shield = Controller.GetPowerup<Shield>();
            if (shield == null)
            {
                if (RingCounter.Rings <= 0)
                {
                    Kill();
                    return;
                }

                RingCounter.Spill(RingsLost);

                if (RingLossSound != null)
                    SoundManager.Instance.PlayClipAtPoint(RingLossSound, transform.position);
            }
            else
            {
                if (spikes)
                {
                    if (SpikeSound != null)
                        SoundManager.Instance.PlayClipAtPoint(SpikeSound, transform.position);
                }
                else
                {
                    if (ReboundSound != null)
                        SoundManager.Instance.PlayClipAtPoint(ReboundSound, transform.position);
                }
            }

            Controller.EndMove<Roll>();
            OnHurt.Invoke(null);
        }

        public void NotifyEnemyKilled(HealthSystem enemy)
        {
            OnEnemyKilled.Invoke(enemy);
        }

        public void OnHurtReboundEnd()
        {
            HurtInvincible = Invincible = true;
            HurtReboundMove.OnEnd.RemoveListener(OnHurtReboundEnd);
        }

        public void OnDeathEnd()
        {
            DeathComplete = true;
            OnDeathComplete.Invoke();
            DeathMove.OnEnd.RemoveListener(OnDeathEnd);
        }

        public override void Kill(Transform source)
        {
            if (IsDead || DeathMove.Active) return;

            // Bring the player to the front
            var spriteRenderer = Controller.RendererObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) spriteRenderer.sortingLayerName = "Foreground";

            DeathMove.Perform();
            DeathMove.OnEnd.AddListener(OnDeathEnd);

            if (DeathSound != null)
                SoundManager.Instance.PlayClipAtPoint(DeathSound, transform.position);

            base.Kill(source);
        }
    }
}
