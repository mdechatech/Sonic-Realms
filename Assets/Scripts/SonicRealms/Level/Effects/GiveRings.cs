using SonicRealms.Core.Actors;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Effects
{
    /// <summary>
    /// When activated, gives the controller rings.
    /// </summary>
    public class GiveRings : ReactiveEffect
    {
        /// <summary>
        /// Number of rings to give.
        /// </summary>
        [Tooltip("Number of rings to give.")]
        public int Amount;

        [Foldout("Animation")]
        public Animator Animator;

        /// <summary>
        /// Animator trigger set when the rings are successfully given.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Animator trigger set when the rings are successfully given.")]
        public string GivenTrigger;
        protected int GivenTriggerHash;

        public override void Reset()
        {
            base.Reset();

            Amount = 1;

            Animator = GetComponent<Animator>();
        }

        public override void Awake()
        {
            base.Awake();

            GivenTriggerHash = Animator.StringToHash(GivenTrigger);
        }

        public override void OnActivate(HedgehogController controller)
        {
            var counter = controller.GetComponentInChildren<RingCounter>();
            if (counter == null || !counter.CanCollect)
                return;

            counter.Rings += Amount;

            if (Animator && GivenTriggerHash != 0)
                Animator.SetTrigger(GivenTriggerHash);
        }
    }
}
