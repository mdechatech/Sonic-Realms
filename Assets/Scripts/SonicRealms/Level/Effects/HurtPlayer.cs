using SonicRealms.Core.Actors;
using SonicRealms.Core.Triggers;

namespace SonicRealms.Level.Effects
{
    /// <summary>
    /// Hurts the player when activated.
    /// </summary>
    public class HurtPlayer : ReactiveObject
    {
        public override void OnActivatorEnter(HedgehogController controller)
        {
            var health = controller.GetComponent<HedgehogHealth>();
            if (health == null) return;
            
            health.TakeDamage(transform);
        }

        public override void OnActivatorStay(HedgehogController controller)
        {
            OnActivatorEnter(controller);
        }
    }
}
