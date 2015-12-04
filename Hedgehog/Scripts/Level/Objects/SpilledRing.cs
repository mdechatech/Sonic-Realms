using System;
using Hedgehog.Core.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Hedgehog.Level.Objects
{
    /// <summary>
    /// Gives a ring bounce physics, as if it were spilled after Sonic got hit.
    /// </summary>
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
        /// The ring's horizontal velocity.
        /// </summary>
        [Tooltip("The ring's horizontal velocity.")]
        public float VelocityX;

        /// <summary>
        /// The ring's vertical velocity.
        /// </summary>
        [Tooltip("The ring's vertical velocity.")]
        public float VelocityY;

        public Vector2 Velocity
        {
            get { return new Vector2(VelocityX, VelocityY); }
            set
            {
                VelocityX = value.x;
                VelocityY = value.y;
            }
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
            Velocity = Random.insideUnitCircle*Random.Range(2.0f, 4.0f);
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
            VelocityY -= Gravity*Time.fixedDeltaTime;

            // Store our change in position
            var diff = Velocity * Time.fixedDeltaTime;
            transform.position += (Vector3) diff;

            if (Velocity.magnitude > 0.01f)
            {
                // Make sure we don't hit ourselves
                var queriesHitTriggers = Physics2D.queriesHitTriggers;
                Physics2D.queriesHitTriggers = false;

                // Circlecast the change in position
                var result = Physics2D.CircleCast(_previousPosition, Radius, transform.position - _previousPosition,
                    (transform.position - _previousPosition).magnitude);

                if (result && result.fraction > 0.0f)
                {
                    // Store positive angle in degrees
                    var angle = DMath.PositiveAngle_d(DMath.Angle(result.normal) * Mathf.Rad2Deg);
                    if (AccurateBounce)
                    {
                        // For an accurate bounce, just reflect off the normal
                        Velocity = Vector2.Reflect(Velocity, result.normal)*AccurateBounceLoss;
                    }
                    else
                    {
                        // Check if the surface is vertical or horizontal
                        if ((angle > 22.5f && angle < 157.5f) || (angle > 202.5f && angle < 337.5f))
                        {
                            // Horizontal surface, bounce vertically
                            VelocityY *= -BounceLoss.y;

                            // If we've lost all momentum, set horizontal velocity to zero too
                            if (VelocityY < 0.01f)
                                VelocityX = 0.0f;
                        }
                        else
                        {
                            // Vertical surface, bounce horizontally
                            VelocityX *= -BounceLoss.x;

                            // If we've lost all momentum, set vertical velocity to zero too
                            if (VelocityX < 0.01f)
                                VelocityY = 0.0f;
                        }
                    }

                    // We hit something, revert the position change
                    transform.position -= (Vector3)diff;
                }
                
                Physics2D.queriesHitTriggers = queriesHitTriggers;
            }

            _previousPosition = transform.position;
        }
    }
}
