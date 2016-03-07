using UnityEngine;

namespace SonicRealms.Core.Actors
{
    /// <summary>
    /// Health system for badniks... which is a one hit kill.
    /// </summary>
    public class BadnikHealth : HealthSystem
    {
        private bool _dying;

        /// <summary>
        /// Whether to add to the killer's combo on death.
        /// </summary>
        [Tooltip("Whether to add to the killer's combo on death.")]
        public bool AddsToCombo;

        public override float Health
        {
            get { return 1f; }
            set { if (value <= 0f) Kill(); }
        }

        public override void Reset()
        {
            base.Reset();
            AddsToCombo = true;
        }

        public override void Awake()
        {
            base.Awake();
            _dying = false;
        }

        public override void TakeDamage(float damage, Transform source)
        {
            if (damage > 0f) Kill(source);
        }

        public override void Kill(Transform source)
        {
            if (_dying) return;

            var hitbox = source.GetComponent<SonicHitbox>();
            if (hitbox != null)
            {
                hitbox.NotifyEnemyKilled(this);

                if (AddsToCombo)
                {
                    var score = hitbox.Controller.GetComponent<ScoreCounter>();
                    if (score != null) score.AddCombo(transform);
                }
            }

            _dying = true;
            base.Kill(source);
        }
    }
}
