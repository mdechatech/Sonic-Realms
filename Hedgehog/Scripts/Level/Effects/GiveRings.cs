using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;

namespace Hedgehog.Level.Effects
{
    /// <summary>
    /// When activated, gives the controller rings.
    /// </summary>
    public class GiveRings : ReactiveObject
    {
        /// <summary>
        /// Number of rings to give.
        /// </summary>
        public int Amount;

        public override void Reset()
        {
            base.Reset();
            Amount = 1;
        }

        public override void OnActivateEnter(HedgehogController controller)
        {
            var counter = controller.GetComponentInChildren<RingCounter>();
            if (counter == null) return;

            counter.Amount += Amount;
        }
    }
}
