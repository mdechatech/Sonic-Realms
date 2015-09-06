using UnityEngine;

namespace Hedgehog.Terrain
{
    /// <summary>
    /// Contains data from the result of a terrain cast.
    /// </summary>
    public struct TerrainCastHit
    {
        /// <summary>
        /// The resulting raycast hit data.
        /// </summary>
        public RaycastHit2D Hit;

        /// <summary>
        /// The properties of the terrain hit, if any.
        /// </summary>
        public TerrainProperties Properties;

        /// <summary>
        /// The side from which the linecast originated.
        /// </summary>
        public TerrainSide Side;

        /// <summary>
        /// The angle of incline of the terrain hit, if any.
        /// </summary>
        public float SurfaceAngle;

        /// <summary>
        /// The normal angle of the terrain hit, if any.
        /// </summary>
        public float NormalAngle;

        public TerrainCastHit(RaycastHit2D hit, TerrainProperties properties, TerrainSide fromSide = TerrainSide.All)
        {
            Hit = hit;
            Properties = properties;
            Side = fromSide;
            NormalAngle = hit ? DMath.Angle(hit.normal) : 0.0f;
            SurfaceAngle = hit ? NormalAngle - DMath.HalfPi : 0.0f;
        }

        public static implicit operator bool(TerrainCastHit terrainInfo)
        {
            return terrainInfo.Hit;
        }
    }
}