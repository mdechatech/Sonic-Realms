using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SonicRealms.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public class MenuScreen : MonoBehaviour
    {
        public const int DefaultScreenID = -1;

        public int ScreenID;
        public MenuScreenState State;

        public List<MenuScreen> Screens;

        public Selectable FirstSelectable;

        [HideInInspector] public UnityEvent OnFinishOpening;
        [HideInInspector] public UnityEvent OnFinishClosing;
        [HideInInspector] public UnityEvent OnOpenNextScreenEarly;

        protected List<Selectable> Selectables; 

        protected Animator Animator;

        public string OpenTrigger;
        public string PreviousInt;
        public string CloseTrigger;
        public string DestinationInt;

        protected int OpenTriggerHash;
        protected int PreviousIntHash;
        protected int CloseTriggerHash;
        protected int DestinationIntHash;

        public void Reset()
        {
            FirstSelectable = GetComponentInChildren<Selectable>();

            OnFinishOpening = new UnityEvent();
            OnFinishClosing = new UnityEvent();
            OnOpenNextScreenEarly = new UnityEvent();

            ScreenID = FindObjectsOfType<MenuScreen>().Aggregate(DefaultScreenID, (i, screen) => Mathf.Max(i, screen.ScreenID)) + 1;

            OpenTrigger = "Open";
            PreviousInt = "";
            CloseTrigger = "Close";
            DestinationInt = "";
        }

        public void Awake()
        {
            State = MenuScreenState.Closed;
            Animator = GetComponent<Animator>();

            OnFinishOpening = OnFinishOpening ?? new UnityEvent();
            OnFinishClosing = OnFinishClosing ?? new UnityEvent();
            OnOpenNextScreenEarly = OnOpenNextScreenEarly ?? new UnityEvent();

            OpenTriggerHash = Animator.StringToHash(OpenTrigger);
            PreviousIntHash = Animator.StringToHash(PreviousInt);
            CloseTriggerHash = Animator.StringToHash(CloseTrigger);
            DestinationIntHash = Animator.StringToHash(DestinationInt);
        }

        protected void FindScreens()
        {
            Screens = new List<MenuScreen>();
            foreach (var t in transform)
            {
                var screen = (t as Transform).GetComponent<MenuScreen>();
                if (screen != null) Screens.Add(screen);
            }
        }

        public void Open(MenuScreen previous)
        {
            State = MenuScreenState.Opening;

            if (PreviousIntHash != 0)
                Animator.SetInteger(PreviousIntHash, previous == null ? DefaultScreenID : previous.ScreenID);

            if (CloseTriggerHash != 0)
                Animator.ResetTrigger(CloseTriggerHash);

            if (OpenTriggerHash != 0)
                Animator.SetTrigger(OpenTriggerHash);

            Animator.Update(0f);
        }

        public void Close(MenuScreen destination)
        {
            State = MenuScreenState.Closing;

            if(DestinationIntHash != 0)
                Animator.SetInteger(DestinationIntHash, destination == null ? DefaultScreenID : destination.ScreenID);

            if(OpenTriggerHash != 0)
                Animator.ResetTrigger(OpenTriggerHash);

            if(CloseTriggerHash != 0)
                Animator.SetTrigger(CloseTriggerHash);

            Animator.Update(0f);
        }

        public void OpenNextScreenEarly()
        {
            OnOpenNextScreenEarly.Invoke();
        }

        public void FinishOpening()
        {
            State = MenuScreenState.Open;
            OnFinishOpening.Invoke();
        }

        public void FinishClosing()
        {
            State = MenuScreenState.Closed;
            OnFinishClosing.Invoke();
        }
    }
}
