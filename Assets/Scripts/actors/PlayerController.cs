using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
	private bool grounded;
	private float vx, vy, vg;

	// If grounded, the angle of the incline the player is standing on
	private float surfaceAngle;

	private WallMode wallMode;

	// The layer mask which represents all terrain to check for collision with
	private int terrainMask;

	[SerializeField]
	private float gravity;

	// Sensors
	[SerializeField]
	private Transform sensorGroundLeft;
	[SerializeField]
	private Transform sensorGroundRight;
	[SerializeField]
	private Transform sensorSideLeft;
	[SerializeField]
	private Transform sensorSideRight;
	[SerializeField]
	private Transform sensorCeilLeft;
	[SerializeField]
	private Transform sensorCeilRight;
	[SerializeField]
	private Transform sensorTileLeft;
	[SerializeField]
	private Transform sensorTileRight;


	// Use this for initialization
	void Start () {
		grounded = false;
		vx = vy = vg = 0.0f;
		wallMode = WallMode.Floor;
		surfaceAngle = 0.0f;
		terrainMask = 1 << LayerMask.NameToLayer("Terrain");

		collider2D.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {

	}
	

	void FixedUpdate()
	{
		transform.position = new Vector3 (transform.position.x + (vx * Time.fixedDeltaTime), transform.position.y + (vy * Time.fixedDeltaTime));

		if(!grounded)
		{
			// See if the player landed
			RaycastHit2D groundCheck = Physics2D.Linecast(sensorGroundLeft.position, sensorGroundRight.position, terrainMask);
			grounded = groundCheck;
			
			if(grounded)
			{
				// If so, set vertical velocity to zero
				vy = 0;

				Debug.DrawLine (sensorGroundLeft.position, sensorGroundRight.position, Color.red);
			} else {
				// Otherwise, apply gravity and keep the player pointing straight down
				transform.eulerAngles = new Vector3();
				vy -= gravity;

				Debug.DrawLine (sensorGroundLeft.position, sensorGroundRight.position, Color.green);
			}
		}

		if(grounded)
		{
			SurfaceInfo s = GetSurface(terrainMask);
			
			if(s.hit)
			{
				if(s.footing == Footing.Left)
				{
					// Rotate the player to the surface on its left foot
					transform.eulerAngles = new Vector3(0.0f, 0.0f, (s.raycast.normal.Angle() * Mathf.Rad2Deg) - 90.0f);

					// Overlap routine - if the player's right foot is submerged, correct the player's rotation
					RaycastHit2D overlapCheck = Physics2D.Linecast(sensorSideRight.position, sensorGroundRight.position, terrainMask);

					Debug.DrawLine(sensorSideRight.position, sensorTileRight.position, Color.gray);

					if(overlapCheck) 
					{
						transform.eulerAngles = new Vector3(0.0f, 0.0f, 
						                                    (overlapCheck.point - s.raycast.point).Angle() * Mathf.Rad2Deg);

						Debug.DrawLine(sensorTileRight.position, overlapCheck.point, Color.red);
                    }

					// Keep the player on the surface
                    transform.position += (Vector3)s.raycast.point - sensorGroundLeft.position;
                    
                    Debug.DrawLine(sensorSideLeft.position, s.raycast.point, Color.green);
					Debug.DrawLine(s.raycast.point, sensorTileLeft.position, Color.red);

                } else if(s.footing == Footing.Right)
				{
					// Rotate the player to the surface on its right foot
					transform.eulerAngles = new Vector3(0.0f, 0.0f, (s.raycast.normal.Angle() * Mathf.Rad2Deg) - 90.0f);

					// Overlap routine - if the player's left foot is submerged, correct the player's rotation
					RaycastHit2D overlapCheck = Physics2D.Linecast(sensorSideLeft.position, sensorGroundLeft.position, terrainMask);

					Debug.DrawLine(sensorSideLeft.position, sensorTileLeft.position, Color.gray);

					if(overlapCheck) 
					{

						transform.eulerAngles = new Vector3(0.0f, 0.0f, 
						                                    (s.raycast.point - overlapCheck.point).Angle() * Mathf.Rad2Deg);

						Debug.DrawLine(sensorTileLeft.position, overlapCheck.point, Color.red);
                    }
                    
					// Keep the player on the surface
                    transform.position += (Vector3)s.raycast.point - sensorGroundRight.position;

					Debug.DrawLine(sensorSideRight.position, s.raycast.point, Color.green);
					Debug.DrawLine(s.raycast.point, sensorTileRight.position, Color.red);
                }
            } else {
				grounded = false;
			}
		}
	}

	/// <summary>
	/// Gets data about the surface closest to the player's feet, including its footing and raycast info.
	/// </summary>
	/// <returns>The surface.</returns>
	/// <param name="layerMask">A mask indicating what layers are surfaces.</param>
	private SurfaceInfo GetSurface(int layerMask)
	{
		RaycastHit2D checkLeft = Physics2D.Linecast (sensorSideLeft.position, sensorTileLeft.position, layerMask);
		RaycastHit2D checkRight = Physics2D.Linecast (sensorSideRight.position, sensorTileRight.position, layerMask);

		if(checkLeft && checkRight)
		{
			if(AMath.Highest(checkLeft.point, checkRight.point, wallMode.Normal()) > 0.0f)
			{
				return new SurfaceInfo(true, checkLeft, Footing.Left);
			} else {
                return new SurfaceInfo(true, checkRight, Footing.Right);
            }
		} else if(checkLeft)
		{
			return new SurfaceInfo(true, checkLeft, Footing.Left);
		} else if(checkRight)
		{
			return new SurfaceInfo(true, checkRight, Footing.Right);
		}

		return default(SurfaceInfo);
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
