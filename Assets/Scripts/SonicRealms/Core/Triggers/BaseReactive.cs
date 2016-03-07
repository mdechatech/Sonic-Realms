using SonicRealms.Core.Actors;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Base class for creating things that react to controllers.
    /// </summary>
    public abstract class BaseReactive : MonoBehaviour
    {
        /// <summary>
        /// The object trigger, if any. This defaults to the the first trigger on this object.
        /// </summary>
        [Tooltip("The object trigger, if any. This defaults to the the first trigger on this object.")]
        public ObjectTrigger ObjectTrigger;

        /// <summary>
        /// The object's animator, if any.
        /// </summary>
        [Foldout("Animation")]
        public Animator Animator;

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
            ObjectTrigger = ObjectTrigger ? ObjectTrigger : GetComponent<ObjectTrigger>();
            Animator = Animator ? Animator : GetComponent<Animator>();
        }

        public virtual void Start()
        {
            
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
