using System;

namespace SonicRealms.Legacy.Game
{
    [Serializable]
    public class NextLevelCharacterOverride
    {
        public SrLegacyCharacterData Character;
        public SrLegacyLevelData NextLevel;
    }
}
