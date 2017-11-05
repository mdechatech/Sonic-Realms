using UnityEngine;

namespace SonicRealms.Core.Internal
{
    public class SrMinMaxSliderAttribute : PropertyAttribute
    {
        public readonly float Max;
        public readonly float Min;

        public SrMinMaxSliderAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}