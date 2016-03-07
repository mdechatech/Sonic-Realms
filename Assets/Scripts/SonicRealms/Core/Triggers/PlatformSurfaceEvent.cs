using System;
using SonicRealms.Core.Utils;
using UnityEngine.Events;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// An event for when a controller lands on a platform, invoked with the offending controller and
    /// the offending platform.
    /// </summary>
    [Serializable]
    public class PlatformSurfaceEvent : UnityEvent<TerrainCastHit>
    {
    }
}
