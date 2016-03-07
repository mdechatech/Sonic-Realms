using SonicRealms.Core.Actors;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Effects
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

        public override void OnActivate(HedgehogController controller)
        {
            if(MustBeGrounded && !controller.Grounded) return;

            controller.CollisionMask &= ~CollisionLayers.LayersMask;
            controller.CollisionMask |= NewLayer;
        }

        public override void OnActivateStay(HedgehogController controller)
        {
            OnActivate(controller);
        }

        protected void OnDrawGizmos()
        {
            var layerNumber = CollisionLayers.GetLayerNumber(NewLayer);
            if (layerNumber >= 0 && layerNumber <= 9)
                Gizmos.DrawIcon(transform.position, "Collision Layers/gizmo_layer_" + layerNumber);
        }
    }
}
