using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level.Effects
{
    /// <summary>
    /// Sets the player's collision layer to one thing if ground speed is negative and another thing if positive.
    /// </summary>
    public class SwitchLayerTwoWay : ReactiveObject
    {
        /// <summary>
        /// What to set the player's collision mask to if its ground speed is negative.
        /// </summary>
        [Tooltip("What to set the player's collision mask to if its ground speed is negative.")]
        public LayerMask NegativeLayer;

        /// <summary>
        /// What to set the player's collison mask to if the ground speed is positive.
        /// </summary>
        [Tooltip("what to set the player's collision mask to if the ground speed is positive.")]
        public LayerMask PositiveLayer;

        public override void OnActivateStay(HedgehogController controller)
        {
            if (!controller.Grounded) return;

            controller.CollisionMask &= ~CollisionLayers.LayersMask;
            controller.CollisionMask |= controller.GroundVelocity > 0.0f ? PositiveLayer : NegativeLayer;
        }
    }
}
