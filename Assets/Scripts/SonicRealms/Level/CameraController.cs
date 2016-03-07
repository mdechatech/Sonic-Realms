using System;
using SonicRealms.Core.Utils;
using SonicRealms.Level.Effects;
using UnityEngine;
using UnityEngine.Events;

namespace SonicRealms.Level
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
        [SerializeField]
        private Transform _target;
        public Transform Target
        {
            get { return _target; }
            set
            {
                if (_target == value) return;

                _target = value;
                OnChangeTarget.Invoke(value);
            }
        }

        [Foldout("Follow")]
        public Vector2 FollowSpeed;

        [Foldout("Follow")]
        public Transform FollowBoundsMin;

        [Foldout("Follow")]
        public Transform FollowBoundsMax;

        [Foldout("Focus")]
        public Vector2 FocusSpeed;

        [HideInInspector]
        public Camera Camera;

        [Foldout("Bounds")]
        public Transform LevelBoundsMin;

        [Foldout("Bounds")]
        public Transform LevelBoundsMax;

        [Foldout("Debug")]
        public bool FollowTarget;

        [Foldout("Debug")]
        public Vector2 BasePosition;

        [Foldout("Debug")]
        public Vector2 PanOffset;

        [Foldout("Debug")]
        public Vector2 ExtraOffset;

        [Space, Foldout("Debug")]
        public Vector2 PanStart;

        [Foldout("Debug")]
        public Vector2 PanEnd;

        [Foldout("Debug")]
        public float PanTimer;

        [Foldout("Debug")]
        public float PanTime;

        [Foldout("Debug")]
        public float PanDelayTimer;

        [Space, Foldout("Debug")]
        public float WaitTimer;

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
        /// Whether to snap right onto the controller the first time around, instead of being limited by Follow Speed.
        /// </summary>
        [Tooltip("Whether to snap right onto the controller the first time around, instead of being limited by Follow Speed.")]
        public bool SnapOnInit;

        /// <summary>
        /// Invoked when the camera controller changes target.
        /// </summary>
        public ChangeTargetEvent OnChangeTarget;

        public CameraControllerState State;

        public void Reset()
        {
            Target = null;
            SnapOnInit = false;

            FollowSpeed = new Vector2(9.6f, 9.6f);

            OnChangeTarget = new ChangeTargetEvent();
        }

        public void Awake()
        {
            State = CameraControllerState.Follow;
            BasePosition = transform.position;
            PanOffset = Vector2.zero;
            FollowTarget = true;
            WaitTimer = 0f;

            OnChangeTarget = OnChangeTarget ?? new ChangeTargetEvent();
            Camera = GetComponent<Camera>();
        }

        public void Start()
        {
            if (Target)
            {
                OnChangeTarget.Invoke(null);
                if (SnapOnInit) Snap();
            }
        }

        public void FixedUpdate()
        {
            HandleState();
            HandlePosition();
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
            if (LevelBoundsMax == null || LevelBoundsMin == null) return position;

            float x = position.x, y = position.y;
            var cameraBounds = new Bounds(position,
                new Vector3(Camera.orthographicSize*2f*Camera.aspect, Camera.orthographicSize*2f));

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
            position += PanOffset + ExtraOffset;

            // temporary workaround to account for direction of gravity
            // rotates the boundaries but always follows in cardinal directions, making it look a bit ugly
            var min = DMath.RotateBy(FollowBoundsMin.position, -Rotation*Mathf.Deg2Rad, transform.position);
            var max = DMath.RotateBy(FollowBoundsMax.position, -Rotation*Mathf.Deg2Rad, transform.position);

            if (position.x > max.x)
                x += Mathf.Min(FollowSpeed.x * deltaTime, position.x - max.x);
            else if (position.x < min.x)
                x -= Mathf.Min(FollowSpeed.x * deltaTime, min.x - position.x);

            if (position.y > max.y)
                y += Mathf.Min(FollowSpeed.y * deltaTime, position.y - max.y);
            else if (position.y < min.y)
                y -= Mathf.Min(FollowSpeed.y * deltaTime, min.y - position.y);
            

            const float ChangeThreshold = 0.005f;
            if (Mathf.Abs(x - BasePosition.x) < ChangeThreshold) x = BasePosition.x;
            if (Mathf.Abs(y - BasePosition.y) < ChangeThreshold) y = BasePosition.y;
            
            BasePosition = DoLevelBoundsCheck(new Vector2(x, y));
        }

        /// <summary>
        /// Focuses on the given position for one step.
        /// </summary>
        /// <param name="position"></param>
        public void DoFocus(Vector2 position)
        {
            DoFocus(position, Time.deltaTime);
        }

        /// <summary>
        /// Focuses on the given position using the given timestep.
        /// </summary>
        /// <param name="position">The given posiiton.</param>
        /// <param name="deltaTime">The timestep.</param>
        public void DoFocus(Vector2 position, float deltaTime)
        {
            float x = BasePosition.x, y = BasePosition.y;

            if (position.x > transform.position.x)
                x += Mathf.Min(FocusSpeed.x*deltaTime, position.x - transform.position.x);
            else if (position.x < transform.position.x)
                x -= Mathf.Min(FocusSpeed.x*deltaTime, transform.position.x - position.x);

            if (position.y > transform.position.y)
                y += Mathf.Min(FocusSpeed.y*deltaTime, position.y - transform.position.y);
            else if (position.y < transform.position.y)
                y -= Mathf.Min(FocusSpeed.y*deltaTime, transform.position.y - position.y);
            
            const float ChangeThreshold = 0.005f;
            if (Mathf.Abs(x - BasePosition.x) < ChangeThreshold) x = BasePosition.x;
            if (Mathf.Abs(y - BasePosition.y) < ChangeThreshold) y = BasePosition.y;

            BasePosition = DoLevelBoundsCheck(new Vector2(x, y));
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

        public void Follow(Transform target)
        {
            Follow(Target, FollowSpeed);
        }

        public void Follow(Transform target, Vector2 followSpeed)
        {
            Follow(target, followSpeed, FollowBoundsMin, FollowBoundsMax);
        }

        public void Follow(Transform target, Vector2 followSpeed, Transform minBounds, Transform maxBounds)
        {
            State = CameraControllerState.Follow;
            Target = target;
            FollowTarget = true;
            FollowSpeed = followSpeed;
            FollowBoundsMin = minBounds;
            FollowBoundsMax = maxBounds;
        }

        public void Focus(Transform target)
        {
            Focus(target, FocusSpeed);
        }

        public void Focus(Transform target, Vector2 focusSpeed)
        {
            State = CameraControllerState.Focus;
            Target = target;
            FocusSpeed = focusSpeed;
        }

        /// <summary>
        /// Pans to the specified offset position over the specified duration, in seconds.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="seconds"></param>
        public void Pan(Vector2 offset, float seconds, float delay = 0f)
        {
            PanStart = PanOffset;
            PanEnd = offset;
            PanTimer = 0f;
            PanTime = seconds;

            PanDelayTimer = State == CameraControllerState.Pan ? Mathf.Min(PanDelayTimer, delay) : delay;
            State = CameraControllerState.Pan;
        }

        /// <summary>
        /// Pans to the specified offset position at the specified speed, in units per second.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="speed"></param>
        public void PanAtSpeed(Vector2 offset, float speed, float delay = 0f)
        {
            Pan(offset, (offset - PanOffset).magnitude/speed, delay);
        }
        
        /// <summary>
        /// Interrupts normal behavior for the specified duration.
        /// </summary>
        /// <param name="seconds">The specified duration, in seconds.</param>
        public void Wait(float seconds)
        {
            WaitTimer = seconds;
            State = CameraControllerState.Wait;
        }

        public void Follow()
        {
            State = CameraControllerState.Follow;
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
        /// Instantly travels to the target transform.
        /// </summary>
        public void Snap()
        {
            To(BasePosition = Target.position);
        }

        /// <summary>
        /// Moves the camera and based on its current state and changes state based on the controller's acive moves.
        /// </summary>
        protected void HandleState()
        {
            if (!Target) return;

            var t = Target.transform;
            if (State == CameraControllerState.Follow)
            {
                if(FollowTarget) DoFollow(t.position);
            } else if(State == CameraControllerState.Focus)
            {
                DoFocus(t.position);
            }
            else if (State == CameraControllerState.Pan)
            {
                if(FollowTarget) DoFollow(t.position);

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
                        PanOffset = Vector2.Lerp(PanStart, PanEnd, PanTimer / PanTime);
                    }
                    else
                    {
                        PanTimer = PanTime;
                        State = CameraControllerState.Follow;
                    }
                }
            }
            else if (State == CameraControllerState.Wait)
            {
                if ((WaitTimer -= Time.deltaTime) < 0f)
                {
                    WaitTimer = 0f;
                    State = CameraControllerState.Follow;
                }
            }
        }

        protected void HandlePosition()
        {
            var result = BasePosition;
            if (State != CameraControllerState.Focus) result += PanOffset + ExtraOffset;
            result = DoLevelBoundsCheck(result);
            To(result);
        }
    }
}
