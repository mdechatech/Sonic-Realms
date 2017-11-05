using UnityEngine;

namespace SonicRealms.Core.Internal
{
    public class SrWaitForAll : CustomYieldInstruction
    {
        private readonly CustomYieldInstruction[] _instructions;

        public override bool keepWaiting
        {
            get
            {
                for (var i = 0; i < _instructions.Length; ++i)
                {
                    if (_instructions[i].keepWaiting)
                        return true;
                }

                return false;
            }
        }

        public SrWaitForAll(params CustomYieldInstruction[] instructions)
        {
            this._instructions = instructions;
        }
    }
}
