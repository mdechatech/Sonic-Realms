using System;
using UnityEngine.Events;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Event for when a player enters or exits a reactive area.
    /// </summary>
    [Serializable]
    public class ReactiveAreaEvent : UnityEvent<ReactiveArea>
    {
    }
}
