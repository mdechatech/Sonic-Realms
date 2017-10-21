using System;
using UnityEngine;

namespace SonicRealms.Legacy.UI
{
    [Serializable]
    public class UiStateTransition
    {
        public string FromState { get { return _fromState; } }

        public bool FromAnyState { get { return _fromAnyState; } }

        public string ToState { get { return _toState; } }

        public bool ToAnyState { get { return _toAnyState; } }

        public UiStateTransitionHandler Handler { get { return _handler; } }

        [SerializeField]
        private bool _fromAnyState;

        [SerializeField]
        private string _fromState;

        [SerializeField]
        private string _toState;

        [SerializeField]
        private bool _toAnyState;

        [SerializeField]
        private UiStateTransitionHandler _handler;

        public bool AppliesTo(string fromState, string toState)
        {
            if (_fromAnyState && _toAnyState)
                return true;

            if (_fromAnyState && fromState == FromState)
                return true;

            if (_toAnyState && toState == ToState)
                return true;

            if (fromState == FromState && toState == ToState)
                return true;

            return false;
        }
    }
}
