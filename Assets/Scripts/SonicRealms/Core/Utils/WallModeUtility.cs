using SonicRealms.Core.Actors;

namespace SonicRealms.Core.Utils
{
    public static class WallModeUtility
    {
        public static WallMode FromSurface(float surfaceAngleDegrees)
        {
            var fixedAngle = SrMath.PositiveAngle_d(surfaceAngleDegrees);
            if(fixedAngle >= 315 || fixedAngle <= 45)
                return WallMode.Floor;

            if (fixedAngle <= 135)
                return WallMode.Right;

            if(fixedAngle <= 225)
                return WallMode.Ceiling;

            if (fixedAngle < 315)
                return WallMode.Left;

            return WallMode.None;
        }

        public static float ToSurface(this WallMode mode)
        {
            switch (mode)
            {
                case WallMode.Left:
                    return 270;

                case WallMode.Ceiling:
                    return 180;

                case WallMode.Right:
                    return 90;

                case WallMode.Floor:
                    return 0;
            }

            return -1;
        }

        public static float ToNormal(this WallMode mode)
        {
            if (mode == WallMode.None)
                return -1;

            return ToSurface(mode) + 90;
        }
    }
}
