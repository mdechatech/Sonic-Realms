using Hedgehog.Core.Triggers;
using UnityEngine;
using UnityEngine.Events;

namespace Hedgehog.Level.Platforms.Movers
{
    [RequireComponent(typeof(MovingPlatform))]
    public abstract class BasePlatformMover : MonoBehaviour
    {
        /// <summary>
        /// The time it will take for the object to make a round trip, in seconds.
        /// </summary>
        [SerializeField, Tooltip("Number of seconds for a round trip.")]
        public float Duration = 10.0f;

        /// <summary>
        /// The current timer for the object's position, which counts up to Duration then resets.
        /// Change this to have it start somewhere else on the path.
        /// </summary>
        [SerializeField, Tooltip("Controls position on the path. Counts up to duration.")]
        public float CurrentTime = 0.0f;

        /// <summary>
        /// A plot of the platform's position vs. the current time normalized to a 1x1 graph.
        /// </summary>
        [SerializeField, Tooltip("Position vs. Time normalized to 1x1 graph.")]
        public AnimationCurve PositionCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

        /// <summary>
        /// If true, the object will move opposite to the way it usually does. Usually true means
        /// clockwise and false means counter-clockwise.
        /// </summary>
        [SerializeField, Tooltip("Whether to move in the reverse direction.")]
        public bool ReverseDirection = false;

        /// <summary>
        /// Called when the object completes its cycle.
        /// </summary>
        [SerializeField]
        public UnityEvent OnComplete;

        /// <summary>
        /// The number of cycles (round trips) the object has completed.
        /// </summary>
        [HideInInspector]
        public int CyclesCompleted = 0;

        public virtual void Reset()
        {
            if (gameObject.GetComponent<PlatformTrigger>() == null) 
                gameObject.AddComponent<PlatformTrigger>();
        }

        public virtual void FixedUpdate()
        {
            UpdateTimer(Time.fixedDeltaTime);
            To(PositionCurve.Evaluate(CurrentTime/Duration));
        }

        /// <summary>
        /// Updates the current time by the specified timestep.
        /// </summary>
        /// <param name="timestep">The specified timestep.</param>
        public virtual void UpdateTimer(float timestep)
        {
            if (ReverseDirection)
            {
                CurrentTime -= timestep;
            }
            else
            {
                CurrentTime += timestep;
            }

            if (CurrentTime > Duration)
            {
                OnComplete.Invoke();
                ++CyclesCompleted;
                CurrentTime -= Duration;
                CurrentTime %= Duration;
            }
            else if (CurrentTime < 0.0f)
            {
                OnComplete.Invoke();
                ++CyclesCompleted;
                CurrentTime += Duration;
                CurrentTime %= Duration;
            }
        }

        /// <summary>
        /// Moves the platform based on the current time as a number between 1 and 0.
        /// </summary>
        /// <param name="t">The current time as a number between 1 and 0.</param>
        public abstract void To(float t);
    }
}
