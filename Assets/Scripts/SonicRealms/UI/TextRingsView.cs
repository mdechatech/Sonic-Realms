using UnityEngine;
using UnityEngine.UI;

namespace SonicRealms.UI
{
    /// <summary>
    /// Ring counter.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class TextRingsView : RingsView
    {
        /// <summary>
        /// The text on which to show the ring amount.
        /// </summary>
        [Tooltip("The text on which to show the ring amount.")]
        public Text Text;

        /// <summary>
        /// The target animator.
        /// </summary>
        [Header("Animation")]
        [Tooltip("The target animator.")]
        public Animator Animator;
        
        /// <summary>
        /// Name of an Animator int set to the number of rings collected.
        /// </summary>
        [Tooltip("Name of an Animator int set to the number of rings collected.")]
        public string AmountInt;
        protected int AmountIntHash;

        public void Reset()
        {
            Text = GetComponent<Text>();

            Animator = GetComponent<Animator>();
            AmountInt = "";
        }

        public void Start()
        {
            Animator = Animator ? Animator : GetComponent<Animator>();
            AmountIntHash = Animator == null ? 0 : Animator.StringToHash(AmountInt);
        }

        public override void Show(int rings)
        {
            Text.text = rings.ToString();
            if(Animator != null && AmountIntHash != 0) Animator.SetInteger(AmountIntHash, rings);
        }
    }
}
