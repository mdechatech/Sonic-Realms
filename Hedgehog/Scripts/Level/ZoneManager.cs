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
        public HedgehogController Player;

        /// <summary>
        /// The move that the player performs on death. The level will restart after the move ends.
        /// </summary>
        public Death DeathMove;

        /// <summary>
        /// The current level time.
        /// </summary>
        public DateTime LevelTime;

        /// <summary>
        /// The display on which to show the level time.
        /// </summary>
        public TimerDisplay Timer;

        /// <summary>
        /// The display on which to show the player's rings.
        /// </summary>
        public RingDisplay RingDisplay;

        public void Reset()
        {
            Player = FindObjectOfType<HedgehogController>();
            DeathMove = Player == null ? null : Player.GetMove<Death>();
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

            DeathMove = DeathMove ?? Player.GetMove<Death>();
            DeathMove.OnEnd.AddListener(OnDeathComplete);
        }

        public void Update()
        {
            LevelTime = LevelTime.AddSeconds(Time.deltaTime);
            Timer.Display(LevelTime);
        }

        public void OnDeathComplete()
        {
            Debug.Log("Time to fade out :(");
        }
    }
}
