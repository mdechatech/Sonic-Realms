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
        [Tooltip("The target for the camera to follow.")]
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
                transform.position = new Vector3(FollowTarget.transform.position.x,
                    FollowTarget.transform.position.y,
                    transform.position.z);
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position,
                    new Vector3(FollowTarget.transform.position.x, FollowTarget.transform.position.y,
                        transform.position.z),
                    Time.fixedDeltaTime * (1.0f / Smoothness)
                    );
            }

            if (RotateToGravity && hedgehog != null)
            {
                if (DMath.Equalsf(RotationSmoothness))
                {
                    transform.eulerAngles = new Vector3(
                        transform.eulerAngles.x,
                        transform.eulerAngles.y,
                        RotateToGravity ? hedgehog.GravityDirection + 90.0f : 0.0f);
                }
                else
                {
                    transform.eulerAngles = new Vector3(
                        transform.eulerAngles.x,
                        transform.eulerAngles.y,
                        Mathf.LerpAngle(
                            transform.eulerAngles.z,
                            RotateToGravity ? hedgehog.GravityDirection + 90.0f : 0.0f,
                            Time.fixedDeltaTime*(1.0f/RotationSmoothness)));
                }
            }
        }
    }
}