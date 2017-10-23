using System;
using SonicRealms.Core.Utils;

namespace SonicRealms.Core.Internal
{
    public abstract class SrWaitForActionBase : SrStoppableYieldInstruction
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

    public class SrWaitForAction : SrWaitForActionBase
    {
        private readonly Action _onEventCalled;
        private readonly Action<Action> _remove;

        public override bool keepWaiting { get { return !IsFinished; } }

        public SrWaitForAction(Action<Action> add, Action<Action> remove,
            Action doAfterAdd = null, Action onInvoke = null)
        {
            this._remove = remove;

            _onEventCalled = () =>
            {
                IsFinished = true;
                remove(_onEventCalled);

                if (onInvoke != null)
                    onInvoke();
            };

            add(_onEventCalled);

            if (doAfterAdd != null)
                doAfterAdd();
        }

        public override bool Stop()
        {
            if (IsFinished)
                return false;

            IsFinished = true;
            _remove(_onEventCalled);

            return true;
        }
    }

    public class SrWaitForAction<T> : SrWaitForActionBase
    {
        private readonly Action<T> _onEventCalled;
        private readonly Action<Action<T>> _remove;

        public override bool keepWaiting { get { return !IsFinished; } }

        public SrWaitForAction(Action<Action<T>> add, Action<Action<T>> remove,
            Action doAfterAdd = null, Action<T> onInvoke = null)
        {
            this._remove = remove;

            _onEventCalled = (result) =>
            {
                IsFinished = true;
                remove(_onEventCalled);

                if (onInvoke != null)
                    onInvoke(result);
            };

            add(_onEventCalled);

            if (doAfterAdd != null)
                doAfterAdd();
        }

        public override bool Stop()
        {
            if (IsFinished)
                return false;

            IsFinished = true;
            _remove(_onEventCalled);

            return true;
        }
    }

    public class SrWaitForAction<T1, T2> : SrWaitForActionBase
    {
        private readonly Action<T1, T2> _onEventCalled;
        private readonly Action<Action<T1, T2>> _remove;

        public override bool keepWaiting { get { return !IsFinished; } }

        public SrWaitForAction(Action<Action<T1, T2>> add, Action<Action<T1, T2>> remove,
            Action doAfterAdd = null, Action<T1, T2> onInvoke = null)
        {
            this._remove = remove;

            _onEventCalled = (result1, result2) =>
            {
                IsFinished = true;
                remove(_onEventCalled);

                if (onInvoke != null)
                    onInvoke(result1, result2);
            };

            add(_onEventCalled);

            if (doAfterAdd != null)
                doAfterAdd();
        }

        public override bool Stop()
        {
            if (IsFinished)
                return false;

            IsFinished = true;
            _remove(_onEventCalled);

            return true;
        }
    }
}
