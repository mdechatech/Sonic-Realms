using System;
using SonicRealms.Core.Utils;
using UnityEngine.Events;

namespace SonicRealms.Core.Internal
{
    public abstract class SrWaitForUnityEventBase : SrStoppableYieldInstruction
    {
        private bool _isFinished;

        public event Action Finished;

        public bool IsFinished
        {
            get { return _isFinished; }
            set
            {
                _isFinished = value;

                if (_isFinished && Finished != null)
                    Finished();
            }
        }
    }

    public class SrWaitForUnityEvent : SrWaitForUnityEventBase
    {
        private readonly UnityAction _onEventCalled;
        private readonly UnityEvent _evt;

        public override bool keepWaiting { get { return !IsFinished; } }

        public SrWaitForUnityEvent(UnityEvent evt, Action doAfterAdd = null, UnityAction onInvoke = null)
        {
            this._evt = evt;

            _onEventCalled = () =>
            {
                IsFinished = true;

                _evt.RemoveListener(_onEventCalled);

                if (onInvoke != null)
                    onInvoke();
            };

            _evt.AddListener(_onEventCalled);

            if (doAfterAdd != null)
                doAfterAdd();
        }

        public override bool Stop()
        {
            if (IsFinished)
                return false;

            IsFinished = true;
            _evt.RemoveListener(_onEventCalled);

            return true;
        }
    }

    public class SrWaitForUnityEvent<T> : SrWaitForUnityEventBase
    {
        private readonly UnityAction<T> _onEventCalled;
        private readonly UnityEvent<T> _evt;

        public override bool keepWaiting { get { return !IsFinished; } }

        public SrWaitForUnityEvent(UnityEvent<T> evt, Action doAfterAdd = null, UnityAction<T> onInvoke = null)
        {
            this._evt = evt;

            _onEventCalled = (arg) =>
            {
                IsFinished = true;

                _evt.RemoveListener(_onEventCalled);

                if (onInvoke != null)
                    onInvoke(arg);
            };

            _evt.AddListener(_onEventCalled);

            if (doAfterAdd != null)
                doAfterAdd();
        }

        public override bool Stop()
        {
            if (IsFinished)
                return false;

            IsFinished = true;
            _evt.RemoveListener(_onEventCalled);

            return true;
        }
    }
    public class SrWaitForUnityEvent<T1, T2> : SrWaitForUnityEventBase
    {
        private readonly UnityAction<T1, T2> _onEventCalled;
        private readonly UnityEvent<T1, T2> _evt;

        public override bool keepWaiting { get { return !IsFinished; } }

        public SrWaitForUnityEvent(UnityEvent<T1, T2> evt, Action doAfterAdd = null, UnityAction<T1, T2> onInvoke = null)
        {
            this._evt = evt;

            _onEventCalled = (arg1, arg2) =>
            {
                IsFinished = true;

                _evt.RemoveListener(_onEventCalled);

                if (onInvoke != null)
                    onInvoke(arg1, arg2);
            };

            _evt.AddListener(_onEventCalled);

            if (doAfterAdd != null)
                doAfterAdd();
        }

        public override bool Stop()
        {
            if (IsFinished)
                return false;

            IsFinished = true;
            _evt.RemoveListener(_onEventCalled);

            return true;
        }
    }
}
