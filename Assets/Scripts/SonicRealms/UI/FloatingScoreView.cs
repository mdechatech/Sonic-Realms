using UnityEngine;

namespace SonicRealms.UI
{
    public class FloatingScoreView : ScoreView
    {
        public TextMesh TextMesh;

        public void Reset()
        {
            TextMesh = GetComponentInChildren<TextMesh>();
        }

        public override void Show(int score)
        {
            if(TextMesh != null) TextMesh.text = score.ToString();
        }
    }
}
