using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Plays audio based on effect trigger events.
    /// </summary>
    public class PlayAudioEffectTrigger : ReactiveEffect
    {
        /// <summary>
        /// Audio clip to play when the effect trigger is activated.
        /// </summary>
        [Tooltip("Audio clip to play when the effect trigger is activated.")]
        public AudioClip ActivateSound;

        /// <summary>
        /// Audio clip to loop while the effect trigger is activated.
        /// </summary>
        [Tooltip("Audio clip to loop while the effect trigger is activated.")]
        public AudioClip ActivateStayLoop;

        /// <summary>
        /// Audio clip to play when the effect trigger is deactivated.
        /// </summary>
        [Tooltip("Audio clip to play when the effect trigger is deactivated.")]
        public AudioClip DeactivateSound;

        /// <summary>
        /// Audio clip to play for every player that activates the effect trigger.
        /// </summary>
        [Tooltip("Audio clip to play for every player that activates the effect trigger.")]
        public AudioClip ActivatorEnterSound;

        /// <summary>
        /// Audio clip to loop for every player that is activating the effect trigger.
        /// </summary>
        [Tooltip("Audio clip to loop for every player that is activating the effect trigger.")]
        public AudioClip ActivatorStayLoop;

        /// <summary>
        /// Audio clip to play for every player that deactivates the effect trigger.
        /// </summary>
        [Tooltip("Audio clip to play for every player that deactivates the effect trigger.")]
        public AudioClip ActivatorExitSound;

        // TODO this
    }
}
