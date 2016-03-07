using UnityEngine.UI;

namespace SonicRealms.UI
{
    public class TextLifeCounter : BaseLifeCounter
    {
        public Text Text;

        public override void Display(int lives)
        {
            Text.text = lives.ToString();
        }
    }
}
