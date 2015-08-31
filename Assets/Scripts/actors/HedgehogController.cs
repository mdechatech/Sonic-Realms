using UnityEngine;

/// <summary>
/// Controls the player.
/// </summary>
public class HedgehogController : MonoBehaviour
{
    #region Inspector Fields

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
    public Transform SensorGroundLeft;

    [SerializeField]
    [Tooltip("The bottom middle point.")]
    public Transform SensorGroundMid;

    [SerializeField]
    [Tooltip("The bottom right corner.")]
    public Transform SensorGroundRight;

    [SerializeField]
    [Tooltip("The left middle point.")]
    public Transform SensorSideLeft;

    [SerializeField]
    [Tooltip("The center point.")]
    public Transform SensorSideMid;

    [SerializeField]
    [Tooltip("The right middle point.")]
    public Transform SensorSideRight;

    [SerializeField]
    [Tooltip("The top left corner.")]
    public Transform SensorCeilingLeft;

    [SerializeField]
    [Tooltip("The top middle point.")]
    public Transform SensorCeilingMid;

    [SerializeField]
    [Tooltip("The top right corner.")]
    public Transform SensorCeilingRight;

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
    public float LedgeHeightMax = 0.25f;

    /// <summary>
    /// The maximum depth of a surface below the player's feet that it can drop to without hindrance.
    /// </summary>
    [SerializeField]
    public float SurfaceDepthMax = 0.25f;

    /// <summary>
    /// The minimum speed in units per second the player must be moving at to stagger each physics update,
    /// processing the movement in fractions.
    /// </summary>
    [SerializeField]
    public float StaggerSpeedMax = 5.0f;

    /// <summary>
    /// The minimum angle of an incline at which slope gravity is applied.
    /// </summary>
    [SerializeField]
    public float SlopeGravityAngleMin = 10.0f;

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
    public float SurfaceAngleDiffMax = 70.0f;

    /// <summary>
    /// The amount in degrees past the threshold of changing wall mode that the player
    /// can go.
    /// </summary>
    [SerializeField]
    public float WallModeSwitchAngle = 0.0f;

    /// <summary>
    /// The speed in units per second at which the player must be moving to be able to switch wall modes.
    /// </summary>
    [SerializeField]
    public float WallModeSwitchSpeed = 0.5f;

    /// <summary>
    /// The minimum difference in angle between the surface sensor and the overlap sensor
    /// to have the player's rotation account for it.
    /// </summary>
    [SerializeField]
    public float OverlapAngleMin = -40.0f;

    /// <summary>
    /// The minimum absolute difference in angle between the surface sensor and the overlap
    /// sensor to have the player's rotation account for it.
    /// </summary>
    [SerializeField]
    public float OverlapAngleMinAbs = 7.5f;

    /// <summary>
    /// The minimum angle of an incline from a ceiling or left or right wall for a player to be able to
    /// attach to it from the air.
    /// </summary>
    [SerializeField]
    public float AttachAngleMin = 5.0f;

    /// <summary>
    /// The maximum surface angle difference from a vertical wall at which a player is able to detach from
    /// through use of the directional key opposite to the one in which it is traveling.
    /// </summary>
    [SerializeField]
    public float VerticalDetachAngleMax = 5.0f;
    #endregion

    /// <summary>
    /// Whether the left key was held down since the last update. Key is determined by
    /// Settings.LeftKey.
    /// </summary>
    [HideInInspector]
    private bool _leftKeyDown;

    /// <summary>
    /// Whether the right key was held down since the last update. Key is determined by
    /// Settings.RightKey.
    /// </summary>
    [HideInInspector]
    private bool _rightKeyDown;

