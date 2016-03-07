using System;
using UnityEngine;

namespace SonicRealms.Core.Actors
{
    [Serializable]
    public class HealthEventArgs
    {
        public float Damage;
        public Transform Source;
    }
}
