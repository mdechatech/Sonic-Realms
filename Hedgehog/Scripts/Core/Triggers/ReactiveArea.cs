using System;
using Hedgehog.Core.Actors;
using UnityEngine;
using UnityEngine.Events;

namespace Hedgehog.Core.Triggers
{
    /// <summary>
    /// Helper class for creating areas that react to player events.
    /// </summary>
    [RequireComponent(typeof(AreaTrigger))]
    public class ReactiveArea : BaseReactive
    {
        [HideInInspector]
        public AreaTrigger AreaTrigger;

        protected bool RegisteredEvents;

        /// <summary>
        /// Name of an Animator trigger on the controller to set when it enters the area.
        /// </summary>
        public string InsideTrigger;

        /// <summary>
        /// Name of an Animator bool on the controller to set to true while it's inside the area.
        /// </summary>
        public string InsideBool;

        public override void Reset()
        {
            base.Reset();
            InsideTrigger = InsideBool = "";
        }

        public override void Awake()
        {
            base.Awake();
            AreaTrigger = GetComponent<AreaTrigger>();
        }

        public virtual void OnEnable()
        {
            if (AreaTrigger.InsideRules != null) Start();
        }

        public override void Start()
        {
            base.Start();

            if (RegisteredEvents) return;
            
            AreaTrigger.InsideRules.Add(IsInside);
            AreaTrigger.OnAreaEnter.AddListener(NotifyAreaEnter);
            AreaTrigger.OnAreaStay.AddListener(NotifyAreaStay);
            AreaTrigger.OnAreaExit.AddListener(NotifyAreaExit);

            RegisteredEvents = true;
        }

        public virtual void OnDisable()
        {
            if (!RegisteredEvents) return;

            AreaTrigger.InsideRules.Remove(IsInside);
            AreaTrigger.OnAreaEnter.RemoveListener(NotifyAreaEnter);
            AreaTrigger.OnAreaStay.RemoveListener(NotifyAreaStay);
            AreaTrigger.OnAreaExit.RemoveListener(NotifyAreaExit);

            RegisteredEvents = false;
        }

        public virtual bool IsInside(HedgehogController controller)
        {
            return AreaTrigger.DefaultCollisionRule(controller);
        }
        
        public virtual void OnAreaEnter(HedgehogController controller)
        {
            
        }

        public virtual void OnAreaStay(HedgehogController controller)
        {
            
        }

        public virtual void OnAreaExit(HedgehogController controller)
        {
            
        }
        #region Notify Methods
        public void NotifyAreaEnter(HedgehogController controller)
        {
            OnAreaEnter(controller);
            controller.NotifyReactiveEnter(this);

            if (controller.Animator == null) return;

            var logWarnings = controller.Animator.logWarnings;
            controller.Animator.logWarnings = false;

            SetAreaEnterParameters(controller);

            controller.Animator.logWarnings = logWarnings;
        }

        protected virtual void SetAreaEnterParameters(HedgehogController controller)
        {
            if (!string.IsNullOrEmpty(InsideTrigger))
                controller.Animator.SetTrigger(InsideTrigger);

            if (!string.IsNullOrEmpty(InsideBool))
                controller.Animator.SetBool(InsideBool, true);
        }

        public void NotifyAreaStay(HedgehogController controller)
        {
            // here for consistency, may add something later
            OnAreaStay(controller);
        }

        public void NotifyAreaExit(HedgehogController controller)
        {
            controller.NotifyReactiveExit(this);
            OnAreaExit(controller);

            if (controller.Animator == null) return;

            var logWarnings = controller.Animator.logWarnings;
            controller.Animator.logWarnings = false;

            SetAreaExitParameters(controller);

            controller.Animator.logWarnings = logWarnings;
        }

        protected virtual void SetAreaExitParameters(HedgehogController controller)
        {
            if (!string.IsNullOrEmpty(InsideBool))
                controller.Animator.SetBool(InsideBool, false);
        }
        #endregion
    }
}
