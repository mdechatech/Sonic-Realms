using System;
using UnityEngine;

namespace SonicRealms.Core.Utils
{
    /// <summary>
    /// Making math-related helper functions as we go!
    /// </summary>
    public static class DMath
    {
        /// <summary>
        /// This epsilon is good enough for me!
        /// </summary>
        public const float Epsilon = 0.0001f;

        /// <summary>
        /// Half of pi.
        /// </summary>
        public const float HalfPi = 1.5707963f;

        /// <summary>
        /// One and three quarters pi.
        /// </summary>
        public const float OneThreeFourthsPi = 5.4977871f;

        /// <summary>
        /// Double pi.
        /// </summary>
        public const float DoublePi = 6.283185f;

        /// <summary>
        /// Returns c in the formula a/b = c/d.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static float Proportion(float a, float b, float d)
        {
            return a/b*d;
        }

        /// <summary>
        /// Flips the specified value over a midpoint.
        /// </summary>
        /// <param name="value">The specified value.</param>
        /// <param name="over">The midpoint.</param>
        /// <returns></returns>
        public static float Flip(float value, float over = 0.0f)
        {
            return -(value - over) + over;
        }

        /// <summary>
        /// Floors the specified value to the specified interval with the specified offset. For example,
        /// floor-ing 21 to the interval 10 with offset 3 returns 13.
        /// </summary>
        /// <param name="value">The specified value to round.</param>
        /// <param name="interval">The interval to round to.</param>
        /// <param name="offset">How much to add to the rounded value.</param>
        /// <returns></returns>
        public static float Floor(float value, float interval = 1, float offset = 0)
        {
            return Mathf.Floor((value - offset)/interval)*interval + offset;
        }

        /// <summary>
        /// Ceils the specified value to the specified interval with the specified offset. For example,
        /// ceil-ing 21 to the interval 10 with offset 3 returns 23.
        /// </summary>
        /// <param name="value">The specified value to round.</param>
        /// <param name="interval">The interval to round to.</param>
        /// <param name="offset">How much to add to the rounded value.</param>
        /// <returns></returns>
        public static float Ceil(float value, float interval = 1f, float offset = 0)
        {
            if (interval == 0f) interval = 1f;
            return Mathf.Ceil((value - offset)/interval)*interval + offset;
        }

        /// <summary>
        /// Rounds the specified value to the nearest interval with the specified offset. For example,
        /// rounding 21 to the interval 10 with offset 3 returns 23.
        /// </summary>
        /// <param name="value">The specified value to round.</param>
        /// <param name="interval">The interval to round to.</param>
        /// <param name="offset">How much to add to the rounded value.</param>
        /// <returns></returns>
        public static float Round(float value, float interval = 1.0f, float offset = 0.0f)
        {
            if (interval == 0f) interval = 1f;
            return Mathf.Round((value - offset)/interval)*interval + offset;
        }

        /// <summary>
        /// Performs a modulus that is positive as long as the divisor is positive.
        /// </summary>
        /// <param name="dividend">The dividend.</param>
        /// <param name="divisor">The divisor.</param>
        public static int Modp(int dividend, int divisor)
        {
            return (dividend%divisor + divisor)%divisor;
        }

        /// <summary>
        /// Performs a modulus that is positive as long as the divisor is positive.
        /// </summary>
        /// <param name="dividend">The dividend.</param>
        /// <param name="divisor">The divisor.</param>
        public static float Modp(float dividend, float divisor)
        {
            return (dividend%divisor + divisor)%divisor;
        }

        /// <summary>
        /// Returns the angle of the specified vector in radians.
        /// </summary>
        /// <param name="a">The vector.</param>
        public static float Angle(Vector2 a)
        {
            return Mathf.Atan2(a.y, a.x);
        }

