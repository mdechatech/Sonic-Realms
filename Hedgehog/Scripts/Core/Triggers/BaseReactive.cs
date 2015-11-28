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
        private ObjectTrigger _objectTrigger;

        /// <summary>
        /// Whether this component can activate object triggers, if it has any. Not necessary to override, used
        /// by object effects for convenience.
        /// </summary>
        public virtual bool ActivatesObject
        {
            get { return false; }
        }

        protected bool AutoActivate
        {
            get { return _objectTrigger == null ? false : _objectTrigger.AutoActivate; }
            set { if (_objectTrigger != null) _objectTrigger.AutoActivate = value; }
        }

        public virtual void Reset()
        {

        }

        public virtual void Awake()
        {
            
        }

        public virtual void Start()
        {
            _objectTrigger = GetComponent<ObjectTrigger>();
        }

        /// <summary>
        /// Activates the object's ObjectTrigger, if any.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns>True if there is an object trigger.</returns>
        protected bool ActivateObject(HedgehogController controller = null)
        {
            if (_objectTrigger == null) return false;
            _objectTrigger.Activate(controller);
            return true;
        }

        /// <summary>
        /// Deactivates the object's ObjectTrigger, if any.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns>True if there is an object trigger.</returns>
        protected bool DeactivateObject(HedgehogController controller = null)
        {
            if (_objectTrigger == null) return false;
            _objectTrigger.Deactivate(controller);
            return true;
        }

        /// <summary>
        /// Triggers the object's ObjectTrigger, if any.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns>True if there is an object trigger.</returns>
        protected bool TriggerObject(HedgehogController controller = null)
        {
            if (_objectTrigger == null) return false;
            _objectTrigger.Trigger(controller);
            return true;
        }
    }
}
