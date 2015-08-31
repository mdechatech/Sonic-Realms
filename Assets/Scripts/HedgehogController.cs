using UnityEngine;

/// <summary>
/// Controls the player.
/// </summary>
public class HedgehogController : MonoBehaviour
{
    #region Math Constants
    public const float Epsilon = 0.0001f;
    public const float HalfPi = 1.5707963f;
    public const float DoublePi = 6.283185f;
    #endregion
    #region Time Constants
    private const float DefaultFixedDeltaTime = 0.02f;
    #endregion
    #region Inspector Fields

    [Header("Collision")]
    
    [SerializeField]
    public string AlwaysCollideLayer = "Terrain";

    [Header("Controls")]

    [SerializeField]
    public KeyCode JumpKey = KeyCode.W;

    [SerializeField]
    public KeyCode LeftKey = KeyCode.A;

    [SerializeField]
    public KeyCode RightKey = KeyCode.D;

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
    public float JumpSpeed = 7.5f;

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
    public float DetachSpeed = 3.5f;

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
    #region Input Variables
    /// <summary>
    /// Whether the left key was held down since the last update. Key is determined by LeftKey.
    /// </summary>
    [HideInInspector]
    private bool _leftKeyDown;

    /// <summary>
    /// Whether the right key was held down since the last update. Key is determined by RightKey.
    /// </summary>
    [HideInInspector]
    private bool _rightKeyDown;

    /// <summary>
    /// Whether the jump key was pressed since the last update. Key is determined by JumpKey.
    /// </summary>
    [HideInInspector]
    private bool _jumpKeyDown;
    #endregion
    #region Physics Variables
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
    /// Whether the player is on a moving platform.
    /// </summary>
    [HideInInspector]
    public bool OnMovingPlatform;

    /// <summary>
    /// The layer mask which represents the ground the player checks for collision with.
    /// </summary>
    [HideInInspector]
    public int TerrainMask;

    /// <summary>
    /// The number of the layer of terrain the player checks for collision with.
    /// </summary>
    [HideInInspector]
    public int TerrainLayer;

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
    public Side Footing;

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
    /// The terrain layer that the player is on. The player will collide with the layers "Terrain" and
    /// "Terrain" plus the terrain layer.
    /// </summary>
    /// <value>The terrain layer.</value>
    public int Layer
    {
        get { return TerrainLayer; }
        set
        {
            TerrainLayer = value;
            TerrainMask =
                (1 << LayerMask.NameToLayer(AlwaysCollideLayer)) |
                (1 << LayerMask.NameToLayer(AlwaysCollideLayer + " " + TerrainLayer));
        }
    }
    #endregion

    #region Orientation Definition
    /// <summary>
    /// Represents the four sides based on the Wallmode of a character standing on the mask.
    /// This means that RIGHT is the mask's left side and that LEFT is the mask's right side.
    /// </summary>
    public enum Orientation : int
    {
        Floor, Right, Ceiling, Left, None,
    };
    #endregion
    #region SurfaceInfo Definition
    /// <summary>
    /// A collection of data about the surface at the player's feet.
    /// </summary>
    public struct SurfaceInfo
    {
        /// <summary>
        /// If there is a surface, which foot of the player it is  beneath. Otherwise, Side.none.
        /// </summary>
        public Side Side;

        /// <summary>
        /// The result of the raycast onto the surface at the player's left foot.
        /// </summary>
        public RaycastHit2D LeftCast;

        /// <summary>
        /// The angle, in radians, of the surface on the player's left foot, or 0 if there is none.
        /// </summary>
        public float LeftSurfaceAngle;

        /// <summary>
        /// The result of the raycast onto the surface at the player's right foot.
        /// </summary>
        public RaycastHit2D RightCast;

        /// <summary>
        /// The angle, in radians, of the surface on the player's right foot, or 0 if there is none.
        /// </summary>
        public float RightSurfaceAngle;

        public SurfaceInfo(RaycastHit2D leftCast, RaycastHit2D rightCast, Side Side)
        {
            LeftCast = leftCast;
            LeftSurfaceAngle = (leftCast) ? Angle(leftCast.normal) - HalfPi : 0.0f;
            RightCast = rightCast;
            RightSurfaceAngle = (rightCast) ? Angle(rightCast.normal) - HalfPi : 0.0f;
            this.Side = Side;
        }
    }
    #endregion
    #region Side Definition
    /// <summary>
    /// Represents each sensor on the bottom of the player.
    /// </summary>
    public enum Side
    {
        None, Left, Right
    }
    #endregion

    public void Awake()
    {
        Footing = Side.None;
        Grounded = false;
        OnMovingPlatform = false;
        Vx = Vy = Vg = 0.0f;
        LastSurfaceAngle = 0.0f;
        _leftKeyDown = _rightKeyDown = _jumpKeyDown = false;
        JustJumped = _justLanded = JustDetached = false;
        Wallmode = Orientation.Floor;
        SurfaceAngle = 0.0f;
        Layer = 1;
    }

