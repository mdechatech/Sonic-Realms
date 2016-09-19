using UnityEngine;

namespace SonicRealms.Core.Utils
{
    public class AddIndentLevelAttribute : PropertyAttribute
    {
        public int Amount;

        public AddIndentLevelAttribute(int amount = 1)
        {
            Amount = amount;
        }
    }
}
