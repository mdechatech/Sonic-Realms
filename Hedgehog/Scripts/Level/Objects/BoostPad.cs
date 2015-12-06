using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using UnityEngine;

namespace Hedgehog.Level.Objects
{
    /// <summary>
    /// Adds to a controller's velocity when it enters the area.
    /// </summary>
    [AddComponentMenu("Hedgehog/Objects/Boost Pad")]
    public class BoostPad : ReactiveArea
    {
        /// <summary>
        /// The velocity to add.
        /// </summary>
        [SerializeField, Tooltip("Boost velocity.")]
        public float Velocity;

        /// <summary>
        /// Whether to make the controller faster regardless of the direction it is facing.
        /// </summary>
        [SerializeField, Tooltip("Whether to go faster regardless of which way the controller is facing.")]
        public bool BoostBothWays;

        public override void Reset()
        {
            base.Reset();
            Velocity = 7.2f;
            BoostBothWays = false;
        }

        public override bool IsInside(HedgehogController controller)
        {
            return base.IsInside(controller) && controller.Grounded;
        }

        public override void OnAreaEnter(HedgehogController controller)
        {
            controller.GroundVelocity = Velocity * (BoostBothWays
                ? Mathf.Sign(controller.GroundVelocity)
                : 1.0f);

            TriggerObject();
        }
    }
}
