using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;

namespace Hedgehog.Level.Effects
{
    /// <summary>
    /// Hurts the player when activated.
    /// </summary>
    public class HurtPlayer : ReactiveObject
    {
        public override void OnActivateEnter(HedgehogController controller)
        {
            var health = controller.GetComponent<HedgehogHealth>();
            if (health == null) return;

            health.TakeDamage(transform);
        }

        public override void OnActivateStay(HedgehogController controller)
        {
            OnActivateEnter(controller);
        }
    }
}
