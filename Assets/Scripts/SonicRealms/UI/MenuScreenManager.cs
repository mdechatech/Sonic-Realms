using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SonicRealms.UI
{
    [DisallowMultipleComponent]
    public class MenuScreenManager : MonoBehaviour
    {
        public EventSystem EventSystem;

        public MenuScreen InitialScreen;

        [HideInInspector] public MenuScreen CurrentScreen;
        [HideInInspector] public List<MenuScreen> Screens;

        private GameObject _previouslySelected;

        protected MenuScreen OpeningScreen;
        protected MenuScreen ClosingScreen;
        protected MenuScreen NextScreen;

        public void Reset()
        {
            EventSystem = FindObjectOfType<EventSystem>();
            InitialScreen = null;
        }

        public void Start()
        {
            Screens = FindObjectsOfType<MenuScreen>().ToList();

            Open(InitialScreen);
        }

        public void Open(MenuScreen screen)
        {
            if (CurrentScreen == screen) return;
            if (CurrentScreen == null)
            {
                OpenImmediate(screen);
            }
            else
            {
                NextScreen = screen;
                CloseCurrent(screen);
            }
        }

        public void OpenImmediate(MenuScreen screen)
        {
            OpeningScreen = screen;

            EventSystem.enabled = false;

            screen.gameObject.SetActive(true);
            screen.OnFinishOpening.AddListener(OnFinishOpening);
            screen.Open(CurrentScreen);
        }

        public void CloseCurrent(MenuScreen nextScreen)
        {
            if (CurrentScreen == null) return;

            EventSystem.enabled = false;

            ClosingScreen = CurrentScreen;
            CurrentScreen.OnFinishClosing.AddListener(OnFinishClosing);
            CurrentScreen.OnOpenNextScreenEarly.AddListener(OnOpenNextScreenEarly);
            CurrentScreen.Close(nextScreen);
        }

        public void OnFinishOpening()
        {
            if (ClosingScreen == null)
            {
                EventSystem.enabled = true;
            }

            CurrentScreen = OpeningScreen;
            OpeningScreen.OnFinishOpening.RemoveListener(OnFinishOpening);
            OpeningScreen.gameObject.SetActive(true);

            if(OpeningScreen.FirstSelectable != null)
                SetSelected(OpeningScreen.FirstSelectable.gameObject);

            OpeningScreen = null;
        }

        public void OnFinishClosing()
        {
            if (OpeningScreen == null)
            {
                EventSystem.enabled = true;
            }

            if (NextScreen != null)
            {
                OpenImmediate(NextScreen);
            }

            ClosingScreen.OnFinishClosing.RemoveListener(OnFinishClosing);
            ClosingScreen.OnOpenNextScreenEarly.RemoveListener(OnOpenNextScreenEarly);
            ClosingScreen.gameObject.SetActive(false);

            ClosingScreen = null;
        }

        public void OnOpenNextScreenEarly()
        {
            OpenImmediate(NextScreen);
        }

        static GameObject FindFirstEnabledSelectable(GameObject gameObject)
        {
            GameObject go = null;
            var selectables = gameObject.GetComponentsInChildren<Selectable>(true);
            foreach (var selectable in selectables)
            {
                if (selectable.IsActive() && selectable.IsInteractable())
                {
                    go = selectable.gameObject;
                    break;
                }
            }
            return go;
        }

        //Make the provided GameObject selected
        //When using the mouse/touch we actually want to set it as the previously selected and 
        //set nothing as selected for now.
        private void SetSelected(GameObject go)
        {
            //Select the GameObject.
            EventSystem.SetSelectedGameObject(go);

            //If we are using the keyboard right now, that's all we need to do.
            //var standaloneInputModule = EventSystem.current.currentInputModule as StandaloneInputModule;
            //if (standaloneInputModule != null && standaloneInputModule.inputMode == StandaloneInputModule.InputMode.Buttons)
            //    return;

            //Since we are using a pointer device, we don't want anything selected. 
            //But if the user switches to the keyboard, we want to start the navigation from the provided game object.
            //So here we set the current Selected to null, so the provided gameObject becomes the Last Selected in the EventSystem.
            //EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
