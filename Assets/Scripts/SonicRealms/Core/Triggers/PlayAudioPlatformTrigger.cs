using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Plays audio based on platform trigger events.
    /// </summary>
    public class PlayAudioPlatformTrigger : ReactivePlatform
    {
        /// <summary>
        /// Audio clip to play when a player begins colliding with the platform.
        /// </summary>
        [Tooltip("Audio clip to play when a player begins colliding with the platform.")]
        public AudioClip PlatformEnterSound;

        /// <summary>
        /// Audio clip to loop while a player is colliding with the platform.
        /// </summary>
        [Tooltip("Audio clip to loop while a player is colliding with the platform.")]
        public AudioClip PlatformStayLoop;

        /// <summary>
        /// Audio clip to play when a player stops colliding with the platform.
        /// </summary>
        [Tooltip("Audio clip to play when a player stops colliding with the platform.")]
        public AudioClip PlatformExitSound;

        /// <summary>
        /// Audio clip to play when a player starts standing on the platform.
        /// </summary>
        [Tooltip("Audio clip to play when a player starts standing on the platform.")]
        public AudioClip SurfaceEnterSound;
        
        /// <summary>
        /// Audio clip to loop while a player is standing on the platform.
        /// </summary>
        [Tooltip("Audio clip to loop while a player is standing on the platform.")]
        public AudioClip SurfaceStayLoop;

        /// <summary>
        /// Audio clip to play when a player stops standing on the platform.
        /// </summary>
        [Tooltip("Audio clip to play when a player stops standing on the platform.")]
        public AudioClip SurfaceExitSound;

        // TODO this
    }
}
