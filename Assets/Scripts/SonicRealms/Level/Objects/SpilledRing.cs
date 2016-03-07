using System;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Objects
{
    /// <summary>
    /// Gives a ring bounce physics, as if it were spilled after Sonic got hit.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class SpilledRing : MonoBehaviour
    {
        protected Animator Animator;

        /// <summary>
        /// How long until the ring disappears, in seconds.
        /// </summary>
        [Tooltip("How long until the ring disappears, in seconds.")]
        public float Life;

        [NonSerialized]
        public float CurrentLife;

        /// <summary>
        /// Animator float set to the ring's current life over its total life.
        /// </summary>
        [Tooltip("Animator float set to the ring's current life over its total life.")]
        public string LifePercentFloat;
        protected int LifePercentFloatHash;

        /// <summary>
        /// The ring's gravity, in units per second squared.
        /// </summary>
        [Tooltip("The ring's gravity, in units per second squared.")]
        public float Gravity;

        /// <summary>
        /// The ring's rigidbody2D.
        /// </summary>
        public Rigidbody2D Rigidbody2D;

        /// <summary>
        /// The ring's rigidbody2D's current velocity, in units per second.
        /// </summary>
        public Vector2 Velocity
        {
            get { return Rigidbody2D.velocity; }
            set { Rigidbody2D.velocity = value; }
        }

        /// <summary>
        /// The ring's radius, in units.
        /// </summary>
        [Tooltip("The ring's radius, in units.")]
        public float Radius;

        /// <summary>
        /// After a bounce, the ring has this fraction of its horizontal and vertical velocity.
        /// </summary>
        [Tooltip("After a bounce, the ring has this fraction of its horizontal and vertical velocity.")]
        public Vector2 BounceLoss;

        /// <summary>
        /// If checked, the ring will bounce based on the angle of the surface it hits. Otherwise,
        /// it will only bounce horizontally and vertically like the classics.
        /// </summary>
        [Tooltip("If checked, the ring will bounce based on the angle of the surface it hits. Otherwise, " +
                 "it will only bounce horizontally and vertically like the classics.")]
        public bool AccurateBounce;

        /// <summary>
        /// The ring has this fraction of its velocity after an accurate bounce.
        /// </summary>
        [Tooltip("The ring has this fraction of its velocity after an accurate bounce.")]
        public float AccurateBounceLoss;

        private Vector3 _previousPosition;

        public void Reset()
        {
            Animator = GetComponentInChildren<Animator>();
            Life = 4.2666667f;
            Gravity = 3.375f;
            Radius = 0.08f;
            BounceLoss = new Vector2(0.25f, 0.75f);
            AccurateBounceLoss = 0.75f;
            AccurateBounce = false;
        }

        public void Awake()
        {
            Animator = Animator ?? GetComponent<Animator>();
            CurrentLife = Life;
            Gravity = 3.375f;
            Radius = 0.08f;
        }

        public void Start()
        {
            // So we don't collide with ourselves when circlecasting
            GetComponent<Collider2D>().isTrigger = true;

            Rigidbody2D = GetComponent<Rigidbody2D>();
            Rigidbody2D.gravityScale = 0.0f;

            _previousPosition = transform.position; 

            // Cache animator parameters to hashes
            if (Animator == null || string.IsNullOrEmpty(LifePercentFloat)) return;
            LifePercentFloatHash = Animator.StringToHash(LifePercentFloat);
        }

        public void Update()
        {
            // Update life timer
            CurrentLife -= Time.deltaTime;
            if (CurrentLife < 0.0f)
            {
                Destroy(gameObject);
                return;
            }

            // Update animator
            if (LifePercentFloatHash != 0)
                Animator.SetFloat(LifePercentFloatHash, CurrentLife/Life);
        }

        public void FixedUpdate()
        {
            // Apply gravity
            Rigidbody2D.velocity -= Vector2.up*Gravity*Time.fixedDeltaTime;

            if (Rigidbody2D.velocity.magnitude > 0.01f)
            {
                // Make sure we don't hit ourselves
                var queriesHitTriggers = Physics2D.queriesHitTriggers;
                Physics2D.queriesHitTriggers = false;

                // Circlecast the change in position
                var result = Physics2D.CircleCast(_previousPosition, Radius, transform.position - _previousPosition,
                    (transform.position - _previousPosition).magnitude, CollisionLayers.AllMask);

                Vector2 velocity = Rigidbody2D.velocity;

                if (result && result.fraction > 0f)
                {
                    // Store positive angle in degrees
                    var angle = DMath.PositiveAngle_d(DMath.Angle(result.normal) * Mathf.Rad2Deg);
                    if (AccurateBounce)
                    {
                        // For an accurate bounce, just reflect off the normal
                        velocity = Vector2.Reflect(velocity, result.normal)*AccurateBounceLoss;
                    }
                    else
                    {
                        // Check if the surface is vertical or horizontal
                        if ((angle > 22.5f && angle < 157.5f) || (angle > 202.5f && angle < 337.5f))
                        {
                            // Horizontal surface, bounce vertically
                            velocity = new Vector2(velocity.x, -BounceLoss.y);

                            // If we've lost all momentum, set horizontal velocity to zero too
                            if (velocity.y < 0.01f)
                                velocity = new Vector2(0.0f, 0.0f);
                        }
                        else
                        {
                            // Vertical surface, bounce horizontally
                            velocity = new Vector2(-BounceLoss.x, velocity.y);

                            // If we've lost all momentum, set vertical velocity to zero too
                            if (velocity.x < 0.01f)
                                velocity = new Vector2(0.0f, 0.0f);
                        }
                    }

                    // We hit something, revert the position change
                    transform.position = _previousPosition;
                }

                Rigidbody2D.velocity = velocity;
                Physics2D.queriesHitTriggers = queriesHitTriggers;
            }

            _previousPosition = transform.position;
        }

        public void OnDisable()
        {
            Velocity = new Vector2();
        }
    }
}
