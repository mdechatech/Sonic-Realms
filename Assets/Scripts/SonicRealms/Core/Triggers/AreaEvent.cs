using System;
using SonicRealms.Core.Actors;
using UnityEngine.Events;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// UnityEvents that pass in a controller. Used by OnAreaEnter, OnAreaStay, and OnAreaExit.
    /// </summary>
    [Serializable]
    public class AreaEvent : UnityEvent<Hitbox>
    {
    }
}