        /// <summary>
        /// Returns a unit vector pointing in the specified direction.
        /// </summary>
        /// <param name="angle">The specified angle, in radians.</param>
        /// <returns></returns>
        public static Vector2 AngleToVector(float angle)
        {
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        /// <summary>
        /// Returns the positive vertical distance between a and b if a is higher than b or the negative
        /// vertical distance if the opposite is true.
        /// </summary>
        /// <param name="a">The point a.</param>
        /// <param name="b">The point b.</param>
        public static float Highest(Vector2 a, Vector2 b)
        {
            return Highest(a, b, HalfPi);
        }

        /// <summary>
        /// Returns the positive distance between a and b projected onto the axis in the specifed direction.
        /// If a is higher than b on that axis, a positive value is returned. Otherwise a negative value.
        /// </summary>
        /// <param name="a">The point a.</param>
        /// <param name="b">The point b.</param>
        /// <param name="direction">The direction, in radians, by which the farthest point in that
        /// direction is chosen. For example, if zero is specified, and point a is farther right
        /// than point b, a positive distance will be returned.</param>
        /// <returns>The positive distance between a and b if a is higher than b in the specified
        /// direction or the negative distance if the opposite is true.</returns>
        public static float Highest(Vector2 a, Vector2 b, float direction)
        {
            // Get easy angles out of the way
            if (Equalsf(direction, 0)) return a.x - b.x;
            if (Equalsf(direction, HalfPi)) return a.y - b.y;
            if (Equalsf(direction, Mathf.PI)) return b.x - a.x;
            if (Equalsf(direction, OneThreeFourthsPi)) return b.y - a.y;

            Vector2 diff = Project(a, direction) - Project(b, direction);
            return (Mathf.Abs(Angle(diff) - direction) < HalfPi) ? diff.magnitude : -diff.magnitude;
        }

        /// <summary>
        /// Determines if a is perpendicular to b.
        /// </summary>
        /// <returns><c>true</c> if a is perpendicular to b, otherwise <c>false</c>.</returns>
        /// <param name="a">The vector a.</param>
        /// <param name="b">The vector b.</param>
        public static bool IsPerp(Vector2 a, Vector2 b)
        {
            return Equalsf(0.0f, Vector2.Dot(a, b));
        }

        /// <summary>
        /// Determines if the line defined by the points a1 and a2 is perpendicular to the line defined by
        /// the points b1 and b2.
        /// </summary>
        /// <returns><c>true</c> if the line defined by the points a1 and a2 is perpendicular to the line defined by
        /// the points b1 and b2, otherwise <c>false</c>.</returns>
        /// <param name="a1">The point a1 that defines a line with a2.</param>
        /// <param name="a2">The point a2 that defines a line with a1.</param>
        /// <param name="b1">The point b1 that defines a line with b2.</param>
        /// <param name="b2">The point b2 that defines a line with b1.</param>
        public static bool IsPerp(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            return IsPerp(a2 - a1, b2 - b1);
        }

        /// <summary>
        /// Projects the point q onto a line at the origin at the specified angle.
        /// </summary>
        /// <param name="q">The point q.</param>
        /// <param name="angle">The angle of the line.</param>
        public static Vector2 Project(Vector2 q, float angle)
        {
            return Project(q, new Vector2(), angle);
        }

        /// <summary>
        /// Projects the point q onto a line which intersects the point p and continues in the specified angle.
        /// </summary>
        /// <param name="q">The point q.</param>
        /// <param name="p">The point p.</param>
        /// <param name="angle">The angle of the line.</param>
        public static Vector2 Project(Vector2 q, Vector2 p, float angle)
        {
            return Project(q, p, p + (new Vector2(Mathf.Cos(angle), Mathf.Sin(angle))));
        }

        /// <summary>
        /// Projects the point q onto a line defined by the points lineA and lineB.
        /// </summary>
        /// <param name="q">The point q.</param>
        /// <param name="lineA">The point lineA which defines a line with lineB.</param>
        /// <param name="lineB">The point lineB which defines a line with lineA.</param>
        public static Vector2 Project(Vector2 q, Vector2 lineA, Vector2 lineB)
        {
            Vector2 ab = lineB - lineA;
            return lineA + Vector2.Dot(q - lineA, ab)/Vector2.Dot(ab, ab)*ab;
        }

        /// <summary>
        /// Returns the scalar projection (aka scalar component) of the vector a onto the vector b.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float ScalarProjection(Vector2 a, Vector2 b)
        {
            return ScalarProjection(a, Angle(a) - Angle(b));
        }

        /// <summary>
        /// Returns the scalar projection of vector a (aka scalar component) onto a vector pointing theta radians
        /// clockwise from vector a.
        /// </summary>
        /// <param name="a">The vector a.</param>
        /// <param name="theta">The angle in radians of vector a relative to the vector to project onto.</param>
        /// <returns></returns>
        public static float ScalarProjection(Vector2 a, float theta)
        {
            return a.magnitude*Mathf.Cos(theta);
        }

        /// <summary>
        /// Returns the scalar projection of vector a (aka scalar component) onto a vector pointing theta radians.
        /// </summary>
        /// <param name="a">The vector a.</param>
        /// <param name="theta">The angle theta, in radians.</param>
        /// <returns></returns>
        public static float ScalarProjectionAbs(Vector2 a, float theta)
        {
            return a.magnitude*Mathf.Cos(Angle(a) - theta);
        }

        /// <summary>
        /// Rotates the point by the angle about (0, 0).
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="angle">The angle in radians.</param>
        public static Vector2 RotateBy(Vector2 point, float angle)
        {
            return RotateBy(point, angle, new Vector2(0.0f, 0.0f));
        }

        /// <summary>
        /// Rotates the point by the angle about the specified origin.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="angle">The angle in radians.</param>
        /// <param name="origin">The origin.</param>
        public static Vector2 RotateBy(Vector2 point, float angle, Vector2 origin)
        {
            float s = Mathf.Sin(angle);
            float c = Mathf.Cos(angle);

            Vector2 npoint = point - origin;

            return new Vector2(npoint.x*c - npoint.y*s + origin.x, npoint.x*s + npoint.y*c + origin.y);
        }

        /// <summary>
        /// Rotates the transform by the angle about (0, 0).
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <param name="angle">The angle in radians.</param>
        public static void RotateBy(Transform transform, float angle)
        {
            RotateBy(transform, angle, transform.position);
        }

        /// <summary>
        /// Rotates the transform by the angle about the specified origin.
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <param name="angle">The angle in radians.</param>
        /// <param name="origin">The origin.</param>
        public static void RotateBy(Transform transform, float angle, Vector2 origin)
        {
            transform.position = RotateBy(transform.position, angle, origin);
            transform.eulerAngles = new Vector3(0.0f, 0.0f, transform.eulerAngles.z + (angle*Mathf.Rad2Deg));
        }

        /// <summary>
        /// Rotates the point to the angle about (0, 0).
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="angle">The angle in radians.</param>
        public static Vector2 RotateTo(Vector2 point, float angle)
        {
            return RotateBy(point, angle - Angle(point));
        }

        /// <summary>
        /// Rotates the point to the angle about the specified origin.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="angle">The angle in radians.</param>
        /// <param name="origin">The origin.</param>
        public static Vector2 RotateTo(Vector2 point, float angle, Vector2 origin)
        {
            return RotateBy(point, angle - Mathf.Atan2(point.x - origin.x, point.y - origin.y), origin);
        }

        /// <summary>
        /// Rotates the transform to the angle about (0, 0).
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <param name="angle">The angle in radians.</param>
        public static void RotateTo(Transform transform, float angle)
        {
            RotateTo(transform, angle, transform.position);
        }

        /// <summary>
        /// Rotates the transform to the angle about the specified origin.
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <param name="angle">The angle in radians.</param>
        /// <param name="origin">The origin.</param>
        public static void RotateTo(Transform transform, float angle, Vector2 origin)
        {
            transform.position = RotateTo(transform.position, angle, origin);
            transform.eulerAngles = new Vector3(0.0f, 0.0f, angle*Mathf.Rad2Deg);
        }

        /// <summary>
        /// Returns the midpoint between two points.
        /// </summary>
        /// <param name="a">The point a.</param>
        /// <param name="b">The point b.</param>
        public static Vector2 Midpoint(Vector2 a, Vector2 b)
        {
            return (a + b)/2.0f;
        }

        /// <summary>
        /// Checks for approximate equality to zero.
        /// </summary>
        /// <param name="a">The float a.</param>
        public static bool Equalsf(float a)
        {
            return Equalsf(a, 0.0f);
        }

        /// <summary>
        /// Checks for approximate equality between two floats with AMath.Epsilon.
        /// </summary>
        /// <param name="a">The float a.</param>
        /// <param name="b">The float b.</param>
        public static bool Equalsf(float a, float b)
        {
            return Equalsf(a, b, Epsilon);
        }

        /// <summary>
        /// Checks for approximate equality between two floats with an epsilon.
        /// </summary>
        /// <param name="a">The float a.</param>
        /// <param name="b">The float b.</param>
        /// <param name="epsilon">The epsilon.</param>
        public static bool Equalsf(float a, float b, float epsilon)
        {
            return Math.Abs(a - b) < epsilon;
        }

        /// <summary>
        /// Returns the signed angle, in radians, between two vectors.
        /// 
        /// If the shortest arc from a to b is counterclockwise (increasing in angle),
        /// the sign is positive; otherwise the sign is negative.
        /// </summary>
        /// <returns>The angle.</returns>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        public static float ShortestArc(Vector2 a, Vector2 b)
        {
            return ShortestArc(Angle(a), Angle(b));
        }

        /// <summary>
        /// Returns the signed angle difference between two rays pointing in the specified direction, in radians.
        /// 
        /// If the shortest arc from a to b is counterclockwise (increasing in angle),
        /// the sign is positive; otherwise the sign is negative.
        /// </summary>
        /// <returns>The angle in radians between two rays pointing in the specified direction.</returns>
        /// <param name="a">The angle of the first ray, in radians.</param>
        /// <param name="b">The angle of the second ray, in radians.</param>
        public static float ShortestArc(float a, float b)
        {
            return Modp(b - a + Mathf.PI, DoublePi) - Mathf.PI;
        }

        /// <summary>
        /// Returns the signed angle, in degrees, between two vectors.
        /// 
        /// If the shortest arc from a to b is counterclockwise (increasing in angle),
        /// the sign is positive; otherwise the sign is negative.
        /// </summary>
        /// <returns>The angle.</returns>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        public static float ShortestArc_d(Vector2 a, Vector2 b)
        {
            return ShortestArc_d(Angle(a)*Mathf.Rad2Deg, Angle(b)*Mathf.Rad2Deg);
        }

        /// <summary>
        /// Returns the signed angle difference between two rays pointing in the specified direction, in degrees.
        /// 
        /// If the shortest arc from a to b is counterclockwise (increasing in angle),
        /// the sign is positive; otherwise the sign is negative.
        /// </summary>
        /// <returns>The angle in degrees between two rays pointing in the specified direction.</returns>
        /// <param name="a">The angle of the first ray, in degrees.</param>
        /// <param name="b">The angle of the second ray, in degrees.</param>
        public static float ShortestArc_d(float a, float b)
        {
            return Modp(b - a + 180.0f, 360.0f) - 180.0f;
        }

        /// <summary>
        /// Returns the positive length of the arc from a traveling counter-clockwise to b, in radians.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float PositiveArc(float a, float b)
        {
            var diff = PositiveAngle(b) - PositiveAngle(a);
            if (diff < 0.0f) return diff + DoublePi;
            return diff;
        }

        /// <summary>
        /// Returns the length of the arc from a traveling counter-clockwise to b, in degrees.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float PositiveArc_d(float a, float b)
        {
            var diff = PositiveAngle_d(b) - PositiveAngle_d(a);
            if (diff < 0.0f) return diff + 360.0f;
            return diff;
        }

        /// <summary>
        /// Returns the specified angle in the range 0 to 2pi.
        /// </summary>
        /// <param name="angle">The specified angle in radians.</param>
        /// <returns></returns>
        public static float PositiveAngle(float angle)
        {
            return Modp(angle, DoublePi);
        }

        /// <summary>
        /// Returns the specified angle in the range 0 to 360.
        /// </summary>
        /// <param name="angle">The specified angle in degrees.</param>
        /// <returns></returns>
        public static float PositiveAngle_d(float angle)
        {
            return Modp(angle, 360.0f);
        }

        /// <summary>
        /// Returns whether the specified angle is inside the arc formed between the angle a
        /// traveling counter-clockwise to the angle b.
        /// </summary>
        /// <param name="angle">The specified angle in radians.</param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool AngleInRange(float angle, float a, float b)
        {
            return PositiveArc(a, angle) <= PositiveArc(a, b);
        }

        /// <summary>
        /// Returns whether the specified angle is inside the arc formed between the angle a
        /// traveling counter-clockwise (upwards) to b.
        /// </summary>
        /// <param name="angle">The specified angle in degrees.</param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool AngleInRange_d(float angle, float a, float b)
        {
            return PositiveArc_d(a, angle) <= PositiveArc_d(a, b);
        }
    }
}