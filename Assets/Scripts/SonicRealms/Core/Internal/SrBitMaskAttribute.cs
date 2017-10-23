using UnityEngine;

namespace SonicRealms.Core.Internal
{
    public class SrBitMaskAttribute : PropertyAttribute
    {
        public System.Type propType;
        public SrBitMaskAttribute(System.Type aType)
        {
            propType = aType;
        }
    }
}
