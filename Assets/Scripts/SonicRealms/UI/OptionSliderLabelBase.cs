using UnityEngine;

namespace SonicRealms.UI
{
    public abstract class OptionSliderLabelBase : MonoBehaviour
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
