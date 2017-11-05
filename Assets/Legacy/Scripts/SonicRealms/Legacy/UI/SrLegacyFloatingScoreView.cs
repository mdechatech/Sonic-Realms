using UnityEngine;
using UnityEngine.Serialization;

namespace SonicRealms.Legacy.UI
{
    public class SrLegacyFloatingScoreView : SrLegacyScoreView
    {
        public TextMesh TextMesh { get { return _textMesh; } set { _textMesh = value; } }

        public override int Value
        {
            get { return _value; }
            set
            {
                _value = value;
                UpdateScore();
            }
        }

        [SerializeField]
        [FormerlySerializedAs("TextMesh")]
        [Tooltip("Text Mesh used to show the score value.")]
        private TextMesh _textMesh;

        private int _value;

        protected void Reset()
        {
            _textMesh = GetComponentInChildren<TextMesh>();
        }

        private void UpdateScore()
        {
            if (_textMesh)
                _textMesh.text = _value.ToString();
        }
    }
}
