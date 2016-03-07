using System;
using System.Collections.Generic;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Triggers;
using UnityEngine;

namespace SonicRealms.Core.Utils
{
    /// <summary>
    /// Helpers for the terrain.
    /// </summary>
    public static class TerrainUtility
    {
        #region Terrain Utilities

        private const int TerrainCastHitCount = 256;
        private static readonly TerrainCastHit[] TerrainCastHitCache = new TerrainCastHit[TerrainCastHitCount];
        private static int TerrainCastHitIndex = 0;

        static TerrainUtility()
        {
            for (var i = 0; i < TerrainCastHitCache.Length; ++i)
                TerrainCastHitCache[i] = new TerrainCastHit(default(RaycastHit2D));
        }

        private static TerrainCastHit GetTerrainCastHit()
        {
            var result = TerrainCastHitCache[TerrainCastHitIndex];
            TerrainCastHitIndex = (TerrainCastHitIndex + 1)%TerrainCastHitCount;
            return result;
        }

        private const int MaxTerrainCastResults = 128;
        private static readonly RaycastHit2D[] LinecastResults =
            new RaycastHit2D[MaxTerrainCastResults];

        /// <summary>
        /// Performs a linecast against the terrain taking into account a controller's attributes.
        /// </summary>
        /// <param name="source">The controller that queried the linecast.</param>
        /// <param name="start">The beginning of the linecast.</param>
        /// <param name="end">The end of the linecast.</param>
        /// <param name="fromSide">The side from which the linecast originated, if any.</param>
        /// <returns></returns>
        public static TerrainCastHit TerrainCast(this HedgehogController source, Vector2 start,
            Vector2 end, ControllerSide fromSide = ControllerSide.All)
        {
            var amount = Physics2D.LinecastNonAlloc(start, end, LinecastResults, source.CollisionMask);
            if (amount == 0) return null;

            for (var i = 0; i < amount; ++i)
            {
                var hit = GetTerrainCastHit().Initialize(LinecastResults[i], fromSide, source, start, end);
                if (!TransformSelector(hit)) continue;
                return hit;
            }

            return null;
        }
        #endregion
        #region Terrain Cast Selectors
        /// <summary>
        /// Returns whether the specified raycast hit can be collided with based on the source's
        /// collision info and the transform's terrain properties.
        /// </summary>
        /// <param name="hit">The specified raycast hit.</param>
        /// <returns></returns>
        private static bool TransformSelector(TerrainCastHit hit)
        {
            var platformTrigger = hit.Collider.GetComponent<PlatformTrigger>();
            if (platformTrigger != null) return platformTrigger.IsSolid(hit);

            // Otherwise if there are any area triggers, the object is not solid
            if (hit.Collider.GetComponent<AreaTrigger>() != null) return false;

            return true;
        }
        #endregion
        #region ControllerSide Utilities
        public static float ControllerSideToNormal(ControllerSide side)
        {
            switch (side)
            {
                case ControllerSide.Right:
                default:
                    return 0.0f;

                case ControllerSide.Top:
                    return 90.0f;

                case ControllerSide.Left:
                    return 180.0f;

                case ControllerSide.Bottom:
                    return 270.0f;
            }
        }

        /// <summary>
        /// Turns the specified normal angle and returns the closest side. For example,
        /// a surface whose normal is 90 will return the top side.
        /// </summary>
        /// <param name="normal">The specified normal angle, in degrees.</param>
        /// <returns></returns>
        public static ControllerSide NormalToControllerSide(float normal)
        {
            normal = DMath.PositiveAngle_d(normal);
            if (normal >= 315.0f || normal < 45.0f)
            {
                return ControllerSide.Right;
            }
            else if (normal < 135.0f)
            {
                return ControllerSide.Top;
            }
            else if (normal < 225.0f)
            {
                return ControllerSide.Left;
            }

            return ControllerSide.Bottom;
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
