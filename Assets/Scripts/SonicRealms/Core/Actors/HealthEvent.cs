using System;
using UnityEngine.Events;

namespace SonicRealms.Core.Actors
{
    [Serializable]
    public class HealthEvent : UnityEvent<HealthEventArgs>
    {
    }
}
