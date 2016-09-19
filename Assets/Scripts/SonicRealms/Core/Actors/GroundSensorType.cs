using System;

namespace SonicRealms.Core.Actors
{
    /// <summary>
    /// Represents each sensor on the bottom of the player.
    /// </summary>
    [Flags]
    public enum GroundSensorType
    {
        None = 0,
        Left = 1 << 0,
        Right = 1 << 1,
        Both = Left | Right
    }

    public static class GroundSideUtility
    {
        public static GroundSensorType Flip(this GroundSensorType side)
        {
            switch (side)
            {
                case GroundSensorType.None:
                    return GroundSensorType.Both;

                case GroundSensorType.Left:
                    return GroundSensorType.Right;

                case GroundSensorType.Right:
                    return GroundSensorType.Left;

                case GroundSensorType.Both:
                    return GroundSensorType.None;
            }

            return GroundSensorType.None;
        }

        public static bool Has(this GroundSensorType _this, GroundSensorType side)
        {
            return (_this & side) == side;
        }
    }
}