using Hedgehog.Core.Actors;
using UnityEngine;

namespace Hedgehog.Core.Utils
{
    /// <summary>
    /// Contains data from the result of a terrain cast.
    /// </summary>
    public class TerrainCastHit
    {
        /// <summary>
        /// The resulting raycast hit data.
        /// </summary>
        public RaycastHit2D Hit;

        /// <summary>
        /// The side from which the linecast originated.
        /// </summary>
        public TerrainSide Side;

        /// <summary>
        /// The controller that initiated the cast, if any.
        /// </summary>
        public HedgehogController Source;

        /// <summary>
        /// The angle of incline of the terrain hit, if any.
        /// </summary>
        public float SurfaceAngle;

        /// <summary>
        /// The normal angle of the terrain hit, if any.
        /// </summary>
        public float NormalAngle;

        public TerrainCastHit(RaycastHit2D hit, TerrainSide fromSide = TerrainSide.All,
            HedgehogController source = null)
        {
            Hit = hit;
            Side = fromSide;
            Source = source;
            NormalAngle = hit ? DMath.Angle(hit.normal) : 0.0f;
            SurfaceAngle = hit ? NormalAngle - DMath.HalfPi : 0.0f;
        }

        public static implicit operator bool(TerrainCastHit terrainInfo)
        {
            return terrainInfo == null ? false : terrainInfo.Hit;
        }
    }
}