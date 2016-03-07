using SonicRealms.Core.Actors;
using SonicRealms.Core.Triggers;

namespace SonicRealms.Level.Effects
{
    /// <summary>
    /// Kills the player when activated.
    /// </summary>
    public class KillPlayer : ReactiveObject
    {
        public override void OnActivate(HedgehogController controller)
        {
            var health = controller.GetComponent<HedgehogHealth>();
            if (health == null)
                return;

            health.Kill();
        }

        public override void OnActivateStay(HedgehogController controller)
        {
            OnActivate(controller);
        }
    }
}
