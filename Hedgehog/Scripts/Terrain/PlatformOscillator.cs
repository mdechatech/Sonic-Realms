using UnityEngine;
using UnityEngine.Events;

namespace Hedgehog.Terrain
{
    /// <summary>
    /// Moves an object between two points, with options for smoothness.
    /// </summary>
    public class PlatformOscillator : MonoBehaviour
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
        Tooltip("1 for smoothest movement, 0 for jagged movement")] 
        public float Smoothness = 1.0f;

        /// <summary>
        /// If true, the object will move opposite to the way it usually does.
        /// </summary>
        [SerializeField, Tooltip("Whether to move in the reverse direction.")]
        public bool ReverseDirection = false;

        /// <summary>
        /// The current timer for the object's position, which counts up to Duration then resets.
        /// Change this to have it start somewhere else on the path.
        /// </summary>
        [SerializeField]
        public float CurrentTime = 0.0f;

        /// <summary>
        /// Called when the object completes its cycle.
        /// </summary>
        [SerializeField]
        public UnityEvent OnComplete;

        /// <summary>
        /// The number of cycles (round trips) the object has completed.
        /// </summary>
        [HideInInspector]
        public int CyclesCompleted;

        public void Awake()
        {
            CyclesCompleted = 0;
        }

        public void FixedUpdate()
        {
            if (ReverseDirection)
            {
                CurrentTime -= Time.fixedDeltaTime;
                if (CurrentTime < 0.0f)
                {
                    OnComplete.Invoke();
                    ++CyclesCompleted;
                    CurrentTime += Duration;
                }
            }
            else
            {
                CurrentTime += Time.fixedDeltaTime;
                if (CurrentTime > Duration)
                {
                    OnComplete.Invoke();
                    ++CyclesCompleted;
                    CurrentTime -= Duration;
                }
            }

            transform.position = Vector2.Lerp(StartPoint, EndPoint,
                Mathf.Lerp(
                    // Triangle wave
                    (CurrentTime/Duration) < 0.5f
                        ? (CurrentTime/Duration*2)
                        : 1.0f - (CurrentTime/Duration*2),
                    // Sine wave
                    Mathf.Sin((CurrentTime - Mathf.PI)/Duration*DMath.DoublePi)*0.5f + 0.5f,
                    // Interpolation
                    Smoothness));
        }
    }
}
