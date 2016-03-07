using UnityEngine.Events;

namespace SonicRealms.UI
{
    public abstract class BaseYesNoDialog : BaseDialog
    {
        public UnityEvent OnYes;
        public UnityEvent OnNo;

        public override void Reset()
        {
            base.Reset();
            OnYes = new UnityEvent();
            OnNo = new UnityEvent();
        }

        public override void Awake()
        {
            base.Awake();
            OnYes = OnYes ?? new UnityEvent();
            OnNo = OnNo ?? new UnityEvent();
        }

        public override void Close(DialogChoice choice)
        {
            base.Close(choice);
            if(choice == DialogChoice.Yes) OnYes.Invoke();
            else if(choice == DialogChoice.No) OnNo.Invoke();
        }
    }
}
