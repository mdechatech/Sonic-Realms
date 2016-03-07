using System;
using UnityEngine.Events;

namespace SonicRealms.Core.Moves
{
    /// <summary>
    /// Used for when a move is performed.
    /// </summary>
    [Serializable]
    public class MoveEvent : UnityEvent<Move>
    {
    }
}
