using System;
using UnityEngine;

namespace SonicRealms.Level
{
    public class LevelTimer : MonoBehaviour
    {
        public TimeSpan Time;

        public void Update()
        {
            Time = Time.Add(TimeSpan.FromSeconds(UnityEngine.Time.deltaTime));
        }
    }
}
