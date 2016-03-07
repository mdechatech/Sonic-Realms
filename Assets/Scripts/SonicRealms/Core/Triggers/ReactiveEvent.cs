using System;
using UnityEngine.Events;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// An event for when a controller enters or exits a reactive.
    /// </summary>
    [Serializable]
    public class ReactiveEvent : UnityEvent<BaseReactive>
    {
    }
}
