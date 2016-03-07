using UnityEngine.UI;

namespace SonicRealms.UI
{
    public class TextScoreView : ScoreView
    {
        public Text Text;

        public override void Show(int score)
        {
            Text.text = score.ToString();
        }
    }
}
