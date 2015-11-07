using System;
using System.Collections.Generic;
using System.Linq;
using Hedgehog.Core.Actors;
using UnityEngine;
using UnityEngine.Events;

namespace Hedgehog.Core.Triggers
{
    /// <summary>
    /// UnityEvent that passes in a controller. The controller is optional, an object can be activated/deactivated
    /// anonymously.
    /// </summary>
    [Serializable]
    public class ObjectEvent : UnityEvent<HedgehogController> {  }

    /// <summary>
    /// Hook up to these events to react when the object is activated. Activation must be
    /// performed by other triggers.
    /// </summary>
    [AddComponentMenu("Hedgehog/Triggers/Object Trigger")]
    public class ObjectTrigger : BaseTrigger
    {
        /// <summary>
        /// A list of things activating the object.
        /// </summary>
        [HideInInspector]
        public List<HedgehogController> Collisions;

        /// <summary>
        /// Whether to automatically trigger when the object is touched. If there is no platform
        /// or area trigger, an area trigger is automatically added. If there is already a platform
        /// trigger, the object is activated when it is landed on.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to automatically trigger when the object is touched.")]
        public bool AutoActivate;

        private PlatformTrigger _platformTrigger;
        private AreaTrigger _areaTrigger;
        
        /// <summary>
        /// Whether the trigger can be activated if it is already on. If true, OnActivateEnter and
        /// OnActivateExit will be invoked for any number of objects that trigger it.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether the trigger can be activated if it is already on.")]
        public bool AllowReactivation;

        /// <summary>
        /// Invoked when the object is activated. This will not occur if the object is already activated.
        /// </summary>
        [SerializeField]
        public ObjectEvent OnActivateEnter;

        /// <summary>
        /// Invoked while the object is activated, each FixedUpdate. This will occur for every controller
        /// that is activating it.
        /// </summary>
        [SerializeField]
        public ObjectEvent OnActivateStay;

        /// <summary>
        /// Invoked when the object is deactivated. This will not occur if the object still has something
        /// activating it.
        /// </summary>
        [SerializeField]
        public ObjectEvent OnActivateExit;

        [HideInInspector]
        public bool Activated;

        #region Animation
        /// <summary>
        /// Reference to the target animator.
        /// </summary>
        [SerializeField]
        [Tooltip("Reference to the target animator.")]
        public Animator Animator;

        /// <summary>
        /// Name of an Animator trigger set when the object is activated.
        /// </summary>
        [SerializeField]
        [Tooltip("Name of an Animator trigger set when the object is activated.")]
        public string ActivatedTrigger;

        /// <summary>
        /// Name of an Animator bool set to whether the object is activated.
        /// </summary>
        [SerializeField]
        [Tooltip("Name of an Animator bool set to whether the object is activated.")]
        public string ActivatedBool;
        #endregion

        public override void Reset()
        {
            base.Reset();

            AutoActivate = false;
            AllowReactivation = true;
            OnActivateEnter = new ObjectEvent();
            OnActivateStay = new ObjectEvent();
            OnActivateExit = new ObjectEvent();

            Animator = GetComponentInChildren<Animator>();
            ActivatedTrigger = "";
            ActivatedBool = "";
        }

        public override void Awake()
        {
            base.Awake();

            Collisions = new List<HedgehogController>();
            OnActivateEnter = OnActivateEnter ?? new ObjectEvent();
            OnActivateStay = OnActivateStay ?? new ObjectEvent();
            OnActivateExit = OnActivateExit ?? new ObjectEvent();
            Activated = false;

            Animator = Animator ?? GetComponentInChildren<Animator>();
        }

        public void Start()
        {
            if (!AutoActivate) return;
            if ((_areaTrigger = GetComponent<AreaTrigger>()) != null)
            {
                _areaTrigger.OnAreaEnter.AddListener(Activate);
                _areaTrigger.OnAreaExit.AddListener(Deactivate);
                _areaTrigger.IgnoreLayers = true;
                _areaTrigger.TriggerFromChildren = TriggerFromChildren;
            } else
            {
                _platformTrigger = GetComponent<PlatformTrigger>() ?? gameObject.AddComponent<PlatformTrigger>();
                _platformTrigger.OnSurfaceEnter.AddListener(hit => Activate(hit.Controller));
                _platformTrigger.OnSurfaceExit.AddListener(hit => Deactivate(hit.Controller));
                _platformTrigger.TriggerFromChildren = TriggerFromChildren;
            }
        }

        public void FixedUpdate()
        {
            if (Animator != null && ActivatedBool.Length > 0)
                Animator.SetBool(ActivatedBool, Activated);

            if (Collisions.Count <= 0) return;

            foreach (var collision in new List<HedgehogController>(Collisions))
            {
                OnActivateStay.Invoke(collision);
            }
        }

        /// <summary>
        /// Activates the object with the ability to specify the controller that activated it, if any.
        /// </summary>
        /// <param name="controller"></param>
        public void Activate(HedgehogController controller = null)
        {
            var any = Collisions.Any();
            if (controller != null && !Collisions.Contains(controller)) Collisions.Add(controller);
            if (!AllowReactivation && any) return;

            Activated = true;
            OnActivateEnter.Invoke(controller);
            BubbleEvent(controller);

            if (Animator != null && ActivatedTrigger.Length > 0)
                Animator.SetTrigger(ActivatedTrigger);
        }

        /// <summary>
        /// Deactivates the object with the ability to specify the controller that deactivated it, if any.
        /// </summary>
        /// <param name="controller"></param>
        public void Deactivate(HedgehogController controller = null)
        {
            if (controller != null) Collisions.Remove(controller);
            if (!AllowReactivation || Collisions.Any()) return;

            Activated = false;
            OnActivateExit.Invoke(controller);
            BubbleEvent(controller, true);
        }

        /// <summary>
        /// Activates and deactivates the object such that OnActivateStay is never called.
        /// </summary>
        /// <param name="controller"></param>
        public void Trigger(HedgehogController controller = null)
        {
            Activate(controller);
            Deactivate(controller);
        }

        protected void BubbleEvent(HedgehogController controller = null, bool isExit = false)
        {
            foreach (var trigger in GetComponentsInParent<ObjectTrigger>().Where(
                trigger => trigger != this && trigger.TriggerFromChildren))
            {
                if (isExit) trigger.Activate(controller);
                else trigger.Deactivate(controller);
            }
        }

        public override bool HasController(HedgehogController controller)
        {
            return Collisions.Contains(controller);
        }

        public static implicit operator bool(ObjectTrigger objectTrigger)
        {
            return objectTrigger != null && objectTrigger.Activated;
        }
    }
}
