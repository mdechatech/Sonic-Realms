using UnityEngine;

namespace SonicRealms.Core.Internal
{
    public class SrFoldoutAttribute : PropertyAttribute
    {
        public string Name;
        public int? Order;

        public SrFoldoutAttribute(string name)
        {
            Name = name;
            Order = null;
        }

        public SrFoldoutAttribute(string name, int order)
        {
            Name = name;
            Order = order;
        }
    }
}
