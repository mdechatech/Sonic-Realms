using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;

namespace Hedgehog.Level.Effects
{
    /// <summary>
    /// Kills the player when activated.
    /// </summary>
    public class KillPlayer : ReactiveObject
    {
        public override void OnActivateEnter(HedgehogController controller)
        {
            var health = controller.GetComponent<HedgehogHealth>();
            if (health == null)
                return;

            health.Kill();
        }

        public override void OnActivateStay(HedgehogController controller)
        {
            OnActivateEnter(controller);
        }
    }
}
