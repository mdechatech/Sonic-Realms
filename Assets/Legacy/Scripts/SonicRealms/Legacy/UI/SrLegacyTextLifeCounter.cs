using UnityEngine.UI;

namespace SonicRealms.Legacy.UI
{
    public class SrLegacyTextLifeCounter : SrLegacyBaseLifeCounter
    {
        public Text Text;

        public override void Display(int lives)
        {
            Text.text = lives.ToString();
        }
    }
}
