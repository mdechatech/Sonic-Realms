using System.Linq;
using UnityEngine;

namespace SonicRealms.Core.Utils
{
    /// <summary>
    /// Helpers that wrap around Unity's 2D physics.
    /// </summary>
    public static class Physics2DUtility
    {
        #region Raycast Allocation Variables
        private const int MaxRaycastResults = 8;
        private static readonly RaycastHit2D[] RaycastResults = new RaycastHit2D[MaxRaycastResults];

        private static int _previousRaycastResultAmount;
        private static int _raycastResultAmount = 0;
        #endregion

        #region Linecast Utilities
        /// <summary>
        /// Calls LinecastNonAlloc. Only uses one allocated array, so use the results immediately or copy
        /// them over because it will change the next time this is called.
        /// </summary>
        /// <param name="start">The start point.</param>
        /// <param name="end">The end point.</param>
        /// <param name="layerMask">Which layers to cast against.</param>
        /// <returns></returns>
        public static RaycastHit2D[] LinecastNonAlloc(Vector2 start, Vector2 end,
            int layerMask = Physics2D.DefaultRaycastLayers)
        {
            _previousRaycastResultAmount = _raycastResultAmount;
            _raycastResultAmount = Physics2D.LinecastNonAlloc(start, end, RaycastResults, layerMask);

            if (_raycastResultAmount >= _previousRaycastResultAmount) return RaycastResults;

            for (var i = _raycastResultAmount; i < _previousRaycastResultAmount; ++i)
            {
                RaycastResults[i] = default(RaycastHit2D);
            }

            return RaycastResults;
        }
        #endregion
        #region Collider2D Utilities
        /// <summary>
        /// RETURNS NULL FOR CIRCLES. Gets the points of the specified collider in world space.
        /// </summary>
        /// <param name="collider2D">The specified collider.</param>
        /// <returns></returns>
        public static Vector2[] GetPoints(Collider2D collider2D)
        {
            if (collider2D is BoxCollider2D)
                return GetPoints((BoxCollider2D)collider2D);
            if (collider2D is PolygonCollider2D)
                return GetPoints((PolygonCollider2D)collider2D);
            if (collider2D is EdgeCollider2D)
                return GetPoints((EdgeCollider2D)collider2D, false);
            return null;
        }

        /// <summary>
        /// Gets the points of the specified collider in world space.
        /// </summary>
        /// <param name="boxCollider2D">The specified collider.</param>
        /// <returns></returns>
        public static Vector2[] GetPoints(BoxCollider2D boxCollider2D)
        {
            var halfWidth = boxCollider2D.size.x / 2.0f;
            var halfHeight = boxCollider2D.size.y / 2.0f;
            return new[]
            {
                new Vector2(boxCollider2D.offset.x - halfWidth, boxCollider2D.offset.y + halfHeight),
                new Vector2(boxCollider2D.offset.x - halfWidth, boxCollider2D.offset.y - halfHeight),
                new Vector2(boxCollider2D.offset.x + halfWidth, boxCollider2D.offset.y - halfHeight),
                new Vector2(boxCollider2D.offset.x + halfWidth, boxCollider2D.offset.y + halfHeight),
            }.Select(p => (Vector2)boxCollider2D.transform.TransformPoint(p)).ToArray();
        }

        /// <summary>
        /// Gets the points of the specified collider in world space.
        /// </summary>
        /// <param name="edgeCollider2D">The specified collider.</param>
        /// <param name="mirror">Whether to mirror the path on itself such that the path
        /// ends where it begins.</param>
        /// <returns></returns>
        public static Vector2[] GetPoints(EdgeCollider2D edgeCollider2D, bool mirror = false)
        {
            return mirror
                ? edgeCollider2D.points.Concat(edgeCollider2D.points
                    .Where((vector2, i) => i > 0 && i < edgeCollider2D.pointCount - 1)
                    .Reverse())
                    .Select(vector2 => (Vector2)edgeCollider2D.transform.TransformPoint(vector2))
                    .ToArray()
                : edgeCollider2D.points.Select(
                    vector2 => (Vector2)edgeCollider2D.transform.TransformPoint(vector2)).ToArray();
        }

        /// <summary>
        /// Returns the point on the path walking counter-clockwise.
        /// </summary>
        /// <param name="path">The path defined by the shape of the collider.</param>
        /// <param name="t">Number between 0 and 1 that represents the position on the path.
        /// 0 is the top-left corner and 0.5 is the bottom-right corner.</param>
        /// <returns></returns>
        public static Vector2[] GetPoints(PolygonCollider2D polygonCollider2D)
        {
            return polygonCollider2D.points.Select(
                vector2 => (Vector2)polygonCollider2D.transform.TransformPoint(vector2)).ToArray();
        }

        /// <summary>
        /// Returns the length of the specified path.
        /// </summary>
        /// <param name="points">The list of points that defines the path.</param>
        /// <param name="connectFirstAndLastPoints">Whether to connect the first and last
        /// points on the path, creating a closed shape.</param>
        /// <returns></returns>
        public static float GetPathLength(Vector2[] points, bool connectFirstAndLastPoints = true)
        {
            if (points.Length < 2)
                return 0.0f;

            var result = 0.0f;
            
            if (connectFirstAndLastPoints)
            {
                result += Vector2.Distance(points[0], points[points.Length - 1]);
            }

            for (var i = 1; i < points.Length; ++i)
            {
                result += Vector2.Distance(points[i], points[i - 1]);
            }

            return result;
        }

