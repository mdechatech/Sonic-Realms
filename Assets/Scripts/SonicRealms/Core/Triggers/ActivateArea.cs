using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Generic area that activates the object trigger when a controller is inside it.
    /// </summary>
    public class ActivateArea : ReactiveArea
    {
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
        }

        public override void OnAreaEnter(AreaCollision collision)
        {
            if(Check(collision))
                ActivateEffectTrigger(collision.Controller);
        }

        public override void OnAreaStay(AreaCollision collision)
        {
            if (Check(collision))
                ActivateEffectTrigger(collision.Controller);
            else
                DeactivateEffectTrigger(collision.Controller);
        }

        public override void OnAreaExit(AreaCollision collision)
        {
            DeactivateEffectTrigger(collision.Controller);
        }

        public bool Check(AreaCollision collision)
        {
            if (LimitGrounded && !GroundedLimiter.Allows(collision.Controller))
                return false;

            if (LimitVelocity && !VelocityLimiter.Allows(collision))
                return false;

            if (LimitAirSpeed && !AirSpeedLimiter.Allows(collision))
                return false;

            if (LimitGroundSpeed && !GroundSpeedLimiter.Allows(collision))
                return false;

            if (LimitSurfaceAngle && !SurfaceAngleLimiter.Allows(collision.Controller))
                return false;

            if (LimitMoves && !MovesLimiter.Allows(collision.Controller))
                return false;

            if (LimitPowerups && !PowerupsLimiter.Allows(collision.Controller))
                return false;

            return true;
        }
    }
}
