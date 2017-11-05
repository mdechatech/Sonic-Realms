using UnityEngine;

namespace SonicRealms.Core.Internal
{
    public class SrSetIndentLevelAttribute : PropertyAttribute
    {
        public int Level;

        public SrSetIndentLevelAttribute(int level)
        {
            Level = level;
        }
    }
}
