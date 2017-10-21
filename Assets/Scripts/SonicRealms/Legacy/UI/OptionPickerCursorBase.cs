using UnityEngine;

namespace SonicRealms.Legacy.UI
{
    public abstract class OptionPickerCursorBase : MonoBehaviour
    {
        private bool _isShown;

        public bool IsShown { get { return _isShown; } }

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

        public void ChangeSelection()
        {
            OnChangeSelection();
        }

        protected virtual void OnShow()
        {

        }

        protected virtual void OnHide()
        {

        }

        protected virtual void OnChangeSelection()
        {

        }
    }
}