    /// <summary>
    /// Whether the jump key was pressed since the last update. Key is determined by
    /// Settings.JumpKey.
    /// </summary>
    [HideInInspector]
    private bool _jumpKeyDown;

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
    /// Represents the current orientation of the player.
    /// </summary>
    [HideInInspector]
    public WallMode WallMode;

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
                (1 << LayerMask.NameToLayer(Settings.LayerTerrain)) |
                (1 << LayerMask.NameToLayer(Settings.LayerTerrain + " " + TerrainLayer));
        }
    }

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
            LeftSurfaceAngle = (leftCast) ? leftCast.normal.Angle() - AMath.HALF_PI : 0.0f;
            RightCast = rightCast;
            RightSurfaceAngle = (rightCast) ? rightCast.normal.Angle() - AMath.HALF_PI : 0.0f;
            this.Side = Side;
        }
    }

    /// <summary>
    /// Represents each sensor on the bottom of the player.
    /// </summary>
    public enum Side
    {
        None, Left, Right
    }

    public void Awake()
    {
        Footing = Side.None;
        Grounded = false;
        OnMovingPlatform = false;
        Vx = Vy = Vg = 0.0f;
        LastSurfaceAngle = 0.0f;
        _leftKeyDown = _rightKeyDown = _jumpKeyDown = false;
        JustJumped = _justLanded = JustDetached = false;
        WallMode = WallMode.Floor;
        SurfaceAngle = 0.0f;
        Layer = 1;

        GetComponent<Collider2D>().isTrigger = true;
    }

    public void Start()
    {

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
        _leftKeyDown = Input.GetKey(Settings.LeftKey);
        _rightKeyDown = Input.GetKey(Settings.RightKey);
        if (!_jumpKeyDown && Grounded) _jumpKeyDown = Input.GetKeyDown(Settings.JumpKey);
    }


    public void FixedUpdate()
    {
        HandleInput(Time.fixedDeltaTime);

        // Stagger routine - if the player's gotta go fast, move it in increments of StaggerSpeedThreshold to prevent tunneling
        var vt = Mathf.Sqrt(Vx * Vx + Vy * Vy);

        if (vt < StaggerSpeedMax)
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
                if (vc > StaggerSpeedMax)
                {
                    HandleForces(Time.fixedDeltaTime * (StaggerSpeedMax / vt));
                    transform.position += (new Vector3(Vx * Time.fixedDeltaTime, Vy * Time.fixedDeltaTime)) * (StaggerSpeedMax / vt);
                    vc -= StaggerSpeedMax;
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
        var timeScale = timeStep / Settings.DefaultFixedDeltaTime;

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
                if (Vg > 0 && Mathf.Abs(AMath.AngleDiffd(SurfaceAngle, 90.0f)) < VerticalDetachAngleMax)
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
                if (Vg < 0 && Mathf.Abs(AMath.AngleDiffd(SurfaceAngle, 270.0f)) < VerticalDetachAngleMax)
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

            if (Input.GetKey(KeyCode.Space)) Vg = MaxSpeed * Mathf.Sign(Vg);

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
        var originalAngle = AMath.Modp((new Vector2(Vx, Vy)).Angle() * Mathf.Rad2Deg, 360.0f);

        var surfaceNormal = (SurfaceAngle + 90.0f) * Mathf.Deg2Rad;
        Vx += JumpSpeed * Mathf.Cos(surfaceNormal);
        Vy += JumpSpeed * Mathf.Sin(surfaceNormal);

        var newAngle = AMath.Modp((new Vector2(Vx, Vy)).Angle() * Mathf.Rad2Deg, 360.0f);
        var angleDifference = AMath.AngleDiffd(originalAngle, newAngle);

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
        var timeScale = timeStep / Settings.DefaultFixedDeltaTime;

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

            if (Mathf.Abs(AMath.AngleDiffd(SurfaceAngle, 0.0f)) > SlopeGravityAngleMin)
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
            if (SurfaceAngle > 90.0f - VerticalDetachAngleMax &&
               SurfaceAngle < 270.0f + VerticalDetachAngleMax &&
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
        WallMode = WallMode.Floor;
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
        var angleDegrees = AMath.Modp(angleRadians * Mathf.Rad2Deg, 360.0f);
        Vg = groundSpeed;
        SurfaceAngle = LastSurfaceAngle = angleDegrees;
        Grounded = _justLanded = true;

        // WallModeSwitchAngle may be set to only attach right or left at extreme angles
        if (SurfaceAngle < 45.0f + WallModeSwitchAngle || SurfaceAngle > 315.0f - WallModeSwitchAngle)
            WallMode = WallMode.Floor;

        else if (SurfaceAngle > 135.0f - WallModeSwitchAngle && SurfaceAngle < 225.0 + WallModeSwitchAngle)
            WallMode = WallMode.Ceiling;

        else if (SurfaceAngle > 45.0f + WallModeSwitchAngle && SurfaceAngle < 135.0f - WallModeSwitchAngle)
            WallMode = WallMode.Right;

        else
            WallMode = WallMode.Left;

        if (LockUponLanding)
        {
            LockUponLanding = false;
            LockHorizontal();
        }

        transform.RotateTo(angleRadians);
    }

    /// <summary>
    /// Calculates the ground velocity as the result of an impact on the specified surface angle.
    /// </summary>
    /// <returns>Whether the player should attach to the specified incline.</returns>
    /// <param name="angleRadians">The angle of the surface impacted, in radians.</param>
    private bool HandleImpact(float angleRadians)
    {
        var sAngled = AMath.Modp(angleRadians * Mathf.Rad2Deg, 360.0f);
        var sAngler = sAngled * Mathf.Deg2Rad;

        // The player can't possibly land on something if he's traveling 90 degrees
        // within the normal
        var surfaceNormal = AMath.Modp(sAngled + 90.0f, 360.0f);
        var playerAngle = (new Vector2(Vx, Vy)).Angle() * Mathf.Rad2Deg;
        var surfaceDifference = AMath.AngleDiffd(playerAngle, surfaceNormal);
        if (Mathf.Abs(surfaceDifference) < 90.0f)
        {
            return false;
        }

        // Ground attachment
        if (Mathf.Abs(AMath.AngleDiffd(sAngled, 180.0f)) > AttachAngleMin &&
            Mathf.Abs(AMath.AngleDiffd(sAngled, 90.0f)) > AttachAngleMin &&
            Mathf.Abs(AMath.AngleDiffd(sAngled, 270.0f)) > AttachAngleMin)
        {
            float groundSpeed;
            if (Vy > 0.0f && (AMath.Equalsf(sAngled, 0.0f, AttachAngleMin) || (AMath.Equalsf(sAngled, 180.0f, AttachAngleMin))))
            {
                groundSpeed = Vx;
                Attach(groundSpeed, sAngler);
                return true;
            }
            // groundspeed = (airspeed) * (angular difference between air direction and surface normal direction) / (90 degrees)
            groundSpeed = Mathf.Sqrt(Vx * Vx + Vy * Vy) *
                          -AMath.Clamp(AMath.AngleDiffd(Mathf.Atan2(Vy, Vx) * Mathf.Rad2Deg, sAngled - 90.0f) / 90.0f, -1.0f, 1.0f);

            if (sAngled > 90.0f - VerticalDetachAngleMax &&
                sAngled < 270.0f + VerticalDetachAngleMax &&
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
            if (WallMode == WallMode.Floor && checkLeft.point.y > checkRight.point.y ||
               WallMode == WallMode.Ceiling && checkLeft.point.y < checkRight.point.y ||
               WallMode == WallMode.Right && checkLeft.point.x < checkRight.point.x ||
               WallMode == WallMode.Left && checkLeft.point.x > checkRight.point.x)
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
            cast = Physics2D.Linecast((Vector2)SensorGroundLeft.position - WallMode.UnitVector() * LedgeHeightMax,
                SensorGroundLeft.position,
                TerrainMask);
        }
        else
        {
            cast = Physics2D.Linecast((Vector2)SensorGroundRight.position - WallMode.UnitVector() * LedgeHeightMax,
                SensorGroundRight.position,
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
            // Cast from the player's side to below the player's feet based on its wall mode (orientation)
            cast = Physics2D.Linecast((Vector2)SensorGroundLeft.position - WallMode.UnitVector() * LedgeHeightMax,
                                      (Vector2)SensorGroundLeft.position + WallMode.UnitVector() * SurfaceDepthMax,
                                      TerrainMask);

            if (!cast)
            {
                return default(RaycastHit2D);
            }
            if (AMath.Equalsf(cast.fraction, 0.0f))
            {
                for (var check = WallMode.AdjacentCW(); check != WallMode; check = check.AdjacentCW())
                {
                    cast = Physics2D.Linecast((Vector2)SensorGroundLeft.position - check.UnitVector() * LedgeHeightMax,
                        (Vector2)SensorGroundLeft.position + check.UnitVector() * SurfaceDepthMax,
                        TerrainMask);

                    if (cast && !AMath.Equalsf(cast.fraction, 0.0f))
                        return cast;
                }

                return default(RaycastHit2D);
            }

            return cast;
        }
        cast = Physics2D.Linecast((Vector2)SensorGroundRight.position - WallMode.UnitVector() * LedgeHeightMax,
            (Vector2)SensorGroundRight.position + WallMode.UnitVector() * SurfaceDepthMax,
            TerrainMask);

        if (!cast)
        {
            return default(RaycastHit2D);
        }
        if (AMath.Equalsf(cast.fraction, 0.0f))
        {
            for (var check = WallMode.AdjacentCW(); check != WallMode; check = check.AdjacentCW())
            {
                cast = Physics2D.Linecast((Vector2)SensorGroundRight.position - check.UnitVector() * LedgeHeightMax,
                    (Vector2)SensorGroundRight.position + check.UnitVector() * SurfaceDepthMax,
                    TerrainMask);

                if (cast && !AMath.Equalsf(cast.fraction, 0.0f))
                    return cast;
            }

            return default(RaycastHit2D);
        }

        return cast;
    }

    /// COLLISION SUBROUTINES

    /// <summary>
    /// Collision check with side sensors for when player is in the air.
    /// </summary>
    /// <returns><c>true</c>, if a collision was found, <c>false</c> otherwise.</returns>
    private bool AirSideCheck()
    {
        var sideLeftCheck = Physics2D.Linecast(SensorSideMid.position, SensorSideLeft.position, TerrainMask);
        var sideRightCheck = Physics2D.Linecast(SensorSideMid.position, SensorSideRight.position, TerrainMask);

        if (sideLeftCheck)
        {
            if (!JustJumped)
            {
                Vx = 0;
            }

            transform.position += (Vector3)sideLeftCheck.point - SensorSideLeft.position +
                ((Vector3)sideLeftCheck.point - SensorSideLeft.position).normalized * AMath.Epsilon;
            return true;
        }
        if (sideRightCheck)
        {
            if (!JustJumped)
            {
                Vx = 0;
            }

            transform.position += (Vector3)sideRightCheck.point - SensorSideRight.position +
                                  ((Vector3)sideRightCheck.point - SensorSideRight.position).normalized * AMath.Epsilon;
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

        if (Physics2D.OverlapPointNonAlloc(SensorCeilingLeft.position, cmem, TerrainMask) > 0)
        {
            var horizontalCheck = Physics2D.Linecast(SensorCeilingMid.position, SensorCeilingLeft.position, TerrainMask);
            var verticalCheck = Physics2D.Linecast(SensorSideLeft.position, SensorCeilingLeft.position, TerrainMask);

            if (Vector2.Distance(horizontalCheck.point, SensorCeilingLeft.position) < Vector2.Distance(verticalCheck.point, SensorCeilingLeft.position))
            {
                transform.position += (Vector3)horizontalCheck.point - SensorCeilingLeft.position;

                if (!JustDetached) HandleImpact(horizontalCheck.normal.Angle() - AMath.HALF_PI);
                if (Vy > 0) Vy = 0;
            }
            else
            {
                transform.position += (Vector3)verticalCheck.point - SensorCeilingLeft.position;

                if (!JustDetached) HandleImpact(verticalCheck.normal.Angle() - AMath.HALF_PI);
                if (Vy > 0) Vy = 0;
            }
            return true;

        }
        if (Physics2D.OverlapPointNonAlloc(SensorCeilingRight.position, cmem, TerrainMask) > 0)
        {
            var horizontalCheck = Physics2D.Linecast(SensorCeilingMid.position, SensorCeilingRight.position, TerrainMask);
            var verticalCheck = Physics2D.Linecast(SensorSideRight.position, SensorCeilingRight.position, TerrainMask);

            if (Vector2.Distance(horizontalCheck.point, SensorCeilingRight.position) <
                Vector2.Distance(verticalCheck.point, SensorCeilingRight.position))
            {
                transform.position += (Vector3)horizontalCheck.point - SensorCeilingRight.position;

                if (!JustDetached) HandleImpact(horizontalCheck.normal.Angle() - AMath.HALF_PI);
                if (Vy > 0) Vy = 0;
            }
            else
            {
                transform.position += (Vector3)verticalCheck.point - SensorCeilingRight.position;

                if (!JustDetached) HandleImpact(verticalCheck.normal.Angle() - AMath.HALF_PI);
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
                    transform.position += (Vector3)groundLeftCheck.point - SensorGroundLeft.position;
                }
                if (groundRightCheck)
                {
                    transform.position += (Vector3)groundRightCheck.point - SensorGroundRight.position;
                }
            }
            else
            {
                if (groundLeftCheck && groundRightCheck)
                {
                    if (groundLeftCheck.point.y > groundRightCheck.point.y)
                    {
                        HandleImpact(groundLeftCheck.normal.Angle() - AMath.HALF_PI);
                    }
                    else
                    {
                        HandleImpact(groundRightCheck.normal.Angle() - AMath.HALF_PI);
                    }
                }
                else if (groundLeftCheck)
                {
                    HandleImpact(groundLeftCheck.normal.Angle() - AMath.HALF_PI);
                }
                else
                {
                    HandleImpact(groundRightCheck.normal.Angle() - AMath.HALF_PI);
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
        var sideLeftCheck = Physics2D.Linecast(SensorSideMid.position, SensorSideLeft.position, TerrainMask);
        var sideRightCheck = Physics2D.Linecast(SensorSideMid.position, SensorSideRight.position, TerrainMask);

        if (sideLeftCheck)
        {
            Vg = 0;
            transform.position += (Vector3)sideLeftCheck.point - SensorSideLeft.position +
                ((Vector3)sideLeftCheck.point - SensorSideLeft.position).normalized * AMath.Epsilon;

            // If running down a wall and hits the floor, orient the player onto the floor
            if (WallMode == WallMode.Right)
            {
                transform.RotateBy(-90.0f);
                WallMode = WallMode.Floor;
            }

            return true;
        }
        if (sideRightCheck)
        {
            Vg = 0;
            transform.position += (Vector3)sideRightCheck.point - SensorSideRight.position +
                                  ((Vector3)sideRightCheck.point - SensorSideRight.position).normalized * AMath.Epsilon;

            // If running down a wall and hits the floor, orient the player onto the floor
            if (WallMode == WallMode.Left)
            {
                transform.RotateTo(90.0f);
                WallMode = WallMode.Floor;
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
        var ceilLeftCheck = Physics2D.Linecast(SensorCeilingMid.position, SensorCeilingLeft.position, TerrainMask);
        var ceilRightCheck = Physics2D.Linecast(SensorCeilingMid.position, SensorCeilingRight.position, TerrainMask);

        if (ceilLeftCheck)
        {
            Vg = 0;

            // Add epsilon to prevent sticky collisions
            transform.position += (Vector3)ceilLeftCheck.point - SensorCeilingLeft.position +
                ((Vector3)ceilLeftCheck.point - SensorCeilingLeft.position).normalized * AMath.Epsilon;

            return true;
        }
        if (ceilRightCheck)
        {
            Vg = 0;
            transform.position += (Vector3)ceilRightCheck.point - SensorCeilingRight.position +
                                  ((Vector3)ceilRightCheck.point - SensorCeilingRight.position).normalized * AMath.Epsilon;

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
                var rightDiff = AMath.AngleDiffd(s.RightSurfaceAngle * Mathf.Rad2Deg, LastSurfaceAngle);
                var leftDiff = AMath.AngleDiffd(s.LeftSurfaceAngle * Mathf.Rad2Deg, LastSurfaceAngle);
                var overlapDiff = AMath.AngleDiffr(s.LeftSurfaceAngle, s.RightSurfaceAngle) * Mathf.Rad2Deg;

                if (s.Side == Side.Left)
                {
                    // If the surface's angle is a small enough difference from that of the previous begin surface checks
                    if (_justLanded || Mathf.Abs(leftDiff) < SurfaceAngleDiffMax)
                    {
                        // Check angle differences between feet for player rotation
                        if (Mathf.Abs(overlapDiff) > OverlapAngleMinAbs && overlapDiff > OverlapAngleMin)
                        {
                            // If tolerable, rotate between the surfaces beneath the two feet
                            transform.RotateTo((s.RightCast.point - s.LeftCast.point).Angle(), SensorGroundMid.position);
                            transform.position += (Vector3)s.LeftCast.point - SensorGroundLeft.position;
                            Footing = Side.Left;
                        }
                        else
                        {
                            // Else just rotate for the left foot
                            transform.RotateTo(s.LeftSurfaceAngle, s.LeftCast.point);
                            transform.position += (Vector3)s.LeftCast.point - SensorGroundLeft.position;
                            Footing = Side.Left;
                        }

                        if (s.LeftCast.collider.gameObject.tag == Settings.TagMovingPlatform)
                        {
                            // TODO moving platforms!??!
                        }
                        // Else see if the other surface's angle is tolerable
                    }
                    else if (Mathf.Abs(rightDiff) < SurfaceAngleDiffMax)
                    {
                        transform.RotateTo(s.RightSurfaceAngle, SensorGroundMid.position);
                        transform.position += (Vector3)s.RightCast.point - SensorGroundRight.position;
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
                    if (_justLanded || Mathf.Abs(rightDiff) < SurfaceAngleDiffMax)
                    {
                        if (Mathf.Abs(overlapDiff) > OverlapAngleMinAbs && overlapDiff > OverlapAngleMin)
                        {
                            transform.RotateTo((s.RightCast.point - s.LeftCast.point).Angle(), SensorGroundMid.position);
                            transform.position += (Vector3)s.RightCast.point - SensorGroundRight.position;
                            Footing = Side.Right;
                        }
                        else
                        {
                            transform.RotateTo(s.RightSurfaceAngle, s.RightCast.point);
                            transform.position += (Vector3)s.RightCast.point - SensorGroundRight.position;
                            Footing = Side.Right;
                        }

                    }
                    else if (Mathf.Abs(leftDiff) < SurfaceAngleDiffMax)
                    {
                        transform.RotateTo(s.LeftSurfaceAngle, SensorGroundMid.position);
                        transform.position += (Vector3)s.LeftCast.point - SensorGroundLeft.position;
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
                var leftDiff = AMath.AngleDiffd(s.LeftSurfaceAngle * Mathf.Rad2Deg, LastSurfaceAngle);
                if (_justLanded || Mathf.Abs(leftDiff) < SurfaceAngleDiffMax)
                {
                    transform.RotateTo(s.LeftSurfaceAngle, s.LeftCast.point);
                    transform.position += (Vector3)s.LeftCast.point - SensorGroundLeft.position;
                    Footing = Side.Left;
                }
                else
                {
                    Detach();
                }
            }
            else
            {
                var rightDiff = AMath.AngleDiffd(s.RightSurfaceAngle * Mathf.Rad2Deg, LastSurfaceAngle);
                if (_justLanded || Mathf.Abs(rightDiff) < SurfaceAngleDiffMax)
                {
                    transform.RotateTo(s.RightSurfaceAngle, s.RightCast.point);
                    transform.position += (Vector3)s.RightCast.point - SensorGroundRight.position;
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
                        Mathf.Abs(AMath.AngleDiffd(LastSurfaceAngle, SurfaceAngle)) < SurfaceAngleDiffMax))
        {
            if (WallMode == WallMode.Floor)
            {
                if (SurfaceAngle > 45.0f + WallModeSwitchAngle && SurfaceAngle < 180.0f) WallMode = WallMode.Right;
                else if (SurfaceAngle < 315.0f - WallModeSwitchAngle && SurfaceAngle > 180.0f) WallMode = WallMode.Left;
            }
            else if (WallMode == WallMode.Right)
            {
                if (SurfaceAngle > 135.0f + WallModeSwitchAngle) WallMode = WallMode.Ceiling;
                else if (SurfaceAngle < 45.0f - WallModeSwitchAngle) WallMode = WallMode.Floor;
            }
            else if (WallMode == WallMode.Ceiling)
            {
                if (SurfaceAngle > 225.0f + WallModeSwitchAngle) WallMode = WallMode.Left;
                else if (SurfaceAngle < 135.0f - WallModeSwitchAngle) WallMode = WallMode.Right;
            }
            else if (WallMode == WallMode.Left)
            {
                if (SurfaceAngle > 315.0f + WallModeSwitchAngle || SurfaceAngle < 180.0f) WallMode = WallMode.Floor;
                else if (SurfaceAngle < 225.0f - WallModeSwitchAngle) WallMode = WallMode.Ceiling;
            }

            Vx = Vg * Mathf.Cos(SurfaceAngle * Mathf.Deg2Rad);
            Vy = Vg * Mathf.Sin(SurfaceAngle * Mathf.Deg2Rad);

            _justLanded = false;
            return true;
        }

        return false;
    }
}