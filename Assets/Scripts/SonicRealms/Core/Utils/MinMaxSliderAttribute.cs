using UnityEngine;

namespace SonicRealms.Core.Utils
{
    public class MinMaxSliderAttribute : PropertyAttribute
    {
        public readonly float Max;
        public readonly float Min;

        public MinMaxSliderAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}