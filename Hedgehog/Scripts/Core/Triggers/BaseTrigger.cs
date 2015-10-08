using System;
using Hedgehog.Core.Actors;
using UnityEngine;
using UnityEngine.Events;

namespace Hedgehog.Core.Triggers
{
    /// <summary>
    /// Base class for level objects that receive events.
    /// </summary>
    public abstract class BaseTrigger : MonoBehaviour
    {
        /// <summary>
        /// Whether children of the trigger can set off events. Turning this on makes
        /// it easy to work with groups of colliders/objects.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether children of the trigger can set off events. Turning this on makes it easy to work" +
                 " with groups of colliders/objects.")]
        public bool TriggerFromChildren;

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
            TriggerFromChildren = true;
            OnEnter = new TriggerEvent();
            OnStay = new TriggerEvent();
            OnExit = new TriggerEvent();
        }

        public virtual void Awake()
        {
            OnEnter = OnEnter ?? new TriggerEvent();
            OnStay = OnStay ?? new TriggerEvent();
            OnExit = OnExit ?? new TriggerEvent();
        }

        public abstract bool HasController(HedgehogController controller);

        /// <summary>
        /// Returns whether these properties apply to the specified transform.
        /// </summary>
        /// <param name="platform">The specified transform.</param>
        /// <returns></returns>
        public bool AppliesTo(Transform platform)
        {
            if (!TriggerFromChildren && platform != transform) return false;

            var check = platform;

            while (check != null)
            {
                if (check == this) return true;
                check = check.parent;
            }

            return false;
        }

        public static bool Selector<TTrigger>(TTrigger trigger, Transform originalTransform)
            where TTrigger : BaseTrigger
        {
            return originalTransform == trigger.transform || trigger.TriggerFromChildren;
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
