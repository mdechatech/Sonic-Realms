using System;

namespace SonicRealms.Legacy.Game
{
    [Serializable]
    public class GlobalSaveData
    {
        public bool Fullscreen;
        public int FullscreenScreenSize;
        public int WindowedAspect;
        public int WindowedScreenSize;

        public int MasterVolume;
        public int MusicVolume;
        public int FxVolume;
    }
}
