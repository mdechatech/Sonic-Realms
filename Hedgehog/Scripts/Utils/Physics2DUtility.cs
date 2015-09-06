using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hedgehog.Utils
{
    public static class Physics2DUtility
    {
        #region Raycast Allocation Variables
        private const int MaxRaycastResults = 16;
        private static readonly RaycastHit2D[] RaycastResults = new RaycastHit2D[MaxRaycastResults];

        private static int _previousRaycastResultAmount;
        private static int _raycastResultAmount = 0;
        #endregion

        public static RaycastHit2D[] LinecastNonAlloc(Vector2 start, Vector2 end,
            int layerMask = Physics2D.DefaultRaycastLayers)
        {
            _previousRaycastResultAmount = _raycastResultAmount;
            _raycastResultAmount = Physics2D.LinecastNonAlloc(start, end, RaycastResults, layerMask);

            if (_raycastResultAmount < _previousRaycastResultAmount)
            {
                for (var i = _raycastResultAmount;
                    i < _previousRaycastResultAmount;
                    i++)
                {
                    RaycastResults[i] = default(RaycastHit2D);
                }
            }

            return RaycastResults;
        }

        public static RaycastHit2D ClosestWithTag(IEnumerable<RaycastHit2D> raycastHit2Ds, ICollection<string> tags)
        {
            if (!raycastHit2Ds.Any()) return default(RaycastHit2D);
            return raycastHit2Ds.FirstOrDefault(raycastHit2D => raycastHit2D && tags.Contains(raycastHit2D.collider.tag));
        }

        public static RaycastHit2D ClosestWithTag(IEnumerable<RaycastHit2D> raycastHit2Ds, string tag)
        {
            if (!raycastHit2Ds.Any()) return default(RaycastHit2D);
            return raycastHit2Ds.FirstOrDefault(raycastHit2D => raycastHit2D.collider.tag == tag);
        }

        public static RaycastHit2D ClosestWithName(IEnumerable<RaycastHit2D> raycastHit2Ds, string name)
        {
            if (!raycastHit2Ds.Any()) return default(RaycastHit2D);
            return raycastHit2Ds.FirstOrDefault(raycastHit2D => raycastHit2D && raycastHit2D.collider.name == name);
        }

        public static RaycastHit2D ClosestWithNameRecursive(IEnumerable<RaycastHit2D> raycastHit2Ds,
            ICollection<string> names)
        {
            if (!raycastHit2Ds.Any() || names.Count == 0) return default(RaycastHit2D);
            return
                raycastHit2Ds.FirstOrDefault(
                    raycastHit2D => raycastHit2D && HasNameRecursive(raycastHit2D.collider.transform, names));
        }

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

        public static bool HasNameRecursive(Transform transform, ICollection<string> names)
        {
            return names.Any(name => HasNameRecursive(transform, name));
        }
    }
}
