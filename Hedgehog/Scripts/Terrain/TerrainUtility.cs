using System.Collections.Generic;
using System.Linq;
using Hedgehog.Actors;
using Hedgehog.Utils;
using UnityEngine;

namespace Hedgehog.Terrain
{
    public static class TerrainUtility
    {
        /// <summary>
        /// Searches for terrain properties on the transform and all its parents, taking into
        /// account any terrain properties' maximum level.
        /// </summary>
        /// <param name="terrain">The terrain transform.</param>
        /// <param name="maxLevel">The maximum amount of parents to go through.</param>
        /// <returns></returns>
        public static TerrainProperties SearchProperties(Transform terrain, int maxLevel = int.MaxValue)
        {
            var check = terrain;
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
                TransformSelector(raycastHit2D.transform, source, raycastSide)).FirstOrDefault();
        }
        #region Terrain Cast Selectors
        /// <summary>
        /// Returns whether the specified transform can be collided with based on the source's collision info and
        /// the transform's terrain properties.
        /// </summary>
        /// <param name="transform">The specified transform.</param>
        /// <param name="source">The controller that is queried the information.</param>
        /// <param name="raycastSide">The side from which the raycast originated, if any.</param>
        /// <returns></returns>
        private static bool TransformSelector(Transform transform, HedgehogController source,
            TerrainSide raycastSide = TerrainSide.All)
        {
            if (!CollisionModeSelector(transform, source)) return false;
            return PropertiesSelector(transform, raycastSide);
        }

        /// <summary>
        /// Returns whether the transform can be collided with based on the specified controller's
        /// collision mode and collision data.
        /// </summary>
        /// <param name="source">The specified controller.</param>
        /// <param name="transform">The specified transform.</param>
        /// <returns></returns>
        public static bool CollisionModeSelector(Transform transform, HedgehogController source)
        {
            switch (source.CollisionMode)
            {
                case CollisionMode.Layers:
                    return (source.TerrainMask & (1 << transform.gameObject.layer)) > 0;

                case CollisionMode.Tags:
                    return source.TerrainTags.Contains(transform.tag);

                case CollisionMode.Names:
                default:
                    return source.TerrainNames.Contains(transform.name);
            }
        }

        /// <summary>
        /// Returns whether the specified transform can be collided with based solely on the
        /// specified transform's terrain properties, if any.
        /// </summary>
        /// <param name="transform">The specified transform.</param>
        /// <param name="raycastSide">The side from which the raycast originates, if any.</param>
        /// <returns></returns>
        public static bool PropertiesSelector(Transform transform, TerrainSide raycastSide = TerrainSide.All)
        {
            var properties = SearchProperties(transform);
            if (properties == null) return true;

            return properties.CollidesWithSide(raycastSide);
        }
        #endregion
    }
}
