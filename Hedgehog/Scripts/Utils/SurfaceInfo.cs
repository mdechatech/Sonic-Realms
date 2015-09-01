using UnityEngine;

namespace Hedgehog.Utils
{
    /// <summary>
    /// A collection of data about the surface at the player's feet.
    /// </summary>
    public struct SurfaceInfo
    {
        /// <summary>
        /// If there is a surface, which foot of the player it is  beneath. Otherwise, Side.none.
        /// </summary>
        public Side Side;

        /// <summary>
        /// The result of the raycast onto the surface at the player's left foot.
        /// </summary>
        public RaycastHit2D LeftCast;

        /// <summary>
        /// The angle, in radians, of the surface on the player's left foot, or 0 if there is none.
        /// </summary>
        public float LeftSurfaceAngle;

        /// <summary>
        /// The result of the raycast onto the surface at the player's right foot.
        /// </summary>
        public RaycastHit2D RightCast;

        /// <summary>
        /// The angle, in radians, of the surface on the player's right foot, or 0 if there is none.
        /// </summary>
        public float RightSurfaceAngle;

        public SurfaceInfo(RaycastHit2D leftCast, RaycastHit2D rightCast, Side Side)
        {
            LeftCast = leftCast;
            LeftSurfaceAngle = (leftCast) ? DMath.Angle(leftCast.normal) - DMath.HalfPi : 0.0f;
            RightCast = rightCast;
            RightSurfaceAngle = (rightCast) ? DMath.Angle(rightCast.normal) - DMath.HalfPi : 0.0f;
            this.Side = Side;
        }
    }
}
