using UnityEngine;

namespace SonicRealms.Core.Internal
{
    public class SrAddIndentLevelAttribute : PropertyAttribute
    {
        public int Amount;

        public SrAddIndentLevelAttribute(int amount = 1)
        {
            Amount = amount;
        }
    }
}
