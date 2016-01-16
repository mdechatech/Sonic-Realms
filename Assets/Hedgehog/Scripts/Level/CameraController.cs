using System;
using Hedgehog.Core.Actors;
using Hedgehog.Core.Moves;
using Hedgehog.Core.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Hedgehog.Level
{
    [Serializable]
    public class ChangeTargetEvent : UnityEvent<Transform> { }

    /// <summary>
    /// A generic camera controller. Has methods for following, panning, and lagging similar to the nature
    /// of the 16 bit games.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        public Transform Target;
        private Transform _previousTarget;

        public bool FollowPlayer;
        public float FollowSpeed;

        public Camera Camera;

        public Transform LevelBoundsMin;
        public Transform LevelBoundsMax;

        public Transform FollowBoundsMin;
        public Transform FollowBoundsMax;

        public Vector2 BasePosition;
        public Vector2 OffsetPosition;

        public Vector2 PanStart;
        public Vector2 PanEnd;
        public float PanTimer;
        public float PanTime;
        public float PanDelayTimer;

        public float WaitTimer;

        /// <summary>
        /// Whether to rotate the camera based on the controller's direction of gravity.
        /// </summary>
        [Tooltip("Whether to rotate the camera based on the controller's direction of gravity.")]
        public bool RotateToGravity;

        /// <summary>
        /// How smoothly the rotation occurs, 1 being smoothest and 0 being instant.
        /// </summary>
        [Range(0.0f, 1.0f)]
        [Tooltip("How smoothly the rotation occurs, 1 being smoothest and 0 being instant.")]
        public float RotationSmoothness;

        public float Rotation
        {
            get { return transform.eulerAngles.z; }
            set { transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, value); }
        }

        /// <summary>
        /// Whether to round position to the nearest pixel (0.01 unit).
        /// </summary>
        [Tooltip("Whether to round position to the nearest pixel (0.01 unit).")]
        public bool PixelPerfect;

        /// <summary>
        /// Whether to snap right onto the controller the first time around, instead of being limited by Follow Speed.
        /// </summary>
        [Tooltip("Whether to snap right onto the controller the first time around, instead of being limited by Follow Speed.")]
        public bool SnapOnInit;

        /// <summary>
        /// Invoked when the camera controller changes target.
        /// </summary>
        public ChangeTargetEvent OnChangeTarget;

        public State CurrentState;
        public enum State
        {
            Idle,
            Follow,
            Waiting,
            Pan,
        }

        public void Reset()
        {
            Camera = GetComponent<Camera>();
            Target = null;
            RotateToGravity = true;
            RotationSmoothness = 0.2f;
            PixelPerfect = true;
            SnapOnInit = false;

            FollowSpeed = 9.6f;

            OnChangeTarget = new ChangeTargetEvent();
        }

        public void Awake()
        {
            CurrentState = State.Follow;
            BasePosition = transform.position;
            OffsetPosition = Vector2.zero;
            WaitTimer = 0f;

            OnChangeTarget = OnChangeTarget ?? new ChangeTargetEvent();
        }

        public void Start()
        {
            Camera = Camera ?? GetComponent<Camera>();
            if(Target) OnChangeTarget.Invoke(null);
        }

        public void FixedUpdate()
        {
            CheckChangeTarget();
            HandleState();
            HandlePosition();

            /*
            // Rotate to player's direction of gravity
            if (RotateToGravity)
            {
                if (DMath.Equalsf(RotationSmoothness))
                {
                    Rotate(Target.GravityDirection + 90.0f);
                }
                else
                {
                    Rotate(Mathf.LerpAngle(Rotation, Target.GravityDirection + 90.0f,
                        Time.fixedDeltaTime * (1.0f / RotationSmoothness)));
                }
            }
            else
            {
                Rotate(0.0f);
            }
            */
        }

        /// <summary>
        /// Corrects the given position to be pixel perfect.
        /// </summary>
        /// <param name="position">The given position.</param>
        /// <returns></returns>
        public Vector2 DoPixelPerfectCheck(Vector2 position)
        {
            return new Vector2(DMath.Round(position.x, 0.01f), DMath.Round(position.y, 0.01f));
        }
        
        /// <summary>
        /// Corrects the given position to be inside the current level bounds, taking into account the size
        /// of the current camera.
        /// </summary>
        /// <param name="position">The position to correct.</param>
        /// <returns>The corrected position.</returns>
        public Vector2 DoLevelBoundsCheck(Vector2 position)
        {
            float x = position.x, y = position.y;
            var cameraBounds = new Bounds(Camera.transform.position,
                new Vector3(Camera.orthographicSize*2f*Camera.aspect,
                    Camera.orthographicSize*2f));

            if (x - cameraBounds.extents.x < LevelBoundsMin.position.x)
                x = LevelBoundsMin.position.x + cameraBounds.extents.x;
            else if (x + cameraBounds.extents.x > LevelBoundsMax.position.x)
                x = LevelBoundsMax.position.x - cameraBounds.extents.x;

            if (y - cameraBounds.extents.y < LevelBoundsMin.position.y)
                y = LevelBoundsMin.position.y + cameraBounds.extents.y;
            else if (y + cameraBounds.extents.y > LevelBoundsMax.position.y)
                y = LevelBoundsMax.position.y - cameraBounds.extents.y;

            return new Vector2(x, y);
        }

        /// <summary>
        /// Follows the given position for one step.
        /// </summary>
        /// <param name="position">The position to follow.</param>
        public void DoFollow(Vector2 position)
        {
            DoFollow(position, Time.deltaTime);
        }

        /// <summary>
        /// Follows the given position using the given timestep.
        /// </summary>
        /// <param name="position">The position to follow.</param>
        /// <param name="deltaTime">The timestep.</param>
        public void DoFollow(Vector2 position, float deltaTime)
        {
            float x = BasePosition.x, y = BasePosition.y;
            position += OffsetPosition;

            if (position.x > FollowBoundsMax.position.x)
                x += Mathf.Min(FollowSpeed*deltaTime, position.x - FollowBoundsMax.position.x);
            else if (position.x < FollowBoundsMin.position.x)
                x -= Mathf.Min(FollowSpeed*deltaTime, FollowBoundsMin.position.x - position.x);

            if (position.y > FollowBoundsMax.position.y)
                y += Mathf.Min(FollowSpeed*deltaTime, position.y - FollowBoundsMax.position.y);
            else if (position.y < FollowBoundsMin.position.y)
                y -= Mathf.Min(FollowSpeed*deltaTime, FollowBoundsMin.position.y - position.y);

            BasePosition = new Vector2(x, y);
        }

        /// <summary>
        /// Sets the level bounds for the camera.
        /// </summary>
        /// <param name="min">A transform representing the bottom left corner of the bounds.</param>
        /// <param name="max">A transform representing the top right corner of the bounds.</param>
        public void SetBounds(Transform min, Transform max)
        {
            LevelBoundsMin = min;
            LevelBoundsMax = max;
        }

        /// <summary>
        /// Pans to the specified offset position over the specified duration, in seconds.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="seconds"></param>
        public void Pan(Vector2 offset, float seconds, float delay = 0f)
        {
            PanStart = OffsetPosition;
            PanEnd = offset;
            PanTimer = 0f;
            PanTime = seconds;

            PanDelayTimer = CurrentState == State.Pan ? Mathf.Min(PanDelayTimer, delay) : delay;
            CurrentState = State.Pan;
        }

        /// <summary>
        /// Pans to the specified offset position at the specified speed, in units per second.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="speed"></param>
        public void PanAtSpeed(Vector2 offset, float speed, float delay = 0f)
        {
            Pan(offset, (offset - OffsetPosition).magnitude/speed, delay);
        }
        
        /// <summary>
        /// Interrupts normal behavior for the specified duration.
        /// </summary>
        /// <param name="seconds">The specified duration, in seconds.</param>
        public void Wait(float seconds)
        {
            WaitTimer = seconds;
            CurrentState = State.Waiting;
        }

        public void Follow()
        {
            CurrentState = State.Follow;
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
        /// Checks to see if its current target isn't the same as last update and calls events as a result.
        /// </summary>
        public void CheckChangeTarget()
        {
            if (Target != _previousTarget) OnChangeTarget.Invoke(_previousTarget);
        }

        /// <summary>
        /// Moves the camera and based on its current state and changes state based on the controller's acive moves.
        /// </summary>
        protected void HandleState()
        {
            var t = Target.transform;
            if (CurrentState == State.Follow)
            {
                if(FollowPlayer) DoFollow(t.position);
            }
            else if (CurrentState == State.Pan)
            {
                if(FollowPlayer) DoFollow(t.position);

                if (PanDelayTimer > 0f)
                {
                    if ((PanDelayTimer -= Time.deltaTime) < PanTime)
                    {
                        PanDelayTimer = 0f;
                    }
                }
                else
                {
                    if (PanTimer < PanTime)
                    {
                        PanTimer += Time.deltaTime;
                        OffsetPosition = Vector2.Lerp(PanStart, PanEnd, PanTimer / PanTime);
                    }
                    else
                    {
                        PanTimer = PanTime;
                        CurrentState = State.Follow;
                    }
                }
            }
            else if (CurrentState == State.Waiting)
            {
                if ((WaitTimer -= Time.deltaTime) < 0f)
                {
                    WaitTimer = 0f;
                    CurrentState = State.Follow;
                }
            }
        }

        protected void HandlePosition()
        {
            var result = BasePosition + OffsetPosition;
            if (PixelPerfect) result = DoPixelPerfectCheck(result);
            result = DoLevelBoundsCheck(result);
            To(result);
        }
    }
}
