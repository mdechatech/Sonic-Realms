using UnityEngine;

namespace SonicRealms.Core.Utils
{
    public class EnumSelectionGridAttribute : PropertyAttribute
    {
        public int MinElementWidth { get; set; }
        public bool NicifyNames { get; set; }

        public EnumSelectionGridAttribute()
        {
            MinElementWidth = 100;
            NicifyNames = true;
        }

    }
}
