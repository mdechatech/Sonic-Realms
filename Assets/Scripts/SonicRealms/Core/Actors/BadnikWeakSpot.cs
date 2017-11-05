using SonicRealms.Core.Internal;
using SonicRealms.Core.Moves;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Actors
{
    /// <summary>
    /// The part of the badnik that makes it go 'poof' and bounces up the player when hit.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BadnikWeakSpot : ReactiveArea
    {
        /// <summary>
        /// The badnik's health system. It will sustain damage under the appropriate conditions.
        /// </summary>
        [Tooltip("The badnik's health system. It will sustain damage under the appropriate conditions.")]
        public HealthSystem Badnik;

        [HideInInspector]
        public Collider2D Collider2D;

        /// <summary>
        /// When hit from the top and traveling downwards, the player bounces by this factor.
        /// </summary>
        [Tooltip("When hit from the top and traveling downwards, the player bounces by this factor.")]
        public float TopBounceMultiplier;

        /// <summary>
        /// When hit from the bottom or traveling upwards, the player's speed is reduced by this amount.
        /// </summary>
        [Tooltip("When hit from the bottom or traveling upwards, the player's speed is reduced by this amount.")]
        public float BottomSpeedReduction;

        public override void Reset()
        {
            base.Reset();
            Badnik = GetComponentInParent<HealthSystem>();

            TopBounceMultiplier = 1f;
            BottomSpeedReduction = 1f;
        }

        public override void Start()
        {
            base.Start();
            Collider2D = GetComponent<Collider2D>();
        }

        public override bool CanTouch(AreaCollision.Contact contact)
        {
            return base.CanTouch(contact) || contact.Hitbox.CompareTag(Hitbox.AttackHitboxTag);
        }

        public override void OnAreaStay(AreaCollision collision)
        {
            var sonicHitbox = collision.Latest.Hitbox as SonicHitbox;
            if (sonicHitbox == null) return;

            // The weak spot can hurt the player back, too!
            if (sonicHitbox.Vulnerable && !sonicHitbox.Harmful)
            {
                sonicHitbox.Health.TakeDamage(transform);
            }

            if (sonicHitbox.Harmful)
            {
                Badnik.TakeDamage(sonicHitbox.transform);

                if (!collision.Controller.Grounded)
                {
                    var vy = collision.Controller.RelativeVelocity.y;

                    // If the controller is traveling upwards or is "lower" than the badnik relative to the direction of
                    // gravity, don't bounce and instead just apply speed reduction
                    if (vy > 0f && SrMath.HeightDifference(collision.Controller.transform.position, transform.position,
                        collision.Controller.GravityDirection * Mathf.Deg2Rad) > 0f)
                    {
                        vy -= BottomSpeedReduction * Mathf.Sign(vy);
                    }
                    else if(vy < 0f)
                    {
                        // Otherwise perform the bounce
                        var jump = collision.Controller.GetComponent<MoveManager>().Get<Jump>();

                        if (jump == null)
                        {
                            vy = 2.1f;
                        }
                        else if (jump.HeldDown)
                        {
                            collision.Controller.ApplyGravityOnce();
                            vy *= -TopBounceMultiplier;
                        }
                        else
                        {
                            vy = jump.ReleaseSpeed;
                        }
                    }

                    collision.Controller.RelativeVelocity = new Vector2(collision.Controller.RelativeVelocity.x, vy);
                }
            }
        }
    }
}
