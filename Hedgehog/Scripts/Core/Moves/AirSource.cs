using Hedgehog.Core.Actors;
using UnityEngine;

namespace Hedgehog.Core.Moves
{
    /// <summary>
    /// Prevents the controller's breath meter from decreasing while underwater.
    /// </summary>
    public class AirSource : Move
    {
        /// <summary>
        /// Whether the air source is limited.
        /// </summary>
        [Tooltip("Whether the air source is limited.")]
        public bool LimitedDuration;

        /// <summary>
        /// If limited duration, how long it lasts in seconds.
        /// </summary>
        [Tooltip("If limited duration, how long it lasts in seconds.")]
        public float Duration;

        /// <summary>
        /// If limited duration, how long until it runs out, in seconds.
        /// </summary>
        [Tooltip("If limited duration, how long until it runs out, in seconds.")]
        public float RemainingTime;

        protected BreathMeter BreathMeter;

        public override void Reset()
        {
            LimitedDuration = false;
            Duration = 60.0f;
        }

        public override void OnManagerAdd()
        {
            BreathMeter = Controller.GetComponent<BreathMeter>();
            if (!BreathMeter)
            {
                Debug.LogWarning("Tried to give air, but the controller has no breath meter!");
                return;
            }

            Perform();
        }

        public override void OnActiveEnter()
        {
            BreathMeter.HasAir = true;
            if (LimitedDuration) RemainingTime = Duration;
        }

        public override void OnActiveUpdate()
        {
            if (!LimitedDuration) return;

            RemainingTime -= Time.deltaTime;
            if (RemainingTime > 0.0f) return;

            RemainingTime = 0.0f;
            Remove();
        }

        public override void OnActiveExit()
        {
            BreathMeter.HasAir = false;
        }

        public override void OnManagerRemove()
        {
            BreathMeter.HasAir = false;
        }
    }
}
