using SonicRealms.Core.Actors;
using SonicRealms.Core.Triggers;
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

        /// <summary>
        /// An audio clip to play when the ring is collected.
        /// </summary>
        [Tooltip("An audio clip to play when the ring is collected.")]
        public AudioClip CollectedSound;

        /// <summary>
        /// Next ring sound will pan right if true, pan left otherwise.
        /// </summary>
        protected static bool PanRight;

        public override void Reset()
        {
            base.Reset();
            Value = 1;
            CollectedTrigger = "";
            CollectedSound = null;
        }

        public override void Awake()
        {
            base.Awake();
            Collected = false;
            
            if (Animator != null && !string.IsNullOrEmpty(CollectedTrigger))
                CollectedTriggerHash = Animator.StringToHash(CollectedTrigger);
        }

        public override void OnAreaStay(Hitbox hitbox)
        {
            var controller = hitbox.Controller;
            if (Collected) return;

            var collector = controller.GetComponent<RingCounter>();
            if (collector == null || !collector.CanCollect) return;

            collector.Rings += Value;

            if(Animator != null && CollectedTriggerHash != 0)
                Animator.SetTrigger(CollectedTriggerHash);

            Collected = true;

            if (CollectedSound != null)
            {
                var source = SoundManager.Instance.PlayClipAtPoint(CollectedSound, transform.position);
                source.panStereo = (PanRight = !PanRight) ? 1f : -1f;
            }

            TriggerObject(controller);
        }
    }
}
