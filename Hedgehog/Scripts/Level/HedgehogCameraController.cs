using Hedgehog.Core.Actors;
using Hedgehog.Core.Moves;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level
{
    /// <summary>
    /// Camera controller that can emulate behavior from Sonic 1, 2, 3, K, and CD. Not implemented: the previous
    /// position table used for spindash lag.
    /// </summary>
    public class HedgehogCameraController : MonoBehaviour
    {
        /// <summary>
        /// The hedgehog controller to follow.
        /// </summary>
        [Tooltip("The hedgehog controller to follow.")]
        public HedgehogController Target;
        private HedgehogController _previousTarget;

        /// <summary>
        /// Whether to rotate the camera based on the controller's direction of gravity.
        /// </summary>
        [Tooltip("Whether to rotate the camera based on the controller's direction of gravity.")]
        public bool RotateToGravity;

        /// <summary>
        /// Whether to snap right onto the controller the first time around, instead of being limited by Follow Speed.
        /// </summary>
        [Tooltip("Whether to snap right onto the controller the first time around, instead of being limited by Follow Speed.")]
        public bool SnapOnInit;

        // Cached moves for optimization
        protected LookUp LookUp;
        protected Duck Duck;
        protected Spindash Spindash;

        [HideInInspector]
        public State CurrentState;
        public enum State
        {
            Idle,
            Follow,
            Lag,
            LookUp,
            LookDown,
            ForwardShift,
        }

        #region Follow State
        /// <summary>
        /// Maximum speed at which the camera follows the controller, in units per second.
        /// </summary>
        [Header("Follow")]
        [Tooltip("Maximum speed at which the camera follows the controller, in units per second.")]
        public float FollowSpeed;

        /// <summary>
        /// Center of the view border, in units. Center of the camera is (0, 0).
        /// </summary>
        [Tooltip("Center of the view border, in units. Center of the camera is (0, 0).")]
        public Vector2 FollowCenter;

        /// <summary>
        /// Half the size of the view border, in units. If the player is this much away from the center, the camera
        /// starts catching up.
        /// </summary>
        [Tooltip("Half the size of the view border, in units. If the player is this much away from the center, the camera " +
                 "starts catching up.")]
        public Vector2 FollowRadius;
        #endregion
        #region LookUp State
        /// <summary>
        /// Current amount the camera has scrolled, in units.
        /// </summary>
        protected float LookAmount;

        /// <summary>
        /// Whether the camera is returning to its neutral position after looking.
        /// </summary>
        protected bool LookReturning;

        /// <summary>
        /// When looking up, how long to wait before panning upwards.
        /// </summary>
        [Header("Look Up")]
        [Tooltip("When looking up, how long to wait before panning upwards.")]
        public float LookUpLag;

        /// <summary>
        /// When looking up, how far up to scroll, in units.
        /// </summary>
        [Tooltip("When looking up, how far up to scroll, in units.")]
        public float LookUpPanAmount;

        /// <summary>
        /// When looking up, how quickly the camera scrolls, in units per second.
        /// </summary>
        [Tooltip("When looking up, how quickly the camera scrolls, in units per second.")]
        public float LookUpPanSpeed;
        #endregion
        #region LookDown State
        /// <summary>
        /// When looking down, how long to wait before panning downwards.
        /// </summary>
        [Header("Look Down")]
        [Tooltip("When looking down, how long to wait before panning downwards.")]
        public float LookDownLag;

        /// <summary>
        /// When looking down, how far down to scroll, in units.
        /// </summary>
        [Tooltip("When looking down, how far down to scroll, in units.")]
        public float LookDownPanAmount;

        /// <summary>
        /// When looking down, how quickly the camera scrolls, in units per second.
        /// </summary>
        [Tooltip("When looking down, how quickly the camera scrolls, in units per second.")]
        public float LookDownPanSpeed;
        #endregion
        #region Spindash
        /// <summary>
        /// Time left in the lag state.
        /// </summary>
        [HideInInspector]
        public float LagTimer;

        /// <summary>
        /// How long to lag after a spindash, in seconds.
        /// </summary>
        [Header("Spindash")]
        [Tooltip("How long to lag after a spindash, in seconds.")]
        public float SpindashLag;
        #endregion
        #region ForwardShift State
        private Vector2 _previousTargetPosition;

        /// <summary>
        /// Whether to shift the camera forward when moving at a certain speed, as seen in Sonic CD.
        /// </summary>
        [Header("Forward Shift (Sonic CD)")]
        [Tooltip("Whether to shift the camera forward when moving at a certain speed, as seen in Sonic CD.")]
        public bool DoForwardShift;

        /// <summary>
        /// Minimum horizontal speed for the forward shift to occur.
        /// </summary>
        [Tooltip("Minimum horizontal speed for the forward shift to occur.")]
        public float ForwardShiftMinSpeed;

        /// <summary>
        /// How far the camera shifts forward, in units.
        /// </summary>
        [Tooltip("How far the camera shifts forward, in units.")]
        public float ForwardShiftPanAmount;

        /// <summary>
        /// How quickly the camera shifts forward, in units per second.
        /// </summary>
        [Tooltip("How quickly the camera shifts forward, in units per second.")]
        public float ForwardShiftPanSpeed;
        #endregion

        public void Reset()
        {
            Target = FindObjectOfType<HedgehogController>();
            RotateToGravity = true;
            SnapOnInit = false;

            FollowSpeed = 9.6f;
            FollowCenter = new Vector2(0.0f, 0.0f);
            FollowRadius = new Vector2(0.08f, 0.32f);

            LookUpLag = 2.0f;
            LookUpPanAmount = 1.04f;
            LookUpPanSpeed = 1.2f;

            LookDownLag = 2.0f;
            LookDownPanAmount = 0.88f;
            LookDownPanSpeed = 1.2f;

            SpindashLag = 0.266667f;

            DoForwardShift = false;
            ForwardShiftMinSpeed = 3.59f;
            ForwardShiftPanAmount = 0.64f;
            ForwardShiftPanSpeed = 1.2f;
        }

        public void Awake()
        {
            CurrentState = State.Follow;
            LagTimer = 0.0f;
            LookAmount = 0.0f;
        }

        public void Start()
        {
            // Try and find a target if one isn't specified
            if (Target == null)
            {
                Target = FindObjectOfType<HedgehogController>();
                if (Target == null)
                {
                    Debug.LogError(
                        string.Format("Hedgehog Camera Controller \"{0}\" has no Target and couldn't find one either!", name));
                    enabled = false;
                    return;
                }
                else
                {
                    Debug.LogWarning(
                        string.Format("Hedgehog Camera Controller \"{0}\" had no Target, so it found \"{1}\" automagically.",
                            name, Target.name));
                }
            }

            OnChangeTarget(null);
        }

        public void LateUpdate()
        {
            CheckChangeTarget();
            HandleState();

            if(RotateToGravity)
                Rotate(Target.GravityDirection + 90.0f);

            _previousTargetPosition = Target.transform.position;
        }

        /// <summary>
        /// Called automatically when the camera detects a change in target.
        /// </summary>
        /// <param name="previousTarget">The camera's previous target. Nullable.</param>
        public void OnChangeTarget(HedgehogController previousTarget)
        {
            // Add listeners for look up and duck
            Target.MoveManager.OnPerform.AddListener(OnPerformMove);
            Target.MoveManager.OnEnd.AddListener(OnEndMove);

            if(SnapOnInit) To(Target.transform.position);

            _previousTargetPosition = Target.transform.position;
            _previousTarget = Target;

            // Cache moves for speed
            LookUp = Target.GetMove<LookUp>();
            Duck = Target.GetMove<Duck>();
            Spindash = Target.GetMove<Spindash>();

            // Clean up listeners from the last target
            if (previousTarget == null)
                return;
            previousTarget.MoveManager.OnPerform.RemoveListener(OnPerformMove);
            previousTarget.MoveManager.OnEnd.RemoveListener(OnEndMove);
        }

        /// <summary>
        /// Checks to see if its current target isn't the same as last update and calls events as a result.
        /// </summary>
        public void CheckChangeTarget()
        {
            if (Target != _previousTarget)
            {
                OnChangeTarget(_previousTarget);
            }
        }

        /// <summary>
        /// Moves the camera and based on its current state and changes state based on the controller's acive moves.
        /// </summary>
        protected void HandleState()
        {
            var t = Target.transform;

            if (CurrentState == State.Follow)
            {
                // State change for looking up
                if (LookUp != null && LookUp.Active)
                {
                    CurrentState = State.LookUp;
                    LagTimer = LookUpLag;
                    LookAmount = 0.0f;
                    return;
                }

                // State change for ducking (and not spindashing)
                if (Duck != null && Duck.Active && (Spindash == null || !Spindash.Active))
                {
                    CurrentState = State.LookDown;
                    LagTimer = LookDownLag;
                    LookAmount = 0.0f;
                    return;
                }

                // State change for Sonic CD forward shift, check horizontal speed
                if (DoForwardShift && Target.RelativeVelocity.x > ForwardShiftMinSpeed)
                {
                    CurrentState = State.ForwardShift;
                    LookAmount = 0.0f;
                    return;
                }

                // Get the controller's distance from the camera
                var offset = (Vector2)(t.position - (transform.position + (Vector3)FollowCenter));

                // Get maximum camera movement
                var followSpeed = FollowSpeed*Time.deltaTime;
                float changeX = 0.0f, changeY = 0.0f;

                // If controller distance is greater than follow radius, set changes in position
                if (offset.x < -FollowRadius.x)
                {
                    changeX = offset.x + FollowRadius.x;
                }
                else if (offset.x > FollowRadius.x)
                {
                    changeX = offset.x - FollowRadius.x;
                }

                if (offset.y < -FollowRadius.y)
                {
                    changeY = offset.y + FollowRadius.y;
                }
                else if (offset.y > FollowRadius.y)
                {
                    changeY = offset.y - FollowRadius.y;
                }

                // Limit position change to follow speed
                var change = new Vector3(Mathf.Clamp(changeX, -followSpeed, followSpeed),
                    Mathf.Clamp(changeY, -followSpeed, followSpeed));

                // Apply position change
                transform.position += change;
            }
            else if (CurrentState == State.Lag)
            {
                // Decrease lag timer, back to normal if it's zero
                LagTimer -= Time.deltaTime;
                if (LagTimer < 0.0f)
                {
                    LagTimer = 0.0f;
                    CurrentState = State.Follow;
                }
            }
            else if (CurrentState == State.LookUp)
            {
                // Lag before beginning camera movement
                LagTimer -= Time.deltaTime;
                if (LagTimer < 0.0f)
                    LagTimer = 0.0f;
                else
                    return;

                Vector2 change;
                if (LookReturning)
                {
                    // Keep track of how much the camera's gone back
                    LookAmount -= LookUpPanSpeed * Time.deltaTime;

                    // If look amount is zero, we're back to normal
                    if (LookAmount < 0.0f)
                    {
                        LookAmount = 0.0f;
                        LookReturning = false;
                        CurrentState = State.Follow;
                        return;
                    }

                    // Return to original position by moving in the opposite direction
                    change = -DMath.AngleToVector((Target.GravityDirection + 180.0f)*Mathf.Deg2Rad)*LookUpPanSpeed*
                             Time.deltaTime;
                }
                else
                {
                    // Keep track of how far the camera's gone
                    LookAmount += LookUpPanSpeed*Time.deltaTime;
                    if (LookAmount > LookUpPanAmount)
                    {
                        LookAmount = LookUpPanAmount;
                        return;
                    }

                    // Translate based on gravity, pan speed, and change in time
                    change = DMath.AngleToVector((Target.GravityDirection + 180.0f)*Mathf.Deg2Rad)*LookUpPanSpeed*
                             Time.deltaTime;
                }

                // Apply translation
                To((Vector2)transform.position + change);
            }
            else if (CurrentState == State.LookDown)
            {
                // Lag before beginning camera movement
                LagTimer -= Time.deltaTime;
                if (LagTimer < 0.0f)
                    LagTimer = 0.0f;
                else
                    return;

                // Back to normal if a spindash starts
                if (Spindash.Active)
                {
                    LookReturning = true;
                }

                // If look amount is zero, we're back to normal
                Vector2 change;
                if (LookReturning)
                {
                    // Keep track of how much the camera's gone back
                    LookAmount -= LookDownPanSpeed * Time.deltaTime;
                    if (LookAmount < 0.0f)
                    {
                        LookAmount = 0.0f;
                        LookReturning = false;
                        CurrentState = State.Follow;
                        return;
                    }

                    // Return to original position by moving in the opposite direction
                    change = -DMath.AngleToVector(Target.GravityDirection * Mathf.Deg2Rad) * LookDownPanSpeed *
                             Time.deltaTime;
                }
                else
                {
                    // Keep track of how far the camera's gone
                    LookAmount += LookDownPanSpeed * Time.deltaTime;
                    if (LookAmount > LookDownPanAmount)
                    {
                        LookAmount = LookDownPanAmount;
                        return;
                    }

                    // Translate based on gravity, pan speed, and change in time
                    change = DMath.AngleToVector(Target.GravityDirection * Mathf.Deg2Rad) * LookDownPanSpeed *
                             Time.deltaTime;
                }

                // Apply translation
                To((Vector2)transform.position + change);
            }
            else if (CurrentState == State.ForwardShift)
            {
                // Apply change in position from the last update, necessary here since the camera is panning
                // forward and following the target simulataneously
                To((Vector2)transform.position + ((Vector2)t.position - _previousTargetPosition));

                // Pan back if target doesn't meet the conditions anymore
                if (Target.RelativeVelocity.x < ForwardShiftMinSpeed || !DoForwardShift)
                {
                    LookReturning = true;
                }

                Vector2 change;
                if (LookReturning)
                {
                    // Keep track of how much the camera's gone back
                    LookAmount -= ForwardShiftPanSpeed*Time.deltaTime;

                    // If look amount is zero, we're back to normal
                    if (LookAmount < 0.0f)
                    {
                        LookAmount = 0.0f;
                        LookReturning = false;
                        CurrentState = State.Follow;
                        return;
                    }

                    // Return to original position by moving in the opposite direction
                    change = -DMath.AngleToVector((Target.GravityDirection + 90.0f)*Mathf.Deg2Rad)*ForwardShiftPanSpeed*
                             Time.deltaTime;
                }
                else
                {
                    // Keep track of how far the camera's gone
                    LookAmount += ForwardShiftPanSpeed * Time.deltaTime;
                    if (LookAmount > ForwardShiftPanAmount)
                    {
                        LookAmount = ForwardShiftPanAmount;
                        return;
                    }

                    // Translate based on gravity, pan speed, and change in time
                    change = DMath.AngleToVector((Target.GravityDirection + 90.0f)*Mathf.Deg2Rad)*ForwardShiftPanSpeed*
                             Time.deltaTime;
                }

                // Apply translation
                To((Vector2)transform.position + change);
            }
        }

        /// <summary>
        /// Sets only the x and y components of the camera's position to the specified position.
        /// </summary>
        /// <param name="position">The specified position.</param>
        public void To(Vector2 position)
        {
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }

        /// <summary>
        /// Rotates the camera about the z-axis to the specified orientation.
        /// </summary>
        /// <param name="degrees">The specified orientation, in degrees.</param>
        public void Rotate(float degrees)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, degrees);
        }

        /// <summary>
        /// Begins the spindash lag timer.
        /// </summary>
        public void DoSpindashLag()
        {
            LagTimer = SpindashLag;
            CurrentState = State.Lag;
        }

        public void OnPerformMove(Move move)
        {
            
        }

        public void OnEndMove(Move move)
        {
            if (move is Spindash)
            {
                if(!DoForwardShift)
                    DoSpindashLag();
            }
            else if (move is LookUp || move is Duck)
            {
                LagTimer = 0.0f;
                LookReturning = true;
            }
        }
    }
}
