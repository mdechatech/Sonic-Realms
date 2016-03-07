using UnityEngine;

namespace SonicRealms.Core.Utils
{
    /// <summary>
    /// Rotates the object about its parent.
    /// </summary>
    public class RotateAroundParent : MonoBehaviour
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

        public void Update()
        {
            Time += RevolutionsPerSecond*UnityEngine.Time.deltaTime;
            Time %= 1f;

            transform.position = transform.parent.position +
                                 (Vector3) DMath.AngleToVector(Time*Mathf.PI*2f)*Radius;
        }
    }
}
