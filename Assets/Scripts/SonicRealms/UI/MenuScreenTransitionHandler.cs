using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SonicRealms.UI
{
    public class MenuScreenTransitionHandler : UiStateTransitionHandler
    {
        [SerializeField]
        private List<Screen> _screens;

        private Dictionary<string, Screen> _screenLookup; 

        public override IEnumerator Handle(string fromState, string toState)
        {
            Skip(fromState, toState);

            yield break;
        }

        public override void Skip(string fromState, string toState)
        {
            Screen toScreen;
            Screen fromScreen;

            EventSystem.current.sendNavigationEvents = false;

            GetScreens(fromState, toState, out fromScreen, out toScreen);

            if (fromScreen != null)
            {
                for (var i = 0; i < fromScreen.Containers.Count; ++i)
                    fromScreen.Containers[i].SetActive(false);
            }

            if (toScreen != null)
            {
                for (var i = 0; i < toScreen.Containers.Count; ++i)
                    toScreen.Containers[i].SetActive(true);
            }

            EventSystem.current.sendNavigationEvents = true;

            if (toScreen != null && toScreen.FirstSelectable)
            {
                EventSystem.current.SetSelectedGameObject(toScreen.FirstSelectable.gameObject);
            }
        }

        private bool GetScreens(string fromState, string toState, out Screen fromScreen, out Screen toScreen)
        {
            var result = true;

            fromScreen = null;
            toScreen = null;

            if (fromState != null)
            {
                if (!_screenLookup.TryGetValue(fromState, out fromScreen))
                    Debug.LogError(string.Format("No screen found for state {0}.", fromState));
            }

            if (toState != null)
            {
                if (!_screenLookup.TryGetValue(toState, out toScreen))
                    Debug.LogError(string.Format("No screen found for state {0}.", toState));
            }

            return fromScreen != null && toScreen != null;
        }

        protected void Awake()
        {
            _screenLookup = _screens.ToDictionary(s => s.State, s => s);
        }

        [Serializable]
        public class Screen
        {
            public string State;
            public Selectable FirstSelectable;
            public List<GameObject> Containers;
        }
    }
}
