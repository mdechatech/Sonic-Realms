using System.Collections.Generic;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Moves;
using SonicRealms.Core.Triggers;
using UnityEngine;

namespace SonicRealms.Level.Effects
{
    /// <summary>
    /// Gives a powerup to a controller. The powerup comes in the form of an object with moves.
    /// The object is placed under the move manager's transform, causing the controller to
    /// recieve the moves.
    /// </summary>
    public class GivePowerup : ReactiveObject
    {
        /// <summary>
        /// All powerups on this object will be given to the player.
        /// </summary>
        [Tooltip("All powerups on this object will be given to the player.")]
        public GameObject PowerupContainer;

        public override void Reset()
        {
            PowerupContainer = gameObject;
        }

        public override void OnActivate(HedgehogController controller)
        {
            Apply(controller);
        }

        /// <summary>
        /// Gives the powerup object to the specified controller.
        /// </summary>
        /// <param name="controller">The specified controller.</param>
        public virtual void Apply(HedgehogController controller)
        {
            var manager = controller.GetComponent<PowerupManager>();
            if (!manager) return;

            manager.Add(Instantiate(PowerupContainer));
        }
    }
}
