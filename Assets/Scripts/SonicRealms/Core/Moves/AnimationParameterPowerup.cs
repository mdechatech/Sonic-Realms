using UnityEngine;

namespace SonicRealms.Core.Moves
{
    /// <summary>
    /// When given to a player, sets a bool on its Animator to true for a duration.
    /// When removed, sets it back to false.
    /// </summary>
    public class AnimationParameterPowerup : Move
    {
        /// <summary>
        /// Name of an Animator bool to set to true while the player has this powerup.
        /// </summary>
        [ControlFoldout]
        [Tooltip("Name of an Animator bool to set to true while the player has this powerup.")]
        public string Parameter;
        protected int ParameterHash;

        /// <summary>
        /// How long for the powerup to last.
        /// </summary>
        [ControlFoldout]
        [Tooltip("How long for the powerup to last.")]
        public float Time;

        [DebugFoldout]
        public float TimeRemaining;

        public override void Reset()
        {
            base.Reset();
            Parameter = "Speed Shoes";
            Time = 20f;
        }

        public override void Awake()
        {
            base.Awake();
            ParameterHash = Animator.StringToHash(Parameter);
        }

        public override void OnManagerAdd()
        {
            var logWarnings = Animator.logWarnings;
            Animator.SetBool(ParameterHash, true);
            Animator.logWarnings = logWarnings;

            TimeRemaining = Time;
            Perform();
        }

        public override void OnActiveUpdate()
        {
            if ((TimeRemaining -= UnityEngine.Time.deltaTime) <= 0f)
                End();
        }

        public override void OnActiveExit()
        {
            Remove();
        }

        public override void OnManagerRemove()
        {
            var logWarnings = Animator.logWarnings;
            Animator.SetBool(ParameterHash, false);
            Animator.logWarnings = logWarnings;
        }
    }
}
