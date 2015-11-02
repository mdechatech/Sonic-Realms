using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Core.Moves
{
    public class GroundControl : ControlState
    {
        #region Control Fields
        /// <summary>
        /// The name of the input axis.
        /// </summary>
        [SerializeField]
        [Tooltip("The name of the input axis.")]
        public string InputAxis;

        /// <summary>
        /// Whether to invert the axis input.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to invert the axis input.")]
        public bool InvertAxis;
        #endregion
        #region Physics Fields
        /// <summary>
        /// Ground acceleration in units per second squared.
        /// </summary>
        [SerializeField]
        [Tooltip("Ground acceleration in units per second squared.")]
        public float Acceleration;

        /// <summary>
        /// Whether acceleration is allowed.
        /// </summary>
        public bool AccelerationLocked;

        /// <summary>
        /// Ground deceleration in units per second squared.
        /// </summary>
        [SerializeField]
        [Tooltip("Ground deceleration units per second squared.")]
        public float Deceleration;

        /// <summary>
        /// Whether deceleration is allowed.
        /// </summary>
        public bool DecelerationLocked;

        /// <summary>
        /// Top running speed in unit per second.
        /// </summary>
        [SerializeField]
        [Tooltip("Top running speed in units per second.")]
        public float TopSpeed;
        #endregion
        #region Animation Fields
        /// <summary>
        /// Name of an Animator float set to magnitude of ground control input.
        /// </summary>
        [SerializeField]
        [Tooltip("Name of an Animator float set to magnitude of ground control input.")]
        public string InputAxisFloat;

        /// <summary>
        /// Name of an Animator bool set to whether the controller is grounded.
        /// </summary>
        [SerializeField]
        [Tooltip("Name of an Animator bool set to whether the controller is grounded.")]
        public string GroundedBool;

        /// <summary>
        /// Name of an Animator bool set to whether the controller is accelerating.
        /// </summary>
        [SerializeField]
        [Tooltip("Name of an Animator bool set to whether the controller is accelerating.")]
        public string AcceleratingBool;

        /// <summary>
        /// Name of an Animator bool set to whether the controller is braking.
        /// </summary>
        [SerializeField]
        [Tooltip("Name of an Animator bool set to whether the controller is braking.")]
        public string BrakingBool;

        /// <summary>
        /// Name of an Animator float set to ground speed in units per second.
        /// </summary>
        [SerializeField]
        [Tooltip("Name of an Animator float set to ground speed in units per second.")]
        public string SpeedFloat;

        /// <summary>
        /// Name of an Animator float set to absolute ground speed in units per second.
        /// </summary>
        [SerializeField]
        [Tooltip("Name of an Animator float set to absolute ground speed in units per second.")]
        public string AbsoluteSpeedFloat;

        /// <summary>
        /// Name of an Animator float set to surface angle in degrees per second.
        /// </summary>
        [SerializeField]
        [Tooltip("Name of an Animator float set to surface angle in degrees per second.")]
        public string SurfaceAngleFloat;

        /// <summary>
        /// Name of an Animator bool set to whether running at top speed or faster.
        /// </summary>
        [SerializeField]
        [Tooltip("Name of an Animator bool set to whether running at top speed or faster.")]
        public string TopSpeedBool;

        /// <summary>
        /// Name of an Animator bool set to whether the control lock is on.
        /// </summary>
        [SerializeField]
        [Tooltip("Name of an Animator bool set to whether the control lock is on.")]
        public string ControlLockBool;

        /// <summary>
        /// Name of an Animator float set to the control lock timer.
        /// </summary>
        [SerializeField]
        [Tooltip("Name of an Animator float set to the control lock timer.")]
        public string ControlLockTimerFloat;
        #endregion
        #region Properties
        /// <summary>
        /// Whether the controller is accelerating.
        /// </summary>
        public bool Accelerating
        {
            get
            {
                return !ControlLocked &&
                       (Controller.GroundVelocity > 0.0f && _axisMagnitude > 0.0f ||
                        Controller.GroundVelocity < 0.0f && _axisMagnitude < 0.0f);
            }
        }

        /// <summary>
        /// Whether the controller is braking.
        /// </summary>
        public bool Braking
        {
            get
            {
                return !ControlLocked &&
                       (Controller.GroundVelocity > 0.0f && _axisMagnitude < 0.0f ||
                        Controller.GroundVelocity < 0.0f && _axisMagnitude > 0.0f);
            }
        }

        /// <summary>
        /// Whether the controller is at top speed.
        /// </summary>
        public bool AtTopSpeed
        {
            get { return Mathf.Abs(Controller.GroundVelocity) - TopSpeed > -0.1f; }
        }
        #endregion
        private float _axisMagnitude;

        /// <summary>
        /// Whether the control lock is on.
        /// </summary>
        public bool ControlLocked;
        
        /// <summary>
        /// Time until the control lock is switched off, in seconds. Set to zero if the control is not locked.
        /// </summary>
        public float ControlLockTimer;

        public void Reset()
        {
            InputAxis = "Horizontal";
            InvertAxis = false;

            Acceleration = 1.6875f;
            AccelerationLocked = false;
            Deceleration = 18.0f;
            DecelerationLocked = false;
            TopSpeed = 3.6f;

            SpeedFloat = GroundedBool = SurfaceAngleFloat = ControlLockBool = ControlLockTimerFloat = 
                TopSpeedBool = "";
        }

        public override void Awake()
        {
            base.Awake();
            _axisMagnitude = 0.0f;

            ControlLocked = false;
            ControlLockTimer = 0.0f;
        }

        public override void OnStateEnter(ControlState previousState)
        {
            Controller.OnSteepDetach.AddListener(OnSteepDetach);
            Controller.AutoFlip = false;
        }

        public override void OnStateExit(ControlState nextState)
        {
            Controller.OnSteepDetach.RemoveListener(OnSteepDetach);
            Controller.AutoFlip = true;

            if(Animator == null) return;
            if(AcceleratingBool.Length > 0)
                Animator.SetBool(AcceleratingBool, false);

            if(BrakingBool.Length > 0)
                Animator.SetBool(BrakingBool, false);
        }

        public override void OnStateUpdate()
        {
            ControlLockTimer -= Time.deltaTime;
            if (ControlLockTimer < 0.0f)
            {
                Unlock();
            }
        }

        public override void OnStateFixedUpdate()
        {
            UpdateControlLockTimer();
            if (!ControlLocked)
            {
                Controller.ApplyGroundFriction = !Accelerate(_axisMagnitude);
            }

            if (Mathf.Abs(Controller.GroundVelocity) < Controller.DetachSpeed &&
                DMath.AngleInRange_d(Controller.RelativeSurfaceAngle, 50.0f, 310.0f))
            {
                Lock();
            }

            if (!DMath.Equalsf(_axisMagnitude))
                Controller.FacingForward = Controller.GroundVelocity >= 0.0f;
        }

        public override void GetInput()
        {
            _axisMagnitude = Input.GetAxis(InputAxis)*(InvertAxis ? -1.0f : 1.0f);
        }

        public override void SetAnimatorParameters()
        {
            if(InputAxisFloat.Length > 0)
                Animator.SetFloat(InputAxisFloat, _axisMagnitude);

            if(GroundedBool.Length > 0)
                Animator.SetBool(GroundedBool, Controller.Grounded);

            if(AcceleratingBool.Length > 0)
                Animator.SetBool(AcceleratingBool, Accelerating);

            if(BrakingBool.Length > 0)
                Animator.SetBool(BrakingBool, Braking);

            if (SpeedFloat.Length > 0)
                Animator.SetFloat(SpeedFloat, Controller.GroundVelocity);

            if(AbsoluteSpeedFloat.Length > 0)
                Animator.SetFloat(AbsoluteSpeedFloat, Mathf.Abs(Controller.GroundVelocity));

            if(SurfaceAngleFloat.Length > 0)
                Animator.SetFloat(SurfaceAngleFloat, Controller.SurfaceAngle);

            if(TopSpeedBool.Length > 0)
                Animator.SetBool(TopSpeedBool, AtTopSpeed);

            if(ControlLockBool.Length > 0)
                Animator.SetBool(ControlLockBool, ControlLocked);

            if(ControlLockTimerFloat.Length > 0)
                Animator.SetFloat(ControlLockTimerFloat, ControlLockTimer);
        }

        public void OnSteepDetach()
        {
            Lock();
        }

        public void UpdateControlLockTimer()
        {
            UpdateControlLockTimer(Time.fixedDeltaTime);
        }

        public void UpdateControlLockTimer(float timestep)
        {
            if (!ControlLocked) return;

            ControlLockTimer -= timestep;
            if (ControlLockTimer < 0.0f) Unlock();
        }

        /// <summary>
        /// Locks ground control for the specified duration.
        /// </summary>
        /// <param name="time"></param>
        public void Lock(float time = 0.5f)
        {
            ControlLockTimer = time;
            ControlLocked = true;
        }

        /// <summary>
        /// Unlocks ground control.
        /// </summary>
        public void Unlock()
        {
            ControlLockTimer = 0.0f;
            ControlLocked = false;
        }

        /// <summary>
        /// Accelerates the controller forward.
        /// </summary>
        /// <param name="magnitude">A value between 0 and 1, 0 being no acceleration and 1 being the amount
        /// defined by Acceleration.</param>
        /// <returns>Whether any acceleration occurred.</returns>
        public bool AccelerateForward(float magnitude = 1.0f)
        {
            return Accelerate(magnitude);
        }

        /// <summary>
        /// Accelerates the controller backward.
        /// </summary>
        /// <param name="magnitude">A value between 0 and 1, 0 being no acceleration and 1 being the amount
        /// defined by Acceleration.</param>
        /// <returns>Whether any acceleration occurred.</returns>
        public bool AccelerateBackward(float magnitude = 1.0f)
        {
            return Accelerate(-magnitude);
        }

        /// <summary>
        /// Accelerates the controller using Time.fixedDeltaTime as the timestep.
        /// </summary>
        /// <param name="magnitude">A value between -1 and 1, positive moving it forward and negative moving
        /// it back.</param>
        /// <returns>Whether any acceleration occurred.</returns>
        public bool Accelerate(float magnitude)
        {
            return Accelerate(magnitude, Time.fixedDeltaTime);
        }

        /// <summary>
        /// Accelerates the controller.
        /// </summary>
        /// <param name="magnitude">A value between -1 and 1, positive moving it forward and negative moving
        /// it back.</param>
        /// <param name="timestep">The timestep, in seconds</param>
        /// <returns>Whether any acceleration occurred.</returns>
        public bool Accelerate(float magnitude, float timestep)
        {
            magnitude = Mathf.Clamp(magnitude, -1.0f, 1.0f);
            if (DMath.Equalsf(magnitude)) return false;

            if (magnitude < 0.0f)
            {
                if (!DecelerationLocked && Controller.GroundVelocity > 0.0f)
                {
                    Controller.GroundVelocity += Deceleration*magnitude*timestep;
                    return true;
                }
                else if (!AccelerationLocked && Controller.GroundVelocity > -TopSpeed)
                {
                    Controller.GroundVelocity += Acceleration*magnitude*timestep;
                    return true;
                }
            }
            else if (magnitude > 0.0f)
            {
                if (!DecelerationLocked && Controller.GroundVelocity < 0.0f)
                {
                    Controller.GroundVelocity += Deceleration*magnitude*timestep;
                    return true;
                }
                else if (!AccelerationLocked && Controller.GroundVelocity < TopSpeed)
                {
                    Controller.GroundVelocity += Acceleration*magnitude*timestep;
                    return true;
                }
            }

            return false;
        }
    }
}
