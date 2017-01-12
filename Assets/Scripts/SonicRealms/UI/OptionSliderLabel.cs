using UnityEngine;
using UnityEngine.UI;

namespace SonicRealms.UI
{
    [RequireComponent(typeof(Text))]
    public class OptionSliderLabel : OptionSliderLabelBase
    {
        [SerializeField]
        private Color _focusedColor;

        [SerializeField]
        private Color _unfocusedColor;

        private Text _text;

        protected void Reset()
        {
            _focusedColor = Color.white;
            _unfocusedColor = GetComponent<Text>().color;
        }

        protected void Awake()
        {
            _text = GetComponent<Text>();
        }

        protected override void OnFocus()
        {
            _text.color = _focusedColor;
        }

        protected override void OnUnfocus()
        {
            _text.color = _unfocusedColor;
        }
    }
}
