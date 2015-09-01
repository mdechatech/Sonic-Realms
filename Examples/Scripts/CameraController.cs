using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
	/// <summary>
	/// The target for the camera to follow.
	/// </summary>
	[SerializeField]
	private GameObject followTarget;

	/// <summary>
	/// Must be 1 or higher. The higher this is, the smoother the camera movement.
	/// </summary>
	[SerializeField]
	private float easeFactor;

	/// <summary>
	/// Whether or not the camera stays put in terms of depth.
	/// </summary>
	[SerializeField]
	private bool zLock;

	/// <summary>
	/// If the z lock is off, the amount in distance to maintain from the target in terms of depth.
	/// </summary>
	[SerializeField]
	private float zOffset;

	private void FixedUpdate()
	{
		if(followTarget != null)
		{
			Camera.main.transform.position = new Vector3 (
				Camera.main.transform.position.x + (followTarget.transform.position.x - Camera.main.transform.position.x) / easeFactor, 
				Camera.main.transform.position.y + (followTarget.transform.position.y - Camera.main.transform.position.y) / easeFactor,
				(zLock) ? 
					Camera.main.transform.position.z : 
					Camera.main.transform.position.z + (followTarget.transform.position.z - zOffset) / easeFactor);
		}
	}
}