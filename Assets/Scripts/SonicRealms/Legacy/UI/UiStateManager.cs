using System.Collections;
using System.Collections.Generic;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Legacy.UI
{
    /// <summary>
    /// Simple state machine that uses strings as states and defines transitions using 
    /// <see cref="UiStateTransition"/> and <see cref="UiStateTransitionHandler"/>.
    /// </summary>
    public class UiStateManager : MonoBehaviour
    {
        /// <summary>
        /// Transitions listed in order of their priority. When changing state, the manager 
        /// will use the first appropriate transition handler in the list.
        /// </summary>
        public List<UiStateTransition> Transitions { get { return _transitions; } }

        public string CurrentState { get { return _currentState; } }

        public string TransitionFromState { get { return _fromState; } }

        public string TransitionToState { get { return _toState; } }

        public bool IsInTransition { get { return _isInTransition; } }

        public UiStateTransition CurrentTransition { get { return _transition; } }

        [SerializeField]
        private string _firstState;

        [SerializeField, Foldout("Transitions")]
        private List<UiStateTransition> _transitions;

        private string _currentState;

        private Coroutine _transitionCoroutine;
        private UiStateTransition _transition;
        private string _fromState;
        private string _toState;

        private bool _isInTransition;

        public void To(string toState)
        {
            To(_currentState, toState);
        }

        public void To(string fromState, string toState)
        {
            var transition = GetTransitionFor(fromState, toState);
            if (transition == null)
            {
                Debug.LogError(string.Format("No transition handler available for state change: {0} -> {1}",
                    fromState, toState));
                return;
            }

            if (_isInTransition)
                SkipCurrentTransition();

            _transitionCoroutine = StartCoroutine(DoTransition(fromState, toState, transition));
        }

        public void SkipCurrentTransition()
        {
            if (!_isInTransition)
                return;

            if(_transition != null && _transition.Handler != null)
                _transition.Handler.Skip(_fromState, _toState);

            if (_transitionCoroutine != null)
                StopCoroutine(_transitionCoroutine);
        }

        public UiStateTransition GetTransitionFor(string fromState, string toState)
        {
            for (var i = 0; i < _transitions.Count; ++i)
            {
                var transition = _transitions[i];
                if (transition.AppliesTo(fromState, toState))
                    return transition;
            }

            return null;
        }

        private IEnumerator DoTransition(string fromState, string toState,
            UiStateTransition transition)
        {
            _isInTransition = true;
            _transition = transition;
            _fromState = fromState;
            _toState = toState;

            if (transition.Handler)
                yield return transition.Handler.Handle(fromState, toState);

            _currentState = toState;
            _transition = null;
            _fromState = null;
            _toState = null;
            _isInTransition = false;
        }

        protected void Awake()
        {
            _currentState = _firstState ?? string.Empty;
        }

        protected void Start()
        {
            if (!string.IsNullOrEmpty(_currentState))
                To(null, _currentState);
        }
    }
}