    public void Start()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    public void Update()
    {
        GetInput();
    }

    /// <summary>
    /// Stores keyboard input for the next fixed update (and HandleInput).
    /// </summary>
    private void GetInput()
    {
        _leftKeyDown = Input.GetKey(LeftKey);
        _rightKeyDown = Input.GetKey(RightKey);
        if (!_jumpKeyDown && Grounded) _jumpKeyDown = Input.GetKeyDown(JumpKey);
    }

    public void FixedUpdate()
    {
        HandleInput(Time.fixedDeltaTime);

        // Stagger routine - if the player's gotta go fast, move it in increments of StaggerSpeedThreshold to prevent tunneling
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
        var timeScale = timeStep / DefaultFixedDeltaTime;

        if (Grounded)
        {
            if (HorizontalLock)
            {
                HorizontalLockTimer -= timeStep;
                if (HorizontalLockTimer < 0.0f)
                {
                    HorizontalLock = false;
                    HorizontalLockTimer = 0.0f;
                }
            }

            if (_jumpKeyDown)
            {
                _jumpKeyDown = false;
                Jump();
            }

            if (_leftKeyDown && !HorizontalLock)
            {
                if (Vg > 0 && Mathf.Abs(AngleDiffd(SurfaceAngle, 90.0f)) < MaxVerticalDetachAngle)
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
            else if (_rightKeyDown && !HorizontalLock)
            {
                if (Vg < 0 && Mathf.Abs(AngleDiffd(SurfaceAngle, 270.0f)) < MaxVerticalDetachAngle)
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
            if (_leftKeyDown) Vx -= AirAcceleration * timeScale;
            else if (_rightKeyDown) Vx += AirAcceleration * timeScale;
        }
    }

    public void Jump()
    {
        JustJumped = true;

        // Forces the player to leave the ground using the constant ForceJumpAngleDifference.
        // Helps prevent sticking to surfaces when the player's gotta go fast.
        var originalAngle = Modp(Angle(new Vector2(Vx, Vy)) * Mathf.Rad2Deg, 360.0f);

        var surfaceNormal = (SurfaceAngle + 90.0f) * Mathf.Deg2Rad;
        Vx += JumpSpeed * Mathf.Cos(surfaceNormal);
        Vy += JumpSpeed * Mathf.Sin(surfaceNormal);

        var newAngle = Modp(Angle(new Vector2(Vx, Vy)) * Mathf.Rad2Deg, 360.0f);
        var angleDifference = AngleDiffd(originalAngle, newAngle);

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
        GroundSurfaceCheck();

        Detach();
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
        var timeScale = timeStep / DefaultFixedDeltaTime;

        if (Grounded)
        {
            var prevVg = Vg;

            // Friction from deceleration
            if (!_leftKeyDown && !_rightKeyDown)
            {
                if (Vg != 0.0f && Mathf.Abs(Vg) < GroundDeceleration)
                {
                    Vg = 0.0f;
                }
                else if (Vg > 0.0f)
                {
                    Vg -= GroundDeceleration * timeScale;
                }
                else if (Vg < 0.0f)
                {
                    Vg += GroundDeceleration * timeScale;
                }
            }

            // Slope gravity
            var slopeForce = 0.0f;

            if (Mathf.Abs(AngleDiffd(SurfaceAngle, 0.0f)) > SlopeGravityBeginAngle)
            {
                slopeForce = SlopeGravity * Mathf.Sin(SurfaceAngle * Mathf.Deg2Rad);
                Vg -= slopeForce * timeScale;
            }

            // Speed limit
            if (Vg > MaxSpeed) Vg = MaxSpeed;
            else if (Vg < -MaxSpeed) Vg = -MaxSpeed;

            if (Mathf.Abs(slopeForce) > GroundAcceleration)
            {
                if (_rightKeyDown && prevVg > 0.0f && Vg < 0.0f) LockHorizontal();
                else if (_leftKeyDown && prevVg < 0.0f && Vg > 0.0f) LockHorizontal();
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
        Footing = Side.None;
        LockUponLanding = lockUponLanding;
    }

    /// <summary>
    /// Attaches the player to a surface within the reach of its surface sensors. The angle of attachment
    /// need not be perfect; the method works reliably for angles within 45 degrees of the one specified.
    /// </summary>
    /// <param name="groundSpeed">The ground speed of the player after attaching.</param>
    /// <param name="angleRadians">The angle of the surface, in radians.</param>
    private void Attach(float groundSpeed, float angleRadians)
    {
        var angleDegrees = Modp(angleRadians * Mathf.Rad2Deg, 360.0f);
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

        RotateTo(transform, angleRadians);
    }

    /// <summary>
    /// Calculates the ground velocity as the result of an impact on the specified surface angle.
    /// </summary>
    /// <returns>Whether the player should attach to the specified incline.</returns>
    /// <param name="angleRadians">The angle of the surface impacted, in radians.</param>
    private bool HandleImpact(float angleRadians)
    {
        var sAngled = Modp(angleRadians * Mathf.Rad2Deg, 360.0f);
        var sAngler = sAngled * Mathf.Deg2Rad;

        // The player can't possibly land on something if he's traveling 90 degrees
        // within the normal
        var surfaceNormal = Modp(sAngled + 90.0f, 360.0f);
        var playerAngle = Angle(new Vector2(Vx, Vy)) * Mathf.Rad2Deg;
        var surfaceDifference = AngleDiffd(playerAngle, surfaceNormal);
        if (Mathf.Abs(surfaceDifference) < 90.0f)
        {
            return false;
        }

        // Ground attachment
        if (Mathf.Abs(AngleDiffd(sAngled, 180.0f)) > MinFlatAttachAngle &&
            Mathf.Abs(AngleDiffd(sAngled, 90.0f)) > MinFlatAttachAngle &&
            Mathf.Abs(AngleDiffd(sAngled, 270.0f)) > MinFlatAttachAngle)
        {
            float groundSpeed;
            if (Vy > 0.0f && (Equalsf(sAngled, 0.0f, MinFlatAttachAngle) || (Equalsf(sAngled, 180.0f, MinFlatAttachAngle))))
            {
                groundSpeed = Vx;
                Attach(groundSpeed, sAngler);
                return true;
            }
            // groundspeed = (airspeed) * (angular difference between air direction and surface normal direction) / (90 degrees)
            groundSpeed = Mathf.Sqrt(Vx * Vx + Vy * Vy) *
                          -Mathf.Clamp(AngleDiffd(Mathf.Atan2(Vy, Vx) * Mathf.Rad2Deg, sAngled - 90.0f) / 90.0f, -1.0f, 1.0f);

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
    /// Locks the player's horizontal control on the ground for the time specified by HorizontalLockTime.
    /// </summary>
    private void LockHorizontal()
    {
        HorizontalLock = true;
        HorizontalLockTimer = HorizontalLockTime;
    }

    /// <summary>
    /// Gets data about the surface closest to the player's feet, including its Side and raycast info.
    /// </summary>
    /// <returns>The surface.</returns>
    /// <param name="layerMask">A mask indicating what layers are surfaces.</param>
    public SurfaceInfo GetSurface(int layerMask)
    {
        // Linecasts are straight vertical or horizontal from the ground sensors
        var checkLeft = SurfaceCast(Side.Left);
        var checkRight = SurfaceCast(Side.Right);

        if (checkLeft && checkRight)
        {
            // If both sensors have surfaces, return the one with the highest based on wallmode
            if (Wallmode == Orientation.Floor && checkLeft.point.y > checkRight.point.y ||
               Wallmode == Orientation.Ceiling && checkLeft.point.y < checkRight.point.y ||
               Wallmode == Orientation.Right && checkLeft.point.x < checkRight.point.x ||
               Wallmode == Orientation.Left && checkLeft.point.x > checkRight.point.x)
                return new SurfaceInfo(checkLeft, checkRight, Side.Left);
            return new SurfaceInfo(checkLeft, checkRight, Side.Right);
        }
        if (checkLeft)
        {
            return new SurfaceInfo(checkLeft, checkRight, Side.Left);
        }
        if (checkRight)
        {
            return new SurfaceInfo(checkLeft, checkRight, Side.Right);
        }

        return default(SurfaceInfo);
    }

    private RaycastHit2D GroundCast(Side Side)
    {
        RaycastHit2D cast;
        if (Side == Side.Left)
        {
            cast = Physics2D.Linecast((Vector2)SensorBottomLeft.position - Wallmode.UnitVector() * LedgeClimbHeight,
                SensorBottomLeft.position,
                TerrainMask);
        }
        else
        {
            cast = Physics2D.Linecast((Vector2)SensorBottomRight.position - Wallmode.UnitVector() * LedgeClimbHeight,
                SensorBottomRight.position,
                TerrainMask);
        }

        return cast;
    }

    /// <summary>
    /// Returns the result of a linecast from the specified Side onto the surface based on wallmode.
    /// </summary>
    /// <returns>The result of the linecast.</returns>
    /// <param name="footing">The side to linecast from.</param>
    private RaycastHit2D SurfaceCast(Side footing)
    {
        RaycastHit2D cast;
        if (footing == Side.Left)
        {
            // Cast from the player's side to below the player's feet based on its wall mode (Wallmode)
            cast = Physics2D.Linecast((Vector2)SensorBottomLeft.position - Wallmode.UnitVector() * LedgeClimbHeight,
                                      (Vector2)SensorBottomLeft.position + Wallmode.UnitVector() * LedgeDropHeight,
                                      TerrainMask);

            if (!cast)
            {
                return default(RaycastHit2D);
            }
            if (Equalsf(cast.fraction, 0.0f))
            {
                for (var check = Wallmode.AdjacentCW(); check != Wallmode; check = check.AdjacentCW())
                {
                    cast = Physics2D.Linecast((Vector2)SensorBottomLeft.position - check.UnitVector() * LedgeClimbHeight,
                        (Vector2)SensorBottomLeft.position + check.UnitVector() * LedgeDropHeight,
                        TerrainMask);

                    if (cast && !Equalsf(cast.fraction, 0.0f))
                        return cast;
                }

                return default(RaycastHit2D);
            }

            return cast;
        }
        cast = Physics2D.Linecast((Vector2)SensorBottomRight.position - Wallmode.UnitVector() * LedgeClimbHeight,
            (Vector2)SensorBottomRight.position + Wallmode.UnitVector() * LedgeDropHeight,
            TerrainMask);

        if (!cast)
        {
            return default(RaycastHit2D);
        }
        if (Equalsf(cast.fraction, 0.0f))
        {
            for (var check = Wallmode.AdjacentCW(); check != Wallmode; check = check.AdjacentCW())
            {
                cast = Physics2D.Linecast((Vector2)SensorBottomRight.position - check.UnitVector() * LedgeClimbHeight,
                    (Vector2)SensorBottomRight.position + check.UnitVector() * LedgeDropHeight,
                    TerrainMask);

                if (cast && !Equalsf(cast.fraction, 0.0f))
                    return cast;
            }

            return default(RaycastHit2D);
        }

        return cast;
    }

    #region Collision Subroutines
    /// <summary>
    /// Collision check with side sensors for when player is in the air.
    /// </summary>
    /// <returns><c>true</c>, if a collision was found, <c>false</c> otherwise.</returns>
    private bool AirSideCheck()
    {
        var sideLeftCheck = Physics2D.Linecast(SensorMiddleMiddle.position, SensorMiddleLeft.position, TerrainMask);
        var sideRightCheck = Physics2D.Linecast(SensorMiddleMiddle.position, SensorMiddleRight.position, TerrainMask);

        if (sideLeftCheck)
        {
            if (!JustJumped)
            {
                Vx = 0;
            }

            transform.position += (Vector3)sideLeftCheck.point - SensorMiddleLeft.position +
                ((Vector3)sideLeftCheck.point - SensorMiddleLeft.position).normalized * Epsilon;
            return true;
        }
        if (sideRightCheck)
        {
            if (!JustJumped)
            {
                Vx = 0;
            }

            transform.position += (Vector3)sideRightCheck.point - SensorMiddleRight.position +
                                  ((Vector3)sideRightCheck.point - SensorMiddleRight.position).normalized * Epsilon;
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
        var cmem = new Collider2D[1];

        if (Physics2D.OverlapPointNonAlloc(SensorTopLeft.position, cmem, TerrainMask) > 0)
        {
            var horizontalCheck = Physics2D.Linecast(SensorTopMiddle.position, SensorTopLeft.position, TerrainMask);
            var verticalCheck = Physics2D.Linecast(SensorMiddleLeft.position, SensorTopLeft.position, TerrainMask);

            if (Vector2.Distance(horizontalCheck.point, SensorTopLeft.position) < Vector2.Distance(verticalCheck.point, SensorTopLeft.position))
            {
                transform.position += (Vector3)horizontalCheck.point - SensorTopLeft.position;

                if (!JustDetached) HandleImpact(Angle(horizontalCheck.normal) - HalfPi);
                if (Vy > 0) Vy = 0;
            }
            else
            {
                transform.position += (Vector3)verticalCheck.point - SensorTopLeft.position;

                if (!JustDetached) HandleImpact(Angle(verticalCheck.normal) - HalfPi);
                if (Vy > 0) Vy = 0;
            }
            return true;

        }
        if (Physics2D.OverlapPointNonAlloc(SensorTopRight.position, cmem, TerrainMask) > 0)
        {
            var horizontalCheck = Physics2D.Linecast(SensorTopMiddle.position, SensorTopRight.position, TerrainMask);
            var verticalCheck = Physics2D.Linecast(SensorMiddleRight.position, SensorTopRight.position, TerrainMask);

            if (Vector2.Distance(horizontalCheck.point, SensorTopRight.position) <
                Vector2.Distance(verticalCheck.point, SensorTopRight.position))
            {
                transform.position += (Vector3)horizontalCheck.point - SensorTopRight.position;

                if (!JustDetached) HandleImpact(Angle(horizontalCheck.normal) - HalfPi);
                if (Vy > 0) Vy = 0;
            }
            else
            {
                transform.position += (Vector3)verticalCheck.point - SensorTopRight.position;

                if (!JustDetached) HandleImpact(Angle(verticalCheck.normal) - HalfPi);
                if (Vy > 0) Vy = 0;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Collision check with ground sensors for when player is in the air.
    /// </summary>
    /// <returns><c>true</c>, if a collision was found, <c>false</c> otherwise.</returns>
    private bool AirGroundCheck()
    {
        var groundLeftCheck = GroundCast(Side.Left);
        var groundRightCheck = GroundCast(Side.Right);

        if (groundLeftCheck || groundRightCheck)
        {
            if (JustJumped)
            {
                if (groundLeftCheck)
                {
                    transform.position += (Vector3)groundLeftCheck.point - SensorBottomLeft.position;
                }
                if (groundRightCheck)
                {
                    transform.position += (Vector3)groundRightCheck.point - SensorBottomRight.position;
                }
            }
            else
            {
                if (groundLeftCheck && groundRightCheck)
                {
                    if (groundLeftCheck.point.y > groundRightCheck.point.y)
                    {
                        HandleImpact(Angle(groundLeftCheck.normal) - HalfPi);
                    }
                    else
                    {
                        HandleImpact(Angle(groundRightCheck.normal) - HalfPi);
                    }
                }
                else if (groundLeftCheck)
                {
                    HandleImpact(Angle(groundLeftCheck.normal) - HalfPi);
                }
                else
                {
                    HandleImpact(Angle(groundRightCheck.normal) - HalfPi);
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
        var sideLeftCheck = Physics2D.Linecast(SensorMiddleMiddle.position, SensorMiddleLeft.position, TerrainMask);
        var sideRightCheck = Physics2D.Linecast(SensorMiddleMiddle.position, SensorMiddleRight.position, TerrainMask);

        if (sideLeftCheck)
        {
            Vg = 0;
            transform.position += (Vector3)sideLeftCheck.point - SensorMiddleLeft.position +
                ((Vector3)sideLeftCheck.point - SensorMiddleLeft.position).normalized * Epsilon;

            // If running down a wall and hits the floor, orient the player onto the floor
            if (Wallmode == Orientation.Right)
            {
                RotateBy(transform, -90.0f);
                Wallmode = Orientation.Floor;
            }

            return true;
        }
        if (sideRightCheck)
        {
            Vg = 0;
            transform.position += (Vector3)sideRightCheck.point - SensorMiddleRight.position +
                                  ((Vector3)sideRightCheck.point - SensorMiddleRight.position).normalized * Epsilon;

            // If running down a wall and hits the floor, orient the player onto the floor
            if (Wallmode == Orientation.Left)
            {
                RotateTo(transform, 90.0f);
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
        var ceilLeftCheck = Physics2D.Linecast(SensorTopMiddle.position, SensorTopLeft.position, TerrainMask);
        var ceilRightCheck = Physics2D.Linecast(SensorTopMiddle.position, SensorTopRight.position, TerrainMask);

        if (ceilLeftCheck)
        {
            Vg = 0;

            // Add epsilon to prevent sticky collisions
            transform.position += (Vector3)ceilLeftCheck.point - SensorTopLeft.position +
                ((Vector3)ceilLeftCheck.point - SensorTopLeft.position).normalized * Epsilon;

            return true;
        }
        if (ceilRightCheck)
        {
            Vg = 0;
            transform.position += (Vector3)ceilRightCheck.point - SensorTopRight.position +
                                  ((Vector3)ceilRightCheck.point - SensorTopRight.position).normalized * Epsilon;

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
                var rightDiff = AngleDiffd(s.RightSurfaceAngle * Mathf.Rad2Deg, LastSurfaceAngle);
                var leftDiff = AngleDiffd(s.LeftSurfaceAngle * Mathf.Rad2Deg, LastSurfaceAngle);
                var overlapDiff = AngleDiffr(s.LeftSurfaceAngle, s.RightSurfaceAngle) * Mathf.Rad2Deg;

                if (s.Side == Side.Left)
                {
                    // If the surface's angle is a small enough difference from that of the previous begin surface checks
                    if (_justLanded || Mathf.Abs(leftDiff) < MaxSurfaceAngleDifference)
                    {
                        // Check angle differences between feet for player rotation
                        if (Mathf.Abs(overlapDiff) > MinFlatOverlapRange && overlapDiff > MinOverlapAngle)
                        {
                            // If tolerable, rotate between the surfaces beneath the two feet
                            RotateTo(transform, Angle(s.RightCast.point - s.LeftCast.point), SensorBottomMiddle.position);
                            transform.position += (Vector3)s.LeftCast.point - SensorBottomLeft.position;
                            Footing = Side.Left;
                        }
                        else
                        {
                            // Else just rotate for the left foot
                            RotateTo(transform, s.LeftSurfaceAngle, s.LeftCast.point);
                            transform.position += (Vector3)s.LeftCast.point - SensorBottomLeft.position;
                            Footing = Side.Left;
                        }

                        // Else see if the other surface's angle is tolerable
                    }
                    else if (Mathf.Abs(rightDiff) < MaxSurfaceAngleDifference)
                    {
                        RotateTo(transform, s.RightSurfaceAngle, SensorBottomMiddle.position);
                        transform.position += (Vector3)s.RightCast.point - SensorBottomRight.position;
                        Footing = Side.Right;
                        // Else the surfaces are untolerable. detach from the surface
                    }
                    else
                    {
                        Detach();
                    }
                    // Same thing but with the other foot
                }
                else if (s.Side == Side.Right)
                {
                    if (_justLanded || Mathf.Abs(rightDiff) < MaxSurfaceAngleDifference)
                    {
                        if (Mathf.Abs(overlapDiff) > MinFlatOverlapRange && overlapDiff > MinOverlapAngle)
                        {
                            RotateTo(transform, Angle(s.RightCast.point - s.LeftCast.point), SensorBottomMiddle.position);
                            transform.position += (Vector3)s.RightCast.point - SensorBottomRight.position;
                            Footing = Side.Right;
                        }
                        else
                        {
                            RotateTo(transform, s.RightSurfaceAngle, s.RightCast.point);
                            transform.position += (Vector3)s.RightCast.point - SensorBottomRight.position;
                            Footing = Side.Right;
                        }

                    }
                    else if (Mathf.Abs(leftDiff) < MaxSurfaceAngleDifference)
                    {
                        RotateTo(transform, s.LeftSurfaceAngle, SensorBottomMiddle.position);
                        transform.position += (Vector3)s.LeftCast.point - SensorBottomLeft.position;
                        Footing = Side.Left;
                    }
                    else
                    {
                        Detach();
                    }
                }
            }
            else if (s.LeftCast)
            {
                var leftDiff = AngleDiffd(s.LeftSurfaceAngle * Mathf.Rad2Deg, LastSurfaceAngle);
                if (_justLanded || Mathf.Abs(leftDiff) < MaxSurfaceAngleDifference)
                {
                    RotateTo(transform, s.LeftSurfaceAngle, s.LeftCast.point);
                    transform.position += (Vector3)s.LeftCast.point - SensorBottomLeft.position;
                    Footing = Side.Left;
                }
                else
                {
                    Detach();
                }
            }
            else
            {
                var rightDiff = AngleDiffd(s.RightSurfaceAngle * Mathf.Rad2Deg, LastSurfaceAngle);
                if (_justLanded || Mathf.Abs(rightDiff) < MaxSurfaceAngleDifference)
                {
                    RotateTo(transform, s.RightSurfaceAngle, s.RightCast.point);
                    transform.position += (Vector3)s.RightCast.point - SensorBottomRight.position;
                    Footing = Side.Right;
                }
                else
                {
                    Detach();
                }
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
                        Mathf.Abs(AngleDiffd(LastSurfaceAngle, SurfaceAngle)) < MaxSurfaceAngleDifference))
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
    #endregion

    #region Math Utilities
    public static float AngleDiffd(float a, float b)
    {
        return Modp(b - a + 180.0f, 360.0f) - 180.0f;
    }

    public static float AngleDiffr(float a, float b)
    {
        return Modp(b - a + Mathf.PI, Mathf.PI * 2.0f) - Mathf.PI;
    }

    public static float Modp(float dividend, float divisor)
    {
        return ((dividend % divisor) + divisor) % divisor;
    }

    public static bool Equalsf(float a, float b, float epsilon = Epsilon)
    {
        return (a >= b - Epsilon && a <= b + epsilon);
    }

    /// <summary>
    /// Returns the angle of the specified vector in radians.
    /// </summary>
    /// <param name="a">The vector.</param>
    public static float Angle(Vector2 a)
    {
        return Mathf.Atan2(a.y, a.x);
    }

    /// <summary>
    /// Returns the positive vertical distance between a and b if a is higher than b or the negative
    /// vertical distance if the opposite is true.
    /// </summary>
    /// <param name="a">The point a.</param>
    /// <param name="b">The point b.</param>
    public static float Highest(Vector2 a, Vector2 b)
    {
        return Highest(a, b, Mathf.PI / 2);
    }

    /// <summary>
    /// Returns the positive distance between a and b projected onto the axis in the speicifed direction
    /// if a is higher than b on that axis or the negative distance if the opposite is true.
    /// </summary>
    /// <param name="a">The point a.</param>
    /// <param name="b">The point b.</param>
    /// <param name="angle">The positive distance between a and b if a is higher than b in the specified
    /// direction or the negative distance if the opposite is true.</param>
    public static float Highest(Vector2 a, Vector2 b, float angle)
    {
        Vector2 diff = Projection(a, angle) - Projection(b, angle);
        return (Mathf.Abs(Angle(diff) - angle) < 1.57f) ? diff.magnitude : -diff.magnitude;
    }

    /// <summary>
    /// Determines if a is perpendicular to b.
    /// </summary>
    /// <returns><c>true</c> if a is perpendicular to b, otherwise <c>false</c>.</returns>
    /// <param name="a">The vector a.</param>
    /// <param name="b">The vector b.</param>
    public static bool IsPerp(Vector2 a, Vector2 b)
    {
        return Equalsf(0.0f, Vector2.Dot(a, b));
    }

    /// <summary>
    /// Determines if the line defined by the points a1 and a2 is perpendicular to the line defined by
    /// the points b1 and b2.
    /// </summary>
    /// <returns><c>true</c> if the line defined by the points a1 and a2 is perpendicular to the line defined by
    /// the points b1 and b2, otherwise <c>false</c>.</returns>
    /// <param name="a1">The point a1 that defines a line with a2.</param>
    /// <param name="a2">The point a2 that defines a line with a1.</param>
    /// <param name="b1">The point b1 that defines a line with b2.</param>
    /// <param name="b2">The point b2 that defines a line with b1.</param>
    public static bool IsPerp(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
    {
        return IsPerp(a2 - a1, b2 - b1);
    }

    /// <summary>
    /// Projects the point q onto a line at the origin at the specified angle.
    /// </summary>
    /// <param name="q">The point q.</param>
    /// <param name="angle">The angle of the line.</param>
    public static Vector2 Projection(Vector2 q, float angle)
    {
        return Projection(q, new Vector2(), angle);
    }

    /// <summary>
    /// Projects the point q onto a line which intersects the point p and continues in the specified angle.
    /// </summary>
    /// <param name="q">The point q.</param>
    /// <param name="p">The point p.</param>
    /// <param name="angle">The angle of the line.</param>
    public static Vector2 Projection(Vector2 q, Vector2 p, float angle)
    {
        return Projection(q, p, p + (new Vector2(Mathf.Cos(angle), Mathf.Sin(angle))));
    }

    /// <summary>
    /// Projects the point q onto a line defined by the points lineA and lineB.
    /// </summary>
    /// <param name="q">The point q.</param>
    /// <param name="lineA">The point lineA which defines a line with lineB.</param>
    /// <param name="lineB">The point lineB which defines a line with lineA.</param>
    public static Vector2 Projection(Vector2 q, Vector2 lineA, Vector2 lineB)
    {
        Vector2 ab = lineB - lineA;
        return lineA + ((Vector2.Dot(q - lineA, ab) / Vector2.Dot(ab, ab)) * ab);
    }

    /// <summary>
    /// Rotates the point by the angle about (0, 0).
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="angle">The angle in radians.</param>
    public static Vector2 RotateBy(Vector2 point, float angle)
    {
        return RotateBy(point, angle, new Vector2(0.0f, 0.0f));
    }

    /// <summary>
    /// Rotates the point by the angle about the specified origin.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="angle">The angle in radians.</param>
    /// <param name="origin">The origin.</param>
    public static Vector2 RotateBy(Vector2 point, float angle, Vector2 origin)
    {
        float s = Mathf.Sin(angle);
        float c = Mathf.Cos(angle);

        Vector2 npoint = point - origin;

        return new Vector2(npoint.x * c - npoint.y * s + origin.x, npoint.x * s + npoint.y * c + origin.y);
    }

    /// <summary>
    /// Rotates the transform by the angle about (0, 0).
    /// </summary>
    /// <param name="transform">The transform.</param>
    /// <param name="angle">The angle in radians.</param>
    public static void RotateBy(Transform transform, float angle)
    {
        RotateBy(transform, angle, transform.position);
    }

    /// <summary>
    /// Rotates the transform by the angle about the specified origin.
    /// </summary>
    /// <param name="transform">The transform.</param>
    /// <param name="angle">The angle in radians.</param>
    /// <param name="origin">The origin.</param>
    public static void RotateBy(Transform transform, float angle, Vector2 origin)
    {
        transform.position = RotateBy(transform.position, angle, origin);
        transform.eulerAngles = new Vector3(0.0f, 0.0f, transform.eulerAngles.z + (angle * Mathf.Rad2Deg));
    }

    /// <summary>
    /// Rotates the point to the angle about (0, 0).
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="angle">The angle in radians.</param>
    public static Vector2 RotateTo(Vector2 point, float angle)
    {
        return RotateBy(point, angle - Angle(point));
    }

    /// <summary>
    /// Rotates the point to the angle about the specified origin.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="angle">The angle in radians.</param>
    /// <param name="origin">The origin.</param>
    public static Vector2 RotateTo(Vector2 point, float angle, Vector2 origin)
    {
        return RotateBy(point, angle - Mathf.Atan2(point.x - origin.x, point.y - origin.y), origin);
    }

    /// <summary>
    /// Rotates the transform to the angle about (0, 0).
    /// </summary>
    /// <param name="transform">The transform.</param>
    /// <param name="angle">The angle in radians.</param>
    public static void RotateTo(Transform transform, float angle)
    {
        RotateTo(transform, angle, transform.position);
    }

    /// <summary>
    /// Rotates the transform to the angle about the specified origin.
    /// </summary>
    /// <param name="transform">The transform.</param>
    /// <param name="angle">The angle in radians.</param>
    /// <param name="origin">The origin.</param>
    public static void RotateTo(Transform transform, float angle, Vector2 origin)
    {
        transform.position = RotateTo(transform.position, angle, origin);
        transform.eulerAngles = new Vector3(0.0f, 0.0f, angle * Mathf.Rad2Deg);
    }

    /// <summary>
    /// Returns the midpoint between two points.
    /// </summary>
    /// <param name="a">The point a.</param>
    /// <param name="b">The point b.</param>
    public static Vector2 Midpoint(Vector2 a, Vector2 b)
    {
        return (a + b) / 2.0f;
    }
    #endregion
}

#region Utility Class Extensions
/// <summary>
/// Contains extension methods for HedgehogController utility classes
/// </summary>
public static class HedgehogControllerExtensions
{

    /// <summary>
    /// Returns the wall mode of a surface with the specified angle.
    /// </summary>
    /// <returns>The wall mode.</returns>
    /// <param name="angleRadians">The surface angle in radians.</param>
    public static HedgehogController.Orientation FromSurfaceAngle(float angleRadians)
    {
        float angle = HedgehogController.Modp(angleRadians, HedgehogController.DoublePi);

        if (angle <= Mathf.PI * 0.25f || angle > Mathf.PI * 1.75f)
            return HedgehogController.Orientation.Floor;
        else if (angle > Mathf.PI * 0.25f && angle <= Mathf.PI * 0.75f)
            return HedgehogController.Orientation.Right;
        else if (angle > Mathf.PI * 0.75f && angle <= Mathf.PI * 1.25f)
            return HedgehogController.Orientation.Ceiling;
        else
            return HedgehogController.Orientation.Left;
    }

    /// <summary>
    /// Returns a unit vector which represents the direction in which a wall mode points.
    /// </summary>
    /// <returns>The vector which represents the direction in which a wall mode points.</returns>
    /// <param name="orientation">The wall mode.</param>
    public static Vector2 UnitVector(this HedgehogController.Orientation orientation)
    {
        switch (orientation)
        {
            case HedgehogController.Orientation.Floor:
                return new Vector2(0.0f, -1.0f);

            case HedgehogController.Orientation.Ceiling:
                return new Vector2(0.0f, 1.0f);

            case HedgehogController.Orientation.Left:
                return new Vector2(-1.0f, 0.0f);

            case HedgehogController.Orientation.Right:
                return new Vector2(1.0f, 0.0f);

            default:
                return default(Vector2);
        }
    }

    /// <summary>
    /// Returns the normal angle in radians (-pi to pi) of a flat wall in the specified wall mode.
    /// </summary>
    /// <param name="orientation">The wall mode.</param>
    public static float Normal(this HedgehogController.Orientation orientation)
    {
        switch (orientation)
        {
            case HedgehogController.Orientation.Floor:
                return HedgehogController.HalfPi;

            case HedgehogController.Orientation.Right:
                return Mathf.PI;

            case HedgehogController.Orientation.Ceiling:
                return -HedgehogController.HalfPi;

            case HedgehogController.Orientation.Left:
                return 0.0f;

            default:
                return default(float);
        }
    }

    /// <summary>
    /// Returns the wall mode which points in the direction opposite to this one.
    /// </summary>
    /// <param name="orientation">The wall mode.</param>
    public static HedgehogController.Orientation Opposite(this HedgehogController.Orientation orientation)
    {
        switch (orientation)
        {
            case HedgehogController.Orientation.Floor:
                return HedgehogController.Orientation.Ceiling;

            case HedgehogController.Orientation.Right:
                return HedgehogController.Orientation.Left;

            case HedgehogController.Orientation.Ceiling:
                return HedgehogController.Orientation.Floor;

            case HedgehogController.Orientation.Left:
                return HedgehogController.Orientation.Right;

            default:
                return HedgehogController.Orientation.None;
        }
    }

    /// <summary>
    /// Returns the wall mode adjacent to this traveling clockwise.
    /// </summary>
    /// <returns>The adjacent wall mode.</returns>
    /// <param name="orientation">The given wall mode.</param>
    public static HedgehogController.Orientation AdjacentCW(this HedgehogController.Orientation orientation)
    {
        switch (orientation)
        {
            case HedgehogController.Orientation.Floor:
                return HedgehogController.Orientation.Left;

            case HedgehogController.Orientation.Right:
                return HedgehogController.Orientation.Floor;

            case HedgehogController.Orientation.Ceiling:
                return HedgehogController.Orientation.Right;

            case HedgehogController.Orientation.Left:
                return HedgehogController.Orientation.Ceiling;

            default:
                return HedgehogController.Orientation.None;
        }
    }

    /// <summary>
    /// Returns the wall mode adjacent to this traveling counter-clockwise.
    /// </summary>
    /// <returns>The adjacent wall mode.</returns>
    /// <param name="orientation">The given wall mode.</param>
    public static HedgehogController.Orientation AdjacentCCW(this HedgehogController.Orientation orientation)
    {
        switch (orientation)
        {
            case HedgehogController.Orientation.Floor:
                return HedgehogController.Orientation.Right;

            case HedgehogController.Orientation.Right:
                return HedgehogController.Orientation.Ceiling;

            case HedgehogController.Orientation.Ceiling:
                return HedgehogController.Orientation.Left;

            case HedgehogController.Orientation.Left:
                return HedgehogController.Orientation.Floor;

            default:
                return HedgehogController.Orientation.None;
        }
    }
}
#endregion