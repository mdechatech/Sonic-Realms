using System;
using System.Collections.Generic;
using Hedgehog.Actors;
using UnityEngine;
using UnityEngine.Events;

namespace Hedgehog
{
    /// <summary>
    /// Base class for level objects that receive events.
    /// </summary>
    public abstract class BaseTrigger : MonoBehaviour
    {
        /// <summary>
        /// Whether children also send events from this trigger.
        /// </summary>
        [SerializeField]
        public bool TriggersFromChildren;

        /// <summary>
        /// Called when a controller enters the trigger.
        /// </summary>
        [SerializeField]
        public TriggerEvent OnEnter;

        /// <summary>
        /// Called when a controller stays on the trigger.
        /// </summary>
        [SerializeField]
        public TriggerEvent OnStay;

        /// <summary>
        /// Called when a controller exits the trigger.
        /// </summary>
        [SerializeField]
        public TriggerEvent OnExit;

        public virtual void Reset()
        {
            TriggersFromChildren = false;
            OnEnter = new TriggerEvent();
            OnStay = new TriggerEvent();
            OnExit = new TriggerEvent();
        }

        /// <summary>
        /// Returns whether these properties apply to the specified transform.
        /// </summary>
        /// <param name="platform">The specified transform.</param>
        /// <returns></returns>
        public bool AppliesTo(Transform platform)
        {
            if (!TriggersFromChildren && platform != transform) return false;

            var check = platform;

            while (check != null)
            {
                if (check == this) return true;
                check = check.parent;
            }

            return false;
        }

        public static bool Selector(BaseTrigger baseTrigger, Transform originalTransform)
        {
            return originalTransform == baseTrigger.transform ||
                   baseTrigger.TriggersFromChildren;
        }
    }

    /// <summary>
    /// An event for when a controller is on a trigger, invoked with the offending controller.
    /// </summary>
    [Serializable]
    public class TriggerEvent : UnityEvent<HedgehogController>
    {
        
    }
}
