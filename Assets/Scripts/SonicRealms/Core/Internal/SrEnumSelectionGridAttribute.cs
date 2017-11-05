using UnityEngine;

namespace SonicRealms.Core.Internal
{
    public class SrEnumSelectionGridAttribute : PropertyAttribute
    {
        public int MinElementWidth { get; set; }
        public bool NicifyNames { get; set; }

        public SrEnumSelectionGridAttribute()
        {
            MinElementWidth = 100;
            NicifyNames = true;
        }

    }
}
