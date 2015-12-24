using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level.Effects
{
    /// <summary>
    /// Switches the player's collision layer when activated.
    /// </summary>
    public class SwitchLayer : ReactiveObject
    {
        /// <summary>
        /// Whether the controller must be on the ground to activate.
        /// </summary>
        [Tooltip("Whether the controller must be on the ground to activate.")]
        public bool MustBeGrounded;

        /// <summary>
        /// What layers to make the player collide with.
        /// </summary>
        [Tooltip("What layers to make the player collide with.")]
        public LayerMask NewLayer;

        public override void Reset()
        {
            MustBeGrounded = false;
        }

        public override void OnActivateEnter(HedgehogController controller)
        {
            if(MustBeGrounded && !controller.Grounded) return;

            controller.CollisionMask &= ~CollisionLayers.LayersMask;
            controller.CollisionMask |= NewLayer;
        }

        public override void OnActivateStay(HedgehogController controller)
        {
            OnActivateEnter(controller);
        }
    }
}
