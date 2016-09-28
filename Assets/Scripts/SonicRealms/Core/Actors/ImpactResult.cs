namespace SonicRealms.Core.Actors
{
    public struct ImpactResult
    {
        public bool ShouldAttach;
        public float GroundSpeed;
        public float SurfaceAngle;

        public static implicit operator bool(ImpactResult impactResult)
        {
            return impactResult.ShouldAttach;
        }

        public override string ToString()
        {
            return string.Format("GroundSpeed: {0}, ShouldAttach: {1}, SurfaceAngle: {2}", GroundSpeed, ShouldAttach, SurfaceAngle);
        }
    }
}
