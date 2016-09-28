using System;
using UnityEngine.Events;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// An event for when a controller collides with a platform, invoked with the offending controller
    /// and the offending platform.
    /// </summary>
    [Serializable]
    public class PlatformTriggerCollisionEvent : UnityEvent<PlatformCollision>
    {
    }
}
