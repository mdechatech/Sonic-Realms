using UnityEngine;

namespace SonicRealms.Core.Utils
{
    public class FoldoutAttribute : PropertyAttribute
    {
        public string Name;
        public int? Order;

        public FoldoutAttribute(string name)
        {
            Name = name;
            Order = null;
        }

        public FoldoutAttribute(string name, int order)
        {
            Name = name;
            Order = order;
        }
    }
}
