using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Actors
{
    /// <summary>
    /// Generic class for health. Adds the ability to take damage, die, and become invincible.
    /// </summary>
    public abstract class HealthSystem : MonoBehaviour
    {
        public abstract float Health
        {
            get; set;
        }

        public virtual float MaxHealth
        {
            get { return Health; }
        }

        public virtual bool Invincible
        {
            get; set;
        }

        /// <summary>
        /// Time left until invincibility wears off.
        /// </summary>
        [HideInInspector]
        public float InvincibleTimer;

        #region Events
        /// <summary>
        /// Invoked when hurt.
        /// </summary>
        [Foldout("Events")]
        public HealthEvent OnHurt;

        /// <summary>
        /// Invoked when killed.
        /// </summary>
        [Foldout("Events")]
        public HealthEvent OnDeath;
        #endregion
        #region Animation
        [Foldout("Animation")]
        public Animator Animator;

        /// <summary>
        /// Name of an Animator trigger set when hurt.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator trigger set when hurt.")]
        public string HurtTrigger;
        protected int HurtTriggerHash;

        /// <summary>
        /// Name of an Animator float, when hurt, set to the damage taken. HurtDamageFloat is set before HurtTrigger.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator float, when hurt, set to the damage taken. HurtDamageFloat is set before HurtTrigger.")]
        public string HurtDamageFloat;
        protected int HurtDamageFloatHash;

        /// <summary>
        /// Name of an Animator trigger set when killed.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator trigger set when killed.")]
        public string DeathTrigger;
        protected int DeathTriggerHash;
        #endregion

        [Foldout("Debug")]
        public bool IsDead;

        public virtual void Reset()
        {
            OnHurt = new HealthEvent();
            OnDeath = new HealthEvent();

            Animator = GetComponentInParent<Animator>();
            HurtTrigger = "";
            HurtDamageFloat = "";
            DeathTrigger = "";
        }

        public virtual void Awake()
        {
            OnHurt = OnHurt ?? new HealthEvent();
            OnDeath = OnDeath ?? new HealthEvent();

            InvincibleTimer = 0f;
            Invincible = false;

            HurtTriggerHash = Animator.StringToHash(HurtTrigger);
            HurtDamageFloatHash = Animator.StringToHash(HurtDamageFloat);
            DeathTriggerHash = Animator.StringToHash(DeathTrigger);
        }

        public virtual void Start()
        {

        }

        public virtual void Update()
        {
            if (!Invincible) return;

            if (InvincibleTimer > 0f)
            {
                if ((InvincibleTimer -= Time.deltaTime) < 0f)
                {
                    InvincibleTimer = 0f;
                    Invincible = false;
                }
            }
        }

        public void TakeDamage()
        {
            TakeDamage(1f, null);
        }

        public void TakeDamage(float damage)
        {
            TakeDamage(damage, null);
        }

        public void TakeDamage(Transform source)
        {
            TakeDamage(1f, source);
        }

        public virtual void TakeDamage(float damage, Transform source)
        {
            if (Invincible || IsDead) return;

            Health -= damage;
            OnHurt.Invoke(new HealthEventArgs { Damage = damage, Source = source });

            if (Animator != null)
            {
                if (HurtDamageFloatHash != 0) Animator.SetFloat(HurtDamageFloatHash, damage);
                if (HurtTriggerHash != 0) Animator.SetTrigger(HurtTriggerHash);
            }

            if (Health > 0f) return;

            Health = 0f;
            OnDeath.Invoke(new HealthEventArgs {Damage = damage, Source = source});

            if (Animator != null && DeathTriggerHash != 0)
                Animator.SetTrigger(DeathTrigger);
        }

        public void Kill()
        {
            Kill(null);
        }

        public virtual void Kill(Transform source)
        {
            if (IsDead) return;

            IsDead = true;
            OnDeath.Invoke(new HealthEventArgs { Damage = 0f, Source = source });
            if (Animator != null && DeathTriggerHash != 0)
                Animator.SetTrigger(DeathTrigger);
        }

        public void MakeInvincible()
        {
            MakeInvincible(10f);
        }

        public virtual void MakeInvincible(float time)
        {
            InvincibleTimer = time;
            Invincible = true;
        }
    }
}
