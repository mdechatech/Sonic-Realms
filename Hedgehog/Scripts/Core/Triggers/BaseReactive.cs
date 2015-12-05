using Hedgehog.Core.Actors;
using UnityEngine;
using UnityEngine.Events;

namespace Hedgehog.Core.Triggers
{
    /// <summary>
    /// An event for when a controller enters or exits a reactive.
    /// </summary>
    public class ReactiveEvent : UnityEvent<BaseReactive> { }

    /// <summary>
    /// Base class for creating things that react to controllers.
    /// </summary>
    public abstract class BaseReactive : MonoBehaviour
    {
        /// <summary>
        /// The object's object trigger, if any.
        /// </summary>
        public ObjectTrigger ObjectTrigger;

        /// <summary>
        /// The object's animator, if any.
        /// </summary>
        public Animator Animator;

        /// <summary>
        /// Whether this component can activate object triggers, if it has any. Not necessary to override, used
        /// by object effects for convenience.
        /// </summary>
        public virtual bool ActivatesObject
        {
            get { return false; }
        }

        /// <summary>
        /// Whether the object trigger is activated, or false if there isn't one.
        /// </summary>
        public bool Activated
        {
            get { return ObjectTrigger == null ? false : ObjectTrigger.Activated; }
            set
            {
                if (ObjectTrigger == null) return;
                if (value) ObjectTrigger.Activate();
                else ObjectTrigger.Deactivate();
            }
        }

        public virtual void Reset()
        {
            ObjectTrigger = GetComponent<ObjectTrigger>();
            Animator = GetComponent<Animator>();
        }

        public virtual void Awake()
        {
            
        }

        public virtual void Start()
        {
            ObjectTrigger = ObjectTrigger ? ObjectTrigger : GetComponent<ObjectTrigger>();
            Animator = Animator ? Animator : GetComponent<Animator>();
        }

        /// <summary>
        /// Activates the object's ObjectTrigger, if any.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns>True if there is an object trigger.</returns>
        protected bool ActivateObject(HedgehogController controller = null)
        {
            if (ObjectTrigger == null) return false;
            ObjectTrigger.Activate(controller);
            return true;
        }

        /// <summary>
        /// Deactivates the object's ObjectTrigger, if any.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns>True if there is an object trigger.</returns>
        protected bool DeactivateObject(HedgehogController controller = null)
        {
            if (ObjectTrigger == null) return false;
            ObjectTrigger.Deactivate(controller);
            return true;
        }

        /// <summary>
        /// Triggers the object's ObjectTrigger, if any.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns>True if there is an object trigger.</returns>
        protected bool TriggerObject(HedgehogController controller = null)
        {
            if (ObjectTrigger == null) return false;
            ObjectTrigger.Trigger(controller);
            return true;
        }
    }
}
