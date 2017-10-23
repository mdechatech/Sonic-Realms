using UnityEngine;

namespace SonicRealms.Core.Internal
{
    /// <summary>
    /// Use with <see cref="AnimationCurve"/> to control how it's drawn.
    /// </summary>
    public class SrCurveOptionsAttribute : PropertyAttribute
    {
        public float? XMin { get; set; }
        public float? XMax { get; set; }
        public float? YMin { get; set; }
        public float? YMax { get; set; }
        public uint? Color { get; set; }

        public SrCurveOptionsAttribute(float xMin, float yMin, float xMax, float yMax)
        {
            XMin = xMin;
            YMin = yMin;
            XMax = xMax;
            YMax = yMax;
        }

        public SrCurveOptionsAttribute(uint color, float xMin, float yMin, float xMax, float yMax)
        {
            Color = color;
            XMin = xMin;
            YMin = yMin;
            XMax = xMax;
            YMax = yMax;
        }

        public SrCurveOptionsAttribute(uint color)
        {
            Color = color;
        }

    } // Class
} // Namespace
