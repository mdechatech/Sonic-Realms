using System;
using UnityEngine;

namespace SonicRealms.Core.Actors
{
    /// <summary>
    /// Some data to pass in when a health-related event occurs.
    /// </summary>
    [Serializable]
    public class HealthEventArgs
    {
        public float Damage;
        public Transform Source;
    }
}
