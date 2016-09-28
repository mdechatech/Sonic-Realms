using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Generic platform that activates the effect trigger when a controller collides with it.
    /// </summary>
    public class ActivatePlatform : ReactivePlatform
    {
        /// <summary>
        /// Whether to activate the platform right before it's collided with.
        /// </summary>
        [Tooltip("Whether to activate the platform right before it's collided with.")]
        public bool WhenPreColliding;

        /// <summary>
        /// Whether to activate the platform when collided with.
        /// </summary>
        [Tooltip("Whether to activate the platform when collided with.")]
        public bool WhenColliding;

        /// <summary>
        /// Whether to activate the platform when a controller stands on it.
        /// </summary>
        [Tooltip("Whether to activate the platform when a controller stands on it.")]
        public bool WhenOnSurface;

        public bool LimitGrounded;
        public GroundedLimiter GroundedLimiter;

        public bool LimitVelocity;
        public VelocityLimiter VelocityLimiter;

        public bool LimitAirSpeed;
        public AirSpeedLimiter AirSpeedLimiter;

        public bool LimitGroundSpeed;
        public GroundSpeedLimiter GroundSpeedLimiter;

        public bool LimitSurfaceAngle;
        public SurfaceAngleLimiter SurfaceAngleLimiter;

        public bool LimitMoves;
        public MovesLimiter MovesLimiter;

        public bool LimitPowerups;
        public PowerupsLimiter PowerupsLimiter;

        public override void Reset()
        {
            base.Reset();

            if (!GetComponent<EffectTrigger>())
                gameObject.AddComponent<EffectTrigger>();

            WhenColliding = false;
            WhenOnSurface = true;
        }

        public override void OnPreCollide(PlatformCollision.Contact contact)
        {
            if (WhenPreColliding && Check(new PlatformCollision(new[] {contact})))
                BlinkEffectTrigger(contact.Controller);
        }

        public override void OnPlatformEnter(PlatformCollision collision)
        {
            if (!WhenColliding)
                return;

            if (Check(collision))
                ActivateEffectTrigger(collision.Controller);
        }

        public override void OnPlatformStay(PlatformCollision collision)
        {
            if (!WhenColliding)
                return;

            if (Check(collision))
                ActivateEffectTrigger(collision.Controller);
            else
                DeactivateEffectTrigger(collision.Controller);
        }

        public override void OnPlatformExit(PlatformCollision collision)
        {
            if (!WhenColliding)
                return;

            DeactivateEffectTrigger(collision.Controller);
        }

        public bool Check(PlatformCollision collision)
        {
            if (LimitGrounded && !GroundedLimiter.Allows(collision.Controller))
                return false;

            if (LimitVelocity && !VelocityLimiter.Allows(collision))
                return false;

            if (LimitAirSpeed && !AirSpeedLimiter.Allows(collision))
                return false;

            if (LimitGroundSpeed && !GroundSpeedLimiter.Allows(collision))
                return false;

            if (LimitSurfaceAngle && !SurfaceAngleLimiter.Allows(collision.Latest.HitData))
                return false;

            if (LimitMoves && !MovesLimiter.Allows(collision.Controller))
                return false;

            if (LimitPowerups && !PowerupsLimiter.Allows(collision.Controller))
                return false;

            return true;
        }

        public override void OnSurfaceEnter(SurfaceCollision collision)
        {
            if (!WhenOnSurface)
                return;

            if(Check(collision))
                ActivateEffectTrigger(collision.Controller);
        }

        public override void OnSurfaceStay(SurfaceCollision collision)
        {
            if (!WhenOnSurface)
                return;

            if (Check(collision))
                ActivateEffectTrigger(collision.Controller);
            else
                DeactivateEffectTrigger(collision.Controller);
        }

        public override void OnSurfaceExit(SurfaceCollision collision)
        {
            if (!WhenOnSurface)
                return;

            DeactivateEffectTrigger(collision.Controller);
        }

        public bool Check(SurfaceCollision collision)
        {
            if (LimitGrounded && !GroundedLimiter.Allows(collision.Controller))
                return false;

            if (LimitVelocity && !VelocityLimiter.Allows(collision))
                return false;

            if (LimitAirSpeed && !AirSpeedLimiter.Allows(collision))
                return false;

            if (LimitGroundSpeed && !GroundSpeedLimiter.Allows(collision))
                return false;

            if (LimitSurfaceAngle && !SurfaceAngleLimiter.Allows(collision.Latest.HitData))
                return false;

            if (LimitMoves && !MovesLimiter.Allows(collision.Controller))
                return false;

            if (LimitPowerups && !PowerupsLimiter.Allows(collision.Controller))
                return false;

            return true;
        }
    }
}
