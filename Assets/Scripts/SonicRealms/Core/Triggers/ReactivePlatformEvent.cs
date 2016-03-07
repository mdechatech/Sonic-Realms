using System;
using UnityEngine.Events;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Event for when a player enters or exits a reactive platform.
    /// </summary>
    [Serializable]
    public class ReactivePlatformEvent : UnityEvent<ReactivePlatform>
    {
    }
}
