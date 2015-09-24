using Hedgehog.Core.Utils;

namespace Hedgehog.Core.Actors
{
    /// <summary>
    /// A collection of data about the surface at the player's feet.
    /// </summary>
    public struct SurfaceInfo
    {
        /// <summary>
        /// If there is a surface, which foot of the player it is  beneath. Otherwise, Footing.none.
        /// </summary>
        public Footing Side;

        /// <summary>
        /// The result of the raycast onto the surface at the player's left foot.
        /// </summary>
        public TerrainCastHit LeftCast;

        /// <summary>
        /// The result of the raycast onto the surface at the player's right foot.
        /// </summary>
        public TerrainCastHit RightCast;

        public SurfaceInfo(TerrainCastHit leftCast, TerrainCastHit rightCast, Footing Side)
        {
            LeftCast = leftCast;
            RightCast = rightCast;
            this.Side = Side;
        }
    }
}
