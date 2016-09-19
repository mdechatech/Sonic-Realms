using UnityEngine;

namespace SonicRealms.Core.Utils
{
    public class SetIndentLevelAttribute : PropertyAttribute
    {
        public int Level;

        public SetIndentLevelAttribute(int level)
        {
            Level = level;
        }
    }
}
