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
        public ControllerSide Side;

        /// <summary>
        /// The controller that initiated the cast, if any.
        /// </summary>
        public HedgehogController Controller;

        /// <summary>
        /// The angle of incline of the terrain hit in radians, if any.
        /// </summary>
        public float SurfaceAngle;

        /// <summary>
        /// The normal angle of the terrain hit in radians, if any.
        /// </summary>
        public float NormalAngle;

        /// <summary>
        /// The start point of the terrain cast.
        /// </summary>
        public Vector2 Start;

        /// <summary>
        /// The end point of the terrain cast.
        /// </summary>
        public Vector2 End;

        public TerrainCastHit(RaycastHit2D hit, ControllerSide fromSide = ControllerSide.All,
            HedgehogController controller = null, Vector2 start = default(Vector2), Vector2 end = default(Vector2))
        {
            Start = start;
            End = end;
            Hit = hit;
            Side = fromSide;
            Controller = controller;
            NormalAngle = DMath.Modp(DMath.Angle(hit.normal), DMath.DoublePi);
            SurfaceAngle = DMath.Modp(NormalAngle - DMath.HalfPi, DMath.DoublePi);
        }

        public static implicit operator bool(TerrainCastHit terrainInfo)
        {
            return terrainInfo == null ? false : terrainInfo.Hit;
        }
    }
}