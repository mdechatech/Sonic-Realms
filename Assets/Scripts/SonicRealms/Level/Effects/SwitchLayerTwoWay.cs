using SonicRealms.Core.Actors;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Effects
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

        protected void OnDrawGizmos()
        {
            var layerNumber = CollisionLayers.GetLayerNumber(NegativeLayer);
            if (layerNumber >= 0 && layerNumber <= 9)
                Gizmos.DrawIcon(transform.position + Vector3.left/2f, "Collision Layers/gizmo_layer_" + layerNumber);

            layerNumber = CollisionLayers.GetLayerNumber(PositiveLayer);
            if (layerNumber >= 0 && layerNumber <= 9)
                Gizmos.DrawIcon(transform.position + Vector3.right/2f, "Collision Layers/gizmo_layer_" + layerNumber);
        }
    }
}
