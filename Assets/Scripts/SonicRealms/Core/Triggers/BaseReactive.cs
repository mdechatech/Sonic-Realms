using System;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Base class for creating things that react to controllers.
    /// </summary>
    public abstract class BaseReactive : MonoBehaviour, IComparable<Component>
    {
        /// <summary>
        /// The object trigger, if any. This defaults to the the first trigger on this object.
        /// </summary>
        [HideInInspector]
        [Tooltip("The effect trigger, if any. This defaults to the the first trigger on this object.")]
        public EffectTrigger EffectTrigger;

        /// <summary>
        /// Whether the object's effect trigger is activated, or false if there isn't one.
        /// </summary>
        public bool IsActivated
        {
            get { return EffectTrigger != null && EffectTrigger.Activated; }
            set
            {
                if (EffectTrigger == null)
                    return;

                if (value)
                    EffectTrigger.Activate();

                else EffectTrigger.Deactivate();
            }
        }

        public virtual void Reset()
        {
            EffectTrigger = GetComponent<EffectTrigger>();
        }

        public virtual void Awake()
        {
            EffectTrigger = EffectTrigger ? EffectTrigger : GetComponent<EffectTrigger>();
        }

        public virtual void Start()
        {
            
        }

        public virtual int CompareTo(Component component)
        {
            return 1;
        }

        /// <summary>
        /// Activates the object's EffectTrigger, if any.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns>True if there is an object trigger.</returns>
        protected bool ActivateEffectTrigger(HedgehogController controller = null)
        {
            if (EffectTrigger == null)
                return false;

            EffectTrigger.Activate(controller);
            return true;
        }

        /// <summary>
        /// Deactivates the object's EffectTrigger, if any.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns>True if there is an object trigger.</returns>
        protected bool DeactivateEffectTrigger(HedgehogController controller = null)
        {
            if (EffectTrigger == null)
                return false;

            EffectTrigger.Deactivate(controller);
            return true;
        }

        /// <summary>
        /// Triggers the object's EffectTrigger, if any.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns>True if there is an object trigger.</returns>
        protected bool BlinkEffectTrigger(HedgehogController controller = null)
        {
            if (EffectTrigger == null)
                return false;

            EffectTrigger.Blink(controller);
            return true;
        }
    }
}
