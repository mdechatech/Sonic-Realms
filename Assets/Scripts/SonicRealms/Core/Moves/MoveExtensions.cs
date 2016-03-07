using System.Collections.Generic;
using SonicRealms.Core.Actors;

namespace SonicRealms.Core.Moves
{
    /// <summary>
    /// Move helpers that can be called straight from HedgehogController.
    /// </summary>
    public static class MoveExtensions
    {
        public static List<Move> GetMoves(this HedgehogController controller)
        {
            var moveManager = controller.GetComponent<MoveManager>();
            if (moveManager == null) return null;
            return moveManager.Moves;
        } 

        public static bool Perform<TMove>(this HedgehogController controller, bool force = false, bool mute = false)
            where TMove : Move
        {
            var moveManager = controller.GetComponent<MoveManager>();
            if (moveManager == null) return false;
            return moveManager.Perform<TMove>(force, mute);
        }

        public static bool EndMove<TMove>(this HedgehogController controller) where TMove : Move
        {
            var moveManager = controller.GetComponent<MoveManager>();
            if (moveManager == null) return false;
            return moveManager.End<TMove>();
        }

        public static bool EndMove(this HedgehogController controller, MoveLayer layer)
        {
            var moveManager = controller.GetComponent<MoveManager>();
            if (moveManager == null) return false;
            return moveManager.End(layer);
        }

        public static bool EndMove(this HedgehogController controller, Move move)
        {
            var moveManager = controller.GetComponent<MoveManager>();
            if (moveManager == null) return false;
            return moveManager.End(move);
        }

        public static void EnableMove<TMove>(this HedgehogController controller)
            where TMove : Move
        {
            var moveManager = controller.GetComponent<MoveManager>();
            if (moveManager == null) return;
            moveManager.Enable<TMove>();
        }

        public static void DisableMove<TMove>(this HedgehogController controller)
            where TMove : Move
        {
            var moveManager = controller.GetComponent<MoveManager>();
            if (moveManager == null) return;
            moveManager.Disable<TMove>();
        }

        public static TMove GetMove<TMove>(this HedgehogController controller) 
            where TMove : Move
        {
            var moveManager = controller.GetComponent<MoveManager>();
            if (moveManager == null) return null;
            return moveManager.Get<TMove>();
        }

        public static bool HasMove<TMove>(this HedgehogController controller)
            where TMove : Move
        {
            var moveManager = controller.GetComponent<MoveManager>();
            if (moveManager == null) return false;
            return moveManager.Has<TMove>();
        }

        public static bool IsPerforming<TMove>(this HedgehogController controller)
            where TMove : Move
        {
            var moveManager = controller.GetComponent<MoveManager>();
            if (moveManager == null) return false;
            return moveManager.IsActive<TMove>();
        }

        public static bool IsAvailable<TMove>(this HedgehogController controller) where TMove : Move
        {
            var moveManager = controller.GetComponent<MoveManager>();
            if (moveManager == null) return false;
            return moveManager.IsAvailable<TMove>();
        }
    }
}
