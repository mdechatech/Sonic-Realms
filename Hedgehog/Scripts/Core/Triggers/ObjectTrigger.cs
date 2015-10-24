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

        public override void Reset()
        {
            base.Reset();

            AutoActivate = false;
            AllowReactivation = true;
            OnActivateEnter = new ObjectEvent();
            OnActivateStay = new ObjectEvent();
            OnActivateExit = new ObjectEvent();
        }

        public override void Awake()
        {
            base.Awake();

            Collisions = new List<HedgehogController>();
            OnActivateEnter = OnActivateEnter ?? new ObjectEvent();
            OnActivateStay = OnActivateStay ?? new ObjectEvent();
            OnActivateExit = OnActivateExit ?? new ObjectEvent();
            Activated = false;
        }

        public void Start()
        {
            if (!AutoActivate) return;
            if ((_platformTrigger = GetComponent<PlatformTrigger>()) != null)
            {
                _platformTrigger.OnSurfaceEnter.AddListener((controller, hit) => Activate(controller));
                _platformTrigger.OnSurfaceExit.AddListener((controller, hit) => Deactivate(controller));
                _platformTrigger.TriggerFromChildren = TriggerFromChildren;
            }
            else
            {
                _areaTrigger = GetComponent<AreaTrigger>() ?? gameObject.AddComponent<AreaTrigger>();
                _areaTrigger.OnAreaEnter.AddListener(Activate);
                _areaTrigger.OnAreaExit.AddListener(Deactivate);
                _areaTrigger.IgnoreLayers = true;
                _areaTrigger.TriggerFromChildren = TriggerFromChildren;
            }
        }

        public virtual void OnEnable()
        {
            if (!TriggerFromChildren) return;
            foreach (var childCollider in transform.GetComponentsInChildren<Collider2D>())
            {
                if (childCollider.transform == transform || 
                    childCollider.GetComponent<PlatformTrigger>() != null || 
                    childCollider.GetComponent<AreaTrigger>() != null)
                    continue;

                if (childCollider.gameObject.GetComponent<ObjectTrigger>() == null)
                    childCollider.gameObject.AddComponent<ObjectTrigger>();
            }
        }

        public void FixedUpdate()
        {
            foreach (var collision in Collisions)
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
            if (!AllowReactivation || any) return;

            Activated = true;
            OnActivateEnter.Invoke(controller);
            BubbleEvent(controller);
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
