using UnityEngine;

namespace SonicRealms.Level.Platforms.Movers
{
    /// <summary>
    /// Moves an object between two points, with options for smoothness.
    /// </summary>
    [AddComponentMenu("Hedgehog/Platforms/Movers/Platform Oscillator")]
    public class OscillatingPlatform : BasePlatformMover
    {
        /// <summary>
        /// Where the platform begins.
        /// </summary>
        [SerializeField,
        Tooltip("Where the platform begins.")]
        public Vector2 StartPoint;

        /// <summary>
        /// Where the platform ends.
        /// </summary>
        [SerializeField,
        Tooltip("Where the platform ends.")]
        public Vector2 EndPoint;

        /// <summary>
        /// Whether the object goes back to the start after hitting the endpoint.
        /// </summary>
        [SerializeField, Tooltip("Whether to make a round trip.")]
        public bool RoundTrip = true;

        public override void To(float t)
        {
            transform.position = RoundTrip
                ? t < 0.5f
                    ? Vector2.Lerp(StartPoint, EndPoint, t*2.0f)
                    : Vector2.Lerp(EndPoint, StartPoint, t*2.0f - 1.0f)
                : Vector2.Lerp(StartPoint, EndPoint, t);
        }
    }
}
