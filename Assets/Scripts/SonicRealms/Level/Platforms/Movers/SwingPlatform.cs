using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Platforms.Movers
{
    /// <summary>
    /// Swings back and forth (or all around).
    /// </summary>
    [AddComponentMenu("Hedgehog/Platforms/Movers/Swing Platform")]
    public class SwingPlatform : BasePlatformMover
    {
        /// <summary>
        /// The platform's pivot position.
        /// </summary>
        [SerializeField]
        [Tooltip("The platform's pivot position.")]
        public Vector2 Pivot;

        /// <summary>
        /// The distance between the pivot position and the platform.
        /// </summary>
        [SerializeField] 
        [Tooltip("The distance between the pivot position and the platform.")]
        public float Radius;

        /// <summary>
        /// The platform swings around this direction, in degrees.
        /// </summary>
        [SerializeField]
        [Tooltip("The platform swings around this direction, in degrees.")]
        public float MidAngle;

        /// <summary>
        /// The platform's swing range, in degrees.
        /// </summary>
        [SerializeField]
        [Tooltip("The platform's swing range, in degrees.")]
        public float Range;

        public override void Reset()
        {
            base.Reset();

            Duration = 3.0f;
            Pivot = transform.position;
            Radius = 1.0f;
            MidAngle = -90.0f;
            Range = 180.0f;
            PingPong = true;
        }

        public override void To(float t)
        {
            var angle = Mathf.Lerp(MidAngle - Range/2.0f, MidAngle + Range/2.0f, t)*Mathf.Deg2Rad;
            transform.position = Pivot + DMath.AngleToVector(angle)*Radius;
        }
    }
}
