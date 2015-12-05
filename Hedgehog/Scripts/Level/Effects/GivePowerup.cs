using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using UnityEngine;

namespace Hedgehog.Level.Effects
{
    /// <summary>
    /// Gives a powerup to a controller. The powerup comes in the form of an object with moves.
    /// The object is placed under the move manager's transform, causing the controller to
    /// recieve the moves.
    /// </summary>
    public class GivePowerup : ReactiveObject
    {
        /// <summary>
        /// The powerup object to give. This object is copied over into the move manager.
        /// </summary>
        [Tooltip("The powerup object to give. This object is copied over into the move manager.")]
        public GameObject Powerup;

        public override void OnActivateEnter(HedgehogController controller)
        {
            Apply(controller);
        }

        /// <summary>
        /// Gives the powerup object to the specified controller.
        /// </summary>
        /// <param name="controller">The specified controller.</param>
        public virtual void Apply(HedgehogController controller)
        {
            controller.MoveManager.AddPowerup(Powerup);
        }
    }
}
