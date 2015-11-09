using System;
using System.Linq;
using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using UnityEngine;

namespace Hedgehog.Level.Effects
{
    /// <summary>
    /// Gets the controller to perform a move when activated.
    /// </summary>
    public class PerformMove : ReactiveObject
    {
        /// <summary>
        /// The name of the the move, case-insensitive, NO SPACES.
        /// </summary>
        [Tooltip("The name of the the move, case-insensitive, NO SPACES.")]
        public string MoveName;

        /// <summary>
        /// Whether to force the controller to perform the move, even if it's not available. Still doesn't
        /// do anything for controllers that don't have the move.
        /// </summary>
        [Tooltip("Whether to force the controller to perform the move, even if it's not available. Still doesn't " +
                 "do anything for controllers that don't have the move.")]
        public bool ForcePerform;

        public void Reset()
        {
            MoveName = "Jump";
            ForcePerform = true;
        }

        public override void OnActivateEnter(HedgehogController controller)
        {
            var move = controller.Moves.FirstOrDefault(move1 =>
                string.Equals(MoveName, move1.GetType().Name, StringComparison.CurrentCultureIgnoreCase));

            if (move == null) return;
            if (ForcePerform)
            {
                controller.ForcePerformMove(move);
            }
            else
            {
                controller.PerformMove(move);
            }
        }

        public override void OnActivateStay(HedgehogController controller)
        {
            OnActivateEnter(controller);
        }
    }
}
