using UnityEngine;

namespace Hedgehog.Core.Actors
{
    /// <summary>
    /// Keeps track of rings and hooks them up to animation.
    /// </summary>
    public class RingCounter : MonoBehaviour
    {
        public HedgehogController Controller;
        public int Amount;

        protected Animator Animator;

        /// <summary>
        /// Name of an Animator int set to the current ring amount.
        /// </summary>
        [Tooltip("Name of an Animator int set to the current ring amount.")]
        public string AmountInt;
        protected int AmountIntHash;

        public void Reset()
        {
            Amount = 0;
            AmountInt = "";
        }

        public void Awake()
        {
            Amount = 0;

            Animator = Controller.Animator;

            if (Animator == null) return;
            AmountIntHash = string.IsNullOrEmpty(AmountInt) ? 0 : Animator.StringToHash(AmountInt);
        }

        public static implicit operator int (RingCounter ringCounter)
        {
            return ringCounter ? ringCounter.Amount : 0;
        }
    }
}
