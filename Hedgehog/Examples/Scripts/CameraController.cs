using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
	/// <summary>
	/// The target for the camera to follow.
	/// </summary>
	[SerializeField]
	public GameObject FollowTarget;

	/// <summary>
	/// Must be 1 or higher. The higher this is, the smoother the camera movement.
	/// </summary>
	[SerializeField]
    public float EaseFactor = 1;

	/// <summary>
	/// Whether or not the camera stays put in terms of depth.
	/// </summary>
	[SerializeField]
    public bool ZLock;

	/// <summary>
	/// If the z lock is off, the amount in distance to maintain from the target in terms of depth.
	/// </summary>
	[SerializeField]
    public float ZOffset;

	public void LateUpdate()
	{
		if(FollowTarget != null)
		{
			Camera.main.transform.position = new Vector3 (
				Camera.main.transform.position.x + (FollowTarget.transform.position.x - Camera.main.transform.position.x) / EaseFactor, 
				Camera.main.transform.position.y + (FollowTarget.transform.position.y - Camera.main.transform.position.y) / EaseFactor,
				(ZLock) ? 
					Camera.main.transform.position.z : 
					Camera.main.transform.position.z + (FollowTarget.transform.position.z - ZOffset) / EaseFactor);
		}
	}
}