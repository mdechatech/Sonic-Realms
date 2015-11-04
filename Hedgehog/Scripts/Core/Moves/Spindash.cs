using UnityEngine;

namespace Hedgehog.Core.Moves
{
    public class Spindash : Move
    {
        /// <summary>
        /// Input for charging.
        /// </summary>
        [SerializeField]
        [Tooltip("Input for charging.")]
        public string ChargeButton;

        /// <summary>
        /// The lowest speed possible after releasing, in units per second.
        /// </summary>
        [SerializeField]
        [Tooltip("The lowest speed possible after releasing, in units per second.")]
        public float BasePower;

        /// <summary>
        /// The bonus speed given for charging up once, in units per second.
        /// </summary>
        [SerializeField]
        [Tooltip("The bonus speed given for charging up once, in units per second.")]
        public float ChargePower;

        /// <summary>
        /// Decay rate of the bonus speed from charging up. Current charge decreases by 
        /// itself times ChargePowerDecay each second.
        /// </summary>
        [SerializeField]
        [Tooltip("Decay rate of the bonus speed from charging up. Current charge decreases by " +
                 "itself times ChargePowerDecay each second.")]
        public float ChargePowerDecay;

        /// <summary>
        /// Maximum total bonus speed from charging up, in units per second.
        /// </summary>
        [SerializeField]
        [Tooltip("Maximum total bonus speed from charging up, in units per second.")]
        public float MaxChargePower;

        /// <summary>
        /// Current charge power.
        /// </summary>
        public float CurrentChargePower;

        public override void Reset()
        {
            base.Reset();

            ChargeButton = "Jump";

            ChargePower = 0.6f;
            MaxChargePower = 2.4f;
            BasePower = 4.8f;
            ChargePowerDecay = 1.875f;
        }

        public override void Awake()
        {
            base.Awake();

            CurrentChargePower = 0.0f;
        }

        public override bool Available()
        {
            return Controller.IsActive<Duck>();
        }

        public override bool InputActivate()
        {
            return Input.GetButton(ChargeButton);
        }

        public override bool InputDeactivate()
        {
            return !Controller.IsActive<Duck>();
        }

        public override void OnActiveEnter(State previousState)
        {
            CurrentChargePower = 0.0f;
        }

        public override void OnActiveUpdate()
        {
            CurrentChargePower -= CurrentChargePower*ChargePowerDecay*Time.deltaTime;

            if (Input.GetButtonDown(ChargeButton))
                CurrentChargePower += ChargePower;

            if (CurrentChargePower > MaxChargePower) CurrentChargePower = MaxChargePower;
        }

        public override void OnActiveExit()
        {
            if(Controller.FacingForward)
                Controller.GroundVelocity += BasePower + CurrentChargePower;
            else
                Controller.GroundVelocity -= BasePower + CurrentChargePower;

            Controller.ForcePerformMove<Roll>();
        }
    }
}
