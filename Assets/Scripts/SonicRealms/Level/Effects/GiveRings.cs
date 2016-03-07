using SonicRealms.Core.Actors;
using SonicRealms.Core.Triggers;
using UnityEngine;

namespace SonicRealms.Level.Effects
{
    /// <summary>
    /// When activated, gives the controller rings.
    /// </summary>
    public class GiveRings : ReactiveObject
    {
        /// <summary>
        /// Number of rings to give.
        /// </summary>
        [Tooltip("Number of rings to give.")]
        public int Amount;

        /// <summary>
        /// Animator trigger set when the rings are successfully given.
        /// </summary>
        [Tooltip("Animator trigger set when the rings are successfully given.")]
        public string GivenTrigger;
        protected int GivenTriggerHash;

        public override void Reset()
        {
            base.Reset();
            Amount = 1;
            GivenTrigger = "";
        }

        public override void Awake()
        {
            base.Awake();
            if (Animator != null && !string.IsNullOrEmpty(GivenTrigger))
                GivenTriggerHash = Animator.StringToHash(GivenTrigger);
        }

        public override void OnActivate(HedgehogController controller)
        {
            var counter = controller.GetComponentInChildren<RingCounter>();
            if (counter == null || !counter.CanCollect)
                return;

            counter.Rings += Amount;

            if (GivenTriggerHash != 0)
                Animator.SetTrigger(GivenTriggerHash);
        }

        public override void OnActivateStay(HedgehogController controller)
        {
            if (Activated) return;
            OnActivate(controller);
        }
    }
}
