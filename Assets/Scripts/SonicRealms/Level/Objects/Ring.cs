using SonicRealms.Core.Actors;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Objects
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
        /// Whether the ring has been collected.
        /// </summary>
        [Tooltip("Whether the ring has been collected.")]
        public bool Collected;

        /// <summary>
        /// An audio clip to play when the ring is collected.
        /// </summary>
        [Tooltip("An audio clip to play when the ring is collected.")]
        public AudioClip CollectedSound;

        [Foldout("Animation")]
        public Animator Animator;

        /// <summary>
        /// Name of an Animator trigger set when collected.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator trigger set when collected.")]
        public string CollectedTrigger;
        protected int CollectedTriggerHash;

        /// <summary>
        /// Next ring sound will pan right if true, pan left otherwise.
        /// </summary>
        protected static bool PanRight;

        public override void Reset()
        {
            base.Reset();

            Value = 1;

            Animator = Animator.GetComponent<Animator>();
        }

        public override void Awake()
        {
            base.Awake();

            Animator = Animator ? Animator : Animator.GetComponent<Animator>();
            CollectedTriggerHash = Animator.StringToHash(CollectedTrigger);
        }

        public override void OnAreaStay(AreaCollision collision)
        {
            if (Collected)
                return;

            var controller = collision.Controller;

            var collector = controller.GetComponent<RingCounter>();
            if (collector == null || !collector.CanCollect)
                return;

            collector.Rings += Value;

            if(Animator && CollectedTriggerHash != 0)
                Animator.SetTrigger(CollectedTriggerHash);

            Collected = true;

            if (CollectedSound != null)
            {
                var source = SoundManager.Instance.PlayClipAtPoint(CollectedSound, transform.position);
                source.panStereo = (PanRight = !PanRight) ? 1f : -1f;
            }

            BlinkEffectTrigger(controller);
        }
    }
}
