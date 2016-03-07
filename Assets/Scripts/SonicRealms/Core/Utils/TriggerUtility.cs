using System.Collections.Generic;
using System.Linq;
using SonicRealms.Core.Actors;
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
                BaseTrigger.ReceivesEvents(trigger, transform));
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
                if (BaseTrigger.ReceivesEvents(trigger, transform)) results.Add(trigger);

                check = check.parent;
            }
        }

        private static readonly List<PlatformTrigger> NotifyPlatformTriggerCache = new List<PlatformTrigger>(); 

        /// <summary>
        /// Notifies all platform triggers eligible to the specified transform about a collision.
        /// </summary>
        /// <param name="transform">The specified transform.</param>
        /// <param name="hit">The collision data to send.</param>
        /// <param name="notifySurfaceCollision">Whether to also notify the triggers of a surface collision.</param>
        /// <returns></returns>
        public static bool NotifyPlatformCollision(Transform transform, TerrainCastHit hit,
            bool notifySurfaceCollision = false, List<PlatformTrigger> platformTriggers = null)
        {
            if (transform == null || !hit) return false;

            if (platformTriggers != null)
            {
                for (var i = 0; i < platformTriggers.Count; ++i)
                {
                    var platformTrigger = platformTriggers[i];
                    platformTrigger.NotifyCollision(hit);
                    if (notifySurfaceCollision) platformTrigger.NotifySurfaceCollision(hit);
                }

                return true;
            }

            GetTriggers(transform, NotifyPlatformTriggerCache);
            if (NotifyPlatformTriggerCache.Count == 0) return false;

            for(var i = 0; i < NotifyPlatformTriggerCache.Count; ++i)
            {
                var platformTrigger = NotifyPlatformTriggerCache[i];
                platformTrigger.NotifyCollision(hit);
                if (notifySurfaceCollision) platformTrigger.NotifySurfaceCollision(hit);
            }

            return true;
        }

        /// <summary>
        /// Notifies all platform triggers eligible to the specified transform about a surface collision.
        /// </summary>
        /// <param name="transform">The specified transform.</param>
        /// <param name="hit">The collision data to send.</param>
        /// <param name="notifyCollision">Whether to also notify the triggers of an ordinary collision.</param>
        /// <returns></returns>
        public static bool NotifySurfaceCollision(Transform transform, TerrainCastHit hit, bool notifyCollision = false,
            List<PlatformTrigger> platformTriggers = null)
        {
            if (transform == null || !hit)
                return false;

            if (platformTriggers != null)
            {
                for (var i = 0; i < platformTriggers.Count; ++i)
                {
                    var platformTrigger = platformTriggers[i];
                    platformTrigger.NotifySurfaceCollision(hit);
                    if (notifyCollision) platformTrigger.NotifyCollision(hit);
                }

                return true;
            }

            GetTriggers(transform, NotifyPlatformTriggerCache);
            if (NotifyPlatformTriggerCache.Count == 0) return false;
            
            for(var i = 0; i < NotifyPlatformTriggerCache.Count; ++i)
            {
                var platformTrigger = NotifyPlatformTriggerCache[i];
                platformTrigger.NotifySurfaceCollision(hit);
                if (notifyCollision) platformTrigger.NotifyCollision(hit);
            }

            return true;
        }

        /// <summary>
        /// Notifies all area triggers eligible to the specified transform about an area collision.
        /// </summary>
        /// <param name="transform">The specified transform.</param>
        /// <param name="controller">The controller to send data about.</param>
        /// <param name="isExit">Whether to notify the triggers of an exit event instead of an enter event.</param>
        /// <returns></returns>
        public static bool NotifyAreaCollision(Transform transform, Hitbox hitbox, bool isExit = false)
        {
            var controller = hitbox.Controller;
            if (controller == null || transform == null)
                return false;

            var any = false;
            foreach (var areaTrigger in GetTriggers<AreaTrigger>(transform))
            {
                areaTrigger.NotifyCollision(hitbox, isExit);
                any = true;
            }

            return any;
        }

        /// <summary>
        /// Activates all object triggers eligible to the specified transform.
        /// </summary>
        /// <param name="transform">The specified transform.</param>
        /// <param name="controller">The controller to send data about.</param>
        /// <returns></returns>
        public static bool ActivateObject(Transform transform, HedgehogController controller = null)
        {
            if (transform == null)
                return false;

            var any = false;
            foreach (var objectTrigger in GetTriggers<ObjectTrigger>(transform))
            {
                objectTrigger.Activate(controller);
                any = true;
            }

            return any;
        }

        /// <summary>
        /// Deactivates all object triggers eligible to the specified transform.
        /// </summary>
        /// <param name="transform">The specified transform.</param>
        /// <param name="controller">The controller to send data about.</param>
        /// <returns></returns>
        public static bool DeactivateObject(Transform transform, HedgehogController controller = null)
        {
            if (transform == null)
                return false;

            var any = false;
            foreach (var objectTrigger in GetTriggers<ObjectTrigger>(transform))
            {
                objectTrigger.Deactivate(controller);
                any = true;
            }

            return any;
        }

        /// <summary>
        /// Triggers (activate and deactivates them once) all object triggers eligible to the specified transform.
        /// </summary>
        /// <param name="transform">The specified transform.</param>
        /// <param name="controller">The controller to send data about.</param>
        /// <returns></returns>
        public static bool TriggerObject(Transform transform, HedgehogController controller = null)
        {
            if (transform == null)
                return false;

            var any = false;
            foreach (var objectTrigger in GetTriggers<ObjectTrigger>(transform))
            {
                objectTrigger.Trigger(controller);
                any = true;
            }

            return any;
        }
    }
}
