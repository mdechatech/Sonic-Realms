using UnityEngine;

namespace SonicRealms.Core.Utils
{
    public class WaitForAll : CustomYieldInstruction
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

        public WaitForAll(params CustomYieldInstruction[] instructions)
        {
            this._instructions = instructions;
        }
    }
}
