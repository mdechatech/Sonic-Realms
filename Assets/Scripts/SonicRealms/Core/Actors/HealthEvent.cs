using System;
using UnityEngine.Events;

namespace SonicRealms.Core.Actors
{
    /// <summary>
    /// Used for all health-related events.
    /// </summary>
    [Serializable]
    public class HealthEvent : UnityEvent<HealthEventArgs>
    {
    }
}
