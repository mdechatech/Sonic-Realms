using SonicRealms.Core.Utils;

namespace SonicRealms.Core.Triggers
{
    public class LimitPlatformCollision : ReactivePlatform
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

        public override bool IsSolid(TerrainCastHit data)
        {
            if (LimitGrounded && !GroundedLimiter.Allows(data.Controller))
                return true;

            if (LimitVelocity && !VelocityLimiter.Allows(data.Controller))
                return true;

            if (LimitAirSpeed && !AirSpeedLimiter.Allows(data.Controller))
                return true;

            if (LimitGroundSpeed && !GroundSpeedLimiter.Allows(data.Controller))
                return true;

            if (LimitSurfaceAngle && !SurfaceAngleLimiter.Allows(data))
                return true;

            if (LimitMoves && !MovesLimiter.Allows(data.Controller))
                return true;

            if (LimitPowerups && !PowerupsLimiter.Allows(data.Controller))
                return true;

            return false;
        }
    }
}
