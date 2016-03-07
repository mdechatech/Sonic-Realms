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
    }
}
