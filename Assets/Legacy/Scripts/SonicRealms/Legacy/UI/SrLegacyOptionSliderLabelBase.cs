using UnityEngine;

namespace SonicRealms.Legacy.UI
{
    public abstract class SrLegacyOptionSliderLabelBase : MonoBehaviour
    {
        public bool IsFocused { get; private set; }

        public void Focus()
        {
            IsFocused = true;
            OnFocus();
        }

        public void Unfocus()
        {
            IsFocused = false;
            OnUnfocus();
        }

        protected virtual void OnFocus()
        {

        }

        protected virtual void OnUnfocus()
        {

        }
    }
}
