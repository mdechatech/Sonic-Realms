using UnityEngine;

namespace SonicRealms.Core.Utils
{
    public class WaitForAny : CustomYieldInstruction
    {
        private readonly CustomYieldInstruction[] _instructions;
        private readonly StoppableYieldInstruction[] _stoppableInstructions;

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

        public WaitForAny(params CustomYieldInstruction[] instructions)
        {
            _instructions = instructions;
        }

        public WaitForAny(bool stopAllWhenDone, params StoppableYieldInstruction[] stoppableInstructions)
        {
            _stopAllWhenDone = stopAllWhenDone;
            _stoppableInstructions = stoppableInstructions;
        }
    }
}
