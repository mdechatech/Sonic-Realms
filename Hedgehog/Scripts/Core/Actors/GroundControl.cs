using UnityEngine;

namespace Hedgehog.Core.Actors
{
    /// <summary>
    /// Standard ground control.
    /// </summary>
    public class GroundControl : MonoBehaviour
    {
        /// <summary>
        /// Acceleration in units per second squared.
        /// </summary>
        [SerializeField]
        [Tooltip("Acceleration in units per second squared.")]
        public float Acceleration;

        /// <summary>
        /// Friction in units per second squared
        /// </summary>
        [SerializeField]
        [Tooltip("Friction in units per second squared.")]
        public float Friction;

        /// <summary>
        /// Deceleration (brake speed) in units per second squared.
        /// </summary>
        [SerializeField]
        [Tooltip("Deceleration (brake speed) in units per second squared.")]
        public float Deceleration;

        /// <summary>
        /// Duration of the horizontal lock, in seconds. The horizontal lock applies when
        /// a controller tries to climb a slope which is too steep.
        /// </summary>
        [SerializeField]
        [Tooltip("Duration of the horizontal lock, in seconds. The horizontal lock applies when " +
                 "a controller tries to climb a slope which is too steep.")]
        public float HorizontalLockTime;


    }
}
