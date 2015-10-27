using System.Collections.Generic;
using System.Linq;
using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using UnityEngine;

namespace Hedgehog.Core.Utils
{
    /// <summary>
    /// Helpers for triggers.
    /// </summary>
    public static class TriggerUtility
    {
        /// <summary>
        /// Gets all the triggers that would receive events from the specified transform.
        /// </summary>
        /// <typeparam name="TTrigger"></typeparam>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static IEnumerable<TTrigger> GetTriggers<TTrigger>(Transform transform) 
            where TTrigger : BaseTrigger
        {
            return transform.GetComponentsInParent<TTrigger>().Where(trigger =>
                trigger.transform == transform || trigger.TriggerFromChildren);
        }

        /// <summary>
        /// Notifies all platform triggers eligible to the specified transform about a collision.
        /// </summary>
        /// <param name="transform">The specified transform.</param>
        /// <param name="hit">The collision data to send.</param>
        /// <param name="notifySurfaceCollision">Whether to also notify the triggers of a surface collision.</param>
        /// <returns></returns>
        public static bool NotifyPlatformCollision(Transform transform, TerrainCastHit hit,
            bool notifySurfaceCollision = false)
        {
            if (transform == null || !hit) return false;

            var any = false;
            foreach (var platformTrigger in GetTriggers<PlatformTrigger>(transform))
            {
                platformTrigger.NotifyCollision(hit.Source, hit);
                if (notifySurfaceCollision) platformTrigger.NotifySurfaceCollision(hit.Source, hit);
                any = true;
            }

            return any;
        }

        /// <summary>
        /// Notifies all platform triggers eligible to the specified transform about a surface collision.
        /// </summary>
        /// <param name="transform">The specified transform.</param>
        /// <param name="hit">The collision data to send.</param>
        /// <param name="notifyCollision">Whether to also notify the triggers of an ordinary collision.</param>
        /// <returns></returns>
        public static bool NotifySurfaceCollision(Transform transform, TerrainCastHit hit, bool notifyCollision = false)
        {
            if (transform == null || !hit)
                return false;
            
            bool any = false;
            foreach (var platformTrigger in GetTriggers<PlatformTrigger>(transform))
            {
                platformTrigger.NotifySurfaceCollision(hit.Source, hit);
                any = true;
            }

            return any;
        }

        /// <summary>
        /// Notifies all area triggers eligible to the specified transform about an area collision.
        /// </summary>
        /// <param name="transform">The specified transform.</param>
        /// <param name="controller">The controller to send data about.</param>
        /// <param name="isExit">Whether to notify the triggers of an exit event instead of an enter event.</param>
        /// <returns></returns>
        public static bool NotifyAreaCollision(Transform transform, HedgehogController controller, bool isExit = false)
        {
            if (transform == null || controller == null)
                return false;

            var any = false;
            foreach (var areaTrigger in GetTriggers<AreaTrigger>(transform))
            {
                areaTrigger.NotifyCollision(controller, transform, isExit);
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
