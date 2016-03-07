using System;
using SonicRealms.Core.Actors;
using UnityEngine.Events;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// UnityEvent that passes in a controller. The controller is optional, an object can be activated/deactivated
    /// anonymously.
    /// </summary>
    [Serializable]
    public class ObjectEvent : UnityEvent<HedgehogController>
    {
    }
}
