using System;
using SonicRealms.Core.Utils;

namespace SonicRealms.Core.Internal
{
    public abstract class SrWaitForEventHandlerBase : SrStoppableYieldInstruction
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

    public class SrWaitForEventHandler : SrWaitForEventHandlerBase
    {
        private readonly EventHandler _onEventCalled;
        private readonly Action<EventHandler> _remove;

        public override bool keepWaiting { get { return !IsFinished; } }

        public SrWaitForEventHandler(Action<EventHandler> add, Action<EventHandler> remove,
            Action doAfterAdd = null, EventHandler onInvoke = null)
        {
            this._remove = remove;

            _onEventCalled = (sender, e) =>
            {
                IsFinished = true;
                remove(_onEventCalled);

                if (onInvoke != null)
                    onInvoke(sender, e);
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

    public class SrWaitForEventHandler<T> : SrWaitForEventHandlerBase where T : EventArgs
    {
        private readonly EventHandler<T> _onEventCalled;
        private readonly Action<EventHandler<T>> _remove;

        public override bool keepWaiting { get { return !IsFinished; } }

        public SrWaitForEventHandler(Action<EventHandler<T>> add, Action<EventHandler<T>> remove,
            Action doAfterAdd = null, EventHandler<T> onInvoke = null)
        {
            this._remove = remove;

            _onEventCalled = (sender, e) =>
            {
                IsFinished = true;
                remove(_onEventCalled);

                if (onInvoke != null)
                    onInvoke(sender, e);
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
