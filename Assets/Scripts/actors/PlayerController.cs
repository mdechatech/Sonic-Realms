using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
	private bool grounded;
	private float vx, vy, vg;
	private bool leftKeyDown, rightKeyDown, jumpKeyDown;

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
	private const float OverlapAngleThreshold = -40.0f;

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
		leftKeyDown = rightKeyDown = jumpKeyDown = false;
		wallMode = WallMode.Floor;
		surfaceAngle = 0.0f;
		terrainMask = 1 << LayerMask.NameToLayer("Terrain");

		// Enable later for ragdoll ?
		collider2D.enabled = false;
	}
	
	// Update is called once per frame
	private void Update () {
		HandleInput ();
	}


	private void HandleInput()
	{
		leftKeyDown = Input.GetKey (Settings.LeftKey);
		rightKeyDown = Input.GetKey (Settings.RightKey);
		if(!jumpKeyDown && grounded) jumpKeyDown = Input.GetKeyDown (Settings.JumpKey);
	}
	

	private void FixedUpdate()
	{
		transform.position = new Vector3 (transform.position.x + (vx * Time.fixedDeltaTime), transform.position.y + (vy * Time.fixedDeltaTime));

		bool justLanded = false;

		if(!grounded)
		{	
			if(grounded)
			{
				// If so, set vertical velocity to zero
				vy = 0;
				justLanded = true;
			} else {
				// Otherwise, apply gravity and keep the player pointing straight down
				transform.eulerAngles = new Vector3();
				vy -= gravity;
				surfaceAngle = 0.0f;

                //Side check
                Vector2 sideMidpoint = Vector2.Lerp(sensorSideLeft.position, sensorSideRight.position, 0.5f);
                RaycastHit2D leftCheck = Physics2D.Linecast(sideMidpoint, sensorSideLeft.position, terrainMask);
                RaycastHit2D rightCheck = Physics2D.Linecast(sideMidpoint, sensorSideRight.position, terrainMask);
                
                if(leftCheck && rightCheck)
                {
                    // not sure what to do here yet
                    vx = 0;
                    transform.position += (Vector3)leftCheck.point - sensorSideLeft.position;
                } else if(leftCheck)
                {
                    vx = 0;
                    transform.position += (Vector3)leftCheck.point - sensorSideLeft.position;
                } else if(rightCheck)
                {
                    vx = 0;
                    transform.position += (Vector3)rightCheck.point - sensorSideRight.position;
                }

				if(leftKeyDown) vx -= airAcceleration;
				else if(rightKeyDown) vx += airAcceleration;
			}

            // See if the player landed
            RaycastHit2D groundCheck = Physics2D.Linecast(sensorGroundLeft.position, sensorGroundRight.position, terrainMask);
            grounded = groundCheck;
		}

		if(grounded)
		{
			Collider2D[] cmem = new Collider2D[1];

			//Crush check - might not even put this in?
			if((Physics2D.OverlapPointNonAlloc(sensorCeilLeft.position, cmem, terrainMask) > 0 &&
                Physics2D.OverlapPointNonAlloc(sensorCeilRight.position, cmem, terrainMask) > 0))
			{
				Debug.Log("ded");
				// TODO kill him!
			}

			//Side check
			Vector2 sideMidpoint = Vector2.Lerp(sensorSideLeft.position, sensorSideRight.position, 0.5f);
            RaycastHit2D leftCheck = Physics2D.Linecast(sideMidpoint, sensorSideLeft.position, terrainMask);
            RaycastHit2D rightCheck = Physics2D.Linecast(sideMidpoint, sensorSideRight.position, terrainMask);

            if(leftCheck && rightCheck)
            {
                // not sure what to do here yet
                vg = 0;
                transform.position += (Vector3)leftCheck.point - sensorSideLeft.position;
            } else if(leftCheck)
            {
                vg = 0;
                transform.position += (Vector3)leftCheck.point - sensorSideLeft.position;
            } else if(rightCheck)
            {
                vg = 0;
                transform.position += (Vector3)rightCheck.point - sensorSideRight.position;
            }


			//Surface check
			SurfaceInfo s = GetSurface(terrainMask);
			
			if(s.hit)
			{
				float prevSurfaceAngle = surfaceAngle;

				if(s.footing == Footing.Left)
				{
					// Overlap routine - if the player's right foot is submerged, correct the player's rotation
					RaycastHit2D overlapCheck = SurfaceCast(Footing.Right);
					
					if(justLanded || !overlapCheck || AMath.AngleDiff(s.raycast.normal, overlapCheck.normal) * Mathf.Rad2Deg < OverlapAngleThreshold)
					{
						// Rotate the player to the surface on its left foot
                        transform.RotateTo(s.raycast.normal.Angle() - Mathf.PI / 2.0f, sensorGroundLeft.position);
						//transform.eulerAngles = new Vector3(0.0f, 0.0f, (s.raycast.normal.Angle() * Mathf.Rad2Deg) - 90.0f);
						
					} else {
						// Correct rotation if the two sensors have similarly oriented surfaces
						transform.eulerAngles = new Vector3(0.0f, 0.0f, (overlapCheck.point - s.raycast.point).Angle() * Mathf.Rad2Deg);
					}

                    // Keep the player on the surface
                    transform.position += (Vector3)s.raycast.point - sensorGroundLeft.position;
					
					surfaceAngle = transform.eulerAngles.z;
					
				} else if(s.footing == Footing.Right)
				{
					// Overlap routine - if the player's left foot is submerged, correct the player's rotation
					RaycastHit2D overlapCheck = SurfaceCast(Footing.Left);
					
					if(justLanded || !overlapCheck || AMath.AngleDiff(s.raycast.normal, overlapCheck.normal) * Mathf.Rad2Deg < OverlapAngleThreshold)
					{
						// Rotate the player to the surface on its right foot
						transform.eulerAngles = new Vector3(0.0f, 0.0f, (s.raycast.normal.Angle() * Mathf.Rad2Deg) - 90.0f);
                    } else {
                        // Correct rotation if the two sensors have similarly oriented surfaces
                        transform.eulerAngles = new Vector3(0.0f, 0.0f, (s.raycast.point - overlapCheck.point).Angle() * Mathf.Rad2Deg);
                    }

                    // Keep the player on the surface
                    transform.position += (Vector3)s.raycast.point - sensorGroundRight.position;
                    
                    surfaceAngle = transform.eulerAngles.z;
                }
                
                // Can only stay on the surface if angle difference is low enough
				if((justLanded ||
				 	Mathf.Abs(AMath.AngleDiff(prevSurfaceAngle * Mathf.Deg2Rad, surfaceAngle * Mathf.Deg2Rad)) * Mathf.Rad2Deg < SurfaceAngleThreshold))
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

					// Input
					if(leftKeyDown)
					{
						vg -= groundAcceleration;
						if(vg > 0) vg -= groundDeceleration;
					} else if(rightKeyDown)
					{
						vg += groundAcceleration;
						if(vg < 0) vg += groundDeceleration;
					} else if(vg != 0.0f && Mathf.Abs(vg) < groundDeceleration)
					{
						vg = 0.0f;
					} else if(vg > 0.0f)
					{
						vg -= groundDeceleration;
					} else if(vg < 0.0f)
					{
						vg += groundDeceleration;
					}

					if(vg > maxSpeed) vg = maxSpeed;
					else if(vg < -maxSpeed) vg = -maxSpeed;

					vx = vg * Mathf.Cos(surfaceAngle * Mathf.Deg2Rad);
					vy = vg * Mathf.Sin(surfaceAngle * Mathf.Deg2Rad);

					justLanded = false;

					if(jumpKeyDown) 
					{
						jumpKeyDown = false;
						
						float surfaceNormal = (surfaceAngle + 90.0f) * Mathf.Deg2Rad;
						vx += jumpSpeed * Mathf.Cos(surfaceNormal);
                        vy += jumpSpeed * Mathf.Sin(surfaceNormal);
                        
						Detach();
					}
				} else {
					Detach();
				}
            } else {
				Detach();
			}
		}
	}

	private void Detach()
	{
		vg = 0.0f;
		surfaceAngle = 0.0f;
		grounded = false;
		wallMode = WallMode.Floor;
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
				return new SurfaceInfo(true, checkLeft, Footing.Left);
			else return new SurfaceInfo(true, checkRight, Footing.Right);
		} else if(checkLeft)
		{
			return new SurfaceInfo(true, checkLeft, Footing.Left);
		} else if(checkRight)
		{
			return new SurfaceInfo(true, checkRight, Footing.Right);
		}

		return default(SurfaceInfo);
	}

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
		/// Whether or not there is a surface.
		/// </summary>
		public bool hit;

		/// <summary>
		/// If there is a surface, which foot of the player it is  beneath. Otherwise, Footing.none.
		/// </summary>
		public Footing footing;

		/// <summary>
		/// The result of the raycast onto the surface at the player's closest foot. This includes the normal
		/// of the surface and its location.
		/// </summary>
		public RaycastHit2D raycast;
		public SurfaceInfo(bool hit, RaycastHit2D raycast, Footing footing)
			{ this.hit = hit; this.raycast = raycast; this.footing = footing; }
	}

	private enum Footing
	{
		None, Left, Right,
	}
}
