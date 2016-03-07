using System;
using UnityEngine;

namespace SonicRealms.Level
{
    [Serializable]
    public class TimeScoreBonusRange
    {
        /// <summary>
        /// Minimum of the time range, in seconds.
        /// </summary>
        [Tooltip("Minimum of the time range, in seconds.")]
        public float TimeMin;

        /// <summary>
        /// Maximum of the time range, in seconds.
        /// </summary>
        [Tooltip("Maximum of the time range, in seconds.")]
        public float TimeMax;

        /// <summary>
        /// Score bonus in this time range.
        /// </summary>
        [Space, Tooltip("Score bonus in this time range.")]
        public int ScoreBonus;
    }
}
