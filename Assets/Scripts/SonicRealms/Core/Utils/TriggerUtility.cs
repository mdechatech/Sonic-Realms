using System.Collections.Generic;
using System.Linq;
using SonicRealms.Core.Triggers;
using UnityEngine;

namespace SonicRealms.Core.Utils
{
    /// <summary>
    /// Helpers for triggers.
    /// </summary>
    public static class TriggerUtility
    {
        /// <summary>
        /// Gets all the triggers that would receive events from the specified transform.
        /// </summary>
        /// <typeparam name="TTrigger">The trigger type.</typeparam>
        /// <param name="transform">The specified transform.</param>
        /// <returns></returns>
        public static IEnumerable<TTrigger> GetTriggers<TTrigger>(Transform transform) 
            where TTrigger : BaseTrigger
        {
            return transform.GetComponentsInParent<TTrigger>(false).Where(trigger =>
                trigger.TriggerFromChildren);
        }

        /// <summary>
        /// Gets all the triggers that would receive events from the specified transform and puts them
        /// into the specified list.
        /// </summary>
        /// <typeparam name="TTrigger">The trigger type.</typeparam>
        /// <param name="transform">The specified transform.</param>
        public static void GetTriggers<TTrigger>(Transform transform, List<TTrigger> results)
            where TTrigger : BaseTrigger
        {
            results.Clear();

            var check = transform;
            while (check != null)
            {
                var trigger = check.GetComponent<TTrigger>();
                if (trigger.TriggerFromChildren)
                    results.Add(trigger);

                check = check.parent;
            }
        }
    }
}
