using System;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Moves;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// On a call to Allows(), this object returns true or false based on move related criteria.
    /// </summary>
    [Serializable]
    public class MovesLimiter : ITriggerLimiter<HedgehogController>
    {
        /// <summary>
        /// If not empty, the name of the move the player must have in its Move Manager. The name is
        /// case-sensitive and without spaces, for example the Electric Special move would be referred
        /// to as "ElectricSpecial".
        /// </summary>
        [Tooltip("If not empty, the name of the move the player must have in its Move Manager. The name is " +
                 "case-sensitive and without spaces, for example the Electric Special move would be referred " +
                 "to as \"ElectricSpecial\".")]
        public string MustHaveMove;

        /// <summary>
        /// If not empty, the name of the move the player must be performing. The name is case-sensitive and
        /// without spaces, for example the Electric Special move would be referred to as "ElectricSpecial".
        /// </summary>
        [Tooltip("If not empty, the name of the move the player must be performing. The name is case-sensitive and " +
                 "without spaces, for example the Electric Special move would be referred to as \"ElectricSpecial\".")]
        public string MustPerformMove;

        /// <summary>
        /// If nonzero, the player must be performing a move on the given move layer. Move layer numbers can be found
        /// in the MoveLayer script.
        /// </summary>
        [Tooltip("If nonzero, the player must be performing a move on the given move layer. Move layer numbers can be found " +
                 "in the MoveLayer script.")]
        public int MustHaveMoveOnLayer;

        /// <summary>
        /// If nonzero, the player must not be performing a move on the given move layer. Move layer numbers can be found
        /// in the MoveLayer script.
        /// </summary>
        [Tooltip("If nonzero, the player must not be performing a move on the given move layer. Move layer numbers can be found " +
                 "in the MoveLayer script.")]
        public int MustNotHaveMoveOnLayer;

        public bool Allows(HedgehogController controller)
        {
            if (!string.IsNullOrEmpty(MustHaveMove) && !controller.HasMove(MustHaveMove))
                return false;

            if (!string.IsNullOrEmpty(MustPerformMove) && !controller.IsPerforming(MustPerformMove))
                return false;

            var moveManager = controller.GetMoveManager();

            if (MustHaveMoveOnLayer != 0 && (moveManager == null || moveManager[MustHaveMoveOnLayer] == null))
                return false;

            if (MustNotHaveMoveOnLayer != 0 && (moveManager != null && moveManager[MustNotHaveMoveOnLayer] != null))
                return false;

            return true;

        }
    }
}
