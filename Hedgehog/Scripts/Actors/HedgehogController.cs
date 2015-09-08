using System;
using System.Collections.Generic;
using Hedgehog.Terrain;
using UnityEngine;
using Hedgehog.Utils;
using UnityEngine.Events;

namespace Hedgehog.Actors
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
    
        [SerializeField]
        public LayerMask InitialTerrainMask;

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
        // Nine sensors arranged like a tic-tac-toe board.
        [SerializeField]
        [Tooltip("The bottom left corner.")]
        public Transform SensorBottomLeft;

        [SerializeField]
        [Tooltip("The bottom middle point.")]
        public Transform SensorBottomMiddle;

        [SerializeField]
        [Tooltip("The bottom right corner.")]
        public Transform SensorBottomRight;

        [SerializeField]
        [Tooltip("The left middle point.")]
        public Transform SensorMiddleLeft;

        [SerializeField]
        [Tooltip("The center point.")]
        public Transform SensorMiddleMiddle;

        [SerializeField]
        [Tooltip("The right middle point.")]
        public Transform SensorMiddleRight;

        [SerializeField]
        [Tooltip("The top left corner.")]
        public Transform SensorTopLeft;

        [SerializeField]
        [Tooltip("The top middle point.")]
        public Transform SensorTopMiddle;

        [SerializeField]
        [Tooltip("The top right corner.")]
        public Transform SensorTopRight;
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
        /// The player's horizontal acceleration in the air in units per second per second.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 0.25f)]
        [Tooltip("Air acceleration in units per second squared.")]
        public float AirAcceleration = 0.16f;

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
        /// The layer mask which represents the ground the player checks for collision with.
        /// </summary>
        [HideInInspector]
        public LayerMask TerrainMask;

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
        public FootSide Footing;

        /// <summary>
        /// If grounded, the properties of the ground beneath the player, if any.
        /// </summary>
        [HideInInspector]
        public TerrainProperties TerrainProperties;

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
        /// The moving platform anchor used to move the player when on moving platforms.
        /// </summary>
        [HideInInspector]
        public MovingPlatformAnchor MovingPlatformAnchor;

        /// <summary>
        /// Stores movement of a moving platform from the last update, if any.
        /// </summary>
        private Vector3 _movingPlatformDelta;
        #endregion

        #region Main Event Functions
        public void Awake()
        {
            Footing = FootSide.None;
            Grounded = false;
            Vx = Vy = Vg = 0.0f;
            LastSurfaceAngle = 0.0f;
            LeftKeyDown = RightKeyDown = JumpKeyPressed = DebugSpindashKeyDown = false;
            JustJumped = _justLanded = JustDetached = false;
            Wallmode = Orientation.Floor;
            TerrainMask = InitialTerrainMask;

            CreateMovingPlatformAnchor();
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
        }
        #endregion

        #region Main Loop Subroutines
        /// <summary>
        /// Stores keyboard input for the next fixed update (and HandleInput).
        /// </summary>
        private void GetInput()
        {
            LeftKeyDown = Input.GetKey(LeftKey);
            RightKeyDown = Input.GetKey(RightKey);
            
            if(!JumpKeyPressed) JumpKeyPressed = Input.GetKeyDown(JumpKey);

            if (!JumpKeyReleased && JumpKeyDown && !Input.GetKey(JumpKey))
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
        private void HandleInput(float timeStep)
        {
            var timeScale = timeStep / Constants.DefaultFixedDeltaTime;

            if (Grounded)
            {
                if (DebugSpindashKeyDown)
                {
                    GroundVelocity = MaxSpeed*Mathf.Sign(GroundVelocity);
                    DebugSpindashKeyDown = false;
                }

                if (HorizontalLock)
                {
                    HorizontalLockTimer -= timeStep;
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
                        Vg -= GroundAcceleration * timeScale;
                        if (Vg > 0) Vg -= GroundDeceleration * timeScale;
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
                        Vg += GroundAcceleration * timeScale;
                        if (Vg < 0) Vg += GroundDeceleration * timeScale;
                    }
                }
            }
            else
            {
                if (LeftKeyDown) Vx -= AirAcceleration * timeScale;
                else if (RightKeyDown) Vx += AirAcceleration * timeScale;

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
        private void HandleForces(float timeStep)
        {
            var timeScale = timeStep /Constants.DefaultFixedDeltaTime;

            if (Grounded)
            {
                var prevVg = Vg;
                var friction = TerrainProperties == null
                    ? 1.0f
                    : TerrainProperties.Friction;

                // Friction from deceleration
                if (!LeftKeyDown && !RightKeyDown)
                {
                    if (Vg != 0.0f && Mathf.Abs(Vg) < GroundDeceleration)
                    {
                        Vg = 0.0f;
                    }
                    else if (Vg > 0.0f)
                    {
                        Vg -= GroundDeceleration*timeScale*friction;
                    }
                    else if (Vg < 0.0f)
                    {
                        Vg += GroundDeceleration*timeScale*friction;
                    }
                }

                // Slope gravity
                var slopeForce = 0.0f;

                if (Mathf.Abs(DMath.AngleDiffd(SurfaceAngle, 0.0f)) > SlopeGravityBeginAngle)
                {
                    slopeForce = SlopeGravity * Mathf.Sin(SurfaceAngle * Mathf.Deg2Rad);
                    Vg -= slopeForce * timeScale;
                }

                // Speed limit
                if (Vg > MaxSpeed) Vg = MaxSpeed;
                else if (Vg < -MaxSpeed) Vg = -MaxSpeed;

                if (Mathf.Abs(slopeForce) > GroundAcceleration)
                {
                    if (RightKeyDown && prevVg > 0.0f && Vg < 0.0f) LockHorizontal();
                    else if (LeftKeyDown && prevVg < 0.0f && Vg > 0.0f) LockHorizontal();
                }

                // Detachment from walls if speed is too low
                if (SurfaceAngle > 90.0f - MaxVerticalDetachAngle &&
                    SurfaceAngle < 270.0f + MaxVerticalDetachAngle &&
                    Mathf.Abs(Vg) < DetachSpeed)
                {
                    Detach(true);
                }
            }
            else
            {
                Vy -= AirGravity * timeScale;
                //transform.eulerAngles = new Vector3(0.0f, 0.0f, Mathf.LerpAngle(transform.eulerAngles.z, 0.0f, Time.deltaTime * 5.0f));
                transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
            }
        }

        /// <summary>
        /// Checks for collision with all sensors, changing position, velocity, and rotation if necessary. This should be called each time
        /// the player changes position. This method does not require a timestep because it only resolves overlaps in the player's collision.
        /// </summary>
        private void HandleCollisions()
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
                anyHit = GroundSideCheck() | GroundCeilingCheck() | GroundSurfaceCheck();
                if (!SurfaceAngleCheck()) Detach();
            }

            if (JustJumped && jumpedPreviousCheck) JustJumped = false;

            if (!anyHit && JustDetached)
                JustDetached = false;
        }
