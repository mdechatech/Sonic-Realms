using SonicRealms.Core.Actors;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Moves
{
    public class GroundControl : Move
    {
        #region Animation
        /// <summary>
        /// Name of an Animator float set to magnitude of ground control input.
        /// </summary>
        [AnimationFoldout]
        [Tooltip("Name of an Animator float set to magnitude of ground control input.")]
        public string InputAxisFloat;
        protected int InputAxisFloatHash;

        /// <summary>
        /// Name of an Animator bool set to whether there is any input.
        /// </summary>
        [AnimationFoldout]
        [Tooltip("Name of an Animator bool set to whether there is any input.")]
        public string InputBool;
        protected int InputBoolHash;

        /// <summary>
        /// Name of an Animator bool set to whether the controller is accelerating.
        /// </summary>
        [AnimationFoldout]
        [Tooltip("Name of an Animator bool set to whether the controller is accelerating.")]
        public string AcceleratingBool;
        protected int AcceleratingBoolHash;

        /// <summary>
        /// Name of an Animator bool set to whether the controller is braking.
        /// </summary>
        [AnimationFoldout]
        [Tooltip("Name of an Animator bool set to whether the controller is braking.")]
        public string BrakingBool;
        protected int BrakingBoolHash;

        /// <summary>
        /// Name of an Animator float set to absolute ground speed divided by top speed.
        /// </summary>
        [AnimationFoldout]
        [Tooltip("Name of an Animator float set to absolute ground speed divided by top speed.")]
        public string TopSpeedPercentFloat;
        protected int TopSpeedPercentFloatHash;
        #endregion
        #region Control
        /// <summary>
        /// The name of the input axis.
        /// </summary>
        [ControlFoldout]
        [Tooltip("The name of the input axis.")]
        public string MovementAxis;

        /// <summary>
        /// Whether to invert the axis input.
        /// </summary>
        [ControlFoldout]
        [Tooltip("Whether to invert the axis input.")]
        public bool InvertAxis;
        #endregion
        #region Physics
        /// <summary>
        /// Ground acceleration in units per second squared.
        /// </summary>
        [PhysicsFoldout]
        [Tooltip("Ground acceleration in units per second squared.")]
        public float Acceleration;

        /// <summary>
        /// Whether acceleration is allowed.
        /// </summary>
        public bool AccelerationLocked;

        /// <summary>
        /// Ground deceleration in units per second squared.
        /// </summary>
        [PhysicsFoldout]
        [Tooltip("Ground deceleration units per second squared.")]
        public float Deceleration;

        /// <summary>
        /// Whether deceleration is allowed.
        /// </summary>
        public bool DecelerationLocked;

        /// <summary>
        /// Top running speed in unit per second.
        /// </summary>
        [PhysicsFoldout]
        [Tooltip("Top running speed in units per second.")]
        public float TopSpeed;

        /// <summary>
        /// Minimum ground speed at which slope gravity is applied, in units per second. Allows Sonic to stand still on
        /// steep slopes.
        /// </summary>
        [PhysicsFoldout]
        [Tooltip("Minimum ground speed at which slope gravity is applied, in units per second. Allows Sonic to stand still on " +
                 "steep slopes.")]
        public float MinSlopeGravitySpeed;
        #endregion
        
        #region Properties
        /// <summary>
        /// Whether the controller is accelerating.
        /// </summary>
        public bool Accelerating
        {
            get
            {
                return !ControlLockTimerOn &&
                       (Controller.GroundVelocity > 0.0f && _axis > 0.0f ||
                        Controller.GroundVelocity < 0.0f && _axis < 0.0f);
            }
        }

        /// <summary>
        /// Whether the controller is braking.
        /// </summary>
        public bool Braking
        {
            get
            {
                return !ControlLockTimerOn &&
                       (Controller.GroundVelocity > 0.0f && _axis < 0.0f ||
                        Controller.GroundVelocity < 0.0f && _axis > 0.0f);
            }
        }

        /// <summary>
        /// Whether the controller is standing still.
        /// </summary>
        public bool Standing
        {
            get { return DMath.Equalsf(Controller.GroundVelocity) && !Braking && !Accelerating; }
        }

        /// <summary>
        /// Whether the controller is at top speed.
        /// </summary>
        public bool AtTopSpeed
        {
            get { return Mathf.Abs(Controller.GroundVelocity) - TopSpeed > -0.1f; }
        }
        #endregion

        private float _axis;

        /// <summary>
        /// Whether control is disabled.
        /// </summary>
        public bool DisableControl;

        /// <summary>
        /// Whether the control lock is on.
        /// </summary>
        public bool ControlLockTimerOn;
        
        /// <summary>
        /// Time until the control lock is switched off, in seconds. Set to zero if the control is not locked.
        /// </summary>
        public float ControlLockTimer;

        protected ScoreCounter Score;

        public override MoveLayer Layer
        {
            get { return MoveLayer.Control; }
        }

        public override void Reset()
        {
            base.Reset();

            MovementAxis = "Horizontal";
            InvertAxis = false;

            InputAxisFloat = InputBool = AcceleratingBool =
                BrakingBool = TopSpeedPercentFloat = "";

            Acceleration = 1.6875f;
            AccelerationLocked = false;
            Deceleration = 18.0f;
            DecelerationLocked = false;
            TopSpeed = 3.6f;
            MinSlopeGravitySpeed = 0.1f;
        }

        public override void Awake()
        {
            base.Awake();
            _axis = 0.0f;

            DisableControl = false;
            ControlLockTimerOn = false;
            ControlLockTimer = 0.0f;

            InputAxisFloatHash = string.IsNullOrEmpty(InputAxisFloat) ? 0 : Animator.StringToHash(InputAxisFloat);
            InputBoolHash = string.IsNullOrEmpty(InputBool) ? 0 : Animator.StringToHash(InputBool);
            AcceleratingBoolHash = string.IsNullOrEmpty(AcceleratingBool) ? 0 : Animator.StringToHash(AcceleratingBool);
            BrakingBoolHash = string.IsNullOrEmpty(BrakingBool) ? 0 : Animator.StringToHash(BrakingBool);
            TopSpeedPercentFloatHash = string.IsNullOrEmpty(TopSpeedPercentFloat) ? 0 : Animator.StringToHash(TopSpeedPercentFloat);
        }

        public override void OnManagerAdd()
        {
            if (Controller.Grounded) Perform();
            Controller.OnAttach.AddListener(OnAttach);
            Score = Controller.GetComponent<ScoreCounter>();
        }

        public void OnAttach()
        {
            Perform();
            if(Score) Score.EndCombo();
        }

        public override void OnActiveEnter(State previousState)
        {
            Manager.End<AirControl>();
            Controller.OnSteepDetach.AddListener(OnSteepDetach);

            _axis = InvertAxis ? -Input.GetAxis(MovementAxis) : Input.GetAxis(MovementAxis);
        }

        public override void OnActiveUpdate()
        {
            if (ControlLockTimerOn || DisableControl) return;
            _axis = InvertAxis ? -Input.GetAxis(MovementAxis) : Input.GetAxis(MovementAxis);
        }

        public override void OnActiveFixedUpdate()
        {
            UpdateControlLockTimer();
            if (!ControlLockTimerOn)
            {
                Controller.ApplyGroundFriction = !Accelerate(_axis);
            }

            if (Mathf.Abs(Controller.GroundVelocity) < Controller.DetachSpeed)
            {
                if (DMath.AngleInRange_d(Controller.RelativeSurfaceAngle, 50.0f, 310.0f))
                {
                    Lock();
                }
            }

            Controller.ApplySlopeGravity = Accelerating || ControlLockTimerOn ||
                                           Mathf.Abs(Controller.GroundVelocity) > MinSlopeGravitySpeed;

            if (!ControlLockTimerOn && !DMath.Equalsf(_axis))
                Controller.FacingForward = Controller.GroundVelocity >= 0.0f;
        }

        public override void OnActiveExit()
        {
            Controller.OnSteepDetach.RemoveListener(OnSteepDetach);
            Controller.ApplySlopeGravity = true;

            if (Animator == null)
                return;
            if (!string.IsNullOrEmpty(AcceleratingBool))
                Animator.SetBool(AcceleratingBool, false);

            if (!string.IsNullOrEmpty(BrakingBool))
                Animator.SetBool(BrakingBool, false);
        }

        public override void SetAnimatorParameters()
        {
            if(InputAxisFloatHash != 0)
                Animator.SetFloat(InputAxisFloatHash, _axis);

            if(InputBoolHash != 0)
                Animator.SetBool(InputBoolHash, !DMath.Equalsf(_axis));

            if(AcceleratingBoolHash != 0)
                Animator.SetBool(AcceleratingBoolHash, Accelerating);

            if(BrakingBoolHash != 0)
                Animator.SetBool(BrakingBoolHash, Braking);

            if(TopSpeedPercentFloatHash != 0)
                Animator.SetFloat(TopSpeedPercentFloatHash, Mathf.Abs(Controller.GroundVelocity)/TopSpeed);
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
            if (!ControlLockTimerOn) return;

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
            ControlLockTimerOn = true;
        }

        /// <summary>
        /// Unlocks ground control.
        /// </summary>
        public void Unlock()
        {
            ControlLockTimer = 0.0f;
            ControlLockTimerOn = false;
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
