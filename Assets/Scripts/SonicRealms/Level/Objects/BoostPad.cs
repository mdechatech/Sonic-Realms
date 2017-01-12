using SonicRealms.Core.Moves;
using SonicRealms.Core.Triggers;
using UnityEngine;

namespace SonicRealms.Level.Objects
{
    /// <summary>
    /// The boost pad from Chemical Plant Zone. Boosts a player that is running on the ground.
    /// </summary>
    [AddComponentMenu("Hedgehog/Objects/Boost Pad")]
    public class BoostPad : ReactiveArea
    {
        /// <summary>
        /// Boost speed in units per second.
        /// </summary>
        [SerializeField, Tooltip("Boost speed in units per second.")]
        public float Speed;

        /// <summary>
        /// How long to lock up the player's control after boosting it in seconds.
        /// </summary>
        [SerializeField, Tooltip("How long to lock up the player's control after boosting it in seconds.")]
        public float ControlLockTime;

        /// <summary>
        /// Whether to make the controller faster regardless of the direction it is facing.
        /// </summary>
        [SerializeField, Tooltip("Whether to go faster regardless of which way the controller is facing.")]
        public bool BoostBothWays;

        public override void Reset()
        {
            base.Reset();

            Speed = 7.2f;
            ControlLockTime = 0.5f;
            BoostBothWays = false;
        }

        public override bool CanTouch(AreaCollision.Contact contact)
        {
            var controller = contact.Controller;
            return controller.Grounded;
        }

        public override void OnAreaEnter(AreaCollision collision)
        {
            var controller = collision.Controller;

            controller.GroundVelocity = Speed * (BoostBothWays
                ? Mathf.Sign(controller.GroundVelocity)
                : 1.0f);

            if (ControlLockTime > 0)
            {
                var groundControl = controller.GetMove<GroundControl>();
                if (groundControl)
                    groundControl.Lock(ControlLockTime);
            }

            BlinkEffectTrigger(collision.Controller);
        }
    }
}
