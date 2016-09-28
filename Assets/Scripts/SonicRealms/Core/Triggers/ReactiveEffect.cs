using System;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Helper class for creating effects that can be activated by an Effect Trigger.
    /// </summary>
    [RequireComponent(typeof(EffectTrigger))]
    public abstract class ReactiveEffect : BaseReactive
    {
        protected bool RegisteredEvents;

        public override void Awake()
        {
            base.Awake();

            EffectTrigger = GetComponent<EffectTrigger>();
        }

        public virtual void OnEnable()
        {
            if (EffectTrigger.OnActivate != null)
                Start();
        }

        public override void Start()
        {
            base.Start();

            if (RegisteredEvents) return;

            EffectTrigger.OnActivate.AddListener(NotifyActivate);
            EffectTrigger.OnActivateStay.AddListener(NotifyActivateStay);
            EffectTrigger.OnDeactivate.AddListener(NotifyDeactivate);

            EffectTrigger.OnActivatorEnter.AddListener(NotifyActivatorEnter);
            EffectTrigger.OnActivatorStay.AddListener(NotifyActivatorStay);
            EffectTrigger.OnActivatorExit.AddListener(NotifyActivatorExit);

            RegisteredEvents = true;
        }

        public virtual void OnDisable()
        {
            if (!RegisteredEvents) return;

            EffectTrigger.OnActivate.RemoveListener(NotifyActivate);
            EffectTrigger.OnActivateStay.RemoveListener(NotifyActivateStay);
            EffectTrigger.OnDeactivate.RemoveListener(NotifyDeactivate);

            EffectTrigger.OnActivatorEnter.RemoveListener(NotifyActivatorEnter);
            EffectTrigger.OnActivatorStay.RemoveListener(NotifyActivatorStay);
            EffectTrigger.OnActivatorExit.RemoveListener(NotifyActivatorEnter);

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
            OnActivatorEnter(controller);
            controller.NotifyActivateObject(this);
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
            foreach (var controller in EffectTrigger.Activators)
                NotifyDeactivate(controller);
        }
    }
}
