using Hedgehog.Terrain;
using UnityEngine;

namespace Hedgehog.Utils
{
    /// <summary>
    /// Contains extension methods for Orientation
    /// </summary>
    public static class OrientationExtensions
    {
        /// <summary>
        /// Returns the wall mode of a surface with the specified angle.
        /// </summary>
        /// <returns>The wall mode.</returns>
        /// <param name="angleRadians">The surface angle in radians.</param>
        public static Orientation FromSurfaceAngle(float angleRadians)
        {
            float angle = DMath.Modp(angleRadians, DMath.DoublePi);

            if (angle <= Mathf.PI*0.25f || angle > Mathf.PI*1.75f)
                return Orientation.Floor;
            else if (angle > Mathf.PI*0.25f && angle <= Mathf.PI*0.75f)
                return Orientation.Right;
            else if (angle > Mathf.PI*0.75f && angle <= Mathf.PI*1.25f)
                return Orientation.Ceiling;
            else
                return Orientation.Left;
        }

        /// <summary>
        /// Returns a unit vector which represents the direction in which a wall mode points.
        /// </summary>
        /// <returns>The vector which represents the direction in which a wall mode points.</returns>
        /// <param name="orientation">The wall mode.</param>
        public static Vector2 UnitVector(this Orientation orientation)
        {
            switch (orientation)
            {
                case Orientation.Floor:
                    return new Vector2(0.0f, -1.0f);

                case Orientation.Ceiling:
                    return new Vector2(0.0f, 1.0f);

                case Orientation.Left:
                    return new Vector2(-1.0f, 0.0f);

                case Orientation.Right:
                    return new Vector2(1.0f, 0.0f);

                default:
                    return default(Vector2);
            }
        }

        /// <summary>
        /// Returns the normal angle in radians (-pi to pi) of a flat wall in the specified wall mode.
        /// </summary>
        /// <param name="orientation">The wall mode.</param>
        public static float Normal(this Orientation orientation)
        {
            switch (orientation)
            {
                case Orientation.Floor:
                    return DMath.HalfPi;

                case Orientation.Right:
                    return Mathf.PI;

                case Orientation.Ceiling:
                    return -DMath.HalfPi;

                case Orientation.Left:
                    return 0.0f;

                default:
                    return default(float);
            }
        }

        /// <summary>
        /// Returns the wall mode which points in the direction opposite to this one.
        /// </summary>
        /// <param name="orientation">The wall mode.</param>
        public static Orientation Opposite(this Orientation orientation)
        {
            switch (orientation)
            {
                case Orientation.Floor:
                    return Orientation.Ceiling;

                case Orientation.Right:
                    return Orientation.Left;

                case Orientation.Ceiling:
                    return Orientation.Floor;

                case Orientation.Left:
                    return Orientation.Right;

                default:
                    return Orientation.None;
            }
        }

        /// <summary>
        /// Returns the wall mode adjacent to this traveling clockwise.
        /// </summary>
        /// <returns>The adjacent wall mode.</returns>
        /// <param name="orientation">The given wall mode.</param>
        public static Orientation AdjacentCW(this Orientation orientation)
        {
            switch (orientation)
            {
                case Orientation.Floor:
                    return Orientation.Left;

                case Orientation.Right:
                    return Orientation.Floor;

                case Orientation.Ceiling:
                    return Orientation.Right;

                case Orientation.Left:
                    return Orientation.Ceiling;

                default:
                    return Orientation.None;
            }
        }

        /// <summary>
        /// Returns the wall mode adjacent to this traveling counter-clockwise.
        /// </summary>
        /// <returns>The adjacent wall mode.</returns>
        /// <param name="orientation">The given wall mode.</param>
        public static Orientation AdjacentCCW(this Orientation orientation)
        {
            switch (orientation)
            {
                case Orientation.Floor:
                    return Orientation.Right;

                case Orientation.Right:
                    return Orientation.Ceiling;

                case Orientation.Ceiling:
                    return Orientation.Left;

                case Orientation.Left:
                    return Orientation.Floor;

                default:
                    return Orientation.None;
            }
        }

        public static TerrainSide ToTerrainSide(this Orientation orientation)
        {
            switch (orientation)
            {
                case Orientation.Floor:
                    return TerrainSide.Bottom;
                
                case Orientation.Right:
                    return TerrainSide.Right;

                case Orientation.Ceiling:
                    return TerrainSide.Top;

                case Orientation.Left:
                    return TerrainSide.Left;

                default:
                    return TerrainSide.None;
            }
        }
    }
}