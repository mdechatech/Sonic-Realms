using SonicRealms.Core.Actors;
using UnityEngine;

namespace SonicRealms.Core.Utils
{
    /// <summary>
    /// Contains data from the result of a terrain cast.
    /// </summary>
    public class TerrainCastHit
    {
        private static TerrainCastHit[] Pool;
        private static int PoolIndex;

        /// <summary>
        /// The resulting raycast hit data.
        /// </summary>
        public RaycastHit2D Raycast;

        /// <summary>
        /// The side from which the linecast originated.
        /// </summary>
        public ControllerSide Side;

        /// <summary>
        /// The controller that initiated the cast, if any.
        /// </summary>
        public HedgehogController Controller;
        
        /// <summary>
        /// The transform of the object hit, if any.
        /// </summary>
        public Transform Transform;

        /// <summary>
        /// The collider2D of the object hit, if any.
        /// </summary>
        public Collider2D Collider;

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

        /// <summary>
        /// The point in world space where the terrain cast hit.
        /// </summary>
        public Vector2 Point
        {
            get { return Raycast.point; }
        }

        static TerrainCastHit()
        {
            Pool = new TerrainCastHit[256];
            PoolIndex = 0;
        }

        public TerrainCastHit(RaycastHit2D hit, ControllerSide fromSide = ControllerSide.All,
            HedgehogController controller = null, Vector2 start = default(Vector2), Vector2 end = default(Vector2))
        {
            Initialize(hit, fromSide, controller, start, end);
        }

        public static TerrainCastHit FromPool(RaycastHit2D hit, ControllerSide fromSide = ControllerSide.All,
            HedgehogController controller = null, Vector2 start = default(Vector2), Vector2 end = default(Vector2))
        {
            var result = Pool[PoolIndex];

            if (result == null)
                result = Pool[PoolIndex] = new TerrainCastHit(hit, fromSide, controller, start, end);
            else
                result.Initialize(hit, fromSide, controller, start, end);

            PoolIndex = (PoolIndex + 1)%Pool.Length;

            return result;
        }

        public TerrainCastHit Initialize(RaycastHit2D hit, ControllerSide fromSide = ControllerSide.All,
            HedgehogController controller = null, Vector2 start = default(Vector2), Vector2 end = default(Vector2))
        {
            Start = start;
            End = end;
            Raycast = hit;
            Side = fromSide;
            Controller = controller;
            NormalAngle = DMath.Modp(DMath.Angle(hit.normal), DMath.DoublePi);
            SurfaceAngle = DMath.Modp(NormalAngle - DMath.HalfPi, DMath.DoublePi);

            if (!hit) return this;

            Transform = hit.transform;
            Collider = hit.collider;

            return this;
        }

        public static implicit operator bool(TerrainCastHit terrainInfo)
        {
            return terrainInfo != null && terrainInfo.Raycast;
        }

        public override string ToString()
        {
            return
                string.Format(
                    "[TerrainCastHit Side: {0}, Controller: {1}, Transform: {2}, SurfaceAngle: {3}, NormalAngle: {4}," +
                    "Start: {5}, Point: {6}, End: {7}]",
                    Side,
                    Controller, Transform, SurfaceAngle, NormalAngle, Start, Raycast.point, End);
        }
    }
}