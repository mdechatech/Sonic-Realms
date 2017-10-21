using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SonicRealms.Legacy.UI
{
    public class TextScoreView : ScoreView
    {
        public override int Value
        {
            get { return _value; }
            set
            {
                _value = value;
                UpdateScore();
            }
        }

        public Text Text { get { return _text; } set { _text = value; } }

        [SerializeField]
        [FormerlySerializedAs("Text")]
        [Tooltip("Text used to show the current score value.")]
        private Text _text;

        private int _value;

        protected void Reset()
        {
            Text = GetComponentInChildren<Text>();
        }

        private void UpdateScore()
        {
            if (Text)
                Text.text = _value.ToString();
        }
    }
}
