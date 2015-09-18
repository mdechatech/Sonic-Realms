using UnityEngine;

namespace Hedgehog.Terrain
{
    /// <summary>
    /// Moves an object between two points, with options for smoothness.
    /// </summary>
    public class ExamplePlatformOscillator : MonoBehaviour
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
        /// How long it takes to make a round trip between the start and end point.
        /// </summary>
        [SerializeField,
        Tooltip("Time to make a round trip between the two points.")]
        public float Duration = 10.0f;

        /// <summary>
        /// How smooth the platform's movement is. At 1 movement is calculated from a sine wave.
        /// At 0, it is calculated from a triangle wave. Numbers in between simply take the average
        /// of these two waves.
        /// </summary>
        [SerializeField, Range(0.0f, 1.0f),
        Tooltip("1 for smoothest, 0 for jagged movement")] 
        public float Smoothness = 1.0f;

        public void FixedUpdate()
        {
            transform.position = Vector2.Lerp(StartPoint, EndPoint,
                Mathf.Lerp(
                    // Triangle wave
                    (Time.fixedTime/Duration)%1.0f < 0.5f ?
                    (Time.fixedTime/Duration*2)%1.0f : 
                    1.0f - (Time.fixedTime/Duration*2)%1.0f,
                    // Sine wave
                    Mathf.Sin((Time.fixedTime - Mathf.PI)/Duration*DMath.DoublePi)*0.5f + 0.5f,
                    // Interpolation
                    Smoothness));
        }
    }
}
