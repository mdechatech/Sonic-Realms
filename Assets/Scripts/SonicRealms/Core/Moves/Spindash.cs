using SonicRealms.Core.Actors;
using SonicRealms.Core.Internal;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Moves
{
    public class Spindash : Move
    {
        /// <summary>
        /// Input for charging.
        /// </summary>
        [InputFoldout]
        [Tooltip("Input for charging.")]
        public string ChargeButton;

        /// <summary>
        /// Input that must be released to execute the spindash.
        /// </summary>
        [InputFoldout]
        [Tooltip("Input that must be released to execute the spindash.")]
        public string ReleaseAxis;

        /// <summary>
        /// This hitbox becomes harmful while rolling, allowing the player to kill while rolling.
        /// </summary>
        [PhysicsFoldout]
        [Tooltip("This hitbox becomes harmful while spindashing, allowing the player to kill while spindashing.")]
        public SonicHitbox Hitbox;

        /// <summary>
        /// The lowest speed possible after releasing, in units per second.
        /// </summary>
        [PhysicsFoldout]
        [Tooltip("The lowest speed possible after releasing, in units per second.")]
        public float BasePower;

        /// <summary>
        /// The bonus speed given for charging up once, in units per second.
        /// </summary>
        [PhysicsFoldout]
        [Tooltip("The bonus speed given for charging up once, in units per second.")]
        public float ChargePower;

        /// <summary>
        /// Decay rate of the bonus speed from charging up. Current charge decreases by 
        /// itself times ChargePowerDecay each second.
        /// </summary>
        [PhysicsFoldout]
        [Tooltip("Decay rate of the bonus speed from charging up. Current charge decreases by " +
                 "itself times ChargePowerDecay each second.")]
        public float ChargePowerDecay;

        /// <summary>
        /// Maximum total bonus speed from charging up, in units per second.
        /// </summary>
        [PhysicsFoldout]
        [Tooltip("Maximum total bonus speed from charging up, in units per second.")]
        public float MaxChargePower;

        /// <summary>
        /// Current charge power, or zero if not currently active.
        /// </summary>
        [DebugFoldout]
        public float CurrentChargePower;

        /// <summary>
        /// An audio clip to play when the spindash is charged.
        /// </summary>
        [SoundFoldout]
        [Tooltip("An audio clip to play when the spindash is charged.")]
        public AudioClip ChargeSound;

        /// <summary>
        /// How many charges it takes to go from minimum pitch to maximum pitch.
        /// </summary>
        [SoundFoldout]
        [Tooltip("How many charges it takes to go from minimum pitch to maximum pitch.")]
        public int ChargePitchSteps;

        /// <summary>
        /// The minimum pitch of the charge sound.
        /// </summary>
        [SoundFoldout]
        [Tooltip("The minimum pitch of the charge sound.")]
        public float ChargePitchMin;

        /// <summary>
        /// The maximum pitch of the charge sound.
        /// </summary>
        [SoundFoldout]
        [Tooltip("The maximum pitch of the charge sound.")]
        public float ChargePitchMax;

        protected GroundControl GroundControl;
        protected Duck Duck;
        protected Roll Roll;
        protected AudioSource ChargeAudioSource;

        public override int Layer
        {
            get { return (int)MoveLayer.Action; }
        }

        public override void Reset()
        {
            base.Reset();

            ChargeButton = "Jump";
            ReleaseAxis = "Vertical";

            Hitbox = Controller.GetComponentInChildren<SonicHitbox>();
            ChargePower = 0.6f;
            MaxChargePower = 2.4f;
            BasePower = 4.8f;
            ChargePowerDecay = 1.875f;

            ChargeSound = null;
        }

        public override void Awake()
        {
            base.Awake();
            CurrentChargePower = 0.0f;
        }

        public override void Start()
        {
            base.Start();

            Duck = Manager.Get<Duck>();
            Roll = Manager.Get<Roll>();
            GroundControl = Manager.Get<GroundControl>();

            if (ChargeSound == null) return;

            ChargeAudioSource = SrSoundManager.CreateSoundEffectSource();
            ChargeAudioSource.name = "Charge Audio Source";
            ChargeAudioSource.clip = ChargeSound;

            ChargeAudioSource.transform.SetParent(transform);
        }

        public override bool Available
        {
            get { return Manager[MoveLayer.Action] is Duck; }
        }

        public override bool ShouldPerform
        {
            get { return Input.GetButtonDown(ChargeButton); }
        }

        public override bool ShouldEnd
        {
            get
            {
                return Controller.WallMode != WallMode.Floor ||
                       (Duck && Mathf.Abs(Controller.GroundVelocity) > Mathf.Abs(Duck.MaxActivateSpeed));
            }
        }

        public override void OnActiveEnter(State previousState)
        {
            CurrentChargePower = 0.0f;

            if (GroundControl != null)
                GroundControl.DisableControl = true;

            Controller.GroundVelocity = 0;

            if (ChargeAudioSource == null)
                return;

            ChargeAudioSource.pitch = ChargePitchMin;
            ChargeAudioSource.Play();

            if (Hitbox != null)
                Hitbox.Harmful = true;
        }

        public override void OnActiveUpdate()
        {
            CurrentChargePower -= CurrentChargePower*ChargePowerDecay*Time.deltaTime;

            if (Input.GetButtonDown(ChargeButton))
                Charge();

            if (CurrentChargePower > MaxChargePower) CurrentChargePower = MaxChargePower;

            if (Input.GetAxis(ReleaseAxis) >= 0f)
                Finish();
        }

        public override void OnActiveExit()
        {
            if (GroundControl != null)
                GroundControl.DisableControl = false;

            if (ChargeAudioSource != null)
                ChargeAudioSource.Stop();

            if (Hitbox != null && !Controller.IsPerforming<Roll>())
                Hitbox.Harmful = false;
        }

        public void Charge()
        {
            CurrentChargePower += ChargePower;

            if (ChargeAudioSource == null) return;

            ChargeAudioSource.pitch += (ChargePitchMax - ChargePitchMin) / ChargePitchSteps;

            if (ChargeAudioSource.pitch > ChargePitchMax)
                ChargeAudioSource.pitch = ChargePitchMax;

            ChargeAudioSource.Play();
        }

        public void Finish()
        {
            if (Controller.IsFacingForward)
                Controller.GroundVelocity += BasePower + CurrentChargePower;
            else
                Controller.GroundVelocity -= BasePower + CurrentChargePower;

            Manager.Perform<Roll>(true, true);

            End();
        }
    }
}
