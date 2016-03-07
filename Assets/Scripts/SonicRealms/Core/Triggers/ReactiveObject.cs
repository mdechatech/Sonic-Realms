using SonicRealms.Core.Actors;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Helper class for creating objects that react to activation.
    /// </summary>
    [AddComponentMenu("")]
    [RequireComponent(typeof(ObjectTrigger))]
    public class ReactiveObject : BaseReactive
    {
        protected bool RegisteredEvents;

        public override void Awake()
        {
            base.Awake();
            ObjectTrigger = GetComponent<ObjectTrigger>();
        }

        public virtual void OnEnable()
        {
            if (ObjectTrigger.OnActivate != null) Start();
        }

        public override void Start()
        {
            base.Start();

            if (RegisteredEvents) return;

            ObjectTrigger.OnActivate.AddListener(NotifyActivate);
            ObjectTrigger.OnActivateStay.AddListener(NotifyActivateStay);
            ObjectTrigger.OnDeactivate.AddListener(NotifyDeactivate);
            ObjectTrigger.OnActivatorEnter.AddListener(NotifyActivatorEnter);
            ObjectTrigger.OnActivatorStay.AddListener(NotifyActivatorStay);
            ObjectTrigger.OnActivatorExit.AddListener(NotifyActivatorExit);

            RegisteredEvents = true;
        }

        public virtual void OnDisable()
        {
            if (!RegisteredEvents) return;

            ObjectTrigger.OnActivate.RemoveListener(NotifyActivate);
            ObjectTrigger.OnActivateStay.RemoveListener(NotifyActivateStay);
            ObjectTrigger.OnDeactivate.RemoveListener(NotifyDeactivate);

            RegisteredEvents = false;
        }

        public virtual void OnActivate(HedgehogController controller)
        {
            
        }

        public virtual void OnActivateStay(HedgehogController controller)
        {
            
        }

        public virtual void OnDeactivate(HedgehogController controller)
        {
            
        }

        public virtual void OnActivatorEnter(HedgehogController controller)
        {

        }

        public virtual void OnActivatorStay(HedgehogController controller)
        {

        }

        public virtual void OnActivatorExit(HedgehogController controller)
        {

        }
        #region Notify Methods
        public void NotifyActivate(HedgehogController controller)
        {
            OnActivate(controller);
        }

        public void NotifyActivateStay(HedgehogController controller)
        {
            OnActivateStay(controller);
        }

        public void NotifyDeactivate(HedgehogController controller)
        {
            OnDeactivate(controller);
        }

        public void NotifyActivatorEnter(HedgehogController controller)
        {
            controller.NotifyActivateObject(this);
            OnActivatorEnter(controller);
        }

        public void NotifyActivatorStay(HedgehogController controller)
        {
            // here for consistency, may add something later
            OnActivatorStay(controller);
        }

        public void NotifyActivatorExit(HedgehogController controller)
        {
            controller.NotifyDeactivateObject(this);
            OnActivatorExit(controller);
        }
        #endregion

        public virtual void OnDestroy()
        {
            foreach (var controller in ObjectTrigger.Activators)
                NotifyDeactivate(controller);
        }
    }
}
