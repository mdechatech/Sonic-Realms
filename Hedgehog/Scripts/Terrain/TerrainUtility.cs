using System;
using System.Collections.Generic;
using System.Linq;
using Hedgehog.Actors;
using Hedgehog.Utils;
using UnityEngine;

namespace Hedgehog.Terrain
{
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
        /// Performs a linecast against the terrain.
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
            return new TerrainCastHit(hit, hit ? SearchProperties(hit.transform) : null, fromSide);
        }

        /// <summary>
        /// Searches for terrain properties on the transform and all its parents, taking into
        /// account any terrain properties' maximum level.
        /// </summary>
        /// <param name="hit">The terrain transform.</param>
        /// <param name="maxLevel">The maximum amount of parents to go through.</param>
        /// <returns></returns>
        public static TerrainProperties SearchProperties(Transform hit, int maxLevel = Int32.MaxValue)
        {
            var check = hit;
            var levelsDown = 0;
            while (maxLevel >= 0 && check != null)
            {
                var properties = check.GetComponent<TerrainProperties>();
                if (properties != null && levelsDown <= properties.AppliesToChildrenLevel)
                    return properties;

                ++levelsDown;
                check = check.parent;
                --maxLevel;
            }

            return null;
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
            if (!CollisionModeSelector(hit, source)) return false;
            return PropertiesSelector(hit, raycastSide);
        }

        /// <summary>
        /// Returns whether the raycast hit can be collided with based on the specified controller's
        /// collision mode and collision data.
        /// </summary>
        /// <param name="source">The specified controller.</param>
        /// <param name="hit">The specified raycast hit.</param>
        /// <returns></returns>
        public static bool CollisionModeSelector(RaycastHit2D hit, HedgehogController source)
        {
            switch (source.CollisionMode)
            {
                case CollisionMode.Layers:
                    return (source.TerrainMask & (1 << hit.transform.gameObject.layer)) > 0;

                case CollisionMode.Tags:
                    return source.TerrainTags.Contains(hit.transform.tag);

                case CollisionMode.Names:
                default:
                    return HasNameRecursive(hit.transform, source.TerrainNames);
            }
        }

        /// <summary>
        /// Returns whether the specified raycast hit can be collided with on the
        /// specified transform's terrain properties, if any.
        /// </summary>
        /// <param name="hit">The specified raycast hit.</param>
        /// <param name="raycastSide">The side from which the raycast originates, if any.</param>
        /// <returns></returns>
        public static bool PropertiesSelector(RaycastHit2D hit, TerrainSide raycastSide = TerrainSide.All)
        {
            var properties = SearchProperties(hit.transform);
            if (properties == null) return true;

            if (properties.Ledge && DMath.Equalsf(hit.fraction, 0.0f))
                return false;

            return properties.CollidesWithSide(raycastSide);
        }
        #endregion
    }
}
