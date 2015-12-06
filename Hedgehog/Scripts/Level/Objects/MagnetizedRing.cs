using UnityEngine;

namespace Hedgehog.Level.Objects
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
        public Vector2 Acceleration;

        /// <summary>
        /// The ring's rigidbody.
        /// </summary>
        protected Rigidbody2D Rigidbody2D;

        public void Reset()
        {
            Acceleration = new Vector2(6.75f, 6.75f);
        }

        public void Awake()
        {
            if(Acceleration == default(Vector2))
                Acceleration = new Vector2(6.75f, 6.75f);
        }

        public void Start()
        {
            Rigidbody2D = GetComponent<Rigidbody2D>();
            Rigidbody2D.gravityScale = 0.0f;
        }

        public void FixedUpdate()
        {
            if (!Target) return;
            Rigidbody2D.velocity += new Vector2(
                Target.position.x > transform.position.x ? Acceleration.x : -Acceleration.x,
                Target.position.y > transform.position.y ? Acceleration.y : -Acceleration.y)
                                    *Time.fixedDeltaTime;
        }
    }
}
