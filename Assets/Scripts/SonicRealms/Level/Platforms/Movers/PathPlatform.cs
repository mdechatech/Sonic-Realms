using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Platforms.Movers
{
    /// <summary>
    /// Moves an object along the path defined by a collider2D.
    /// </summary>
    [AddComponentMenu("Hedgehog/Platforms/Movers/Path Platform")]
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

        public override void Start()
        {
            base.Start();
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
            transform.position = _cachedPath == null
                ? Physics2DUtility.Walk(Path, t)
                : Physics2DUtility.Walk(_cachedPath, t);
        }

        /// <summary>
        /// Updates the platform's path with data from the collider stored in Path.
        /// </summary>
        public void ReconstructPath()
        {
            _previousPath = Path;
            Path.enabled = !DisablePathCollider;
            _cachedPath = Physics2DUtility.GetPoints(Path);
        }
    }
}
