using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level.Platforms.Movers
{
    /// <summary>
    /// Swings back and forth (or all around).
    /// </summary>
    public class SwingPlatform : BasePlatformMover
    {
        [SerializeField] public Vector2 Pivot;
        [SerializeField] public float Radius;
        [SerializeField] public float MidAngle;
        [SerializeField] public float Range;

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
