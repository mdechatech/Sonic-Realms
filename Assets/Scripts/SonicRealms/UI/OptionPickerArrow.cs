using UnityEngine;
using UnityEngine.UI;

namespace SonicRealms.UI
{
    [RequireComponent(typeof(Image))]
    public class OptionPickerArrow : OptionPickerArrowBase
    {
        [SerializeField]
        private Color _hiddenColor;

        [SerializeField]
        private Color _shownColor;

        [SerializeField]
        private Color _focusedColor;

        [SerializeField]
        private Color _pressedColor;

        private Image _image;

        protected void Reset()
        {
            _hiddenColor = Color.clear;
            _shownColor = GetComponent<Image>().color;
            _focusedColor = Color.white;
            _pressedColor = Color.cyan;
        }

        protected void Awake()
        {
            _image = GetComponent<Image>();
        }

        protected override void OnShow()
        {
            _image.enabled = true;
            _image.color = _shownColor;
        }

        protected override void OnHide()
        {
            _image.color = _hiddenColor;
        }

        protected override void OnFocus()
        {
            _image.color = _focusedColor;
        }

        protected override void OnUnfocus()
        {
            _image.color = _shownColor;
        }
    }
}
