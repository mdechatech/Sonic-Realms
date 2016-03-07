using SonicRealms.Level;
using UnityEngine;

namespace SonicRealms.Core.Moves
{
    /// <summary>
    /// Handles the braking sound when Sonic skids to a halt.
    /// </summary>
    [RequireComponent(typeof(GroundControl))]
    public class Skid : Move
    {
        /// <summary>
        /// Minimum speed at which skidding occurs, in units per second.
        /// </summary>
        [ControlFoldout]
        [Tooltip("Minimum speed at which skidding occurs, in units per second.")]
        public float MinimumSpeed;

        /// <summary>
        /// Sound that gets repeated while Sonic is skidding.
        /// </summary>
        [SoundFoldout]
        [Tooltip("Sound that gets repeated while Sonic is skidding.")]
        public AudioClip SkidSound;

        /// <summary>
        /// Point at which the skid sound loops, in seconds.
        /// </summary>
        [SoundFoldout]
        [Tooltip("Point at which the skid sound loops, in seconds.")]
        public float SkidSoundRepeatTime;

        protected AudioSource SkidSoundSource;
        protected GroundControl GroundControl;

        public override MoveLayer Layer
        {
            get { return MoveLayer.Action; }
        }

        public override bool Available
        {
            get { return Controller.Grounded && Mathf.Abs(Controller.GroundVelocity) > MinimumSpeed; }
        }

        public override bool ShouldPerform
        {
            get { return GroundControl.Braking && Manager[MoveLayer.Action] == null && Manager[MoveLayer.Roll] == null; }
        }

        public override bool ShouldEnd
        {
            get { return !Controller.Grounded || !GroundControl.Braking || Manager[MoveLayer.Roll] != null; }
        }

        public override void Reset()
        {
            base.Reset();

            MinimumSpeed = 2.7f;

            SkidSound = null;
            SkidSoundRepeatTime = 0.0667f;
        }

        public override void Start()
        {
            base.Start();
            GroundControl = Manager.Get<GroundControl>();

            if (SkidSound != null)
            {
                SkidSoundSource = SoundManager.Instance.CreateAudioSource();
                SkidSoundSource.clip = SkidSound;
                SkidSoundSource.name = "Skid Sound";
                SkidSoundSource.transform.SetParent(gameObject.transform);
            }
        }

        public override void OnActiveEnter()
        {
            if(SkidSoundSource != null) SkidSoundSource.Play();
        }

        public override void OnActiveUpdate()
        {
            if (SkidSoundSource == null) return;
            if (SkidSoundSource.time > SkidSoundRepeatTime) SkidSoundSource.time = 0f;
        }
    }
}
