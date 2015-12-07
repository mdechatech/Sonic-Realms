using System;
using Hedgehog.Core.Actors;
using Hedgehog.Core.Moves;
using Hedgehog.UI;
using UnityEngine;

namespace Hedgehog.Level
{
    /// <summary>
    /// Modifies the game state in reaction to the target player.
    /// </summary>
    public class ZoneManager : MonoBehaviour
    {
        /// <summary>
        /// The target player.
        /// </summary>
        [Tooltip("The target player.")]
        public HedgehogController Player;

        /// <summary>
        /// The player's health system.
        /// </summary>
        [Tooltip("The player's health system.")]
        public HedgehogHealth Health;

        /// <summary>
        /// The display on which to show the player's rings.
        /// </summary>
        [Tooltip("The display on which to show the player's rings.")]
        public RingDisplay RingDisplay;

        /// <summary>
        /// The current level time.
        /// </summary>
        public TimeSpan LevelTime;

        /// <summary>
        /// The current level time in seconds.
        /// </summary>
        [Header("Timer")]
        /// <summary>
        /// The display on which to show the level time.
        /// </summary>
        [Tooltip("The display on which to show the level time.")]
        public TimerDisplay Timer;

        /// <summary>
        /// The current level time in seconds.
        /// </summary>
        [Tooltip("The current level time in seconds.")]
        public float LevelTimeSeconds;

        /// <summary>
        /// Used to check if LevelTimeSeconds is changed manually.
        /// </summary>
        private float _previousSeconds;

        /// <summary>
        /// Number of seconds after which the player dies due to a time over.
        /// </summary>
        [Tooltip("Number of seconds after which the player dies due to a time over.")]
        public float TimeOverSeconds;

        public void Reset()
        {
            Player = FindObjectOfType<HedgehogController>();
            Health = Player ? Player.GetComponent<HedgehogHealth>() : null;
            RingDisplay = GetComponentInChildren<RingDisplay>();

            Timer = GetComponentInChildren<TimerDisplay>();
            LevelTimeSeconds = 0.0f;
            TimeOverSeconds = 599.0f;
        }

        public void Awake()
        {
            LevelTime = TimeSpan.FromSeconds(LevelTimeSeconds);
            _previousSeconds = LevelTimeSeconds;
        }

        public void Start()
        {
            RingDisplay.Target = Player.GetComponentInChildren<RingCollector>();
            Health.OnDeathComplete.AddListener(OnDeathComplete);
        }

        public void Update()
        {
            if (_previousSeconds != LevelTimeSeconds)
                LevelTime = TimeSpan.FromSeconds(LevelTimeSeconds);

            LevelTime = LevelTime.Add(TimeSpan.FromSeconds(Time.deltaTime));
            LevelTimeSeconds = _previousSeconds = (float)LevelTime.TotalSeconds;
            Timer.Display(LevelTime);

            if (LevelTimeSeconds > TimeOverSeconds)
                Health.Kill();
        }

        public void OnDeathComplete()
        {
            Application.LoadLevel(Application.loadedLevel);
        }
    }
}
