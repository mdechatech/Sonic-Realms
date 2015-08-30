using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the player.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Physics")]

    /// <summary>
    /// The acceleration by gravity in units per second per second.
    /// </summary>
    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("Acceleration in units per second squared, in the downward direction.")]
    private float gravity;

    /// <summary>
    /// The player's maximum speed in units per second.
    /// </summary>
    [SerializeField]
    [Range(0.0f, 100.0f)]
    [Tooltip("Maximum speed in units.")]
    private float maxSpeed;

    [Header("Control")]

    /// <summary>
    /// The player's ground acceleration in units per second per second.
    /// </summary>
    [SerializeField]
    [Range(0.0f, 0.25f)]
    [Tooltip("Ground acceleration in units per second squared.")]
    private float groundAcceleration;

    /// <summary>
    /// The player's friction on the ground in units per second per second.
    /// </summary>
    [SerializeField]
    [Range(0.0f, 0.25f)]
    [Tooltip("Ground deceleration in units per second squared.")]
    private float groundDeceleration;

    /// <summary>
    /// The speed of the player's jump in units per second.
    /// </summary>
    [SerializeField]
    [Range(0.0f, 25.0f)]
    [Tooltip("Jump speed in units per second.")]
    private float jumpSpeed;

    /// <summary>
    /// The player's horizontal acceleration in the air in units per second per second.
    /// </summary>
    [SerializeField]
    [Range(0.0f, 0.25f)]
    [Tooltip("Air acceleration in units per second squared.")]
    private float airAcceleration;

    [Header("Contacts")]

    /// <summary>
    /// If on a moving platform, a transform which is a child of the moving platform used
    /// to monitor changes in position and rotation.
    /// </summary>

    // Nine sensors arranged like a tic-tac-toe board.
    [SerializeField]
    [Tooltip("The player's left foot.")]
    private Transform sensorGroundLeft;

    [SerializeField]
    [Tooltip("The midpoint of the player's feet.")]
    private Transform sensorGroundMid;

    [SerializeField]
    [Tooltip("The player's right foot.")]
    private Transform sensorGroundRight;

    [SerializeField]
    [Tooltip("The player's left side, or the midpoint of its left foot and the left side of its head.")]
    private Transform sensorSideLeft;

    [SerializeField]
    [Tooltip("The midpoint of the player's sides.")]
    private Transform sensorSideMid;
    
    [SerializeField]
    [Tooltip("The player's right side, or the midpoint of its right foot and the right side of its head.")]
    private Transform sensorSideRight;

    [SerializeField]
    [Tooltip("The player's left side of its head.")]
    private Transform sensorCeilLeft;

    [SerializeField]
    [Tooltip("The midpoint of the player's head.")]
    private Transform sensorCeilMid;

    [SerializeField]
    [Tooltip("The player's right side of its head.")]
    private Transform sensorCeilRight;

	/// <summary>
	/// This point is added to the moving platform the player is standing on while noting
	/// changes in position, allowing the player to move with a moving platform.
	/// </summary>
	private GameObject movingPlatformAnchor;

    /// <summary>
    /// Whether the left key was held down since the last update. Key is determined by
    /// Settings.LeftKey.
    /// </summary>
    private bool leftKeyDown;
    
    /// <summary>
    /// Whether the right key was held down since the last update. Key is determined by
    /// Settings.RightKey.
    /// </summary>
    private bool rightKeyDown;
    
    /// <summary>
    /// Whether the jump key was pressed since the last update. Key is determined by
    /// Settings.JumpKey.
    /// </summary>
    private bool jumpKeyDown;

    /// <summary>
    /// The player's horizontal velocity in units per second.
    /// </summary>
    private float vx;
    
    /// <summary>
    /// The player's vertical velocity in units per second.
    /// </summary>
    private float vy;

    /// <summary>
    /// If grounded, the player's ground velocity in units per second.
    /// </summary>
    private float vg;

    /// <summary>
    /// Whether the player is on the ground.
    /// </summary>
    private bool grounded;

    /// <summary>
    /// Whether the player is on a moving platform.
    /// </summary>
    private bool onMovingPlatform;

    /// <summary>
    /// The layer mask which represents the ground the player checks for collision with.
    /// </summary>
    private int terrainMask;

    /// <summary>
    /// The number of the layer of terrain the player checks for collision with.
    /// </summary>
    private int terrainLayer;

    /// <summary>
    /// Whether the player has just landed on the ground. Is used to ignore surface angle
    /// once right after.
    /// </summary>
    private bool justLanded;

    /// <summary>
    /// If grounded and hasn't just landed, the angle of incline the player walked on
    /// one FixedUpdate ago.
    /// </summary>
    private float lastSurfaceAngle;
    
    /// <summary>
    /// If grounded, the angle of incline the player is walking on. Goes hand-in-hand
    /// with rotation.
    /// </summary>
    private float surfaceAngle;

    /// <summary>
    /// If grounded, which sensor on the player defines the primary surface.
    /// </summary>
    private Footing footing;

    /// <summary>
    /// Whether the player has control of horizontal ground movement.
    /// </summary>
    private bool horizontalLock;

    /// <summary>
    /// If horizontally locked, the time in seconds left on it.
    /// </summary>
    private float horizontalLockTimer;

    /// <summary>
    /// If not grounded, whether to activate the horizontal control lock when the player lands.
    /// </summary>
    private bool lockUponLanding;

    /// <summary>
    /// Whether the player has just jumped. Is used to avoid collisions right after.
    /// </summary>
    private bool justJumped;

    /// <summary>
    /// Whether the player has just detached. Is used to avoid reattachments right after.
    /// </summary>
    private bool justDetached;

	/// <summary>
	/// Represents the current orientation of the player.
	/// </summary>
	private WallMode wallMode;

	/// <summary>
	/// The amount in degrees past the threshold of changing wall mode that the player
	/// can go.
	/// </summary>
	private const float WallModeSwitchAngle = 0.0f;

	/// <summary>
	/// The speed in units per second at which the player must be moving to be able to switch wall modes.
	/// </summary>
	private const float WallModeSwitchSpeed = 0.5f;

    /// <summary>
    /// The maximum height of a ledge above the player's feet that it can climb without hindrance.
    /// </summary>
    private const float LedgeHeightMax = 0.25f;

    /// <summary>
    /// The maximum depth of a surface below the player's feet that it can drop to without hindrance.
    /// </summary>
    private const float SurfaceDepthMax = 0.25f;

	/// <summary>
	/// The maximum change in angle between two surfaces that the player can walk in.
	/// </summary>
	private const float SurfaceAngleDiffMax = 70.0f;

	/// <summary>
	/// The minimum difference in angle between the surface sensor and the overlap sensor
	/// to have the player's rotation account for it.
	/// </summary>
	private const float OverlapAngleMin = -40.0f;

    /// <summary>
    /// The minimum absolute difference in angle between the surface sensor and the overlap
    /// sensor to have the player's rotation account for it.
    /// </summary>
    private const float OverlapAngleMinAbs = 7.5f;

    /// <summary>
    /// The minimum speed in units per second the player must be moving at to stagger each physics update,
    /// processing the movement in fractions.
    /// </summary>
    private const float StaggerSpeedMax = 5.0f;

    /// <summary>
    /// The magnitude of the force applied to the player when going up or down slopes.
    /// </summary>
    private const float SlopeFactor = 0.3f;

    /// <summary>
    /// The minimum angle of an incline at which slope gravity is applied.
    /// </summary>
    private const float SlopeGravityAngleMin = 10.0f;

    /// <summary>
    /// The minimum angle of an incline from a ceiling or left or right wall for a player to be able to
    /// attach to it from the air.
    /// </summary>
    private const float AttachAngleMin = 5.0f;

    /// <summary>
    /// The speed in units per second below which the player must be traveling on a wall or ceiling to be
    /// detached from it.
    /// </summary>
    private const float DetachSpeed = 3.5f;

    /// <summary>
    /// The maximum surface angle difference from a vertical wall at which a player is able to detach from
    /// through use of the directional key opposite to the one in which it is traveling.
    /// </summary>
    private const float VerticalDetachAngleMax = 5.0f;

    /// <summary>
    /// The duration in seconds of the horizontal lock.
    /// </summary>
    private const float HorizontalLockTime = 0.25f;
    
    /// <summary>
    /// If the player is moving very quickly and jumps, normally it will not make much of a difference
    /// and the player will usually end up re-attaching to the surface.
    /// 
    /// With this constant, the player is forced to leave the ground by at least the specified angle
    /// difference, in degrees.
    /// </summary>
    private const float ForceJumpAngleDifference = 30.0f;

    /// <summary>
    /// The terrain layer that the player is on. The player will collide with the layers "Terrain" and
    /// "Terrain" plus the terrain layer.
    /// </summary>
    /// <value>The terrain layer.</value>f
    public int Layer {
        get { return terrainLayer; }
        set 
        { 
            terrainLayer = value;
            terrainMask =
                (1 << LayerMask.NameToLayer(Settings.LayerTerrain)) |
                (1 << LayerMask.NameToLayer(Settings.LayerTerrain + " " + terrainLayer));
        }
    }

    /// <summary>
    /// Whether the player is touching the ground.
    /// </summary>
    /// <value><c>true</c> if grounded; otherwise, <c>false</c>.</value>
    public bool Grounded {
        get { return grounded; }
        set { grounded = value; }
    }

	/// <summary>
	/// A collection of data about the surface at the player's feet.
	/// </summary>
	private struct SurfaceInfo
	{
		/// <summary>
		/// If there is a surface, which foot of the player it is  beneath. Otherwise, Footing.none.
		/// </summary>
		public Footing footing;
		
		/// <summary>
		/// The result of the raycast onto the surface at the player's left foot.
		/// </summary>
		public RaycastHit2D leftCast;
		
		/// <summary>
		/// The angle, in radians, of the surface on the player's left foot, or 0 if there is none.
		/// </summary>
		public float leftSurfaceAngle;
		
		/// <summary>
		/// The result of the raycast onto the surface at the player's right foot.
		/// </summary>
		public RaycastHit2D rightCast;
		
		/// <summary>
		/// The angle, in radians, of the surface on the player's right foot, or 0 if there is none.
		/// </summary>
		public float rightSurfaceAngle;
		
		public SurfaceInfo(RaycastHit2D leftCast, RaycastHit2D rightCast, Footing footing)
		{   
			this.leftCast = leftCast;
			this.leftSurfaceAngle = (leftCast) ? leftCast.normal.Angle() - AMath.HALF_PI : 0.0f;
			this.rightCast = rightCast;
			this.rightSurfaceAngle = (rightCast) ? rightCast.normal.Angle() - AMath.HALF_PI : 0.0f;
			this.footing = footing;
		}
	}
	
	/// <summary>
	/// Represents each sensor on the bottom of the player.
	/// </summary>
	private enum Footing
	{
		None, Left, Right,
	}

	private void Awake()
	{
		movingPlatformAnchor = new GameObject ();
		movingPlatformAnchor.name = gameObject.name + " Moving Platform Anchor";
		movingPlatformAnchor.transform.SetParent (gameObject.transform);

		footing = Footing.None;
		grounded = false;
		onMovingPlatform = false;
		vx = vy = vg = 0.0f;
		lastSurfaceAngle = 0.0f;
		leftKeyDown = rightKeyDown = jumpKeyDown = false;
		justJumped = justLanded = justDetached = false;
		wallMode = WallMode.Floor;
		surfaceAngle = 0.0f;
		Layer = 1;
		
		GetComponent<Collider2D>().isTrigger = true;
	}

	private void Start () {

	}
	
	private void Update () {
		GetInput ();
	}


    /// <summary>
    /// Stores keyboard input for the next fixed update (and HandleInput).
    /// </summary>
	private void GetInput()
	{
		leftKeyDown = Input.GetKey (Settings.LeftKey);
		rightKeyDown = Input.GetKey (Settings.RightKey);
		if(!jumpKeyDown && grounded) jumpKeyDown = Input.GetKeyDown (Settings.JumpKey);
	}
	

	private void FixedUpdate()
	{
        HandleInput(Time.fixedDeltaTime);

        // Stagger routine - if the player's gotta go fast, move it in increments of StaggerSpeedThreshold to prevent tunneling
        float vt = Mathf.Sqrt(vx * vx + vy * vy);

        if (vt < StaggerSpeedMax)
        {
            HandleForces();
            transform.position = new Vector2(transform.position.x + (vx * Time.fixedDeltaTime), transform.position.y + (vy * Time.fixedDeltaTime));
            HandleCollisions();
        } else {
            float vc = vt;
            while(vc > 0.0f)
            {
                if(vc > StaggerSpeedMax)
                {
                    HandleForces(Time.fixedDeltaTime * (StaggerSpeedMax / vt));
                    transform.position += (new Vector3(vx * Time.fixedDeltaTime, vy * Time.fixedDeltaTime)) * (StaggerSpeedMax / vt);
                    vc -= StaggerSpeedMax;
                } else {
                    HandleForces(Time.fixedDeltaTime * (vc / vt));
                    transform.position += (new Vector3(vx * Time.fixedDeltaTime, vy * Time.fixedDeltaTime)) * (vc / vt);
                    vc = 0.0f;
                }

                HandleCollisions();

                // If the player's speed changes mid-stagger recalculate current velocity and total velocity
                float vn = Mathf.Sqrt(vx * vx + vy * vy);
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
        float timeScale = timeStep / Settings.DefaultFixedDeltaTime;

        if (grounded)
        {
            if(horizontalLock)
            {
                horizontalLockTimer -= timeStep;
                if(horizontalLockTimer < 0.0f)
                {
                    horizontalLock = false;
                    horizontalLockTimer = 0.0f;
                }
            }

            if(jumpKeyDown) 
            {
                jumpKeyDown = false;
                Jump();
            }
            
            if(leftKeyDown && !horizontalLock)
            {
                if(vg > 0 && Mathf.Abs(AMath.AngleDiffd(surfaceAngle, 90.0f)) < VerticalDetachAngleMax)
                {
                    vx = 0;
                    Detach();
                } else {
                    vg -= groundAcceleration * timeScale;
                    if(vg > 0) vg -= groundDeceleration * timeScale;
                }
            } else if(rightKeyDown && !horizontalLock)
            {
                if(vg < 0 && Mathf.Abs(AMath.AngleDiffd(surfaceAngle, 270.0f)) < VerticalDetachAngleMax)
                {
                    vx = 0;
                    Detach();
                } else {
                    vg += groundAcceleration * timeScale;
                    if(vg < 0) vg += groundDeceleration * timeScale;
                }
            }

            if(Input.GetKey(KeyCode.Space)) vg = maxSpeed * Mathf.Sign(vg);

        } else {
            if(leftKeyDown) vx -= airAcceleration * timeScale;
            else if(rightKeyDown) vx += airAcceleration * timeScale;
        }
    }

    public void Jump()
    {
        justJumped = true;

        // Forces the player to leave the ground using the constant ForceJumpAngleDifference.
        // Helps prevent sticking to surfaces when the player's gotta go fast.
        var originalAngle = AMath.Modp((new Vector2(vx, vy)).Angle() * Mathf.Rad2Deg, 360.0f);

        float surfaceNormal = (surfaceAngle + 90.0f) * Mathf.Deg2Rad;
        vx += jumpSpeed * Mathf.Cos(surfaceNormal);
        vy += jumpSpeed * Mathf.Sin(surfaceNormal);

        var newAngle = AMath.Modp((new Vector2(vx, vy)).Angle() * Mathf.Rad2Deg, 360.0f);
        var angleDifference = AMath.AngleDiffd(originalAngle, newAngle);

        if (Mathf.Abs(angleDifference) < ForceJumpAngleDifference)
        {
            var targetAngle = originalAngle + ForceJumpAngleDifference*Mathf.Sign(angleDifference);
            var magnitude = new Vector2(vx, vy).magnitude;

            var targetAngleRadians = targetAngle*Mathf.Deg2Rad;
            var newVelocity = new Vector2(magnitude*Mathf.Cos(targetAngleRadians),
                magnitude*Mathf.Sin(targetAngleRadians));

            vx = newVelocity.x;
            vy = newVelocity.y;
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
        float timeScale = timeStep / Settings.DefaultFixedDeltaTime;

        if (grounded)
        {
            float prevVg = vg;

            // Friction from deceleration
            if (!leftKeyDown && !rightKeyDown)
            {
                if(vg != 0.0f && Mathf.Abs(vg) < groundDeceleration)
                {
                    vg = 0.0f;
                } else if(vg > 0.0f)
                {
                    vg -= groundDeceleration * timeScale;
                } else if(vg < 0.0f)
                {
                    vg += groundDeceleration * timeScale;
                }
            }

            // Slope gravity
            float slopeForce = 0.0f;

            if(Mathf.Abs(AMath.AngleDiffd(surfaceAngle, 0.0f)) > SlopeGravityAngleMin)
            {
                slopeForce = SlopeFactor * Mathf.Sin(surfaceAngle * Mathf.Deg2Rad);
                vg -= slopeForce * timeScale;
            }

            // Speed limit
            if(vg > maxSpeed) vg = maxSpeed;
            else if(vg < -maxSpeed) vg = -maxSpeed;

            if(Mathf.Abs(slopeForce) > groundAcceleration)
            {
                if(rightKeyDown && prevVg > 0.0f && vg < 0.0f) LockHorizontal();
                else if(leftKeyDown && prevVg < 0.0f && vg > 0.0f) LockHorizontal();
            }

            // Detachment from walls if speed is too low
            if(surfaceAngle > 90.0f - VerticalDetachAngleMax &&
               surfaceAngle < 270.0f + VerticalDetachAngleMax && 
               Mathf.Abs(vg) < DetachSpeed)
            {
                Detach(true);
            }
        } else {
            vy -= gravity * timeScale;
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
        bool anyHit = false;
        bool jumpedPreviousCheck = justJumped;

        if(!grounded)
        {   
            surfaceAngle = 0.0f;
            anyHit = AirSideCheck() | AirCeilingCheck() | AirGroundCheck();
        }
        
        if(grounded)
        {
			anyHit = GroundSideCheck() | GroundCeilingCheck() | GroundSurfaceCheck();
			if(!SurfaceAngleCheck()) Detach();
        }

        if (justJumped && jumpedPreviousCheck) justJumped = false;
        
        if (!anyHit && justDetached)
            justDetached = false;
    }

    /// <summary>
    /// Detaches the player from whatever surface it is on. If the player is not grounded this has no effect.
    /// </summary>
	private void Detach()
	{
        Detach(false);
	}

    /// <summary>
    /// Detach the player from whatever surface it is on. If the player is not grounded this has no effect
    /// other than setting lockUponLanding.
    /// </summary>
    /// <param name="lockUponLanding">If set to <c>true</c> lock horizontal control when the player attaches.</param>
    private void Detach(bool lockUponLanding)
    {
        vg = 0.0f;
        lastSurfaceAngle = 0.0f;
        surfaceAngle = 0.0f;
        grounded = false;
        justDetached = true;
        wallMode = WallMode.Floor;
        footing = Footing.None;
        this.lockUponLanding = lockUponLanding;

		if (movingPlatformAnchor.transform.parent != gameObject.transform)
			movingPlatformAnchor.transform.SetParent (gameObject.transform);
    }

    /// <summary>
    /// Attaches the player to a surface within the reach of its surface sensors. The angle of attachment
    /// need not be perfect; the method works reliably for angles within 45 degrees of the one specified.
    /// </summary>
    /// <param name="groundSpeed">The ground speed of the player after attaching.</param>
    /// <param name="angleRadians">The angle of the surface, in radians.</param>
    private void Attach(float groundSpeed, float angleRadians)
    {
        float angleDegrees = AMath.Modp(angleRadians * Mathf.Rad2Deg, 360.0f);
        vg = groundSpeed;
        surfaceAngle = lastSurfaceAngle = angleDegrees;
        grounded = justLanded = true;

        // WallModeSwitchAngle may be set to only attach right or left at extreme angles
        if (surfaceAngle < 45.0f + WallModeSwitchAngle || surfaceAngle > 315.0f - WallModeSwitchAngle)
            wallMode = WallMode.Floor;

        else if (surfaceAngle > 135.0f - WallModeSwitchAngle && surfaceAngle < 225.0 + WallModeSwitchAngle)
            wallMode = WallMode.Ceiling;

        else if (surfaceAngle > 45.0f + WallModeSwitchAngle && surfaceAngle < 135.0f - WallModeSwitchAngle)
            wallMode = WallMode.Right;

        else 
            wallMode = WallMode.Left;

        if (lockUponLanding)
        {
            lockUponLanding = false;
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
        float sAngled = AMath.Modp(angleRadians * Mathf.Rad2Deg, 360.0f);
        float sAngler = sAngled * Mathf.Deg2Rad;
        float groundSpeed;
        
        // The player can't possibly land on something if he's traveling 90 degrees
        // within the normal
        float surfaceNormal = AMath.Modp(sAngled + 90.0f, 360.0f);
        float playerAngle = (new Vector2(vx, vy)).Angle() * Mathf.Rad2Deg;
        float surfaceDifference = AMath.AngleDiffd(playerAngle, surfaceNormal);
        if(Mathf.Abs(surfaceDifference) < 90.0f)
        {
            return false;
        }

        // Ground attachment
        if (Mathf.Abs(AMath.AngleDiffd(sAngled, 180.0f)) > AttachAngleMin && 
            Mathf.Abs(AMath.AngleDiffd(sAngled, 90.0f)) > AttachAngleMin &&
            Mathf.Abs(AMath.AngleDiffd(sAngled, 270.0f)) > AttachAngleMin)    
        {
            if(vy > 0.0f && (AMath.Equalsf(sAngled, 0.0f, AttachAngleMin) || (AMath.Equalsf(sAngled, 180.0f, AttachAngleMin))))
            {
                groundSpeed = vx;
                Attach(groundSpeed, sAngler);
                return true;
            } else {
                // groundspeed = (airspeed) * (angular difference between air direction and surface normal direction) / (90 degrees)
                groundSpeed = Mathf.Sqrt(vx * vx + vy * vy) * 
                    -AMath.Clamp(AMath.AngleDiffd(Mathf.Atan2(vy, vx) * Mathf.Rad2Deg, sAngled - 90.0f) / 90.0f, -1.0f, 1.0f);

                if(sAngled > 90.0f - VerticalDetachAngleMax &&
                   sAngled < 270.0f + VerticalDetachAngleMax &&
                   Mathf.Abs(groundSpeed) < DetachSpeed)
                {
                    return false;
                } else {
                    Attach(groundSpeed, sAngler);
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Locks the player's horizontal control on the ground for the time specified by HorizontalLockTime.
    /// </summary>
    private void LockHorizontal()
    {
        horizontalLock = true;
        horizontalLockTimer = HorizontalLockTime;
    }

	/// <summary>
	/// Gets data about the surface closest to the player's feet, including its footing and raycast info.
	/// </summary>
	/// <returns>The surface.</returns>
	/// <param name="layerMask">A mask indicating what layers are surfaces.</param>
	private SurfaceInfo GetSurface(int layerMask)
	{
		RaycastHit2D checkLeft, checkRight;

		// Linecasts are straight vertical or horizontal from the ground sensors
		checkLeft = SurfaceCast(Footing.Left); checkRight = SurfaceCast(Footing.Right);
        
        if(checkLeft && checkRight)
		{
			// If both sensors have surfaces, return the one with the highest based on wallmode
			if(wallMode == WallMode.Floor && checkLeft.point.y > checkRight.point.y || 
			   wallMode == WallMode.Ceiling && checkLeft.point.y < checkRight.point.y || 
			   wallMode == WallMode.Right && checkLeft.point.x < checkRight.point.x || 
			   wallMode == WallMode.Left && checkLeft.point.x > checkRight.point.x)
				return new SurfaceInfo(checkLeft, checkRight, Footing.Left);
			else return new SurfaceInfo(checkLeft, checkRight, Footing.Right);
		} else if(checkLeft)
		{
			return new SurfaceInfo(checkLeft, checkRight, Footing.Left);
		} else if(checkRight)
		{
			return new SurfaceInfo(checkLeft, checkRight, Footing.Right);
		}

		return default(SurfaceInfo);
	}

    private RaycastHit2D GroundCast(Footing footing)
    {
        RaycastHit2D cast;
        if (footing == Footing.Left)
        {
            cast = Physics2D.Linecast((Vector2) sensorGroundLeft.position - wallMode.UnitVector()*LedgeHeightMax,
                (Vector2) sensorGroundLeft.position,
                terrainMask);
        }
        else
        {
            cast = Physics2D.Linecast((Vector2) sensorGroundRight.position - wallMode.UnitVector()*LedgeHeightMax,
                (Vector2) sensorGroundRight.position,
                terrainMask);
        }

        return cast;
    }

    /// <summary>
    /// Returns the result of a linecast from the specified footing onto the surface based on wallmode.
    /// </summary>
    /// <returns>The result of the linecast.</returns>
    /// <param name="footing">The footing to linecast from.</param>
    private RaycastHit2D SurfaceCast(Footing footing)
    {
        RaycastHit2D cast;
        if (footing == Footing.Left)
        {
            // Cast from the player's side to below the player's feet based on its wall mode (orientation)
            cast = Physics2D.Linecast((Vector2)sensorGroundLeft.position - wallMode.UnitVector() * LedgeHeightMax,
                                      (Vector2)sensorGroundLeft.position + wallMode.UnitVector() * SurfaceDepthMax,
                                      terrainMask);

            if(!cast)
            {
                return default(RaycastHit2D);
            } else if(AMath.Equalsf(cast.fraction, 0.0f))
            {
                for(WallMode check = wallMode.AdjacentCW(); check != wallMode; check = check.AdjacentCW())
                {
                    cast = Physics2D.Linecast((Vector2)sensorGroundLeft.position - check.UnitVector() * LedgeHeightMax,
                                              (Vector2)sensorGroundLeft.position + check.UnitVector() * SurfaceDepthMax,
                                              terrainMask);

                    if(cast && !AMath.Equalsf(cast.fraction, 0.0f))
                        return cast;
                }

                return default(RaycastHit2D);
            }

            return cast;
        } else {
            cast = Physics2D.Linecast((Vector2)sensorGroundRight.position - wallMode.UnitVector() * LedgeHeightMax,
                                      (Vector2)sensorGroundRight.position + wallMode.UnitVector() * SurfaceDepthMax,
                                      terrainMask);

            if(!cast)
            {
                return default(RaycastHit2D);
            } else if(AMath.Equalsf(cast.fraction, 0.0f))
            {
                for(WallMode check = wallMode.AdjacentCW(); check != wallMode; check = check.AdjacentCW())
                {
                    cast = Physics2D.Linecast((Vector2)sensorGroundRight.position - check.UnitVector() * LedgeHeightMax,
                                              (Vector2)sensorGroundRight.position + check.UnitVector() * SurfaceDepthMax,
                                              terrainMask);
                    
                    if(cast && !AMath.Equalsf(cast.fraction, 0.0f))
                        return cast;
                }
                
                return default(RaycastHit2D);
            }

            return cast;
        }
    }

	private void AttachMoving(Transform platform)
	{
		movingPlatformAnchor.transform.SetParent (platform);
	}

	/// COLLISION SUBROUTINES

	/// <summary>
	/// Collision check with side sensors for when player is in the air.
	/// </summary>
	/// <returns><c>true</c>, if a collision was found, <c>false</c> otherwise.</returns>
	private bool AirSideCheck()
	{
	    var width = (sensorSideLeft.position - sensorSideRight.position).magnitude;
		RaycastHit2D sideLeftCheck = Physics2D.Linecast(sensorSideMid.position, sensorSideLeft.position, terrainMask);
		RaycastHit2D sideRightCheck = Physics2D.Linecast(sensorSideMid.position, sensorSideRight.position, terrainMask);
		
		if(sideLeftCheck)
		{
		    if (!justJumped)
		    {
		        vx = 0;
		    }

            transform.position += (Vector3)sideLeftCheck.point - sensorSideLeft.position +
				((Vector3)sideLeftCheck.point - sensorSideLeft.position).normalized * AMath.Epsilon;
			return true;
		} else if(sideRightCheck)
		{
		    if (!justJumped)
		    {
		        vx = 0;
		    }

			transform.position += (Vector3)sideRightCheck.point - sensorSideRight.position +
				((Vector3)sideRightCheck.point - sensorSideRight.position).normalized * AMath.Epsilon;
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
		Collider2D[] cmem = new Collider2D[1];
		
		if(Physics2D.OverlapPointNonAlloc(sensorCeilLeft.position, cmem, terrainMask) > 0)
		{
			RaycastHit2D horizontalCheck = Physics2D.Linecast(sensorCeilMid.position, sensorCeilLeft.position, terrainMask);
			RaycastHit2D verticalCheck = Physics2D.Linecast(sensorSideLeft.position, sensorCeilLeft.position, terrainMask);
			
			if(Vector2.Distance(horizontalCheck.point, sensorCeilLeft.position) < Vector2.Distance(verticalCheck.point, sensorCeilLeft.position))
			{
				transform.position += (Vector3)horizontalCheck.point - sensorCeilLeft.position;
				
				if(!justDetached) HandleImpact(horizontalCheck.normal.Angle() - AMath.HALF_PI);
				if(vy > 0) vy = 0;
			} else {
				transform.position += (Vector3)verticalCheck.point - sensorCeilLeft.position;
				
				if(!justDetached) HandleImpact(verticalCheck.normal.Angle() - AMath.HALF_PI);
				if(vy > 0) vy = 0;
			}
			return true;
			
		} else if(Physics2D.OverlapPointNonAlloc(sensorCeilRight.position, cmem, terrainMask) > 0)
		{
			RaycastHit2D horizontalCheck = Physics2D.Linecast(sensorCeilMid.position, sensorCeilRight.position, terrainMask);
			RaycastHit2D verticalCheck = Physics2D.Linecast(sensorSideRight.position, sensorCeilRight.position, terrainMask);
			
			if(Vector2.Distance(horizontalCheck.point, sensorCeilRight.position) <
			   Vector2.Distance(verticalCheck.point, sensorCeilRight.position))
			{
				transform.position += (Vector3)horizontalCheck.point - sensorCeilRight.position;
				
				if(!justDetached) HandleImpact(horizontalCheck.normal.Angle() - AMath.HALF_PI);  
				if(vy > 0) vy = 0;
			} else {
				transform.position += (Vector3)verticalCheck.point - sensorCeilRight.position;
				
				if(!justDetached) HandleImpact(verticalCheck.normal.Angle() - AMath.HALF_PI);  
				if(vy > 0) vy = 0;
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
		RaycastHit2D groundLeftCheck = GroundCast(Footing.Left);
	    RaycastHit2D groundRightCheck = GroundCast(Footing.Right);
		
		if(groundLeftCheck || groundRightCheck)
		{
			if(justJumped)
			{
			    if (groundLeftCheck)
			    {
			        transform.position += (Vector3)groundLeftCheck.point - sensorGroundLeft.position;
			    }
			    if (groundRightCheck)
			    {
			        transform.position += (Vector3)groundRightCheck.point - sensorGroundRight.position;
			    }
			} else {
				if(groundLeftCheck && groundRightCheck)
				{
					if(groundLeftCheck.point.y > groundRightCheck.point.y)
					{
						HandleImpact(groundLeftCheck.normal.Angle() - AMath.HALF_PI);
					} else {
						HandleImpact(groundRightCheck.normal.Angle() - AMath.HALF_PI);
					}
				} else if(groundLeftCheck)
				{
					HandleImpact(groundLeftCheck.normal.Angle() - AMath.HALF_PI);
				} else {
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
		RaycastHit2D sideLeftCheck = Physics2D.Linecast(sensorSideMid.position, sensorSideLeft.position, terrainMask);
		RaycastHit2D sideRightCheck = Physics2D.Linecast(sensorSideMid.position, sensorSideRight.position, terrainMask);
		
		if(sideLeftCheck)
		{
			vg = 0;
			transform.position += (Vector3)sideLeftCheck.point - sensorSideLeft.position +
				((Vector3)sideLeftCheck.point - sensorSideLeft.position).normalized * AMath.Epsilon;
			
			// If running down a wall and hits the floor, orient the player onto the floor
			if(wallMode == WallMode.Right)
			{
				transform.RotateBy(-90.0f);
				wallMode = WallMode.Floor;
			}
			
			return true;
		} else if(sideRightCheck)
		{
			vg = 0;
			transform.position += (Vector3)sideRightCheck.point - sensorSideRight.position +
				((Vector3)sideRightCheck.point - sensorSideRight.position).normalized * AMath.Epsilon;
			
			// If running down a wall and hits the floor, orient the player onto the floor
			if(wallMode == WallMode.Left)
			{
				transform.RotateTo(90.0f);
				wallMode = WallMode.Floor;
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
		RaycastHit2D ceilLeftCheck = Physics2D.Linecast(sensorCeilMid.position, sensorCeilLeft.position, terrainMask);
		RaycastHit2D ceilRightCheck = Physics2D.Linecast(sensorCeilMid.position, sensorCeilRight.position, terrainMask);
		
		if(ceilLeftCheck)
		{
			vg = 0;
			
			// Add epsilon to prevent sticky collisions
			transform.position += (Vector3)ceilLeftCheck.point - sensorCeilLeft.position + 
				((Vector3)ceilLeftCheck.point - sensorCeilLeft.position).normalized * AMath.Epsilon;
			
			return true;
		} else if(ceilRightCheck)
		{
			vg = 0;
			transform.position += (Vector3)ceilRightCheck.point - sensorCeilRight.position +
				((Vector3)ceilRightCheck.point - sensorCeilRight.position).normalized * AMath.Epsilon;
			
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
		SurfaceInfo s = GetSurface(terrainMask);
		
		if(s.leftCast || s.rightCast)
		{
			// If both sensors found surfaces, need additional checks to see if rotation needs to account for both their positions
			if(s.leftCast && s.rightCast)
			{
				// Calculate angle changes for tolerance checks
				float rightDiff = AMath.AngleDiffd(s.rightSurfaceAngle * Mathf.Rad2Deg, lastSurfaceAngle);
				float leftDiff = AMath.AngleDiffd(s.leftSurfaceAngle * Mathf.Rad2Deg, lastSurfaceAngle);
				float overlapDiff = AMath.AngleDiffr(s.leftSurfaceAngle, s.rightSurfaceAngle) * Mathf.Rad2Deg;
				
				if(s.footing == Footing.Left)
				{
					// If the surface's angle is a small enough difference from that of the previous begin surface checks
					if(justLanded || Mathf.Abs(leftDiff) < SurfaceAngleDiffMax)
					{
						// Check angle differences between feet for player rotation
						if(Mathf.Abs(overlapDiff) > OverlapAngleMinAbs && overlapDiff > OverlapAngleMin)
						{
							// If tolerable, rotate between the surfaces beneath the two feet
							transform.RotateTo((s.rightCast.point - s.leftCast.point).Angle(), sensorGroundMid.position);
							transform.position += (Vector3)s.leftCast.point - sensorGroundLeft.position;
							footing = Footing.Left;
						} else {
							// Else just rotate for the left foot
							transform.RotateTo(s.leftSurfaceAngle, s.leftCast.point);
							transform.position += (Vector3)s.leftCast.point - sensorGroundLeft.position;
							footing = Footing.Left;
						}
						
						if(s.leftCast.collider.gameObject.tag == Settings.TagMovingPlatform)
						{
							// TODO moving platforms!??!
						}
						// Else see if the other surface's angle is tolerable
					} else if(Mathf.Abs(rightDiff) < SurfaceAngleDiffMax) {
						transform.RotateTo(s.rightSurfaceAngle, sensorGroundMid.position);
						transform.position += (Vector3)s.rightCast.point - sensorGroundRight.position;
						footing = Footing.Right;
						// Else the surfaces are untolerable. detach from the surface
					} else {
						Detach();
					}
					// Same thing but with the other foot
				} else if(s.footing == Footing.Right)
				{
					if(justLanded || Mathf.Abs(rightDiff) < SurfaceAngleDiffMax)
					{
						if(Mathf.Abs(overlapDiff) > OverlapAngleMinAbs && overlapDiff > OverlapAngleMin)
						{
							transform.RotateTo((s.rightCast.point - s.leftCast.point).Angle(), sensorGroundMid.position);
							transform.position += (Vector3)s.rightCast.point - sensorGroundRight.position;
							footing = Footing.Right;
						} else {
							transform.RotateTo(s.rightSurfaceAngle, s.rightCast.point);
							transform.position += (Vector3)s.rightCast.point - sensorGroundRight.position;
							footing = Footing.Right;
						}
						
					} else if(Mathf.Abs(leftDiff) < SurfaceAngleDiffMax) {
						transform.RotateTo(s.leftSurfaceAngle, sensorGroundMid.position);
						transform.position += (Vector3)s.leftCast.point - sensorGroundLeft.position;
						footing = Footing.Left;
					} else {
						Detach();
					}
				}
			} else if(s.leftCast)
			{
				float leftDiff = AMath.AngleDiffd(s.leftSurfaceAngle * Mathf.Rad2Deg, lastSurfaceAngle);
				if(justLanded || Mathf.Abs(leftDiff) < SurfaceAngleDiffMax)
				{
					transform.RotateTo(s.leftSurfaceAngle, s.leftCast.point);
					transform.position += (Vector3)s.leftCast.point - sensorGroundLeft.position;
					footing = Footing.Left;
				} else {
					Detach();
				}
			} else {
				float rightDiff = AMath.AngleDiffd(s.rightSurfaceAngle * Mathf.Rad2Deg, lastSurfaceAngle);
				if(justLanded || Mathf.Abs(rightDiff) < SurfaceAngleDiffMax)
				{
					transform.RotateTo(s.rightSurfaceAngle, s.rightCast.point);
					transform.position += (Vector3)s.rightCast.point - sensorGroundRight.position;
					footing = Footing.Right;
				} else {
					Detach();
				}
			}
			
			return true;
		} else {
			Detach();
		}
		
		return false;
	}

	/// <summary>
	/// Check for changes in angle of incline for when player is on the ground.
	/// </summary>
	/// <returns><c>true</c> if the angle of incline is tolerable, <c>false</c> otherwise.</returns>
	private bool SurfaceAngleCheck()
	{
		if(justLanded)
		{
			surfaceAngle = transform.eulerAngles.z;
			lastSurfaceAngle = surfaceAngle;
		} else {
			lastSurfaceAngle = surfaceAngle;
			surfaceAngle = transform.eulerAngles.z;
		}
		
		// Can only stay on the surface if angle difference is low enough
		if(grounded && (justLanded ||
		                Mathf.Abs(AMath.AngleDiffd(lastSurfaceAngle, surfaceAngle)) < SurfaceAngleDiffMax))
		{
			if(wallMode == WallMode.Floor)
			{
				if(surfaceAngle > 45.0f + WallModeSwitchAngle && surfaceAngle < 180.0f) wallMode = WallMode.Right;
				else if(surfaceAngle < 315.0f - WallModeSwitchAngle && surfaceAngle > 180.0f) wallMode = WallMode.Left;
			} else if(wallMode == WallMode.Right)
			{
				if(surfaceAngle > 135.0f + WallModeSwitchAngle) wallMode = WallMode.Ceiling;
				else if(surfaceAngle < 45.0f - WallModeSwitchAngle) wallMode = WallMode.Floor;
			} else if(wallMode == WallMode.Ceiling)
			{
				if(surfaceAngle > 225.0f + WallModeSwitchAngle) wallMode = WallMode.Left;
				else if(surfaceAngle < 135.0f - WallModeSwitchAngle) wallMode = WallMode.Right;
			} else if(wallMode == WallMode.Left)
			{
				if(surfaceAngle > 315.0f + WallModeSwitchAngle || surfaceAngle < 180.0f) wallMode = WallMode.Floor;
				else if(surfaceAngle < 225.0f - WallModeSwitchAngle) wallMode = WallMode.Ceiling;
			}
			
			vx = vg * Mathf.Cos(surfaceAngle * Mathf.Deg2Rad);
			vy = vg * Mathf.Sin(surfaceAngle * Mathf.Deg2Rad);
			
			justLanded = false;
			return true;
		}
		
		return false;
	}
}