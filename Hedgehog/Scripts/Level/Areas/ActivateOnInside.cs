using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;

namespace Hedgehog.Level.Areas
{
    /// <summary>
    /// Generic area that activates the object trigger when a controller is inside it.
    /// </summary>
    public class ActivateOnInside : ReactiveArea
    {
        public override void OnAreaEnter(HedgehogController controller)
        {
            ActivateObject(controller);
        }

        public override void OnAreaExit(HedgehogController controller)
        {
            DeactivateObject(controller);
        }
    }
}
