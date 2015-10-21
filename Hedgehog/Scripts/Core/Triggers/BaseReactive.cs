using Hedgehog.Core.Actors;
using UnityEngine;

namespace Hedgehog.Core.Triggers
{
    /// <summary>
    /// Base class for creating things that react to controllers.
    /// </summary>
    public abstract class BaseReactive : MonoBehaviour
    {
        private ObjectTrigger _objectTrigger;

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
