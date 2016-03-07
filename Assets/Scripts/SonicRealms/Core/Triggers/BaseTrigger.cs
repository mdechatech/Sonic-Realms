using System;
using SonicRealms.Core.Actors;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// An event for when a controller is on a trigger, invoked with the offending controller.
    /// </summary>
    [Serializable]
    public class TriggerEvent : UnityEvent<HedgehogController> { }

    /// <summary>
    /// Base class for level objects that receive events.
    /// </summary>
    [ExecuteInEditMode]
    public abstract class BaseTrigger : MonoBehaviour
    {
        /// <summary>
        /// Whether children of the trigger can set off events. Turning this on makes
        /// it easy to work with groups of colliders/objects.
        /// </summary>
        [Tooltip("Whether children of the trigger can set off events. Turning this on makes it easy to work" +
                 " with groups of colliders/objects.")]
        public bool TriggerFromChildren;

        public virtual void Reset()
        {
            TriggerFromChildren = true;
        }

        public virtual void Awake()
        {
            // here for consistency
        }

        public virtual void Update()
        {
#if UNITY_EDITOR
            hideFlags = EditorPrefs.GetBool("ShowTriggers", false) ? HideFlags.None : HideFlags.HideInInspector;
#endif
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

        /// <summary>
        /// Returns whether the specified trigger would receive events from the specified transform.
        /// </summary>
        /// <typeparam name="TTrigger"></typeparam>
        /// <param name="trigger"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static bool ReceivesEvents<TTrigger>(TTrigger trigger, Transform transform)
            where TTrigger : BaseTrigger
        {
            return trigger && (transform == trigger.transform || trigger.TriggerFromChildren);
        }
    }
}
