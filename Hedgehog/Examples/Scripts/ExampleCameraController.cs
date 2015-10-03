using Hedgehog.Core.Actors;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Examples
{
    /// <summary>
    /// Example camera controller, good for catching up with fast hedgehogs.
    /// </summary>
    public class ExampleCameraController : MonoBehaviour
    {
        /// <summary>
        /// The target for the camera to follow.
        /// </summary>
        [SerializeField]
        public Transform FollowTarget;

        /// <summary>
        /// How smoothly the target's position is followed, 1 being smoothest.
        /// </summary>
        [SerializeField, Range(0.0f, 1.0f)]
        [Tooltip("How smoothly the target's position is followed, 1 being smoothest.")]
        public float Smoothness;

        /// <summary>
        /// Whether to rotate toward the follow target's direction of gravity, if it is a controller.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to rotate toward the follow target's direction of gravity, if it is a controller.")]
        public bool RotateToGravity;

        /// <summary>
        /// How smoothly the target is followed, 1 being smoothest.
        /// </summary>
        [SerializeField, Range(0.0f, 1.0f)]
        [Tooltip("How smoothly the target's rotation is followed, 1 being smoothest.")]
        public float RotationSmoothness;

        public void Reset()
        {
            Smoothness = 0.0f;
            RotationSmoothness = 0.2f;
            RotateToGravity = true;
        }

        public void FixedUpdate()
        {
            if (FollowTarget == null) return;
            var hedgehog = FollowTarget.GetComponent<HedgehogController>();

            if (DMath.Equalsf(Smoothness))
            {
                Camera.main.transform.position = new Vector3(FollowTarget.transform.position.x,
                    FollowTarget.transform.position.y,
                    Camera.main.transform.position.z);
            }
            else
            {
                Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position,
                    new Vector3(FollowTarget.transform.position.x, FollowTarget.transform.position.y,
                        Camera.main.transform.position.z),
                    Time.fixedDeltaTime * (1.0f / Smoothness)
                    );
            }

            if (RotateToGravity && hedgehog != null)
            {
                if (DMath.Equalsf(RotationSmoothness))
                {
                    Camera.main.transform.eulerAngles = new Vector3(
                        Camera.main.transform.eulerAngles.x,
                        Camera.main.transform.eulerAngles.y,
                        RotateToGravity ? hedgehog.GravityDirection + 90.0f : 0.0f);
                }
                else
                {
                    Camera.main.transform.eulerAngles = new Vector3(
                        Camera.main.transform.eulerAngles.x,
                        Camera.main.transform.eulerAngles.y,
                        Mathf.LerpAngle(
                            Camera.main.transform.eulerAngles.z,
                            RotateToGravity ? hedgehog.GravityDirection + 90.0f : 0.0f,
                            Time.fixedDeltaTime*(1.0f/RotationSmoothness)));
                }
            }
        }
    }
}