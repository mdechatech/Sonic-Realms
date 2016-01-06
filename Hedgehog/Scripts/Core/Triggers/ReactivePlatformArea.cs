using Hedgehog.Core.Actors;
using UnityEngine;

namespace Hedgehog.Core.Triggers
{
    /// <summary>
    /// Helper class for creating platforms that can also deactivate and become areas
    /// (like water you can run across).
    /// </summary>
    [RequireComponent(typeof(PlatformTrigger), typeof(AreaTrigger))]
    public class ReactivePlatformArea : ReactivePlatform
    {
        [HideInInspector]
        public AreaTrigger AreaTrigger;

        /// <summary>
        /// Name of an Animator trigger on the controller to set when it enters the area. No effect if it doesn't
        /// have the parameter.
        /// </summary>
        public string InsideTrigger;

        /// <summary>
        /// Name of an Animator bool on the controller to set to true when it is inside the area, false when it isn't.
        /// No effect if it doesn't have the parameter.
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
            if ((AreaTrigger = GetComponent<AreaTrigger>()) == null)
            {
                AreaTrigger = gameObject.AddComponent<AreaTrigger>();
                AreaTrigger.Reset();
            }
        }

        public override void OnEnable()
        {
            if (AreaTrigger != null && AreaTrigger.InsideRules != null && 
                PlatformTrigger != null && PlatformTrigger.CollisionRules != null) Start();
        }

        public override void Start()
        {
            if (RegisteredEvents) return;

            if (!AreaTrigger.InsideRules.Contains(IsInside))
                AreaTrigger.InsideRules.Add(IsInside);
            AreaTrigger.OnAreaEnter.AddListener(NotifyAreaEnter);
            AreaTrigger.OnAreaStay.AddListener(NotifyAreaStay);
            AreaTrigger.OnAreaExit.AddListener(NotifyAreaExit);

            base.Start();
        }

        public override void OnDisable()
        {
            if (!RegisteredEvents) return;

            AreaTrigger.InsideRules.Remove(IsInside);
            AreaTrigger.OnAreaEnter.RemoveListener(NotifyAreaEnter);
            AreaTrigger.OnAreaStay.RemoveListener(NotifyAreaStay);
            AreaTrigger.OnAreaExit.RemoveListener(NotifyAreaExit);

            base.OnDisable();
        }

        public virtual bool IsInside(Hitbox hitbox)
        {
            return AreaTrigger.DefaultInsideRule(hitbox);
        }

        public virtual void OnAreaEnter(Hitbox hitbox)
        {

        }

        public virtual void OnAreaStay(Hitbox hitbox)
        {

        }

        public virtual void OnAreaExit(Hitbox hitbox)
        {

        }
        #region Notify Methods
        protected void NotifyAreaEnter(Hitbox hitbox)
        {
            var controller = hitbox.Controller;
            controller.NotifyReactiveEnter(this);
            OnAreaEnter(hitbox);

            if (controller.Animator == null)
                return;

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

        public void NotifyAreaStay(Hitbox hitbox)
        {
            // here for consistency, may add something later
            OnAreaStay(hitbox);
        }

        public void NotifyAreaExit(Hitbox hitbox)
        {
            var controller = hitbox.Controller;
            controller.NotifyReactiveExit(this);
            OnAreaExit(hitbox);

            if (controller.Animator == null)
                return;

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
