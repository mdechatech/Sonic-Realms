using UnityEngine;

namespace SonicRealms.Level.Objects
{
    /// <summary>
    /// Ring that has been attracted by an electric shield and will seek out the controller.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class MagnetizedRing : MonoBehaviour
    {
        /// <summary>
        /// The target to follow.
        /// </summary>
        [Tooltip("The target to follow.")]
        public Transform Target;

        /// <summary>
        /// The ring's acceleration, in units per second squared.
        /// </summary>
        [Tooltip("The ring's acceleration, in units per second squared.")]
        public float Acceleration;

        /// <summary>
        /// The ring's rigidbody.
        /// </summary>
        protected Rigidbody2D Rigidbody2D;

        public void Reset()
        {
            Acceleration = 6.75f;
        }

        public void Awake()
        {
            if(Acceleration == 0f) Acceleration = 6.75f;
        }

        public void Start()
        {
            Rigidbody2D = GetComponent<Rigidbody2D>();
            Rigidbody2D.gravityScale = 0.0f;
        }

        public void FixedUpdate()
        {
            if (!Target) return;

            Rigidbody2D.velocity += (Vector2) (Target.position - transform.position).normalized*Acceleration*
                                    Time.deltaTime;
            Rigidbody2D.velocity *= 0.989f;
        }
    }
}
