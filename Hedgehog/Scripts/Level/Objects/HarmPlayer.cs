using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;

namespace Hedgehog.Level.Objects
{
    public class HarmPlayer : ReactiveObject
    {
        public override void OnActivateEnter(HedgehogController controller)
        {
            var health = controller.GetComponent<HedgehogHealth>();
            if (health == null) return;

            health.Hurt(transform.position);
        }

        public override void OnActivateStay(HedgehogController controller)
        {
            OnActivateEnter(controller);
        }
    }
}
