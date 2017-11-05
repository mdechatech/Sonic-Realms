using UnityEngine;
using UnityEngine.UI;

namespace SonicRealms.Legacy.UI
{
    public class SrLegacyResolutionPickerScreenSize : MonoBehaviour
    {
        public SrLegacyResolutionSettings.ScreenSize Value
        {
            get { return _value; }
            set
            {
                _value = value;
                UpdateValue();
            }
        }

        [SerializeField]
        private Text _text;

        private SrLegacyResolutionSettings.ScreenSize _value;

        protected virtual void Reset()
        {
            _text = GetComponentInChildren<Text>();
        }

        protected virtual void UpdateValue()
        {
            _text.text = string.Format("{0}x{1}", _value.Width, _value.Height);
        }
    }
}
