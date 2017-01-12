using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Base class for areas that do something when touched by the player.
    /// </summary>
    [RequireComponent(typeof(AreaTrigger))]
    public abstract class ReactiveArea : BaseReactive
    {
        [HideInInspector]
        public AreaTrigger AreaTrigger;

        protected bool RegisteredEvents;

        public override void Reset()
        {
            base.Reset();

            AreaTrigger = GetComponent<AreaTrigger>();
            AreaTrigger.KeepWhenAlone = false;
        }

        public virtual void OnEnable()
        {
            if (AreaTrigger != null && AreaTrigger.TouchRules != null) Start();
        }

        public override void Awake()
        {
            base.Awake();

            AreaTrigger = AreaTrigger ?? GetComponent<AreaTrigger>();
        }

        public override void Start()
        {
            base.Start();

            if (RegisteredEvents) return;
            
            AreaTrigger.TouchRules.Add(CanTouch);
            AreaTrigger.OnAreaEnter.AddListener(NotifyAreaEnter);
            AreaTrigger.OnAreaStay.AddListener(NotifyAreaStay);
            AreaTrigger.OnAreaExit.AddListener(NotifyAreaExit);

            RegisteredEvents = true;
        }

        public virtual void OnDisable()
        {
            if (!RegisteredEvents) return;

            AreaTrigger.TouchRules.Remove(CanTouch);
            AreaTrigger.OnAreaEnter.RemoveListener(NotifyAreaEnter);
            AreaTrigger.OnAreaStay.RemoveListener(NotifyAreaStay);
            AreaTrigger.OnAreaExit.RemoveListener(NotifyAreaExit);

            RegisteredEvents = false;
        }

        public virtual bool CanTouch(AreaCollision.Contact contact)
        {
            return AreaTrigger.DefaultInsideRule(contact);
        }
        
        public virtual void OnAreaEnter(AreaCollision collision)
        {
            
        }

        public virtual void OnAreaStay(AreaCollision collision)
        {
            
        }

        public virtual void OnAreaExit(AreaCollision collision)
        {
            
        }
        #region Notify Methods
        public void NotifyAreaEnter(AreaCollision collision)
        {
            OnAreaEnter(collision);
            collision.Controller.NotifyAreaEnter(this);
        }

        public void NotifyAreaStay(AreaCollision collision)
        {
            // here for consistency, may add something later
            OnAreaStay(collision);
        }

        public void NotifyAreaExit(AreaCollision collision)
        {
            if (collision.Controller)
                collision.Controller.NotifyAreaExit(this);

            OnAreaExit(collision);
        }
        #endregion

        private bool _isQuitting;
        protected virtual void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (_isQuitting)
                return;

            foreach (var pair in AreaTrigger.CurrentContacts)
            {
                if(pair.Value.Count > 0)
                    NotifyAreaExit(new AreaCollision(pair.Value));
            }
        }
    }
}
