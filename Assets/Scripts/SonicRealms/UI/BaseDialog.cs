using UnityEngine;
using UnityEngine.Events;

namespace SonicRealms.UI
{
    public abstract class BaseDialog : MonoBehaviour
    {
        public bool IsOpen;

        public UnityEvent OnOpen;
        public DialogChoiceEvent OnClose;

        public virtual void Reset()
        {
            OnOpen = new UnityEvent();
            OnClose = new DialogChoiceEvent();
        }

        public virtual void Awake()
        {
            IsOpen = false;
            OnOpen = OnOpen ?? new UnityEvent();
            OnClose = OnClose ?? new DialogChoiceEvent();
        }

        public virtual void Open()
        {
            IsOpen = true;
            OnOpen.Invoke();
            gameObject.SetActive(true);
        }

        public void Close()
        {
            Close(DialogChoice.Close);
        }

        public virtual void Close(DialogChoice choice)
        {
            IsOpen = false;
            OnClose.Invoke(choice);
            gameObject.SetActive(false);
        }
    }
}
