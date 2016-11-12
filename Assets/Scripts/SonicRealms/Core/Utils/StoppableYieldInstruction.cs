using UnityEngine;

namespace SonicRealms.Core.Utils
{
    public abstract class StoppableYieldInstruction : CustomYieldInstruction
    {
        public abstract bool Stop();
    }
}
