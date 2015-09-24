using System;
using System.Collections.Generic;
using System.Linq;
using Hedgehog.Actors;
using Hedgehog.Utils;
using UnityEngine;

namespace Hedgehog.Terrain
{
    /// <summary>
    /// Helpers for the terrain.
    /// </summary>
    public static class TerrainUtility
    {
        #region Name Utilities
        /// <summary>
        /// Returns whether the specified transform, or its parent, or grandparent, and so on, has
        /// the specified name.
        /// </summary>
        /// <param name="transform">The specified transform.</param>
        /// <param name="name">The specified name.</param>
        /// <returns></returns>
        public static bool HasNameRecursive(Transform transform, string name)
        {
            var check = transform;
            while (check != null)
            {
                if (check.name == name) return true;
                check = check.parent;
            }

            return false;
        }
        /// <summary>
        /// Returns whether the specified transform, or its parent, or grandparent, and so on, has
        /// a name contained in the specified name list.
        /// </summary>
        /// <param name="transform">The specified transform.</param>
        /// <param name="names">The specified name list.</param>
        /// <returns></returns>
        public static bool HasNameRecursive(Transform transform, ICollection<string> names)
        {
            return names.Any(name => HasNameRecursive(transform, name));
        }
        #endregion
        #region Terrain Utilities
        /// <summary>
        /// Performs a linecast against the terrain taking into account a controller's attributes.
        /// </summary>
        /// <param name="source">The controller that queried the linecast.</param>
        /// <param name="start">The beginning of the linecast.</param>
        /// <param name="end">The end of the linecast.</param>
        /// <param name="fromSide">The side from which the linecast originated, if any.</param>
        /// <returns></returns>
        public static TerrainCastHit TerrainCast(this HedgehogController source, Vector2 start,
            Vector2 end, TerrainSide fromSide = TerrainSide.All)
        {
            var hit = BestRaycast(source, Physics2DUtility.LinecastNonAlloc(start, end), fromSide);
            return new TerrainCastHit(hit, fromSide, source);
        }

        /// <summary>
        /// Performs a linecast against the terrain.
        /// </summary>
        /// <param name="start">The beginning of the linecast.</param>
        /// <param name="end">The end of the linecast.</param>
        /// <param name="fromSide">The side from which the linecast originated, if any.</param>
        /// <returns></returns>
        public static TerrainCastHit TerrainCast(Vector2 start, Vector2 end, TerrainSide fromSide = TerrainSide.All)
        {
            var hit = BestRaycast(null, Physics2DUtility.LinecastNonAlloc(start, end), fromSide);
            return new TerrainCastHit(hit, fromSide);
        }

        /// <summary>
        /// Returns the closest from a list of raycasts filtered based on the controller's collision mode
        /// and the raycast hit's terrain properties.
        /// </summary>
        /// <param name="source">The controller that queried the info.</param>
        /// <param name="raycasts">The list of raycasts to filter.</param>
        /// <param name="raycastSide">The side from which the raycast originated, if any.</param>
        /// <returns></returns>
        public static RaycastHit2D BestRaycast(HedgehogController source, RaycastHit2D[] raycasts,
            TerrainSide raycastSide = TerrainSide.All)
        {
            return raycasts.Where(raycastHit2D => raycastHit2D &&
                TransformSelector(raycastHit2D, source, raycastSide)).FirstOrDefault();
        }
        #endregion
        #region Terrain Cast Selectors
        /// <summary>
        /// Returns whether the specified raycast hit can be collided with based on the source's
        /// collision info and the transform's terrain properties.
        /// </summary>
        /// <param name="hit">The specified raycast hit.</param>
        /// <param name="source">The controller that is queried the information.</param>
        /// <param name="raycastSide">The side from which the raycast originated, if any.</param>
        /// <returns></returns>
        private static bool TransformSelector(RaycastHit2D hit, HedgehogController source,
            TerrainSide raycastSide = TerrainSide.All)
        {
            return CollisionModeSelector(hit.transform, source) && PlatformTriggerSelector(hit, source, raycastSide);
        }

        /// <summary>
        /// Returns whether the raycast hit can be collided with based on the specified controller's
        /// collision mode and collision data.
        /// </summary>
        /// <param name="source">The specified controller, if any.</param>
        /// <param name="hit">The specified raycast hit.</param>
        /// <returns></returns>
        public static bool CollisionModeSelector(Transform transform, HedgehogController source = null)
        {
            if (source == null) return false;
            switch (source.CollisionMode)
            {
                case CollisionMode.Layers:
                    return (source.TerrainMask & (1 << transform.gameObject.layer)) > 0;

                case CollisionMode.Tags:
                    return source.TerrainTags.Contains(transform.tag);

                case CollisionMode.Names:
                default:
                    return HasNameRecursive(transform, source.TerrainNames);
            }
        }

