using System;
using SonicRealms.Core.Actors;
using UnityEngine.Events;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Event for when an attack trigger is hit by a player's hitbox.
    /// </summary>
    [Serializable]
    public class AttackTriggerEvent : UnityEvent<Hitbox>
    {
    }
}
