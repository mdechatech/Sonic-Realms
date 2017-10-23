using UnityEngine;

namespace SonicRealms.Core.Internal
{
    public abstract class SrStoppableYieldInstruction : CustomYieldInstruction
    {
        public abstract bool Stop();
    }
}
