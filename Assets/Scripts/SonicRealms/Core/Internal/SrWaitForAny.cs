using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Internal
{
    public class SrWaitForAny : CustomYieldInstruction
    {
        private readonly CustomYieldInstruction[] _instructions;
        private readonly SrStoppableYieldInstruction[] _stoppableInstructions;

        private readonly bool _stopAllWhenDone;

        public override bool keepWaiting
        {
            get
            {
                if (_instructions != null)
                {
                    for (var i = 0; i < _instructions.Length; ++i)
                    {
                        if (!_instructions[i].keepWaiting)
                            return false;
                    }

                    return true;
                }

                if (_stoppableInstructions != null)
                {
                    for (var i = 0; i < _stoppableInstructions.Length; ++i)
                    {
                        if (!_stoppableInstructions[i].keepWaiting)
                        {
                            if (_stopAllWhenDone)
                            {
                                for (var j = 0; j < _stoppableInstructions.Length; ++j)
                                {
                                    _stoppableInstructions[j].Stop();
                                }
                            }

                            return false;
                        }
                    }

                    return true;
                }

                return true;
            }
        }

        public SrWaitForAny(params CustomYieldInstruction[] instructions)
        {
            _instructions = instructions;
        }

        public SrWaitForAny(bool stopAllWhenDone, params SrStoppableYieldInstruction[] stoppableInstructions)
        {
            _stopAllWhenDone = stopAllWhenDone;
            _stoppableInstructions = stoppableInstructions;
        }
    }
}
