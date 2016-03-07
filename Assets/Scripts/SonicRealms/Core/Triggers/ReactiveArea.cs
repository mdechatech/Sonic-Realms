using SonicRealms.Core.Actors;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Triggers
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
        [Foldout("Player Animation")]
        [Tooltip("Name of an Animator trigger on the controller to set when it enters the area.")]
        public string InsideTrigger;

        /// <summary>
        /// Name of an Animator bool on the controller to set to true while it's inside the area.
        /// </summary>
        [Foldout("Player Animation")]
        [Tooltip("Name of an Animator bool on the controller to set to true while it's inside the area.")]
        public string InsideBool;

        public override void Reset()
        {
            base.Reset();

            AreaTrigger = GetComponent<AreaTrigger>();
            InsideTrigger = InsideBool = "";
        }

        public override void Awake()
        {
            base.Awake();
            AreaTrigger = GetComponent<AreaTrigger>() ?? gameObject.AddComponent<AreaTrigger>();
        }

        public virtual void OnEnable()
        {
            if (AreaTrigger != null && AreaTrigger.InsideRules != null) Start();
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
        public void NotifyAreaEnter(Hitbox hitbox)
        {
            OnAreaEnter(hitbox);
            hitbox.Controller.NotifyAreaEnter(this);

            if (hitbox.Controller.Animator == null) return;

            var logWarnings = hitbox.Controller.Animator.logWarnings;
            hitbox.Controller.Animator.logWarnings = false;

            SetAreaEnterParameters(hitbox.Controller);

            hitbox.Controller.Animator.logWarnings = logWarnings;
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
            hitbox.Controller.NotifyAreaExit(this);
            OnAreaExit(hitbox);

            if (hitbox.Controller.Animator == null) return;

            var logWarnings = hitbox.Controller.Animator.logWarnings;
            hitbox.Controller.Animator.logWarnings = false;

            SetAreaExitParameters(hitbox.Controller);

            hitbox.Controller.Animator.logWarnings = logWarnings;
        }

        protected virtual void SetAreaExitParameters(HedgehogController controller)
        {
            if (!string.IsNullOrEmpty(InsideBool))
                controller.Animator.SetBool(InsideBool, false);
        }
        #endregion

        public virtual void OnDestroy()
        {
            foreach (var hitbox in AreaTrigger.Collisions)
                NotifyAreaExit(hitbox);
        }
    }
}
