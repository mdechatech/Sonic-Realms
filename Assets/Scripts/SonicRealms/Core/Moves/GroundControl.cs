using System.Security.Cryptography.X509Certificates;
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
        /// Name of an Animator bool set to whether the controller is standing.
        /// </summary>
        [AnimationFoldout]
        [Tooltip("Name of an Animator bool set to whether the controller is standing.")]
        public string StandingBool;
        protected int StandingBoolHash;

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
        [DebugFoldout]
        public bool DisableAcceleration;

        /// <summary>
        /// Ground deceleration in units per second squared.
        /// </summary>
        [PhysicsFoldout]
        [Tooltip("Ground deceleration units per second squared.")]
        public float Deceleration;

        /// <summary>
        /// Whether deceleration is allowed.
        /// </summary>
        [DebugFoldout]
        public bool DisableDeceleration;

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

        [Space, ControlFoldout]
        public float ControlLockDuration;

        [ControlFoldout]
        public float ControlLockAngle;

        protected bool ControlLockEndedThisFrame;
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
            get
            {
                return DMath.Equalsf(Controller.GroundVelocity) && !Braking && !Accelerating &&
                       Mathf.Abs(DMath.ShortestArc_d(Controller.RelativeSurfaceAngle, 0f)) < ControlLockAngle;
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

        private float _axis;

        /// <summary>
        /// Whether control is disabled.
        /// </summary>
        [DebugFoldout]
        public bool DisableControl;

        /// <summary>
        /// Whether the control lock is on.
        /// </summary>
        [DebugFoldout]
        public bool ControlLockTimerOn;

        /// <summary>
        /// Time until the control lock is switched off, in seconds. Set to zero if the control is not locked.
        /// </summary>
        [DebugFoldout]
        public float ControlLockTimer;

        protected ScoreCounter Score;

        public override int Layer
        {
            get { return (int)MoveLayer.Control; }
        }

        public override void Reset()
        {
            base.Reset();

            MovementAxis = "Horizontal";
            InvertAxis = false;

            InputAxisFloat = InputBool = AcceleratingBool =
                BrakingBool = TopSpeedPercentFloat = "";

            Acceleration = 1.6875f;
            DisableAcceleration = false;
            Deceleration = 18.0f;
            DisableDeceleration = false;
            TopSpeed = 3.6f;
            MinSlopeGravitySpeed = 0.1f;
            ControlLockDuration = 0.5f;
            ControlLockAngle = 45f;
        }

        public override void Awake()
        {
            base.Awake();
            _axis = 0.0f;

            DisableControl = false;
            ControlLockTimerOn = false;
            ControlLockTimer = 0.0f;

            InputAxisFloatHash = Animator.StringToHash(InputAxisFloat);
            InputBoolHash = Animator.StringToHash(InputBool);
            AcceleratingBoolHash = Animator.StringToHash(AcceleratingBool);
            BrakingBoolHash = Animator.StringToHash(BrakingBool);
            StandingBoolHash = Animator.StringToHash(StandingBool);
            TopSpeedPercentFloatHash = Animator.StringToHash(TopSpeedPercentFloat);
        }

        public override void OnManagerAdd()
        {
            if (Controller.Grounded) Perform();
            Controller.OnAttach.AddListener(OnAttach);
            Controller.OnSteepDetach.AddListener(OnSteepDetach);
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
            Controller.AddCommandBuffer(ApplyForces, HedgehogController.BufferEvent.AfterForces);

            _axis = InvertAxis ? -Input.GetAxisRaw(MovementAxis) : Input.GetAxisRaw(MovementAxis);
        }

        protected void ApplyForces(HedgehogController controller)
        {
            UpdateControlLockTimer();

            // If we're too steep and not moving quickly enough, start the control lock
            if (!ControlLockTimerOn && 
                Mathf.Abs(controller.GroundVelocity) < controller.DetachSpeed &&
                DMath.AngleInRange_d(controller.RelativeSurfaceAngle, ControlLockAngle, 360f - ControlLockAngle))
                Lock();

            if (!ControlLockTimerOn) Accelerate(_axis);

            // Disable slope gravity when we're not moving, so that Sonic can stand on slopes
            controller.DisableSlopeGravity = !(Accelerating || ControlLockTimerOn ||
                                           Mathf.Abs(controller.GroundVelocity) > MinSlopeGravitySpeed);

            // Disable ground friction while we have player input
            controller.DisableGroundFriction = ControlLockTimerOn || 
                (!DisableAcceleration && Accelerating) ||
                (!DisableDeceleration && Braking);

            if (ControlLockEndedThisFrame) ControlLockEndedThisFrame = false;
        }

        public override void OnActiveUpdate()
        {
            if (ControlLockTimerOn || DisableControl) return;
            _axis = InvertAxis ? -Input.GetAxisRaw(MovementAxis) : Input.GetAxisRaw(MovementAxis);
        }

        public override void OnActiveExit()
        {
            // Set everything back to normal
            Controller.DisableSlopeGravity = false;
            Controller.DisableGroundFriction = false;
            Controller.RemoveCommandBuffer(ApplyForces, HedgehogController.BufferEvent.AfterForces);

            if (Animator == null) return;

            if (AcceleratingBoolHash != 0) Animator.SetBool(AcceleratingBoolHash, false);
            if (BrakingBoolHash != 0) Animator.SetBool(BrakingBoolHash, false);
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

            if(StandingBoolHash != 0)
                Animator.SetBool(StandingBoolHash, Standing);

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
        /// Locks ground control.
        /// </summary>
        public void Lock()
        {
            Lock(ControlLockDuration);
        }

        /// <summary>
        /// Locks ground control for the specified duration.
        /// </summary>
        /// <param name="time"></param>
        public void Lock(float time)
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
            ControlLockEndedThisFrame = true;
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
                if (!DisableDeceleration && Controller.GroundVelocity > 0.0f)
                {
                    var delta = Deceleration*magnitude*timestep;
                    Controller.GroundVelocity += delta;

                    if (Controller.GroundVelocity < 0f)
                        Controller.GroundVelocity = delta;
                }
                else if (!DisableAcceleration)
                {
                    Controller.FacingForward = false;

                    if(Controller.GroundVelocity > -TopSpeed)
                        Controller.GroundVelocity += Acceleration*magnitude*timestep;
                }

                return true;
            }
            else if (magnitude > 0.0f)
            {
                if (!DisableDeceleration && Controller.GroundVelocity < 0f)
                {
                    var delta = Deceleration * magnitude * timestep;
                    Controller.GroundVelocity += delta;

                    if (Controller.GroundVelocity > 0f)
                        Controller.GroundVelocity = delta;
                }
                else if (!DisableAcceleration)
                {
                    Controller.FacingForward = true;

                    if (Controller.GroundVelocity < TopSpeed)
                        Controller.GroundVelocity += Acceleration*magnitude*timestep;
                }

                return true;
            }

            return false;
        }
    }
}