#if UNITY_EDITOR
        public void OnApplicationQuit()
        {
            DestroyMovingPlatformAnchor();
        }
#endif
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
            var groundLeftCheck = Groundcast(FootSide.Left);
            var groundRightCheck = Groundcast(FootSide.Right);

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
        private bool GroundSurfaceCheck()
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

                    if (s.Side == FootSide.Left)
                    {
                        // If the surface's angle is a small enough difference from that of the previous begin surface checks
                        if (_justLanded || Mathf.Abs(leftDiff) < MaxSurfaceAngleDifference)
                        {
                            // Check angle differences between feet for player rotation
                            if (Mathf.Abs(overlapDiff) > MinFlatOverlapRange && overlapDiff > MinOverlapAngle)
                            {
                                // If tolerable, rotate between the surfaces beneath the two feet
                                DMath.RotateTo(transform, DMath.Angle(s.RightCast.Hit.point - s.LeftCast.Hit.point));
                                transform.position += (Vector3)s.LeftCast.Hit.point - SensorBottomLeft.position;
                                Footing = FootSide.Left;
                            }
                            else
                            {
                                // Else just rotate for the left foot
                                DMath.RotateTo(transform, s.LeftCast.SurfaceAngle);
                                transform.position += (Vector3)s.LeftCast.Hit.point - SensorBottomLeft.position;
                                Footing = FootSide.Left;
                            }

                            // Else see if the other surface's angle is tolerable
                        }
                        else if (Mathf.Abs(rightDiff) < MaxSurfaceAngleDifference)
                        {
                            DMath.RotateTo(transform, s.RightCast.SurfaceAngle);
                            transform.position += (Vector3)s.RightCast.Hit.point - SensorBottomRight.position;
                            Footing = FootSide.Right;
                            // Else the surfaces are untolerable. detach from the surface
                        }
                        else
                        {
                            Detach();
                        }

                        // Same thing but with the other foot
                    }
                    else if (s.Side == FootSide.Right)
                    {
                        if (_justLanded || Mathf.Abs(rightDiff) < MaxSurfaceAngleDifference)
                        {
                            if (Mathf.Abs(overlapDiff) > MinFlatOverlapRange && overlapDiff > MinOverlapAngle)
                            {
                                DMath.RotateTo(transform, DMath.Angle(s.RightCast.Hit.point - s.LeftCast.Hit.point));
                                transform.position += (Vector3)s.RightCast.Hit.point - SensorBottomRight.position;
                                Footing = FootSide.Right;
                            }
                            else
                            {
                                DMath.RotateTo(transform, s.RightCast.SurfaceAngle);
                                transform.position += (Vector3)s.RightCast.Hit.point - SensorBottomRight.position;
                                Footing = FootSide.Right;
                            }

                        }
                        else if (Mathf.Abs(leftDiff) < MaxSurfaceAngleDifference)
                        {
                            DMath.RotateTo(transform, s.LeftCast.SurfaceAngle);
                            transform.position += (Vector3)s.LeftCast.Hit.point - SensorBottomLeft.position;
                            Footing = FootSide.Left;
                        }
                        else
                        {
                            Detach();
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
                        Footing = FootSide.Left;
                    }
                    else
                    {
                        Detach();
                    }
                }
                else
                {
                    var rightDiff = DMath.AngleDiffd(s.RightCast.SurfaceAngle * Mathf.Rad2Deg, LastSurfaceAngle);
                    if (_justLanded || Mathf.Abs(rightDiff) < MaxSurfaceAngleDifference)
                    {
                        DMath.RotateTo(transform, s.RightCast.SurfaceAngle);
                        transform.position += (Vector3)s.RightCast.Hit.point - SensorBottomRight.position;
                        Footing = FootSide.Right;
                    }
                    else
                    {
                        Detach();
                    }
                }

                if (Grounded)
                {
                    if (Footing == FootSide.Left)
                    {
                        if (s.LeftCast.Properties != null && s.LeftCast.Properties.MovingPlatform)
                        {
                            if (MovingPlatformAnchor.Platform != s.LeftCast.Hit.transform)
                                MovingPlatformAnchor.LinkPlatform(s.LeftCast.Hit.point, s.LeftCast.Hit.transform);
                        }
                        else
                        {
                            MovingPlatformAnchor.UnlinkPlatform();
                        }
                    }
                    else if (Footing == FootSide.Right)
                    {
                        if (s.RightCast.Properties != null && s.RightCast.Properties.MovingPlatform)
                        {
                            if (MovingPlatformAnchor.Platform != s.RightCast.Hit.transform)
                                MovingPlatformAnchor.LinkPlatform(s.RightCast.Hit.point, s.RightCast.Hit.transform);
                        }
                        else
                        {
                            MovingPlatformAnchor.UnlinkPlatform();
                        }
                    }
                }

                if (Grounded)
                {
                    TerrainProperties = Footing == FootSide.Left
                        ? s.LeftCast.Properties
                        : s.RightCast.Properties;
                }

                return true;
            }
            Detach();

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

        #endregion
        #region Surface Acquisition Functions
        /// <summary>
        /// Detach the player from whatever surface it is on. If the player is not grounded this has no effect
        /// other than setting lockUponLanding.
        /// </summary>
        /// <param name="lockUponLanding">If set to <c>true</c> lock horizontal control when the player attaches.</param>
        private void Detach(bool lockUponLanding = false)
        {
            Vg = 0.0f;
            LastSurfaceAngle = 0.0f;
            SurfaceAngle = 0.0f;
            Grounded = false;
            JustDetached = true;
            Wallmode = Orientation.Floor;
            Footing = FootSide.None;
            LockUponLanding = lockUponLanding;
            MovingPlatformAnchor.UnlinkPlatform();
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
            if (impact.Properties != null && impact.Properties.Ledge && DMath.Equalsf(impact.Hit.fraction, 0.0f))
                return false;

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
        #endregion
        #region Surface Calculation Functions
        /// <summary>
        /// Gets data about the surface closest to the player's feet, including its FootSide and raycast info.
        /// </summary>
        /// <returns>The surface.</returns>
        /// <param name="layerMask">A mask indicating what layers are surfaces.</param>
        public SurfaceInfo GetSurface(int layerMask)
        {
            // Linecasts are straight vertical or horizontal from the ground sensors
            var checkLeft = Surfacecast(FootSide.Left);
            var checkRight = Surfacecast(FootSide.Right);

            FootSide newFooting = FootSide.None;

            if (checkLeft && checkRight)
            {
                // Find the highest point using wall mode orientation
                var distance = DMath.Highest(checkLeft.Hit.point, checkRight.Hit.point, Wallmode.Normal());
                
                // If they are equally high prioritize the one with no terrain properties, then the one
                // with no moving platform properties
                if (DMath.Equalsf(distance, 0.0f))
                {
                    if (checkLeft.Properties == null) newFooting = FootSide.Left;
                    else if(checkRight.Properties == null) newFooting = FootSide.Right;
                    else if(!checkLeft.Properties.MovingPlatform) newFooting = FootSide.Left;
                    else if(!checkRight.Properties.MovingPlatform) newFooting = FootSide.Right;
                    else newFooting = FootSide.Left;
                } else if (distance > 0)
                {
                    newFooting = FootSide.Left;
                }
                else
                {
                    newFooting = FootSide.Right;
                }
            } else if (checkLeft)
            {
                newFooting = FootSide.Left;;
            } else if (checkRight)
            {
                newFooting = FootSide.Right;
            }

            if (newFooting == FootSide.None) return default(SurfaceInfo);
            return new SurfaceInfo(checkLeft, checkRight, newFooting);
        }

        /// <summary>
        /// Casts from LedgeClimbHeight to the player's geet.
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        private TerrainCastHit Groundcast(FootSide side)
        {
            TerrainCastHit cast;
            if (side == FootSide.Left)
            {
                cast = this.TerrainCast(
                    (Vector2) SensorBottomLeft.position - Wallmode.UnitVector()*LedgeClimbHeight,
                    SensorBottomLeft.position, TerrainSide.Bottom);
            }
            else
            {
                cast = this.TerrainCast(
                    (Vector2) SensorBottomRight.position - Wallmode.UnitVector()*LedgeClimbHeight,
                    SensorBottomRight.position, TerrainSide.Bottom);
            }

            return cast;
        }

        /// <summary>
        /// Returns the result of a linecast from the ClimbLedgeHeight to Su
        /// </summary>
        /// <returns>The result of the linecast.</returns>
        /// <param name="footing">The side to linecast from.</param>
        private TerrainCastHit Surfacecast(FootSide footing)
        {
            TerrainCastHit cast;
            if (footing == FootSide.Left)
            {
                // Cast from the player's side to below the player's feet based on its wall mode (Wallmode)
                cast = this.TerrainCast(
                    (Vector2) SensorBottomLeft.position - Wallmode.UnitVector()*LedgeClimbHeight,
                    (Vector2) SensorBottomLeft.position + Wallmode.UnitVector()*LedgeDropHeight,
                    TerrainSide.Bottom);

                if (!cast)
                {
                    return default(TerrainCastHit);
                }
                if (DMath.Equalsf(cast.Hit.fraction, 0.0f))
                {
                    for (var check = Wallmode.AdjacentCW(); check != Wallmode; check = check.AdjacentCW())
                    {
                        cast = this.TerrainCast(
                            (Vector2) SensorBottomLeft.position - check.UnitVector()*LedgeClimbHeight,
                            (Vector2) SensorBottomLeft.position + check.UnitVector()*LedgeDropHeight,
                            TerrainSide.Bottom);

                        if (cast && !DMath.Equalsf(cast.Hit.fraction, 0.0f))
                            return cast;
                    }

                    return default(TerrainCastHit);
                }

                return cast;
            }
            cast = this.TerrainCast(
                    (Vector2) SensorBottomRight.position - Wallmode.UnitVector()*LedgeClimbHeight,
                    (Vector2) SensorBottomRight.position + Wallmode.UnitVector()*LedgeDropHeight,
                    TerrainSide.Bottom);

            if (!cast)
            {
                return default(TerrainCastHit);
            }
            if (DMath.Equalsf(cast.Hit.fraction, 0.0f))
            {
                for (var check = Wallmode.AdjacentCW(); check != Wallmode; check = check.AdjacentCW())
                {
                    cast = this.TerrainCast(
                        (Vector2) SensorBottomRight.position - check.UnitVector()*LedgeClimbHeight,
                        (Vector2) SensorBottomRight.position + check.UnitVector()*LedgeDropHeight,
                        TerrainSide.Bottom);

                    if (cast && !DMath.Equalsf(cast.Hit.fraction, 0.0f))
                        return cast;
                }

                return default(TerrainCastHit);
            }

            return cast;
        }
        #endregion
        #region Moving Platform Functions
        /// <summary>
        /// Destroys the current moving platform anchor and creates a new one.
        /// </summary>
        public void CreateMovingPlatformAnchor()
        {
            DestroyMovingPlatformAnchor();

            MovingPlatformAnchor = (new GameObject()).AddComponent<MovingPlatformAnchor>();
            MovingPlatformAnchor.transform.SetParent(transform);
            MovingPlatformAnchor.name = name + "'s Moving Platform Anchor";
            MovingPlatformAnchor.LinkController(this);
            MovingPlatformAnchor.Moved += OnMovingPlatformMove;
            MovingPlatformAnchor.Destroyed += OnMovingPlatformAnchorDestroyed;

            _movingPlatformDelta = new Vector3();
        }

        /// <summary>
        /// Destroys the current moving platform anchor.
        /// </summary>
        public void DestroyMovingPlatformAnchor()
        {
            if (MovingPlatformAnchor != null && MovingPlatformAnchor.gameObject)
            {
                MovingPlatformAnchor.Moved -= OnMovingPlatformMove;
                MovingPlatformAnchor.Destroyed -= OnMovingPlatformAnchorDestroyed;
                Destroy(MovingPlatformAnchor.gameObject);
            }

            MovingPlatformAnchor = null;
        }

        private void OnMovingPlatformMove(object sender, EventArgs e)
        {
            _movingPlatformDelta += MovingPlatformAnchor.DeltaPosition;
            transform.position += _movingPlatformDelta;
            _movingPlatformDelta = default(Vector3);

            HandleCollisions();
        }

        private void OnMovingPlatformAnchorDestroyed(object sender, EventArgs e)
        {
            CreateMovingPlatformAnchor();
        }
        #endregion
    }
}