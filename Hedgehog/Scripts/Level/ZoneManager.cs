using System;
using Hedgehog.Core.Actors;
using Hedgehog.UI;
using UnityEngine;

namespace Hedgehog.Level
{
    public class ZoneManager : MonoBehaviour
    {
        public HedgehogController Player;

        public DateTime LevelTime;
        public TimerDisplay Timer;

        public RingDisplay RingDisplay;

        public void Reset()
        {
            Player = FindObjectOfType<HedgehogController>();
            Timer = FindObjectOfType<TimerDisplay>();
            RingDisplay = FindObjectOfType<RingDisplay>();
        }

        public void Awake()
        {
            LevelTime = new DateTime();
        }

        public void Start()
        {
            RingDisplay.Target = Player.GetComponentInChildren<RingCollector>();
        }

        public void Update()
        {
            LevelTime = LevelTime.AddSeconds(Time.deltaTime);
            Timer.Display(LevelTime);
        }
    }
}
