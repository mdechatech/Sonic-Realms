using UnityEngine;

namespace SonicRealms.Level.Effects
{
    /// <summary>
    /// Put onto debris objects. Flings the debris towards the controller, like the breakable wall
    /// in Green Hill Zone.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class DebrisExplode : MonoBehaviour
    {
        /// <summary>
        /// Velocity at which the debris is flung, in units per second.
        /// </summary>
        [Tooltip("Velocity at which the debris is flung, in units per second.")]
        public float Power;

        /// <summary>
        /// Gravity, in units per second squared.
        /// </summary>
        [Tooltip("Gravity, in units per second squared.")]
        public float Gravity;

        /// <summary>
        /// Debris life, in seconds.
        /// </summary>
        [Tooltip("Debris life, in seconds.")]
        public float Life;

        private float _lifeCountdown;

        protected DebrisData Data;
        protected Rigidbody2D Rigidbody2D;

        public void Reset()
        {
            Power = 6.0f;
            Life = 1.0f;
        }

        public void Start()
        {
            Data = GetComponent<DebrisData>();
            Rigidbody2D = GetComponent<Rigidbody2D>() ?? gameObject.AddComponent<Rigidbody2D>();

            var dir = (transform.position - 
                (Data.Source.transform.position + (Data.Source.transform.position - Data.Controller.transform.position)))
                .normalized;

            Rigidbody2D.velocity = dir*Power;
        
            _lifeCountdown = Life;
        }

        public void FixedUpdate()
        {
            Rigidbody2D.velocity += Vector2.down*Gravity*Time.fixedDeltaTime;

            _lifeCountdown -= Time.fixedDeltaTime;
            if (_lifeCountdown < 0.0f) Destroy(gameObject);
        }
    }
}
