using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
	private bool grounded;
    private Footing footing;
	private float vx, vy, vg;
	private bool leftKeyDown, rightKeyDown, jumpKeyDown;
    private bool justLanded, justJumped;
    private float lastSurfaceAngle;

	// If grounded, the angle of the incline the player is standing on
	private float surfaceAngle;

	/// <summary>
	/// Represents the current orientation of the player.
	/// </summary>
	private WallMode wallMode;

	/// <summary>
	/// The amount in degrees past the threshold of changing wall mode that the player
	/// can go.
	/// </summary>
	private const float WallModeAngleThreshold = 10.0f;

	/// <summary>
	/// The speed at which the player must be moving to be able to switch wall modes.
	/// </summary>
	private const float WallModeSpeedThreshold = 0.5f;

	/// <summary>
	/// The maximum change in angle between two surfaces that the player can walk in.
	/// </summary>
	private const float SurfaceAngleThreshold = 70.0f;

	/// <summary>
	/// The minimum difference in angle between the surface sensor and the overlap sensor
	/// to have the player's rotation account for it.
	/// </summary>
	private const float OverlapAngleMinimum = -40.0f;

    /// <summary>
    /// The minimum absolute difference in angle between the surface sensor and the overlap
    /// sensor to have the player's rotation account for it.
    /// </summary>
    private const float OverlapAngleThreshold = 7.5f;

    /// <summary>
    /// The minimum speed the player must be moving at to stagger each physics update,
    /// processing the movement in fractions.
    /// </summary>
    private const float StaggerSpeedThreshold = 5.0f;

    /// <summary>
    /// The magnitude of the force applied to the player when going up or down slopes.
    /// </summary>
    private const float SlopeFactor = 0.3f;

    /// <summary>
    /// The minimum
    /// </summary>
    private const float SlopeGravityAngleMinimum = 10.0f;

	// The layer mask which represents all terrain to check for collision with
	private int terrainMask;
	
	[SerializeField]
	private Vector2 size;

	[SerializeField]
	private float gravity;

	[SerializeField]
	private float maxSpeed;

	[SerializeField]
	private float groundAcceleration;

	[SerializeField]
	private float groundDeceleration;

	[SerializeField]
	private float jumpSpeed;

	[SerializeField]
	private float airAcceleration;

	// Sensors
	[SerializeField]
	private Transform sensorGroundLeft;
	[SerializeField]
	private Transform sensorGroundMid;
	[SerializeField]
	private Transform sensorGroundRight;
	[SerializeField]
	private Transform sensorSideLeft;
    [SerializeField]
    private Transform sensorSideMid;
	[SerializeField]
	private Transform sensorSideRight;
	[SerializeField]
	private Transform sensorCeilLeft;
	[SerializeField]
	private Transform sensorCeilMid;
	[SerializeField]
	private Transform sensorCeilRight;
	[SerializeField]
	private Transform sensorTileLeft;
	[SerializeField]
	private Transform sensorTileRight;


	// Use this for initialization
	private void Start () {
		grounded = false;
		vx = vy = vg = 0.0f;
        lastSurfaceAngle = 0.0f;
		leftKeyDown = rightKeyDown = jumpKeyDown = false;
        justJumped = justLanded = false;
		wallMode = WallMode.Floor;
		surfaceAngle = 0.0f;
		terrainMask = 1 << LayerMask.NameToLayer("Terrain");

		// Enable later for ragdoll ?
		collider2D.enabled = false;
	}
	
	// Update is called once per frame
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
        HandleInput();
        HandleForces();

        // Stagger routine - if the player's gotta go fast, move it in increments of StaggerSpeedThreshold to prevent tunneling
        float vt = Mathf.Sqrt(vx * vx + vy * vy);

        if (vt < StaggerSpeedThreshold)
        {
            transform.position = new Vector2(transform.position.x + (vx * Time.fixedDeltaTime), transform.position.y + (vy * Time.fixedDeltaTime));
            HandleCollisions();
        } else {
            // Stagger by lerping with vc / vt while reducing vc with each stagger
            float vc = vt;
            while(vc > 0.0f)
            {
                if(vc > StaggerSpeedThreshold)
                {
                    transform.position += (new Vector3(vx * Time.fixedDeltaTime, vy * Time.fixedDeltaTime)) * (StaggerSpeedThreshold / vt);
                    vc -= StaggerSpeedThreshold;
                } else {
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
    /// Handles the input from the previous update and also applies 
    /// </summary>
    private void HandleInput()
    {
        if (grounded)
        {
            if(jumpKeyDown) 
            {
                jumpKeyDown = false;
                justJumped = true;
                
                float surfaceNormal = (surfaceAngle + 90.0f) * Mathf.Deg2Rad;
                vx += jumpSpeed * Mathf.Cos(surfaceNormal);
                vy += jumpSpeed * Mathf.Sin(surfaceNormal);

                Detach();
            }
            
            if(leftKeyDown)
            {
                vg -= groundAcceleration;
                if(vg > 0) vg -= groundDeceleration;
            } else if(rightKeyDown)
            {
                vg += groundAcceleration;
                if(vg < 0) vg += groundDeceleration;
            }
            
            if(vg > maxSpeed) vg = maxSpeed;
            else if(vg < -maxSpeed) vg = -maxSpeed;
        } else {
            vy -= gravity;

            if(leftKeyDown) vx -= airAcceleration;
            else if(rightKeyDown) vx += airAcceleration;
        }
    }

    /// <summary>
    /// Applies forces on the player and also handles speed-based conditions, such as detaching the player if it is too slow on
    /// an incline.
    /// </summary>
    private void HandleForces()
    {
        if (!leftKeyDown && !rightKeyDown)
        {
            if(vg != 0.0f && Mathf.Abs(vg) < groundDeceleration)
            {
                vg = 0.0f;
            } else if(vg > 0.0f)
            {
                vg -= groundDeceleration;
            } else if(vg < 0.0f)
            {
                vg += groundDeceleration;
            }
        }


        vg -= SlopeFactor * Mathf.Sin(surfaceAngle * Mathf.Deg2Rad);
    }

    /// <summary>
    /// Checks for collision with all sensors, changing position, velocity, and rotation if necessary. This should be called each time
    /// the player changes position.
    /// </summary>
    private void HandleCollisions()
    {
        // To save memory when doing overlaps
        Collider2D[] cmem = new Collider2D[1];

        if(!grounded)
        {   
            transform.eulerAngles = new Vector3();
            surfaceAngle = 0.0f;

            // Ceiling check - either pushes out vertically or horizontally based on the closest distance in either direction to the surface
            if(Physics2D.OverlapPointNonAlloc(sensorCeilLeft.position, cmem, terrainMask) > 0)
            {
                RaycastHit2D horizontalCheck = Physics2D.Linecast(sensorCeilMid.position, sensorCeilLeft.position);
                RaycastHit2D verticalCheck = Physics2D.Linecast(sensorSideLeft.position, sensorCeilLeft.position);

                if(Vector2.Distance(horizontalCheck.point, sensorCeilLeft.position) < Vector2.Distance(verticalCheck.point, sensorCeilLeft.position))
                {
                    transform.position += (Vector3)horizontalCheck.point - sensorCeilLeft.position;
                    vx = 0;
                } else {
                    transform.position += (Vector3)verticalCheck.point - sensorCeilLeft.position;
                    if(vy > 0) vy = 0;
                }
            } else if(Physics2D.OverlapPointNonAlloc(sensorCeilRight.position, cmem, terrainMask) > 0)
            {
                RaycastHit2D horizontalCheck = Physics2D.Linecast(sensorCeilMid.position, sensorCeilRight.position);
                RaycastHit2D verticalCheck = Physics2D.Linecast(sensorSideRight.position, sensorCeilRight.position);
                
                if(Vector2.Distance(horizontalCheck.point, sensorCeilRight.position) < Vector2.Distance(verticalCheck.point, sensorCeilRight.position))
                {
                    transform.position += (Vector3)horizontalCheck.point - sensorCeilRight.position;
                    vx = 0;
                } else {
                    transform.position += (Vector3)verticalCheck.point - sensorCeilRight.position;
                    if(vy > 0) vy = 0;
                }
            }
            
            // Side check
            RaycastHit2D sideLeftCheck = Physics2D.Linecast(sensorSideMid.position, sensorSideLeft.position, terrainMask);
            RaycastHit2D sideRightCheck = Physics2D.Linecast(sensorSideMid.position, sensorSideRight.position, terrainMask);

            if(sideLeftCheck)
            {
                vx = 0;
                transform.position += (Vector3)sideLeftCheck.point - sensorSideLeft.position +
                    ((Vector3)sideLeftCheck.point - sensorSideLeft.position).normalized * AMath.Epsilon;
            } else if(sideRightCheck)
            {
                vx = 0;
                transform.position += (Vector3)sideRightCheck.point - sensorSideRight.position +
                    ((Vector3)sideRightCheck.point - sensorSideRight.position).normalized * AMath.Epsilon;
            }

            // See if the player landed
            if(justJumped)
            {
                justJumped = Physics2D.OverlapPointNonAlloc(sensorGroundLeft.position, cmem, terrainMask) > 0 || 
                    Physics2D.OverlapPointNonAlloc(sensorGroundRight.position, cmem, terrainMask) > 0;
            } else {
                justLanded = Physics2D.OverlapPointNonAlloc(sensorGroundLeft.position, cmem, terrainMask) > 0 || 
                    Physics2D.OverlapPointNonAlloc(sensorGroundRight.position, cmem, terrainMask) > 0;
                grounded = justLanded;
            }
        }
        
        if(grounded)
        {
            // Crush check - might not even put this in?
            if((Physics2D.OverlapPointNonAlloc(sensorCeilLeft.position, cmem, terrainMask) > 0 &&
                Physics2D.OverlapPointNonAlloc(sensorCeilRight.position, cmem, terrainMask) > 0))
            {
                Debug.Log("ded");
                // TODO kill him!
            }
            
            // Side check
            RaycastHit2D ceilLeftCheck = Physics2D.Linecast(sensorCeilMid.position, sensorCeilLeft.position, terrainMask);
            RaycastHit2D ceilRightCheck = Physics2D.Linecast(sensorCeilMid.position, sensorCeilRight.position, terrainMask);

            if(ceilLeftCheck)
            {
                vg = 0;
                transform.position += (Vector3)ceilLeftCheck.point - sensorCeilLeft.position + 
                    ((Vector3)ceilLeftCheck.point - sensorCeilLeft.position).normalized * AMath.Epsilon;
            } else if(ceilRightCheck)
            {
                vg = 0;
                transform.position += (Vector3)ceilRightCheck.point - sensorCeilRight.position +
                    ((Vector3)ceilRightCheck.point - sensorCeilRight.position).normalized * AMath.Epsilon;
            }

            RaycastHit2D sideLeftCheck = Physics2D.Linecast(sensorSideMid.position, sensorSideLeft.position, terrainMask);
            RaycastHit2D sideRightCheck = Physics2D.Linecast(sensorSideMid.position, sensorSideRight.position, terrainMask);
            
            if(sideLeftCheck)
            {
                vg = 0;
                transform.position += (Vector3)sideLeftCheck.point - sensorSideLeft.position +
                    ((Vector3)sideLeftCheck.point - sensorSideLeft.position).normalized * AMath.Epsilon;
            } else if(sideRightCheck)
            {
                vg = 0;
                transform.position += (Vector3)sideRightCheck.point - sensorSideRight.position +
                    ((Vector3)sideRightCheck.point - sensorSideRight.position).normalized * AMath.Epsilon;
            }
            
            
            // Surface check
            SurfaceInfo s = GetSurface(terrainMask);
            
            if(s.leftCast || s.rightCast)
            {
                // If both sensors found surfaces, need additional checks to see if rotation needs to account for both their positions
                if(s.leftCast && s.rightCast)
                {
                    float rightDiff = AMath.AngleDiff(s.rightSurfaceAngle, lastSurfaceAngle * Mathf.Deg2Rad) * Mathf.Rad2Deg;
                    float leftDiff = AMath.AngleDiff(s.leftSurfaceAngle, lastSurfaceAngle * Mathf.Deg2Rad) * Mathf.Rad2Deg;
                    float overlapDiff = AMath.AngleDiff(s.leftSurfaceAngle, s.rightSurfaceAngle) * Mathf.Rad2Deg;

                    if(s.footing == Footing.Left)
                    {
                        if(justLanded || Mathf.Abs(leftDiff) < SurfaceAngleThreshold)
                        {
                            if(Mathf.Abs(overlapDiff) > OverlapAngleThreshold && overlapDiff > OverlapAngleMinimum)
                            {
                                transform.RotateTo((s.rightCast.point - s.leftCast.point).Angle(), sensorGroundMid.position);
                                transform.position += (Vector3)s.leftCast.point - sensorGroundLeft.position;
                                footing = Footing.Left;
                            } else {
                                transform.RotateTo(s.leftSurfaceAngle, s.leftCast.point);
                                transform.position += (Vector3)s.leftCast.point - sensorGroundLeft.position;
                                footing = Footing.Left;
                            }

                        } else if(Mathf.Abs(rightDiff) < SurfaceAngleThreshold) {
                            transform.RotateTo(s.rightSurfaceAngle, sensorGroundMid.position);
                            transform.position += (Vector3)s.rightCast.point - sensorGroundRight.position;
                            footing = Footing.Right;

                        } else {
                            Detach();
                        }
                    } else if(s.footing == Footing.Right)
                    {
                        if(justLanded || Mathf.Abs(rightDiff) < SurfaceAngleThreshold)
                        {
                            if(Mathf.Abs(overlapDiff) > OverlapAngleThreshold && overlapDiff > OverlapAngleMinimum)
                            {
                                transform.RotateTo((s.rightCast.point - s.leftCast.point).Angle(), sensorGroundMid.position);
                                transform.position += (Vector3)s.rightCast.point - sensorGroundRight.position;
                                footing = Footing.Right;
                            } else {
                                transform.RotateTo(s.rightSurfaceAngle, s.rightCast.point);
                                transform.position += (Vector3)s.rightCast.point - sensorGroundRight.position;
                                footing = Footing.Right;
                            }
                            
                        } else if(Mathf.Abs(leftDiff) < SurfaceAngleThreshold) {
                            transform.RotateTo(s.leftSurfaceAngle, sensorGroundMid.position);
                            transform.position += (Vector3)s.leftCast.point - sensorGroundLeft.position;
                            footing = Footing.Left;
                        } else {
                            Detach();
                        }
                    }
                } else if(s.leftCast)
                {
                    float leftDiff = AMath.AngleDiff(s.leftSurfaceAngle, lastSurfaceAngle * Mathf.Deg2Rad) * Mathf.Rad2Deg;
                    if(justLanded || Mathf.Abs(leftDiff) < SurfaceAngleThreshold)
                    {
                        transform.RotateTo(s.leftSurfaceAngle, s.leftCast.point);
                        transform.position += (Vector3)s.leftCast.point - sensorGroundLeft.position;
                        footing = Footing.Left;
                    } else {
                        Detach();
                    }
                } else {
                    float rightDiff = AMath.AngleDiff(s.rightSurfaceAngle, lastSurfaceAngle * Mathf.Deg2Rad) * Mathf.Rad2Deg;
                    if(justLanded || Mathf.Abs(rightDiff) < SurfaceAngleThreshold)
                    {
                        transform.RotateTo(s.rightSurfaceAngle, s.rightCast.point);
                        transform.position += (Vector3)s.rightCast.point - sensorGroundRight.position;
                        footing = Footing.Right;
                    } else {
                        Detach();
                    }
                }
            } else {
                Detach();
            }

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
                Mathf.Abs(AMath.AngleDiff(lastSurfaceAngle * Mathf.Deg2Rad, surfaceAngle * Mathf.Deg2Rad)) * Mathf.Rad2Deg < SurfaceAngleThreshold))
            {
                if(wallMode == WallMode.Floor)
                {
                    if(surfaceAngle > 45.0f + WallModeAngleThreshold && surfaceAngle < 180.0f) wallMode = WallMode.Right;
                    else if(surfaceAngle < 315.0f - WallModeAngleThreshold && surfaceAngle > 180.0f) wallMode = WallMode.Left;
                } else if(wallMode == WallMode.Right)
                {
                    if(surfaceAngle > 135.0f + WallModeAngleThreshold) wallMode = WallMode.Ceiling;
                    else if(surfaceAngle < 45.0f - WallModeAngleThreshold) wallMode = WallMode.Floor;
                } else if(wallMode == WallMode.Ceiling)
                {
                    if(surfaceAngle > 225.0f + WallModeAngleThreshold) wallMode = WallMode.Left;
                    else if(surfaceAngle < 135.0f - WallModeAngleThreshold) wallMode = WallMode.Right;
                } else if(wallMode == WallMode.Left)
                {
                    if(surfaceAngle > 315.0f + WallModeAngleThreshold || surfaceAngle < 180.0f) wallMode = WallMode.Floor;
                    else if(surfaceAngle < 225.0f - WallModeAngleThreshold) wallMode = WallMode.Ceiling;
                }

                vx = vg * Mathf.Cos(surfaceAngle * Mathf.Deg2Rad);
                vy = vg * Mathf.Sin(surfaceAngle * Mathf.Deg2Rad);

                justLanded = false;
            } else {
                Detach();
            }
        }
    }

    /// <summary>
    /// Detaches the player from whatever surface it is on. If the player is not grounded this has no effect.
    /// </summary>
	private void Detach()
	{
		vg = 0.0f;
        lastSurfaceAngle = 0.0f;
		surfaceAngle = 0.0f;
		grounded = false;
		wallMode = WallMode.Floor;
        footing = Footing.None;
	}

    private void ResolveImpact(float surfaceAngle)
    {

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

    /// <summary>
    /// Returns the result of a linecast from the specified footing onto the surface based on wallmode.
    /// </summary>
    /// <returns>The result of the linecast.</returns>
    /// <param name="footing">The footing to linecast from.</param>
    private RaycastHit2D SurfaceCast(Footing footing)
    {
        if (footing == Footing.Left)
        {
            if(wallMode == WallMode.Floor || wallMode == WallMode.Ceiling)
            {
                return Physics2D.Linecast((Vector2)sensorGroundLeft.position + wallMode.UnitVector() * -size.y / 2.0f,
                                          (Vector2)sensorGroundLeft.position - wallMode.UnitVector() * -size.y / 2.0f,
                                          terrainMask);
            } else {
                return Physics2D.Linecast((Vector2)sensorGroundLeft.position + wallMode.UnitVector() * -size.x / 2.0f,
                                          (Vector2)sensorGroundLeft.position - wallMode.UnitVector() * -size.x / 2.0f,
                                          terrainMask);
            }
        } else {
            if(wallMode == WallMode.Floor || wallMode == WallMode.Ceiling)
            {
                return Physics2D.Linecast((Vector2)sensorGroundRight.position + wallMode.UnitVector() * -size.y / 2.0f,
                                          (Vector2)sensorGroundRight.position - wallMode.UnitVector() * -size.y / 2.0f,
                                          terrainMask);
            } else {
                return Physics2D.Linecast((Vector2)sensorGroundRight.position + wallMode.UnitVector() * -size.x / 2.0f,
                                          (Vector2)sensorGroundRight.position - wallMode.UnitVector() * -size.x / 2.0f,
                                          terrainMask);
            }
        }
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
            this.leftSurfaceAngle = (leftCast) ? leftCast.normal.Angle() - Mathf.PI / 2 : 0.0f;
            this.rightCast = rightCast;
            this.rightSurfaceAngle = (rightCast) ? rightCast.normal.Angle() - Mathf.PI / 2 : 0.0f;
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
}
