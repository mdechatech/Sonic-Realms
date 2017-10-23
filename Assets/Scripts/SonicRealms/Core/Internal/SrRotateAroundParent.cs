using UnityEngine;

namespace SonicRealms.Core.Internal
{
    /// <summary>
    /// Rotates the object about its parent.
    /// </summary>
    public class SrRotateAroundParent : MonoBehaviour
    {
        public float RevolutionsPerSecond;
        public float Radius;

        /// <summary>
        /// Current progress of revolution, between 0 and 1.
        /// </summary>
        [Space]
        [Tooltip("Current progress of revolution, between 0 and 1.")]
        public float Time;

        public void Reset()
        {
            RevolutionsPerSecond = 1f;
            Radius = 1f;
        }

        public void FixedUpdate()
        {
            Time += RevolutionsPerSecond*UnityEngine.Time.fixedDeltaTime;
            Time %= 1f;

            transform.position = transform.parent.position + (Vector3) SrMath.UnitVector(Time*Mathf.PI*2f)*Radius;
        }
    }
}
