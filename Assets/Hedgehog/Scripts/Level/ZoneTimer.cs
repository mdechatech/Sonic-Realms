using System;
using UnityEngine;

namespace Hedgehog.Level
{
    public class ZoneTimer : MonoBehaviour
    {
        public TimeSpan Time;

        public void Update()
        {
            Time = Time.Add(TimeSpan.FromSeconds(UnityEngine.Time.deltaTime));
        }
    }
}
