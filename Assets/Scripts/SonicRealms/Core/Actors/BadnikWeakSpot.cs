using SonicRealms.Core.Moves;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Actors
{
    [RequireComponent(typeof(Collider2D))]
    public class BadnikWeakSpot : ReactiveArea
    {
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

        public override bool IsInside(Hitbox hitbox)
        {
            return base.IsInside(hitbox) || hitbox.CompareTag(Hitbox.AttackHitboxTag);
        }

        public override void OnAreaStay(Hitbox hitbox)
        {
            var sonicHitbox = hitbox as SonicHitbox;
            if (sonicHitbox == null) return;

            if (sonicHitbox.Vulnerable && !sonicHitbox.Harmful)
            {
                sonicHitbox.Health.TakeDamage(transform);
            }

            if (sonicHitbox.Harmful)
            {
                Badnik.TakeDamage(sonicHitbox.transform);

                if (!hitbox.Controller.Grounded)
                {
                    var vy = hitbox.Controller.RelativeVelocity.y;

                    // If the controller is traveling upwards or is "lower" than the badnik relative to the direction of
                    // gravity, don't bounce and instead just apply speed reduction
                    if (vy > 0f && DMath.Highest(hitbox.Controller.transform.position, transform.position,
                        hitbox.Controller.GravityDirection * Mathf.Deg2Rad) > 0f)
                    {
                        vy -= BottomSpeedReduction * Mathf.Sign(vy);
                    }
                    else if(vy < 0f)
                    {
                        // Otherwise perform the bounce
                        var jump = hitbox.Controller.GetComponent<MoveManager>().Get<Jump>();

                        if (jump == null)
                        {
                            vy = 2.1f;
                        }
                        else if (jump.HeldDown)
                        {
                            hitbox.Controller.ApplyGravityOnce();
                            vy *= -TopBounceMultiplier;
                        }
                        else
                        {
                            vy = jump.ReleaseSpeed;
                        }
                    }

                    hitbox.Controller.RelativeVelocity = new Vector2(hitbox.Controller.RelativeVelocity.x, vy);
                }
            }
        }
    }
}
