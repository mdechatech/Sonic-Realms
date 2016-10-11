using UnityEngine;

namespace SonicRealms.Core.Utils
{
    public class MinMaxSliderAttribute : PropertyAttribute
    {
        public readonly float Max;
        public readonly float Min;

        public readonly float LabelWidth;
        public readonly string LabelFormat;

        public MinMaxSliderAttribute(float min, float max, float labelWidth = 100, string labelFormat = "{0} - {1}")
        {
            Min = min;
            Max = max;

            LabelWidth = labelWidth;
            LabelFormat = labelFormat;
        }
    }
}