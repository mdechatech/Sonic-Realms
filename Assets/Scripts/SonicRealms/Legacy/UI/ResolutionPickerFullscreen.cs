using UnityEngine;
using UnityEngine.UI;

namespace SonicRealms.Legacy.UI
{
    public class ResolutionPickerFullscreen : MonoBehaviour
    {
        public bool Value
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

        private bool _value;

        private void UpdateValue()
        {
            _text.text = Value ? "on" : "off";
        }

        private void Reset()
        {
            _text = GetComponentInChildren<Text>();
        }
    }
}
