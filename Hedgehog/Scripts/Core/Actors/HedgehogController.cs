using System.Collections.Generic;
using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Hedgehog.Core.Actors
{
    /// <summary>
    /// Controls the player.
    /// </summary>
    public class HedgehogController : MonoBehaviour
    {
        #region Inspector Fields
        #region Collision
        [Header("Collision")]

        [SerializeField]
        public CollisionMode CollisionMode = CollisionMode.Layers;

        /// <summary>
        /// The layer mask which represents the ground the player checks for collision with.
        /// </summary>
        [HideInInspector]
        public LayerMask TerrainMask;

        [SerializeField]
        public List<string> TerrainTags = new List<string>();

        [SerializeField]
        public List<string> TerrainNames = new List<string>();
        #endregion
        #region Controls
        [Header("Controls")]

        [SerializeField]
        public KeyCode JumpKey = KeyCode.W;

        [SerializeField]
        public KeyCode LeftKey = KeyCode.A;

        [SerializeField]
        public KeyCode RightKey = KeyCode.D;

        [SerializeField]
        public KeyCode DebugSpindashKey = KeyCode.Space;
        #endregion
        #region Sensors

        [Header("Sensors")]
        [SerializeField] public Transform SensorSurfaceLeft;
        [SerializeField] public Transform SensorSurfaceRight;
        [SerializeField] public Transform SensorLedgeLeft;
        [SerializeField] public Transform SensorLedgeRight;

        [SerializeField] public Transform SensorBottomLeft;
        [SerializeField] public Transform SensorBottomMiddle;
        [SerializeField] public Transform SensorBottomRight;

        [SerializeField] public Transform SensorMiddleLeft;
        [SerializeField] public Transform SensorMiddleMiddle;
        [SerializeField] public Transform SensorMiddleRight;

        [SerializeField] public Transform SensorTopLeft;
        [SerializeField] public Transform SensorTopMiddle;
        [SerializeField] public Transform SensorTopRight;
        #endregion
        #region Physics
        [Header("Physics")]

        /// <summary>
        /// The player's ground acceleration in units per second per second.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 0.25f)]
        [Tooltip("Ground acceleration in units per second squared.")]
        public float GroundAcceleration = 0.08f;

        /// <summary>
        /// The player's friction on the ground in units per second per second.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 0.25f)]
        [Tooltip("Ground deceleration in units per second squared.")]
        public float GroundDeceleration = 0.12f;

        /// <summary>
        /// The player's braking speed in units per second per second.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 1.00f)]
        [Tooltip("Ground braking speed in units per second squared.")]
        public float GroundBrake = 0.5f;

        /// <summary>
        /// The player's horizontal acceleration in the air in units per second per second.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 0.25f)]
        [Tooltip("Air acceleration in units per second squared.")]
        public float AirAcceleration = 0.16f;

        /// <summary>
        /// Minimum horizontal speed requirement for air drag.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 20.0f)]
        [Tooltip("Horizontal speed requirement for air drag.")]
        public float AirDragHorizontalSpeed = 0.25f;

        /// <summary>
        /// Minimum vertical speed requirement for air drag.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 20.0f)]
        [Tooltip("Vertical speed requirement for air drag.")]
        public float AirDragVerticalSpeed = 4.8f;

        /// <summary>
        /// This coefficient is applied to horizontal speed when horizontal and vertical speed
        /// requirements are met.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 1.0f)]
        [Tooltip("Air drag coefficient applied if horizontal speed is greater than air drag speed.")]
        public float AirDragCoefficient = 0.9738896f;

        /// <summary>
        /// The speed of the player's jump in units per second.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 25.0f)]
        [Tooltip("Jump speed in units per second.")]
        public float JumpSpeed = 8.0f;

        [SerializeField]
        [Range(0.0f, 25.0f)]
        [Tooltip("The maximum vertical speed the player can have after releasing the jump button.")]
        public float ReleaseJumpSpeed = 5.0f;

        /// <summary>
        /// The acceleration by gravity in units per second per second.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 1.0f)]
        [Tooltip("Acceleration in the air, in the downward direction, in units per second squared.")]
        public float AirGravity = 0.3f;

        /// <summary>
        /// The magnitude of the force applied to the player when going up or down slopes.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 1.0f)]
        [Tooltip("Acceleration on downward slopes, the maximum being this value, in units per second squared")]
        public float SlopeGravity = 0.3f;

        /// <summary>
        /// The player's top speed, the maximum it can attain by running, it units per second.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 100.0f)]
        [Tooltip("Maximum speed achieved through running in units per second.")]
        public float TopSpeed = 20.0f;

        /// <summary>
        /// The player's maximum speed in units per second.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 100.0f)]
        [Tooltip("Maximum speed in units per second.")]
        public float MaxSpeed = 20.0f;

        [Header("Physics - Uncommon")]

        /// <summary>
        /// The maximum height of a ledge above the player's feet that it can climb without hindrance.
        /// </summary>
        [SerializeField]
        public float LedgeClimbHeight = 0.25f;

        /// <summary>
        /// The maximum depth of a surface below the player's feet that it can drop to without hindrance.
        /// </summary>
        [SerializeField]
        public float LedgeDropHeight = 0.25f;

        /// <summary>
        /// The minimum speed in units per second the player must be moving at to stagger each physics update,
        /// processing the movement in fractions.
        /// </summary>
        [SerializeField]
        public float AntiTunnelingSpeed = 5.0f;

        /// <summary>
        /// The minimum angle of an incline at which slope gravity is applied.
        /// </summary>
        [SerializeField]
        public float SlopeGravityBeginAngle = 10.0f;

        /// <summary>
        /// The speed in units per second below which the player must be traveling on a wall or ceiling to be
        /// detached from it.
        /// </summary>
        [SerializeField]
        public float DetachSpeed = 3.0f;

        /// <summary>
        /// The duration in seconds of the horizontal lock.
        /// </summary>
        [SerializeField]
        public float HorizontalLockTime = 0.25f;

        /// <summary>
        /// If the player is moving very quickly and jumps, normally it will not make much of a difference
        /// and the player will usually end up re-attaching to the surface.
        /// 
        /// With this constant, the player is forced to leave the ground by at least the specified angle
        /// difference, in degrees.
        /// </summary>
        [SerializeField]
        public float ForceJumpAngleDifference = 30.0f;

        [Header("Physics - Advanced")]

        /// <summary>
        /// The maximum change in angle between two surfaces that the player can walk in.
        /// </summary>
        [SerializeField]
        public float MaxSurfaceAngleDifference = 70.0f;

        /// <summary>
        /// The amount in degrees past the threshold of changing wall mode that the player
        /// can go.
        /// </summary>
        [SerializeField]
        public float HorizontalWallmodeAngleWeight = 0.0f;

        /// <summary>
        /// The speed in units per second at which the player must be moving to be able to switch wall modes.
        /// </summary>
        [SerializeField]
        public float MinWallmodeSwitchSpeed = 0.5f;

        /// <summary>
        /// The minimum difference in angle between the surface sensor and the overlap sensor
        /// to have the player's rotation account for it.
        /// </summary>
        [SerializeField]
        public float MinOverlapAngle = -40.0f;

        /// <summary>
        /// The minimum absolute difference in angle between the surface sensor and the overlap
        /// sensor to have the player's rotation account for it.
        /// </summary>
        [SerializeField]
        public float MinFlatOverlapRange = 7.5f;

        /// <summary>hor
        /// The minimum angle of an incline from a ceiling or left or right wall for a player to be able to
        /// attach to it from the air.
        /// </summary>
        [SerializeField]
        public float MinFlatAttachAngle = 5.0f;

        /// <summary>
        /// The maximum surface angle difference from a vertical wall at which a player is able to detach from
        /// through use of the directional key opposite to the one in which it is traveling.
        /// </summary>
        [SerializeField]
        public float MaxVerticalDetachAngle = 5.0f;
        #endregion
        #region Events
        [SerializeField]
        public UnityEvent OnCrush;
        #endregion
        #endregion
        #region Input Variables
        /// <summary>
        /// Whether the left key was held down since the last update. Key is determined by LeftKey.
        /// </summary>
        [HideInInspector]
        public bool LeftKeyDown;

        /// <summary>
        /// Whether the right key was held down since the last update. Key is determined by RightKey.
        /// </summary>
        [HideInInspector]
        public bool RightKeyDown;

        /// <summary>
        /// Whether the jump key was pressed since the last update. Key is determined by JumpKey.
        /// </summary>
        [HideInInspector]
        public bool JumpKeyPressed;

        /// <summary>
        /// Whether the jump key was held down since the last update. Key is determined by JumpKey.
        /// </summary>
        [HideInInspector]
        public bool JumpKeyDown;

        /// <summary>
        /// Whether the jump key was released since the last update.
        /// </summary>
        [HideInInspector]
        public bool JumpKeyReleased;

        /// <summary>
        /// Temporary. Whether the debug spindash key was pressed since the last update. Key is
        /// determined by DebugSpindashKey.
        /// </summary>
        [HideInInspector]
        public bool DebugSpindashKeyDown;
        #endregion
        #region Physics Variables
        /// <summary>
        /// The player's velocity as a Vector2. Setting velocity this way will detach
        /// the player from whatever surface it is on.
        /// </summary>
        public Vector2 Velocity
        {
            get { return new Vector2(Vx, Vy); }
            set { Detach(); Vx = value.x; Vy = value.y; }
        }

        /// <summary>
        /// The player's velocity on the ground; the faster it's running, the higher in magnitude
        /// this number is. If it's moving forward (counter-clockwise inside a loop), this is positive.
        /// If backwards (clockwise inside a loop), negative.
        /// </summary>
        public float GroundVelocity
        {
            get { return Vg; }
            set { Vg = value; }
        }

        /// <summary>
        /// The player's horizontal velocity in units per second.
        /// </summary>
        [HideInInspector]
        public float Vx;

        /// <summary>
        /// The player's vertical velocity in units per second.
        /// </summary>
        [HideInInspector]
        public float Vy;

        /// <summary>
        /// If grounded, the player's ground velocity in units per second.
        /// </summary>
        [HideInInspector]
        public float Vg;

        /// <summary>
        /// Whether the player is touching the ground.
        /// </summary>
        /// <value><c>true</c> if grounded; otherwise, <c>false</c>.</value>
        [HideInInspector]
        public bool Grounded;

        /// <summary>
        /// Whether the player has just landed on the ground. Is used to ignore surface angle
        /// once right after.
        /// </summary>
        [HideInInspector]
        private bool _justLanded;

        /// <summary>
        /// If grounded and hasn't just landed, the angle of incline the player walked on
        /// one FixedUpdate ago.
        /// </summary>
        [HideInInspector]
        public float LastSurfaceAngle;

        /// <summary>
        /// If grounded, the angle of incline the player is walking on. Goes hand-in-hand
        /// with rotation.
        /// </summary>
        [HideInInspector]
        public float SurfaceAngle;

        /// <summary>
        /// If grounded, which sensor on the player defines the primary surface.
        /// </summary>
        [HideInInspector]
        public Footing Footing;

        /// <summary>
        /// Whether the player has control of horizontal ground movement.
        /// </summary>
        [HideInInspector]
        public bool HorizontalLock;

        /// <summary>
        /// If horizontally locked, the time in seconds left on it.
        /// </summary>
        [HideInInspector]
        public float HorizontalLockTimer;

        /// <summary>
        /// If not grounded, whether to activate the horizontal control lock when the player lands.
        /// </summary>
        [HideInInspector]
        public bool LockUponLanding;

        /// <summary>
        /// Whether the player has just jumped. Is used to avoid collisions right after.
        /// </summary>
        [HideInInspector]
        public bool JustJumped;

        /// <summary>
        /// Whether the player can release the jump key to reduce vertical speed.
        /// </summary>
        [HideInInspector]
        public bool CanReleaseJump;

        /// <summary>
        /// Whether the player has just detached. Is used to avoid reattachments right after.
        /// </summary>
        [HideInInspector]
        public bool JustDetached;

        /// <summary>
        /// Represents the current Wallmode of the player.
        /// </summary>
        [HideInInspector]
        public Orientation Wallmode;

        /// <summary>
        /// If grounded, the surface which is currently defining the controller's position
        /// and rotation.
        /// </summary>
        [HideInInspector]
        public Transform PrimarySurface;

        /// <summary>
        /// The results from the terrain cast which found the primary surface, if any.
        /// </summary>
        public TerrainCastHit PrimarySurfaceHit;

        /// <summary>
        /// The surface which is currently partially defining the controller's rotation, if any.
        /// </summary>
        [HideInInspector]
        public Transform SecondarySurface;

        /// <summary>
        /// The results from the terrain cast which found the secondary surface, if any.
        /// </summary>
        public TerrainCastHit SecondarySurfaceHit;

        private Vector3 _queuedTranslate;
        #endregion

        #region Lifecycle Functions
        public void Awake()
        {
            Footing = Footing.None;
            Grounded = false;
            Vx = Vy = Vg = 0.0f;
            LastSurfaceAngle = 0.0f;
            LeftKeyDown = RightKeyDown = JumpKeyPressed = DebugSpindashKeyDown = false;
            JustJumped = _justLanded = JustDetached = false;
            Wallmode = Orientation.Floor;
            _queuedTranslate = default(Vector3);
        }

        public void Update()
        {
            GetInput();
        }

        public void FixedUpdate()
        {
            HandleInput(Time.fixedDeltaTime);

            // Stagger routine - if the player's gotta go fast, move it in increments of AntiTunnelingSpeed
            // to prevent tunneling.
            var vt = Mathf.Sqrt(Vx * Vx + Vy * Vy);

            if (vt < AntiTunnelingSpeed)
            {
                HandleForces();
                transform.position = new Vector2(transform.position.x + (Vx * Time.fixedDeltaTime), transform.position.y + (Vy * Time.fixedDeltaTime));
                HandleCollisions();
            }
            else
            {
                var vc = vt;
                while (vc > 0.0f)
                {
                    if (vc > AntiTunnelingSpeed)
                    {
                        HandleForces(Time.fixedDeltaTime * (AntiTunnelingSpeed / vt));
                        transform.position += (new Vector3(Vx * Time.fixedDeltaTime, Vy * Time.fixedDeltaTime)) * (AntiTunnelingSpeed / vt);
                        vc -= AntiTunnelingSpeed;
                    }
                    else
                    {
                        HandleForces(Time.fixedDeltaTime * (vc / vt));
                        transform.position += (new Vector3(Vx * Time.fixedDeltaTime, Vy * Time.fixedDeltaTime)) * (vc / vt);
                        vc = 0.0f;
                    }

                    HandleCollisions();

                    // If the player's speed changes mid-stagger recalculate current velocity and total velocity
                    var vn = Mathf.Sqrt(Vx * Vx + Vy * Vy);
                    vc *= vn / vt;
                    vt *= vn / vt;
                }
            }

            if (_queuedTranslate != default(Vector3))
            {
                transform.Translate(_queuedTranslate);
                _queuedTranslate = default(Vector3);
                HandleCollisions();
            }
        }
        #endregion

        #region Lifecycle Subroutines
        /// <summary>
        /// Stores keyboard input for the next fixed update (and HandleInput).
        /// </summary>
        private void GetInput()
        {
            LeftKeyDown = Input.GetKey(LeftKey);
            RightKeyDown = Input.GetKey(RightKey);
            
            if(!JumpKeyPressed) JumpKeyPressed = Input.GetKeyDown(JumpKey);

            if (Grounded)
                JumpKeyReleased = false;
            else if (!JumpKeyReleased && JumpKeyDown && !Input.GetKey(JumpKey))
                JumpKeyReleased = true;

            JumpKeyDown = Input.GetKey(JumpKey);

            if (Grounded) DebugSpindashKeyDown = Input.GetKey(DebugSpindashKey);
        }

        /// <summary>
        /// Handles the input from the previous update using Time.fixedDeltaTime as the timestep.
        /// </summary>
        private void HandleInput()
        {
            HandleInput(Time.fixedDeltaTime);
        }

        /// <summary>
        /// Handles the input from the previous update.
        /// </summary>
        private void HandleInput(float timestep)
        {
            if (Grounded)
            {
                if (DebugSpindashKeyDown)
                {
                    GroundVelocity = MaxSpeed*Mathf.Sign(GroundVelocity);
                    DebugSpindashKeyDown = false;
                }

                if (HorizontalLock)
                {
                    HorizontalLockTimer -= timestep;
                    if (HorizontalLockTimer < 0.0f)
                    {
                        HorizontalLock = false;
                        HorizontalLockTimer = 0.0f;
                    }
                }

                if (JumpKeyPressed)
                {
                    Jump();
                    JumpKeyPressed = false;
                }

                if (LeftKeyDown && !HorizontalLock)
                {
                    if (Vg > 0 && Mathf.Abs(DMath.AngleDiffd(SurfaceAngle, 90.0f)) < MaxVerticalDetachAngle)
                    {
                        Vx = 0;
                        Detach();
                    }
                    else
                    {
                        if (Vg > 0.0f)
                        {
                            Vg -= GroundBrake*timestep;
                            if (Vg < 0.0f) Vg -= GroundAcceleration *timestep;
                        }
                        else if (Vg > -TopSpeed)
                        {
                            Vg -= GroundAcceleration*timestep;
                        }
                    }
                }
                else if (RightKeyDown && !HorizontalLock)
                {
                    if (Vg < 0 && Mathf.Abs(DMath.AngleDiffd(SurfaceAngle, 270.0f)) < MaxVerticalDetachAngle)
                    {
                        Vx = 0;
                        Detach();
                    }
                    else
                    {
                        if (Vg < 0.0f)
                        {
                            Vg += GroundBrake*timestep;
                            if (Vg > 0.0f) Vg += GroundAcceleration*timestep;
                        }
                        else if (Vg < TopSpeed)
                        {
                            Vg += GroundAcceleration*timestep;
                        }
                    }
                }
            }
            else
            {
                if (LeftKeyDown) Vx -= AirAcceleration*timestep;
                else if (RightKeyDown) Vx += AirAcceleration*timestep;

                if (JumpKeyPressed) JumpKeyPressed = false;
                if (JumpKeyReleased)
                {
                    ReleaseJump();
                    JumpKeyReleased = false;
                }
            }
        }

        /// <summary>
        /// Uses Time.fixedDeltaTime as the timestep. Applies forces on the player and also handles speed-based conditions,
        /// such as detaching the player if it is too slow on an incline.
        /// </summary>
        private void HandleForces()
        {
            HandleForces(Time.fixedDeltaTime);
        }

        /// <summary>
        /// Applies forces on the player and also handles speed-based conditions, such as detaching the player if it is too slow on
        /// an incline.
        /// </summary>
        private void HandleForces(float timestep)
        {
            if (Grounded)
            {
                // Friction from deceleration
                if (HorizontalLock || (!LeftKeyDown && !RightKeyDown))
                {
                    if (DMath.Equalsf(Vg) && Mathf.Abs(Vg) < GroundDeceleration)
                    {
                        Vg = 0.0f;
                    }
                    else if (Vg > 0.0f)
                    {
                        Vg -= GroundDeceleration*timestep;
                        if (Vg < 0.0f) Vg = 0.0f;
                    }
                    else if (Vg < 0.0f)
                    {
                        Vg += GroundDeceleration*timestep;
                        if (Vg > 0.0f) Vg = 0.0f;
                    }
                }

                // Slope gravity
                if (Mathf.Abs(DMath.AngleDiffd(SurfaceAngle, 0.0f)) > SlopeGravityBeginAngle)
                {
                    Vg -= SlopeGravity*Mathf.Sin(SurfaceAngle*Mathf.Deg2Rad)*timestep;
                }

                // Speed limit
                if (Vg > MaxSpeed) Vg = MaxSpeed;
                else if (Vg < -MaxSpeed) Vg = -MaxSpeed;

                // Detachment from walls if speed is too low
                if (Wallmode != Orientation.Floor && Mathf.Abs(Vg) < DetachSpeed)
                {
                    if (SurfaceAngle > 90.0f - MaxVerticalDetachAngle &&
                        SurfaceAngle < 270.0f + MaxVerticalDetachAngle)
                    {
                        Detach(true);
                    }
                    else
                    {
                        LockHorizontal();
                    }
                }

               
            }
            else
            {
                Vy -= AirGravity * timestep;
                if (Vy > AirDragVerticalSpeed && Mathf.Abs(Vx) > AirDragHorizontalSpeed)
                {
                    Vx *= Mathf.Pow(AirDragCoefficient, timestep);
                }

                //transform.eulerAngles = new Vector3(0.0f, 0.0f, Mathf.LerpAngle(transform.eulerAngles.z, 0.0f, Time.deltaTime * 5.0f));
                transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
            }
        }

        /// <summary>
        /// Checks for collision with all sensors, changing position, velocity, and rotation if necessary. This should be called each time
        /// the player changes position. This method does not require a timestep because it only resolves overlaps in the player's collision.
        /// </summary>
        public void HandleCollisions(bool triggerEvents = true)
        {
            var anyHit = false;
            var jumpedPreviousCheck = JustJumped;

            if(CrushCheck()) OnCrush.Invoke();

            if (!Grounded)
            {
                SurfaceAngle = 0.0f;
                anyHit = AirSideCheck() | AirCeilingCheck() | AirGroundCheck();
            }

            if (Grounded)
            {
                anyHit = GroundSideCheck() | GroundCeilingCheck() | GroundSurfaceCheck(triggerEvents);
                if (!SurfaceAngleCheck()) Detach();
            }

            if (JustJumped && jumpedPreviousCheck) JustJumped = false;

            if (!anyHit && JustDetached)
                JustDetached = false;
        }
        #endregion
        #region Collision Subroutines
        /// <summary>
        /// Collision check with side sensors for when player is in the air.
        /// </summary>
        /// <returns><c>true</c>, if a collision was found, <c>false</c> otherwise.</returns>
        private bool AirSideCheck()
        {
            var sideLeftCheck = this.TerrainCast(SensorMiddleMiddle.position, SensorMiddleLeft.position,
                TerrainSide.Left);
            var sideRightCheck = this.TerrainCast(SensorMiddleMiddle.position, SensorMiddleRight.position,
                TerrainSide.Right);

            if (sideLeftCheck)
            {
                if (!JustJumped)
                {
                    Vx = 0;
                }

                transform.position += (Vector3)sideLeftCheck.Hit.point - SensorMiddleLeft.position +
                                      ((Vector3)sideLeftCheck.Hit.point - SensorMiddleLeft.position).normalized * DMath.Epsilon;
                return true;
            }
            if (sideRightCheck)
            {
                if (!JustJumped)
                {
                    Vx = 0;
                }

                transform.position += (Vector3)sideRightCheck.Hit.point - SensorMiddleRight.position +
                                      ((Vector3)sideRightCheck.Hit.point - SensorMiddleRight.position)
                                      .normalized * DMath.Epsilon;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Collision check with air sensors for when player is in the air.
        /// </summary>
        /// <returns><c>true</c>, if a collision was found, <c>false</c> otherwise.</returns>
        private bool AirCeilingCheck()
        {
            var leftCheck = this.TerrainCast(SensorMiddleMiddle.position, SensorTopLeft.position);

            if (leftCheck)
            {
                var horizontalCheck = this.TerrainCast(SensorTopMiddle.position, SensorTopLeft.position,
                    TerrainSide.Left);
                var verticalCheck = this.TerrainCast(SensorMiddleLeft.position, SensorTopLeft.position,
                    TerrainSide.Top);

                if (horizontalCheck || verticalCheck)
                {
                    if (Vector2.Distance(horizontalCheck.Hit.point, SensorTopLeft.position) <
                    Vector2.Distance(verticalCheck.Hit.point, SensorTopLeft.position))
                    {
                        transform.position += (Vector3)horizontalCheck.Hit.point - SensorTopLeft.position;

                        if (!JustDetached) HandleImpact(horizontalCheck);
                        if (Vy > 0) Vy = 0;
                    }
                    else
                    {
                        transform.position += (Vector3)verticalCheck.Hit.point - SensorTopLeft.position;

                        if (!JustDetached) HandleImpact(verticalCheck);
                        if (Vy > 0) Vy = 0;
                    }

                    return true;
                }


            }

            var rightCheck = this.TerrainCast(SensorMiddleMiddle.position, SensorTopRight.position);

            if (rightCheck)
            {
                var horizontalCheck = this.TerrainCast(SensorTopMiddle.position, SensorTopRight.position,
                    TerrainSide.Right);
                var verticalCheck = this.TerrainCast(SensorMiddleRight.position, SensorTopRight.position,
                    TerrainSide.Top);

                if (horizontalCheck || verticalCheck)
                {
                    if (Vector2.Distance(horizontalCheck.Hit.point, SensorTopRight.position) <
                    Vector2.Distance(verticalCheck.Hit.point, SensorTopRight.position))
                    {
                        transform.position += (Vector3)horizontalCheck.Hit.point - SensorTopRight.position;

                        if (!JustDetached) HandleImpact(horizontalCheck);
                        if (Vy > 0) Vy = 0;
                    }
                    else
                    {
                        transform.position += (Vector3)verticalCheck.Hit.point - SensorTopRight.position;

                        if (!JustDetached) HandleImpact(verticalCheck);
                        if (Vy > 0) Vy = 0;
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Collision check with ground sensors for when player is in the air.
        /// </summary>
        /// <returns><c>true</c>, if a collision was found, <c>false</c> otherwise.</returns>
        private bool AirGroundCheck()
        {
            var groundLeftCheck = Groundcast(Footing.Left);
            var groundRightCheck = Groundcast(Footing.Right);

            if (groundLeftCheck || groundRightCheck)
            {
                if (JustJumped)
                {
                    if (groundLeftCheck)
                    {
                        transform.position += (Vector3)groundLeftCheck.Hit.point - SensorBottomLeft.position;
                    }
                    if (groundRightCheck)
                    {
                        transform.position += (Vector3)groundRightCheck.Hit.point - SensorBottomRight.position;
                    }
                }
                else
                {
                    if (groundLeftCheck && groundRightCheck)
                    {
                        if (groundLeftCheck.Hit.point.y > groundRightCheck.Hit.point.y)
                        {
                            HandleImpact(groundLeftCheck);
                        }
                        else
                        {
                            HandleImpact(groundRightCheck);
                        }
                    }
                    else if (groundLeftCheck)
                    {
                        HandleImpact(groundLeftCheck);
                    }
                    else
                    {
                        HandleImpact(groundRightCheck);
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Collision check with side sensors for when player is on the ground.
        /// </summary>
        /// <returns><c>true</c>, if a collision was found, <c>false</c> otherwise.</returns>
        private bool GroundSideCheck()
        {
            var sideLeftCheck = this.TerrainCast(SensorMiddleMiddle.position, SensorMiddleLeft.position, TerrainSide.Left);
            var sideRightCheck = this.TerrainCast(SensorMiddleMiddle.position, SensorMiddleRight.position, TerrainSide.Right);

            if (sideLeftCheck)
            {
                Vg = 0;
                transform.position += (Vector3)sideLeftCheck.Hit.point - SensorMiddleLeft.position +
                                      ((Vector3)sideLeftCheck.Hit.point - SensorMiddleLeft.position).normalized * DMath.Epsilon;

                // If running down a wall and hits the floor, orient the player onto the floor
                if (Wallmode == Orientation.Right)
                {
                    DMath.RotateBy(transform, -90.0f);
                    Wallmode = Orientation.Floor;
                }

                return true;
            }
            if (sideRightCheck)
            {
                Vg = 0;
                transform.position += (Vector3)sideRightCheck.Hit.point - SensorMiddleRight.position +
                                      ((Vector3)sideRightCheck.Hit.point - SensorMiddleRight.position)
                                      .normalized * DMath.Epsilon;

                // If running down a wall and hits the floor, orient the player onto the floor
                if (Wallmode == Orientation.Left)
                {
                    DMath.RotateTo(transform, 90.0f);
                    Wallmode = Orientation.Floor;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Collision check with ceiling sensors for when player is on the ground.
        /// </summary>
        /// <returns><c>true</c>, if a collision was found, <c>false</c> otherwise.</returns>
        private bool GroundCeilingCheck()
        {
            var ceilLeftCheck = this.TerrainCast(SensorTopMiddle.position, SensorTopLeft.position, TerrainSide.Left);
            var ceilRightCheck = this.TerrainCast(SensorTopMiddle.position, SensorTopRight.position, TerrainSide.Right);

            if (ceilLeftCheck)
            {
                Vg = 0;

                // Add epsilon to prevent sticky collisions
                transform.position += (Vector3)ceilLeftCheck.Hit.point - SensorTopLeft.position +
                                      ((Vector3)ceilLeftCheck.Hit.point - SensorTopLeft.position).normalized * DMath.Epsilon;

                return true;
            }
            if (ceilRightCheck)
            {
                Vg = 0;
                transform.position += (Vector3)ceilRightCheck.Hit.point - SensorTopRight.position +
                                      ((Vector3)ceilRightCheck.Hit.point - SensorTopRight.position)
                                      .normalized * DMath.Epsilon;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Collision check with ground sensors for when player is on the ground.
        /// </summary>
        /// <returns><c>true</c>, if a collision was found, <c>false</c> otherwise.</returns>
        private bool GroundSurfaceCheck(bool triggerEvents = true)
        {
            var s = GetSurface(TerrainMask);
            if (s.LeftCast || s.RightCast)
            {
                // If both sensors found surfaces, need additional checks to see if rotation needs to account for both their positions
                if (s.LeftCast && s.RightCast)
                {
                    // Calculate angle changes for tolerance checks
                    var rightDiff = DMath.AngleDiffd(s.RightCast.SurfaceAngle * Mathf.Rad2Deg, LastSurfaceAngle);
                    var leftDiff = DMath.AngleDiffd(s.LeftCast.SurfaceAngle * Mathf.Rad2Deg, LastSurfaceAngle);
                    var overlapDiff = DMath.AngleDiffr(s.LeftCast.SurfaceAngle, s.RightCast.SurfaceAngle) * Mathf.Rad2Deg;

                    var overlapSurfaceAngle = DMath.Angle(s.RightCast.Hit.point - s.LeftCast.Hit.point);
                    // = difference between the angle of a line drawn between the surfaces minus the average of 
                    //   their surface angles
                    var overlapSurfaceDiff = DMath.AngleDiffr(overlapSurfaceAngle,
                        (s.LeftCast.SurfaceAngle + s.RightCast.SurfaceAngle)/2.0f)*Mathf.Rad2Deg;
                    if (s.Side == Footing.Left)
                    {
                        // If the surface's angle is a small enough difference from that of the previous begin surface checks
                        if (_justLanded || Mathf.Abs(leftDiff) < MaxSurfaceAngleDifference)
                        {
                            // Check angle differences between feet for player rotation
                            if (overlapDiff > MinOverlapAngle && Mathf.Abs(overlapSurfaceDiff) < MinFlatOverlapRange || 
                                Mathf.Abs(overlapSurfaceDiff) > 135.0f)
                            {
                                // If tolerable, rotate between the surfaces beneath the two feet
                                DMath.RotateTo(transform, overlapSurfaceAngle);
                                transform.position += (Vector3)s.LeftCast.Hit.point - SensorBottomLeft.position;
                                Footing = Footing.Left;
                                SetSurface(s.LeftCast, s.RightCast, triggerEvents);
                            }
                            else
                            {
                                // Else just rotate for the left foot
                                DMath.RotateTo(transform, s.LeftCast.SurfaceAngle);
                                transform.position += (Vector3)s.LeftCast.Hit.point - SensorBottomLeft.position;
                                Footing = Footing.Left;
                                SetSurface(s.LeftCast, null, triggerEvents);
                            }
                        }
                        else if (Mathf.Abs(rightDiff) < MaxSurfaceAngleDifference)
                        {
                            // Else see if the other surface's angle is tolerable
                            DMath.RotateTo(transform, s.RightCast.SurfaceAngle);
                            transform.position += (Vector3)s.RightCast.Hit.point - SensorBottomRight.position;
                            Footing = Footing.Right;
                            SetSurface(s.RightCast, null, triggerEvents);
                        }
                        else
                        {
                            // Else the surfaces are untolerable. detach from the surface
                            Detach(false, triggerEvents);
                        }

                        // Same thing but with the other foot
                    }
                    else if (s.Side == Footing.Right)
                    {
                        if (_justLanded || Mathf.Abs(rightDiff) < MaxSurfaceAngleDifference)
                        {
                            if (overlapDiff > MinOverlapAngle && Mathf.Abs(overlapSurfaceDiff) < MinFlatOverlapRange || 
                                Mathf.Abs(overlapSurfaceDiff) > 135.0f)
                            {
                                DMath.RotateTo(transform, overlapSurfaceAngle);
                                transform.position += (Vector3)s.RightCast.Hit.point - SensorBottomRight.position;
                                Footing = Footing.Right;
                                SetSurface(s.RightCast, s.LeftCast, triggerEvents);
                            }
                            else
                            {
                                DMath.RotateTo(transform, s.RightCast.SurfaceAngle);
                                transform.position += (Vector3)s.RightCast.Hit.point - SensorBottomRight.position;
                                Footing = Footing.Right;
                                SetSurface(s.RightCast, null, triggerEvents);
                            }

                        }
                        else if (Mathf.Abs(leftDiff) < MaxSurfaceAngleDifference)
                        {
                            DMath.RotateTo(transform, s.LeftCast.SurfaceAngle);
                            transform.position += (Vector3)s.LeftCast.Hit.point - SensorBottomLeft.position;
                            Footing = Footing.Left;
                            SetSurface(s.LeftCast, null, triggerEvents);
                        }
                        else
                        {
                            Detach(false, triggerEvents);
                        }
                    }
                }
                else if (s.LeftCast)
                {
                    var leftDiff = DMath.AngleDiffd(s.LeftCast.SurfaceAngle * Mathf.Rad2Deg, LastSurfaceAngle);
                    if (_justLanded || Mathf.Abs(leftDiff) < MaxSurfaceAngleDifference)
                    {
                        DMath.RotateTo(transform, s.LeftCast.SurfaceAngle);
                        transform.position += (Vector3)s.LeftCast.Hit.point - SensorBottomLeft.position;
                        Footing = Footing.Left;
                        SetSurface(s.LeftCast, null, triggerEvents);
                    }
                    else
                    {
                        Detach(false, triggerEvents);
                    }
                }
                else
                {
                    var rightDiff = DMath.AngleDiffd(s.RightCast.SurfaceAngle * Mathf.Rad2Deg, LastSurfaceAngle);
                    if (_justLanded || Mathf.Abs(rightDiff) < MaxSurfaceAngleDifference)
                    {
                        DMath.RotateTo(transform, s.RightCast.SurfaceAngle);
                        transform.position += (Vector3)s.RightCast.Hit.point - SensorBottomRight.position;
                        Footing = Footing.Right;
                        SetSurface(s.RightCast, null, triggerEvents);
                    }
                    else
                    {
                        Detach(false, triggerEvents);
                    }
                }

                return true;
            }

            Detach(false, triggerEvents);
            return false;
        }

        /// <summary>
        /// Check for changes in angle of incline for when player is on the ground.
        /// </summary>
        /// <returns><c>true</c> if the angle of incline is tolerable, <c>false</c> otherwise.</returns>
        private bool SurfaceAngleCheck()
        {
            if (_justLanded)
            {
                SurfaceAngle = transform.eulerAngles.z;
                LastSurfaceAngle = SurfaceAngle;
            }
            else
            {
                LastSurfaceAngle = SurfaceAngle;
                SurfaceAngle = transform.eulerAngles.z;
            }

            // Can only stay on the surface if angle difference is low enough
            if (Grounded && (_justLanded ||
                             Mathf.Abs(DMath.AngleDiffd(LastSurfaceAngle, SurfaceAngle)) < MaxSurfaceAngleDifference))
            {
                if (Wallmode == Orientation.Floor)
                {
                    if (SurfaceAngle > 45.0f + HorizontalWallmodeAngleWeight && SurfaceAngle < 180.0f) Wallmode = Orientation.Right;
                    else if (SurfaceAngle < 315.0f - HorizontalWallmodeAngleWeight && SurfaceAngle > 180.0f) Wallmode = Orientation.Left;
                }
                else if (Wallmode == Orientation.Right)
                {
                    if (SurfaceAngle > 135.0f + HorizontalWallmodeAngleWeight) Wallmode = Orientation.Ceiling;
                    else if (SurfaceAngle < 45.0f - HorizontalWallmodeAngleWeight) Wallmode = Orientation.Floor;
                }
                else if (Wallmode == Orientation.Ceiling)
                {
                    if (SurfaceAngle > 225.0f + HorizontalWallmodeAngleWeight) Wallmode = Orientation.Left;
                    else if (SurfaceAngle < 135.0f - HorizontalWallmodeAngleWeight) Wallmode = Orientation.Right;
                }
                else if (Wallmode == Orientation.Left)
                {
                    if (SurfaceAngle > 315.0f + HorizontalWallmodeAngleWeight || SurfaceAngle < 180.0f) Wallmode = Orientation.Floor;
                    else if (SurfaceAngle < 225.0f - HorizontalWallmodeAngleWeight) Wallmode = Orientation.Ceiling;
                }

                Vx = Vg * Mathf.Cos(SurfaceAngle * Mathf.Deg2Rad);
                Vy = Vg * Mathf.Sin(SurfaceAngle * Mathf.Deg2Rad);

                _justLanded = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks for terrain hitting both horizontal or both vertical sides of a player, aka crushing.
        /// </summary>
        /// <returns></returns>
        private bool CrushCheck()
        {
            return (this.TerrainCast(SensorMiddleMiddle.position, SensorMiddleLeft.position,
                        TerrainSide.Left) &&
                    this.TerrainCast(SensorMiddleMiddle.position, SensorMiddleRight.position,
                        TerrainSide.Right)) ||

                   (this.TerrainCast(SensorMiddleMiddle.position, SensorTopMiddle.position,
                       TerrainSide.Top) &&
                    this.TerrainCast(SensorMiddleMiddle.position, SensorBottomMiddle.position,
                        TerrainSide.Bottom));
        }
        #endregion

        #region Control Functions
        /// <summary>
        /// Locks the player's horizontal control on the ground for the time specified by HorizontalLockTime.
        /// </summary>
        private void LockHorizontal()
        {
            HorizontalLock = true;
            HorizontalLockTimer = HorizontalLockTime;
        }

        public void Jump()
        {
            JustJumped = true;

            // Forces the player to leave the ground using the constant ForceJumpAngleDifference.
            // Helps prevent sticking to surfaces when the player's gotta go fast.
            var originalAngle = DMath.Modp(DMath.Angle(new Vector2(Vx, Vy)) * Mathf.Rad2Deg, 360.0f);

            var surfaceNormal = (SurfaceAngle + 90.0f) * Mathf.Deg2Rad;
            Vx += JumpSpeed * Mathf.Cos(surfaceNormal);
            Vy += JumpSpeed * Mathf.Sin(surfaceNormal);

            var newAngle = DMath.Modp(DMath.Angle(new Vector2(Vx, Vy)) * Mathf.Rad2Deg, 360.0f);
            var angleDifference = DMath.AngleDiffd(originalAngle, newAngle);

            if (Mathf.Abs(angleDifference) < ForceJumpAngleDifference)
            {
                var targetAngle = originalAngle + ForceJumpAngleDifference * Mathf.Sign(angleDifference);
                var magnitude = new Vector2(Vx, Vy).magnitude;

                var targetAngleRadians = targetAngle * Mathf.Deg2Rad;
                var newVelocity = new Vector2(magnitude * Mathf.Cos(targetAngleRadians),
                    magnitude * Mathf.Sin(targetAngleRadians));

                Vx = newVelocity.x;
                Vy = newVelocity.y;
            }

            // Eject self from ground
            Detach();
        }

        public void ReleaseJump()
        {
            if (Vy > ReleaseJumpSpeed) Vy = ReleaseJumpSpeed;
        }

        /// <summary>
        /// Translates the controller at the start of its physics checks.
        /// </summary>
        /// <param name="deltaPosition"></param>
        public void Translate(Vector3 deltaPosition)
        {
            _queuedTranslate = deltaPosition;
        }

        #endregion
        #region Surface Acquisition Functions
        /// <summary>
        /// Detach the player from whatever surface it is on. If the player is not grounded this has no effect
        /// other than setting lockUponLanding.
        /// </summary>
        /// <param name="lockUponLanding">If set to <c>true</c> lock horizontal control when the player attaches.</param>
        private void Detach(bool lockUponLanding = false, bool triggerEvents = true)
        {
            Vg = 0.0f;
            LastSurfaceAngle = 0.0f;
            SurfaceAngle = 0.0f;
            Grounded = false;
            JustDetached = true;
            Wallmode = Orientation.Floor;
            Footing = Footing.None;
            LockUponLanding = lockUponLanding;
            SetSurface(null, null, triggerEvents);
        }

        /// <summary>
        /// Attaches the player to a surface within the reach of its surface sensors. The angle of attachment
        /// need not be perfect; the method works reliably for angles within 45 degrees of the one specified.
        /// </summary>
        /// <param name="groundSpeed">The ground speed of the player after attaching.</param>
        /// <param name="angleRadians">The angle of the surface, in radians.</param>
        private void Attach(float groundSpeed, float angleRadians)
        {
            var angleDegrees = DMath.Modp(angleRadians * Mathf.Rad2Deg, 360.0f);
            Vg = groundSpeed;
            SurfaceAngle = LastSurfaceAngle = angleDegrees;
            Grounded = _justLanded = true;

            // HorizontalWallmodeAngleWeight may be set to only attach right or left at extreme angles
            if (SurfaceAngle < 45.0f + HorizontalWallmodeAngleWeight || SurfaceAngle > 315.0f - HorizontalWallmodeAngleWeight)
                Wallmode = Orientation.Floor;

            else if (SurfaceAngle > 135.0f - HorizontalWallmodeAngleWeight && SurfaceAngle < 225.0 + HorizontalWallmodeAngleWeight)
                Wallmode = Orientation.Ceiling;

            else if (SurfaceAngle > 45.0f + HorizontalWallmodeAngleWeight && SurfaceAngle < 135.0f - HorizontalWallmodeAngleWeight)
                Wallmode = Orientation.Right;

            else
                Wallmode = Orientation.Left;

            if (LockUponLanding)
            {
                LockUponLanding = false;
                LockHorizontal();
            }

            DMath.RotateTo(transform, angleRadians);
        }

        /// <summary>
        /// Calculates the ground velocity as the result of an impact on the specified surface angle.
        /// </summary>
        /// <returns>Whether the player should attach to the specified incline.</returns>
        /// <param name="impact">The impact data as th result of a terrain cast.</param>
        private bool HandleImpact(TerrainCastHit impact)
        {
            var sAngled = DMath.Modp(impact.SurfaceAngle * Mathf.Rad2Deg, 360.0f);
            var sAngler = sAngled * Mathf.Deg2Rad;

            // The player can't possibly land on something if he's traveling 90 degrees
            // within the normal
            var surfaceNormal = DMath.Modp(sAngled + 90.0f, 360.0f);
            var playerAngle = DMath.Angle(new Vector2(Vx, Vy)) * Mathf.Rad2Deg;
            var surfaceDifference = DMath.AngleDiffd(playerAngle, surfaceNormal);
            if (Mathf.Abs(surfaceDifference) < 90.0f)
            {
                return false;
            }

            // Ground attachment
            if (Mathf.Abs(DMath.AngleDiffd(sAngled, 180.0f)) > MinFlatAttachAngle &&
                Mathf.Abs(DMath.AngleDiffd(sAngled, 90.0f)) > MinFlatAttachAngle &&
                Mathf.Abs(DMath.AngleDiffd(sAngled, 270.0f)) > MinFlatAttachAngle)
            {
                float groundSpeed;
                if (Vy > 0.0f && (DMath.Equalsf(sAngled, 0.0f, MinFlatAttachAngle) ||
                    (DMath.Equalsf(sAngled, 180.0f, MinFlatAttachAngle))))
                {
                    groundSpeed = DMath.Equalsf(impact.Hit.fraction, 0.0f) ? 0.0f : Vx;
                    Attach(groundSpeed, sAngler);
                    return true;
                }
                // groundspeed = (airspeed) * (angular difference between air direction and surface normal direction) / (90 degrees)
                groundSpeed = DMath.Equalsf(impact.Hit.fraction, 0.0f) ? 
                    0.0f : 
                    Mathf.Sqrt(Vx * Vx + Vy * Vy) *
                              -Mathf.Clamp(DMath.AngleDiffd(Mathf.Atan2(Vy, Vx) * Mathf.Rad2Deg, sAngled - 90.0f) /
                              90.0f, -1.0f, 1.0f);

                if (sAngled > 90.0f - MaxVerticalDetachAngle &&
                    sAngled < 270.0f + MaxVerticalDetachAngle &&
                    Mathf.Abs(groundSpeed) < DetachSpeed)
                {
                    return false;
                }
                Attach(groundSpeed, sAngler);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the controller's primary and secondary surfaces and triggers their respective platform events.
        /// </summary>
        /// <param name="primarySurfaceHit">The new primary surface.</param>
        /// <param name="secondarySurfaceHit">The new secondary surface.</param>
        /// <param name="triggerEvents">Whether to trigger the surfaces' respective platform events.</param>
        public void SetSurface(TerrainCastHit primarySurfaceHit, TerrainCastHit secondarySurfaceHit = null,
            bool triggerEvents = true)
        {
            if (!triggerEvents)
            {
                PrimarySurface = primarySurfaceHit == null ? null : primarySurfaceHit.Hit.transform;
                PrimarySurfaceHit = primarySurfaceHit;
                SecondarySurface = secondarySurfaceHit == null ? null : secondarySurfaceHit.Hit.transform;
                SecondarySurfaceHit = secondarySurfaceHit;
            }
            else
            {
                var primarySurface = primarySurfaceHit == null ? null : primarySurfaceHit.Hit.transform;
                var secondarySurface = secondarySurfaceHit == null ? null : secondarySurfaceHit.Hit.transform;

                var oldPrimaryTriggers = TerrainUtility.FindAll<PlatformTrigger>(PrimarySurface, BaseTrigger.Selector);
                var newPrimaryTriggers = TerrainUtility.FindAll<PlatformTrigger>(primarySurface, BaseTrigger.Selector);
                var oldSecondaryTriggers = TerrainUtility.FindAll<PlatformTrigger>(SecondarySurface,
                    BaseTrigger.Selector);
                var newSecondaryTriggers = TerrainUtility.FindAll<PlatformTrigger>(secondarySurface,
                    BaseTrigger.Selector);

                var oldPrimarySurface = PrimarySurface;
                var oldPrimarySurfaceHit = PrimarySurfaceHit;
                var oldSecondarySurface = SecondarySurface;
                var oldSecondarySurfaceHit = SecondarySurfaceHit;

                PrimarySurface = primarySurface;
                PrimarySurfaceHit = primarySurfaceHit;
                SecondarySurface = secondarySurface;
                SecondarySurfaceHit = secondarySurfaceHit;

                if (oldPrimaryTriggers != null)
                {
                    if (oldPrimarySurface != primarySurface)
                    {
                        foreach (var primaryTrigger in oldPrimaryTriggers)
                        {
                            primaryTrigger.OnExit.Invoke(this);
                            primaryTrigger.OnSurfaceExit.Invoke(this, oldPrimarySurfaceHit, SurfacePriority.Primary);
                        }

                        if (newPrimaryTriggers != null)
                        {
                            foreach (var newPrimaryTrigger in newPrimaryTriggers)
                            {
                                newPrimaryTrigger.OnEnter.Invoke(this);
                                newPrimaryTrigger.OnSurfaceEnter.Invoke(this, primarySurfaceHit, SurfacePriority.Primary);
                            }
                        }
                    }
                    else
                    {
                        foreach (var primaryTrigger in oldPrimaryTriggers)
                        {
                            primaryTrigger.OnStay.Invoke(this);
                            primaryTrigger.OnSurfaceStay.Invoke(this, primarySurfaceHit, SurfacePriority.Primary);
                        }
                    }
                } else if (newPrimaryTriggers != null && oldPrimarySurface != primarySurface)
                {
                    foreach (var newPrimaryTrigger in newPrimaryTriggers)
                    {
                        newPrimaryTrigger.OnEnter.Invoke(this);
                        newPrimaryTrigger.OnSurfaceEnter.Invoke(this, primarySurfaceHit, SurfacePriority.Primary);
                    }
                }

                if (oldSecondaryTriggers != null)
                {
                    if (oldSecondarySurface != secondarySurface)
                    {
                        foreach (var secondaryTrigger in oldSecondaryTriggers)
                        {
                            secondaryTrigger.OnExit.Invoke(this);
                            secondaryTrigger.OnSurfaceExit.Invoke(this, oldSecondarySurfaceHit, SurfacePriority.Secondary);
                        }

                        if (newSecondaryTriggers != null)
                        {
                            foreach (var newSecondaryTrigger in newSecondaryTriggers)
                            {
                                newSecondaryTrigger.OnEnter.Invoke(this);
                                newSecondaryTrigger.OnSurfaceEnter.Invoke(this, secondarySurfaceHit, SurfacePriority.Secondary);
                            }
                        }
                    }
                    else
                    {
                        foreach (var secondaryTrigger in oldSecondaryTriggers)
                        {
                            secondaryTrigger.OnStay.Invoke(this);
                            secondaryTrigger.OnSurfaceStay.Invoke(this, secondarySurfaceHit, SurfacePriority.Secondary);
                        }
                    }
                } else if (newSecondaryTriggers != null && oldSecondarySurface != secondarySurface)
                {
                    foreach (var newSecondaryTrigger in newSecondaryTriggers)
                    {
                        newSecondaryTrigger.OnEnter.Invoke(this);
                        newSecondaryTrigger.OnSurfaceEnter.Invoke(this, secondarySurfaceHit, SurfacePriority.Secondary);
                    }
                }
            }
        }
        #endregion
        #region Surface Calculation Functions
        /// <summary>
        /// Gets data about the surface closest to the player's feet, including its Footing and raycast info.
        /// </summary>
        /// <returns>The surface.</returns>
        /// <param name="layerMask">A mask indicating what layers are surfaces.</param>
        public SurfaceInfo GetSurface(int layerMask)
        {
            // Linecasts are straight vertical or horizontal from the ground sensors
            var checkLeft = Surfacecast(Footing.Left);
            var checkRight = Surfacecast(Footing.Right);

            Footing newFooting = Footing.None;

            if (checkLeft && checkRight)
            {
                // Find the highest point using wall mode orientation
                var distance = DMath.Highest(checkLeft.Hit.point, checkRight.Hit.point,
                    (SurfaceAngle + 90.0f)*Mathf.Deg2Rad);
                newFooting = distance > 0.0f ? Footing.Left : Footing.Right;
            }
            else if (checkLeft)
            {
                newFooting = Footing.Left;
            } else if (checkRight)
            {
                newFooting = Footing.Right;
            }

            if (newFooting == Footing.None) return default(SurfaceInfo);
            return new SurfaceInfo(checkLeft, checkRight, newFooting);
        }

        /// <summary>
        /// Casts from LedgeClimbHeight to the player's geet.
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        private TerrainCastHit Groundcast(Footing side)
        {
            TerrainCastHit cast;
            if (side == Footing.Left)
            {
                cast = this.TerrainCast(
                    SensorLedgeLeft.position, SensorBottomLeft.position, TerrainSide.Bottom);
            }
            else
            {
                cast = this.TerrainCast(SensorLedgeRight.position, SensorBottomRight.position, TerrainSide.Bottom);
            }

            return cast;
        }

        /// <summary>
        /// Returns the result of a linecast from the ClimbLedgeHeight to Su
        /// </summary>
        /// <returns>The result of the linecast.</returns>
        /// <param name="footing">The side to linecast from.</param>
        private TerrainCastHit Surfacecast(Footing footing)
        {
            TerrainCastHit cast;
            if (footing == Footing.Left)
            {
                // Cast from the player's side to below the player's feet based on its wall mode (Wallmode)
                cast = this.TerrainCast(SensorLedgeLeft.position, SensorSurfaceLeft.position, TerrainSide.Bottom);

                if (!cast)
                {
                    return default(TerrainCastHit);
                }
                return DMath.Equalsf(cast.Hit.fraction, 0.0f) ? default(TerrainCastHit) : cast;
            }
            cast = this.TerrainCast(SensorLedgeRight.position, SensorSurfaceRight.position,
                TerrainSide.Bottom);

            if (!cast)
            {
                return default(TerrainCastHit);
            }
            if (DMath.Equalsf(cast.Hit.fraction, 0.0f))
            {
                return default(TerrainCastHit);
            }

            return cast;
        }
        #endregion
    }
}