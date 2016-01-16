using System;
using Hedgehog.Core.Actors;
using Hedgehog.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        /// The level timer.
        /// </summary>
        [Tooltip("The level timer.")]
        public ZoneTimer Timer;

        /// <summary>
        /// Number of seconds after which the player dies due to a time over.
        /// </summary>
        [Tooltip("Number of seconds after which the player dies due to a time over.")]
        public float TimeOverSeconds;

        public void Reset()
        {
            Player = FindObjectOfType<HedgehogController>();
            Health = Player ? Player.GetComponent<HedgehogHealth>() : null;
            TimeOverSeconds = 599.9f;
        }

        public void Start()
        {
            Health.OnDeathComplete.AddListener(OnDeathComplete);
        }

        public void Update()
        {
            if (Timer.Time.TotalSeconds > TimeOverSeconds)
                Health.Kill();
        }

        public void OnDeathComplete()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
