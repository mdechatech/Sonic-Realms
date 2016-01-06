using UnityEngine;

namespace Hedgehog.Core.Actors
{
    /// <summary>
    /// Health system for badniks... which is a one hit kill.
    /// </summary>
    public class BadnikHealth : HealthSystem
    {
        public override float Health
        {
            get { return 1f; }
            set { if (value <= 0f) Kill(); }
        }

        public override void TakeDamage(float damage, Transform source)
        {
            if (damage > 0f) Kill(source);
        }

        public override void Kill(Transform source)
        {
            var hitbox = source.GetComponent<SonicHitbox>();
            if(hitbox != null) hitbox.NotifyEnemyKilled(this);

            base.Kill(source);
        }
    }
}
