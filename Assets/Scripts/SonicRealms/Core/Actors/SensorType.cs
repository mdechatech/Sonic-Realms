using System;

namespace SonicRealms.Core.Actors
{
    [Flags]
    public enum SensorType
    {
        None = 0,
        BottomLeft = 1 << 0,
        BottomRight = 1 << 1,
        TopLeft = 1 << 2,
        TopRight = 1 << 3,
        CenterLeft = 1 << 4,
        CenterRight = 1 << 5,
        SolidLeft = 1 << 6,
        SolidRight = 1 << 7,
        Other = 1 << 10,

        All = BottomLeft | BottomRight |
            TopLeft | TopRight | 
            CenterLeft | CenterRight |
            SolidLeft | SolidRight
            | Other
    }

    public static class SensorTypeUtility
    {
        public static bool Has(this SensorType _this, SensorType sensor)
        {
            return (_this & sensor) == sensor;
        }

        public static GroundSensorType ToGroundSensor(this SensorType sensor)
        {
            if (sensor.Has(SensorType.BottomLeft) && sensor.Has(SensorType.BottomRight))
                return GroundSensorType.Both;

            if(sensor.Has(SensorType.BottomLeft))
                return GroundSensorType.Left;

            if(sensor.Has(SensorType.BottomRight))
                return GroundSensorType.Right;
            
            return GroundSensorType.None;
        }
    }
}
