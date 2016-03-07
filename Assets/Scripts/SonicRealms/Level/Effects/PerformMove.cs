using System;
using System.Linq;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Moves;
using SonicRealms.Core.Triggers;
using UnityEngine;

namespace SonicRealms.Level.Effects
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

        public override void Reset()
        {
            base.Reset();
            MoveName = "Jump";
            ForcePerform = true;
        }

        public override void OnActivate(HedgehogController controller)
        {
            var manager = controller.GetComponent<MoveManager>();
            if(manager == null) return;

            var move = manager.Moves.FirstOrDefault(move1 =>
                string.Equals(MoveName, move1.GetType().Name, StringComparison.CurrentCultureIgnoreCase));

            if (move == null) return;
            manager.Perform(move, ForcePerform);
        }

        public override void OnActivateStay(HedgehogController controller)
        {
            OnActivate(controller);
        }
    }
}
