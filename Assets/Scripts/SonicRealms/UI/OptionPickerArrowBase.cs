using UnityEngine;

namespace SonicRealms.UI
{
    public abstract class OptionPickerArrowBase : MonoBehaviour
    {
        private bool _isShown;
        private bool _isFocused;

        public bool IsShown { get { return _isShown; } }

        public bool IsFocused { get { return _isFocused; } }

        public void Show()
        {
            _isShown = true;
            OnShow();
        }

        public void Hide()
        {
            _isShown = false;
            OnHide();
        }

        public void Focus()
        {
            _isFocused = true;
            OnFocus();
        }

        public void Unfocus()
        {
            _isFocused = false;
            OnUnfocus();
        }

        public void Press()
        {
            OnPress();
        }

        protected virtual void OnShow()
        {

        }

        protected virtual void OnHide()
        {

        }

        protected virtual void OnFocus()
        {

        }

        protected virtual void OnUnfocus()
        {

        }

        protected virtual void OnPress()
        {

        }
    }
}
