using System;
using UnityEngine.Events;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Event for when a player activates or deactivates a reactive object.
    /// </summary>
    [Serializable]
    public class ReactiveObjectEvent : UnityEvent<ReactiveObject>
    {
    }
}