        /// <summary>
        /// Returns the point on the path walking counter-clockwise.
        /// </summary>
        /// <param name="path">The path defined by the shape of the collider.</param>
        /// <param name="t">Number between 0 and 1 that represents the position on the path.
        /// 0 is the top-left corner and 0.5 is the bottom-right corner.</param>
        /// <returns></returns>
        public static Vector2 Walk(Collider2D path, float t)
        {
            if (path is BoxCollider2D)
                return Walk((BoxCollider2D)path, t);
            if (path is CircleCollider2D)
                return Walk((CircleCollider2D)path, t);
            if (path is PolygonCollider2D)
                return Walk((PolygonCollider2D)path, t);
            if (path is EdgeCollider2D)
                return Walk((EdgeCollider2D)path, t, true);
            return default(Vector2);
        }

        /// <summary>
        /// Returns the point on the path walking counter-clockwise.
        /// </summary>
        /// <param name="path">The path defined by the shape of the collider.</param>
        /// <param name="t">Number between 0 and 1 that represents the position on the path.
        /// 0 is the top-left corner and 0.5 is the bottom-right corner.</param>
        /// <returns></returns>
        public static Vector2 Walk(BoxCollider2D path, float t)
        {
            return Walk(GetPoints(path), t);
        }

        /// <summary>
        /// Returns the point on the path walking counter-clockwise.
        /// </summary>
        /// <param name="path">The path defined by the shape of the collider.</param>
        /// <param name="t">Number between 0 and 1 that represents the position on the path.
        /// 0 is the far right point and 0.5 is the far left point.</param>
        /// <returns></returns>
        public static Vector2 Walk(CircleCollider2D path, float t)
        {
            var radius = path.radius * Mathf.Max(
                Mathf.Abs(path.transform.lossyScale.x), Mathf.Abs(path.transform.lossyScale.y));
            var radians = Mathf.Clamp01(t) * DMath.DoublePi;

            return (Vector2)path.transform.TransformPoint(path.offset) +
                   new Vector2(Mathf.Cos(radians) * radius, Mathf.Sin(radians) * radius);
        }

        /// <summary>
        /// Returns the point on the path.
        /// </summary>
        /// <param name="path">The path defined by the shape of the collider.</param>
        /// <param name="t">Number between 0 and 1 that represents the position on the path.
        /// 0 is the first point on the path.</param>
        /// <returns></returns>
        public static Vector2 Walk(PolygonCollider2D path, float t)
        {
            return Walk(GetPoints(path), t);
        }

        /// <summary>
        /// Returns the point on the path.
        /// </summary>
        /// <param name="path">The path defined by the shape of the collider.</param>
        /// <param name="t">Number between 0 and 1 that represents the position on the path.
        /// 0 is the first point on the path.</param>
        /// /// <param name="mirror">Whether to mirror the path on itself such that the path
        /// ends where it begins.</param>
        /// <returns></returns>
        public static Vector2 Walk(EdgeCollider2D path, float t, bool mirror = false)
        {
            return Walk(GetPoints(path, mirror), t);
        }

        /// <summary>
        /// Returns the point on the path.
        /// </summary>
        /// <param name="path">The path defined by the list of points.</param>
        /// <param name="t">Number between 0 and 1 that represents the position on the path.
        /// 0 is the first point on the path.</param>
        /// <param name="connectFirstAndLastPoints">Whether to connect the first and last
        /// points on the path, creating a closed shape.</param>
        /// <returns></returns>
        public static Vector2 Walk(Vector2[] path, float t, bool connectFirstAndLastPoints = true)
        {
            // Check for easy cases
            if (path.Length == 0)
                return default(Vector2);
            if (path.Length == 1)
                return path[0];

            t = Mathf.Clamp01(t);
            if (t == 0.0f || (t == 1.0f && connectFirstAndLastPoints))
                return path[0];
            if (t == 1.0f && !connectFirstAndLastPoints)
                return path[path.Length - 1];

            // Walk the path to find its length
            var length = GetPathLength(path, connectFirstAndLastPoints);

            // Walk the path again for the answer
            var target = t*length;
            var walkLength = 0.0f;
            for (var i = 0; i < path.Length; i++)
            {
                // Get the length between the current point and the next
                float sideLength;
                if (i == path.Length - 1)
                    if (connectFirstAndLastPoints)
                        sideLength = Vector2.Distance(path[i], path[0]);
                    else
                        sideLength = 0.0f;
                else
                    sideLength = Vector2.Distance(path[i], path[i + 1]);

                // If we passed our target then return the point between the two points being traversed
                walkLength += sideLength;
                if (walkLength > target)
                {
                    if (i == path.Length - 1)
                        return connectFirstAndLastPoints
                            ? Vector2.Lerp(path[i], path[0], 1.0f - (walkLength - target) / sideLength)
                            : path[i];
                    else
                        return Vector2.Lerp(path[i], path[i + 1], 1.0f - (walkLength - target) / sideLength);
                }
            }

            return path[path.Length - 1];
        }
        #endregion
    }
}