        /// <summary>
        /// Returns whether the raycast hit can be collided with based its platform trigger, if any.
        /// </summary>
        /// <param name="hit">The specified raycast hit.</param>
        /// <param name="source">The controller which initiated the raycast, if any.</param>
        /// <param name="raycastSide">The side from which the raycast originated, if any.</param>
        /// <returns></returns>
        public static bool PlatformTriggerSelector(RaycastHit2D hit, HedgehogController source = null,
            TerrainSide raycastSide = TerrainSide.All)
        {
            var enumerable = FindAll<PlatformTrigger>(hit.transform, BaseTrigger.Selector);
            var platformTriggers = enumerable as PlatformTrigger[] ?? enumerable.ToArray();

            return !platformTriggers.Any() ||
                platformTriggers.All(trigger => trigger.CollidesWith(new TerrainCastHit(hit, raycastSide, source)));
        }
        #endregion
        #region Finders
        /// <summary>
        /// Finds the first component starting from the specified transform and working its way up
        /// the hierarchy.
        /// </summary>
        /// <typeparam name="TComponent">The type of component to find.</typeparam>
        /// <param name="transform">The specified transform.</param>
        /// <returns></returns>
        public static TComponent Find<TComponent>(Transform transform)
            where TComponent : Component
        {
            if (transform == null) return null;

            var transformCheck = transform;
            var componentCheck = transform.GetComponent<TComponent>();

            while (transformCheck != null)
            {
                if (componentCheck != null) return componentCheck;

                transformCheck = transformCheck.parent;
                componentCheck = transformCheck.GetComponent<TComponent>();
            }

            return null;
        }

        /// <summary>
        /// Finds the first component starting from the specified transform and working its way up
        /// the hierarchy.
        /// </summary>
        /// <typeparam name="TComponent">The type of component to find.</typeparam>
        /// <param name="transform">The specified transform.</param>
        /// <param name="selector">A predicate that takes the component type and transform currently being
        /// checked.</param>
        /// <returns></returns>
        public static TComponent Find<TComponent>(Transform transform, Func<TComponent, Transform, bool> selector)
            where TComponent : Component
        {
            if (transform == null) return null;

            var transformCheck = transform;
            var componentCheck = transform.GetComponent<TComponent>();

            while (transformCheck != null)
            {
                if (componentCheck != null && selector(componentCheck, transformCheck)) return componentCheck;

                transformCheck = transformCheck.parent;
                componentCheck = transformCheck.GetComponent<TComponent>();
            }

            return null;
        }

        /// <summary>
        /// Finds all components starting from the specified transform and working its way up
        /// the hierarchy (limit one component per transform).
        /// </summary>
        /// <typeparam name="TComponent">The type of component to find.</typeparam>
        /// <param name="transform">The specified transform.</param>
        /// <returns></returns>
        public static IEnumerable<TComponent> FindAll<TComponent>(Transform transform)
            where TComponent : Component
        {
            if (transform == null) return null;

            var result = new List<TComponent>();

            var transformCheck = transform;
            var componentCheck = transform.GetComponent<TComponent>();

            while (transformCheck != null)
            {
                if (componentCheck != null) result.Add(componentCheck);

                transformCheck = transformCheck.parent;
                componentCheck = transformCheck.GetComponent<TComponent>();
            }

            return result;
        }

        /// <summary>
        /// Finds all components starting from the specified transform and working its way up
        /// the hierarchy (limit one component per transform).
        /// </summary>
        /// <typeparam name="TComponent">The type of component to find.</typeparam>
        /// <param name="transform">The specified transform.</param>
        /// <param name="selector">A predicate that takes the component type and transform currently being
        /// checked.</param>
        /// <returns></returns>
        public static IEnumerable<TComponent> FindAll<TComponent>(Transform transform,
            Func<TComponent, Transform, bool> selector)
            where TComponent : Component
        {
            if (transform == null) return null;

            var result = new List<TComponent>();

            var transformCheck = transform;

            while (transformCheck != null)
            {
                var componentCheck = transformCheck.GetComponent<TComponent>();
                if (componentCheck != null && selector(componentCheck, transformCheck)) result.Add(componentCheck);

                transformCheck = transformCheck.parent;
            }

            return result;
        }
        #endregion
    }
}
