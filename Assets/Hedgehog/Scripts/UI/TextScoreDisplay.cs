using UnityEngine.UI;

namespace Hedgehog.UI
{
    public class TextScoreDisplay : BaseScoreDisplay
    {
        public Text Text;

        public override void Display(int score)
        {
            Text.text = score.ToString();
        }
    }
}
