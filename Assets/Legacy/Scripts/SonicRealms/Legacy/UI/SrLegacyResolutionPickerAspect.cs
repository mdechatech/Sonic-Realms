using UnityEngine;
using UnityEngine.UI;

namespace SonicRealms.Legacy.UI
{
    public class SrLegacyResolutionPickerAspect : MonoBehaviour
    {
        public SrLegacyResolutionSettings.AspectRatio Value
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

        private SrLegacyResolutionSettings.AspectRatio _value;

        protected virtual void Reset()
        {
            _text = GetComponentInChildren<Text>();
        }

        protected virtual void UpdateValue()
        {
            _text.text = string.Format("{0}:{1}", _value.Horizontal, _value.Vertical);
        }
    }
}
