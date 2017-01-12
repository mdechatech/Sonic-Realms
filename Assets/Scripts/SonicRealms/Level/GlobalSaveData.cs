using System;

namespace SonicRealms.Level
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
