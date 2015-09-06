using Hedgehog.Terrain;

namespace Hedgehog.Utils
{
    /// <summary>
    /// A collection of data about the surface at the player's feet.
    /// </summary>
    public struct SurfaceInfo
    {
        /// <summary>
        /// If there is a surface, which foot of the player it is  beneath. Otherwise, FootSide.none.
        /// </summary>
        public FootSide Side;

        /// <summary>
        /// The result of the raycast onto the surface at the player's left foot.
        /// </summary>
        public TerrainCastHit LeftCast;

        /// <summary>
        /// The result of the raycast onto the surface at the player's right foot.
        /// </summary>
        public TerrainCastHit RightCast;

        public SurfaceInfo(TerrainCastHit leftCast, TerrainCastHit rightCast, FootSide Side)
        {
            LeftCast = leftCast;
            RightCast = rightCast;
            this.Side = Side;
        }
    }
}
