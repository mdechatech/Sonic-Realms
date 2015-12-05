using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using UnityEngine;

namespace Hedgehog.Level.Objects
{
    /// <summary>
    /// A ring that can be collected.
    /// </summary>
    public class Ring : ReactiveArea
    {
        /// <summary>
        /// How many rings to give when collected.
        /// </summary>
        [Tooltip("How many rings to give when collected.")]
        public int Value;

        /// <summary>
        /// Name of an Animator trigger set when collected.
        /// </summary>
        [Tooltip("Name of an Animator trigger set when collected.")]
        public string CollectedTrigger;
        protected int CollectedTriggerHash;

        /// <summary>
        /// Whether the ring has been collected.
        /// </summary>
        [Tooltip("Whether the ring has been collected.")]
        public bool Collected;

        public override bool ActivatesObject
        {
            get { return true; }
        }

        public override void Reset()
        {
            base.Reset();
            Value = 1;
            CollectedTrigger = "";
        }

        public override void Awake()
        {
            base.Awake();
            Collected = false;
            if (Animator != null && !string.IsNullOrEmpty(CollectedTrigger))
                CollectedTriggerHash = Animator.StringToHash(CollectedTrigger);
        }

        public override void OnAreaStay(HedgehogController controller)
        {
            if (Collected) return;

            var collector = controller.GetComponent<RingCollector>();
            if (collector == null || !collector.CanCollect) return;

            collector.Amount += Value;

            if(Animator != null && CollectedTriggerHash != 0)
                Animator.SetTrigger(CollectedTriggerHash);

            Collected = true;
            TriggerObject(controller);
        }
    }
}
