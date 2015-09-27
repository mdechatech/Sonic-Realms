using System.Linq;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level.Platforms.Movers
{
    /// <summary>
    /// Moves an object along the path defined by a collider2D.
    /// </summary>
    [AddComponentMenu("Hedgehog/Platforms/Movers/Platform Path Mover")]
    public class PathPlatform : BasePlatformMover
    {
        /// <summary>
        /// The path for the object to follow, defined by the collider's shape.
        /// </summary>
        [SerializeField, Tooltip("The path to follow.")]
        public Collider2D Path;

        /// <summary>
        /// Whether to disable the path's collider.
        /// </summary>
        [SerializeField, Tooltip("Whether to disable the path's collider.")]
        public bool DisablePathCollider = true;

        /// <summary>
        /// Setting this to true will cause a performance hit, to paths with lots of points.
        /// It is better to just call ReconstructPath.
        /// 
        /// Whether to always reconstruct the path. Normally the script reconstructs the path if
        /// its transform or any of its parents' transform has changed. If you want to change the
        /// path's size or point data you can just call ReconstructPath afterward.
        /// </summary>
        [SerializeField, Tooltip("Not recommended. Whether to always reconstruct the path regardless of" +
                                 " whether there have been any changes.")]
        public bool AlwaysUpdate = false;

        private Collider2D _previousPath;
        private Vector2[] _cachedPath;

        public void Start()
        {
            ReconstructPath();
        }

        public void Update()
        {
            var hasChanged = Path.transform.hasChanged;
            var check = Path.transform;
            while (check.parent != null && !hasChanged)
            {
                check = check.parent;
                hasChanged = check.hasChanged;
            }

            check.hasChanged = false;

            if (AlwaysUpdate || Path != _previousPath || hasChanged) ReconstructPath();
        }

        /// <summary>
        /// Moves the platform to a point on the path.
        /// </summary>
        /// <param name="t">A point on the path, 0 being its start point and 1 being its end point.</param>
        public override void To(float t)
        {
            transform.position = _cachedPath == null ? Walk(Path, t) : Walk(_cachedPath, t);
        }

        /// <summary>
        /// Updates the platform's path with data from the collider stored in Path.
        /// </summary>
        public void ReconstructPath()
        {
            _previousPath = Path;
            Path.enabled = !DisablePathCollider;
            _cachedPath = GetPoints(Path);
        }
        #region Path Utilities
        /// <summary>
        /// RETURNS NULL FOR CIRCLES. Gets the points of the specified collider in world space.
        /// </summary>
        /// <param name="collider2D">The specified collider.</param>
        /// <returns></returns>
        public static Vector2[] GetPoints(Collider2D collider2D)
        {
            if (collider2D is BoxCollider2D) return GetPoints((BoxCollider2D) collider2D);
            if (collider2D is PolygonCollider2D) return GetPoints((PolygonCollider2D) collider2D);
            if (collider2D is EdgeCollider2D) return GetPoints((EdgeCollider2D) collider2D, true);
            return null;
        }

        /// <summary>
        /// Gets the points of the specified collider in world space.
        /// </summary>
        /// <param name="boxCollider2D">The specified collider.</param>
        /// <returns></returns>
        public static Vector2[] GetPoints(BoxCollider2D boxCollider2D)
        {
            var halfWidth = boxCollider2D.size.x/2.0f;
            var halfHeight = boxCollider2D.size.y/2.0f;
            return new []
            {
                new Vector2(boxCollider2D.offset.x - halfWidth, boxCollider2D.offset.y + halfHeight),
                new Vector2(boxCollider2D.offset.x - halfWidth, boxCollider2D.offset.y - halfHeight),
                new Vector2(boxCollider2D.offset.x + halfWidth, boxCollider2D.offset.y - halfHeight),
                new Vector2(boxCollider2D.offset.x + halfWidth, boxCollider2D.offset.y + halfHeight),
            }.Select(p => (Vector2) boxCollider2D.transform.TransformPoint(p)).ToArray();
        }

        /// <summary>
        /// Gets the points of the specified collider in world space.
        /// </summary>
        /// <param name="edgeCollider2D">The specified collider.</param>
        /// <param name="mirror">Whether to mirror the path on itself such that the path
        /// ends where it begins.</param>
        /// <returns></returns>
        public static Vector2[] GetPoints(EdgeCollider2D edgeCollider2D, bool mirror = true)
        {
            return mirror
                ? edgeCollider2D.points.Concat(edgeCollider2D.points
                    .Where((vector2, i) => i > 0 && i < edgeCollider2D.pointCount - 1)
                    .Reverse())
                    .Select(vector2 => (Vector2) edgeCollider2D.transform.TransformPoint(vector2))
                    .ToArray()
                : edgeCollider2D.points.Select(
                    vector2 => (Vector2) edgeCollider2D.transform.TransformPoint(vector2)).ToArray();
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
        /// Returns the point on the path walking counter-clockwise.
        /// </summary>
        /// <param name="path">The path defined by the shape of the collider.</param>
        /// <param name="t">Number between 0 and 1 that represents the position on the path.
        /// 0 is the top-left corner and 0.5 is the bottom-right corner.</param>
        /// <returns></returns>
        public static Vector2 Walk(Collider2D path, float t)
        {
            if (path is BoxCollider2D) return Walk((BoxCollider2D) path, t);
            if (path is CircleCollider2D) return Walk((CircleCollider2D) path, t);
            if (path is PolygonCollider2D) return Walk((PolygonCollider2D) path, t);
            if (path is EdgeCollider2D) return Walk((EdgeCollider2D) path, t, true);
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
            var radius = path.radius*Mathf.Max(
                Mathf.Abs(path.transform.lossyScale.x), Mathf.Abs(path.transform.lossyScale.y));
            var radians = Mathf.Clamp01(t)*DMath.DoublePi;

            return (Vector2)path.transform.TransformPoint(path.offset) +
                   new Vector2(Mathf.Cos(radians)*radius, Mathf.Sin(radians)*radius);
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
        public static Vector2 Walk(EdgeCollider2D path, float t, bool mirror = true)
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
            if (path.Length == 0) return default(Vector2);

            t = Mathf.Clamp01(t);
            if (t == 0.0f || (t == 1.0f && connectFirstAndLastPoints)) return path[0];
            if (t == 1.0f && !connectFirstAndLastPoints) return path[path.Length - 1];

            // Walk the path to find its length
            var length = 0.0f;
            for (var i = 0; i < path.Length; i++)
            {
                float sideLength;
                if (i == path.Length - 1)
                    if (connectFirstAndLastPoints)
                        sideLength = Vector2.Distance(path[i], path[0]);
                    else
                        break;
                else
                    sideLength = Vector2.Distance(path[i], path[i + 1]);

                length += sideLength;
            }

            // Walk the path again for the answer
            var target = t*length;
            var walkLength = 0.0f;
            for (var i = 0; i < path.Length; i++)
            {
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
                            ? Vector2.Lerp(path[i], path[0], 1.0f - (walkLength - target)/sideLength)
                            : path[i];
                    else
                        return Vector2.Lerp(path[i], path[i + 1], 1.0f - (walkLength - target)/sideLength);  
                }
            }

            return path[path.Length - 1];
        }
        #endregion
    }
}
