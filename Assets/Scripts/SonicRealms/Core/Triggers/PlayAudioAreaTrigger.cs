using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Plays audio based on area trigger events.
    /// </summary>
    public class PlayAudioAreaTrigger
    {
        /// <summary>
        /// Audio clip to play when a player enters the area.
        /// </summary>
        [Tooltip("Audio clip to play when a player enters the area.")]
        public AudioClip AreaEnterSound;

        /// <summary>
        /// Audio clip to loop while a player is inside the area.
        /// </summary>
        [Tooltip("Audio clip to loop while a player is inside the area.")]
        public AudioClip AreaStayLoop;

        /// <summary>
        /// Audio clip to loop when a player exits the area.
        /// </summary>
        [Tooltip("Audio clip to loop while a player is inside the area.")]
        public AudioClip AreaExitLoop;

        // TODO this
    }
}
