using UnityEngine;

/// <summary>
/// Making math-related helper functions as we go!
/// </summary>
public static class AMath
{
	/// <summary>
	/// This epsilon is good enough for me!
	/// </summary>
	public const float Epsilon = 0.0001f;

    /// <summary>
    /// Half of pi.
    /// </summary>
    public const float HALF_PI = Mathf.PI / 2.0f;

	/// <summary>
	/// Performs a modulus that is positive as long as the divisor is positive.
	/// </summary>
	/// <param name="dividend">The dividend.</param>
	/// <param name="divisor">The divisor.</param>
	public static int Modp(int dividend, int divisor)
	{
		return ((dividend % divisor) + divisor) % divisor;
	}

	/// <summary>
	/// Performs a modulus that is positive as long as the divisor is positive.
	/// </summary>
	/// <param name="dividend">The dividend.</param>
	/// <param name="divisor">The divisor.</param>
	public static float Modp(float dividend, float divisor)
	{
		return ((dividend % divisor) + divisor) % divisor;
    }

	/// <summary>
	/// Returns the angle of the specified vector in radians.
	/// </summary>
	/// <param name="a">The vector.</param>
	public static float Angle(this Vector2 a)
	{
		return Mathf.Atan2 (a.y, a.x);
	}

	/// <summary>
	/// Returns the positive vertical distance between a and b if a is higher than b or the negative
	/// vertical distance if the opposite is true.
	/// </summary>
	/// <param name="a">The point a.</param>
	/// <param name="b">The point b.</param>
	public static float Highest(Vector2 a, Vector2 b)
	{
		return Highest (a, b, Mathf.PI / 2);
	}

	/// <summary>
	/// Returns the positive distance between a and b projected onto the axis in the speicifed direction
	/// if a is higher than b on that axis or the negative distance if the opposite is true.
	/// </summary>
	/// <param name="a">The point a.</param>
	/// <param name="b">The point b.</param>
	/// <param name="angle">The positive distance between a and b if a is higher than b in the specified
	/// direction or the negative distance if the opposite is true.</param>
	public static float Highest(Vector2 a, Vector2 b, float direction)
	{
		Vector2 diff = Projection (a, direction) - Projection (b, direction);
		return (Mathf.Abs(diff.Angle() - direction) < 1.57f) ? diff.magnitude : -diff.magnitude;
	}

	/// <summary>
	/// Determines if a is perpendicular to b.
	/// </summary>
	/// <returns><c>true</c> if a is perpendicular to b, otherwise <c>false</c>.</returns>
	/// <param name="a">The vector a.</param>
	/// <param name="b">The vector b.</param>
	public static bool IsPerp(Vector2 a, Vector2 b)
	{
		return Equalsf (0.0f, Vector2.Dot(a, b));
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
		return IsPerp (a2 - a1, b2 - b1);
	}

	/// <summary>
	/// Projects the point q onto a line at the origin at the specified angle.
	/// </summary>
	/// <param name="q">The point q.</param>
	/// <param name="angle">The angle of the line.</param>
	public static Vector2 Projection(Vector2 q, float angle)
	{
		return Projection (q, new Vector2 (), angle);
	}

	/// <summary>
	/// Projects the point q onto a line which intersects the point p and continues in the specified angle.
	/// </summary>
	/// <param name="q">The point q.</param>
	/// <param name="p">The point p.</param>
	/// <param name="angle">The angle of the line.</param>
	public static Vector2 Projection(Vector2 q, Vector2 p, float angle)
	{
		return Projection (q, p, p + (new Vector2 (Mathf.Cos (angle), Mathf.Sin (angle))));
	}

	/// <summary>
	/// Projects the point q onto a line defined by the points lineA and lineB.
	/// </summary>
	/// <param name="q">The point q.</param>
	/// <param name="lineA">The point lineA which defines a line with lineB.</param>
	/// <param name="lineB">The point lineB which defines a line with lineA</para>.</param>
	public static Vector2 Projection(Vector2 q, Vector2 lineA, Vector2 lineB)
	{
		Vector2 ab = lineB - lineA;
		return lineA + ((Vector2.Dot (q - lineA, ab) / Vector2.Dot (ab, ab)) * ab);
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

        return new Vector2(npoint.x * c - npoint.y * s + origin.x, npoint.x * s + npoint.y * c + origin.y);
    }

    /// <summary>
    /// Rotates the transform by the angle about (0, 0).
    /// </summary>
    /// <param name="transform">The transform.</param>
    /// <param name="angle">The angle in radians.</param>
    public static void RotateBy(this Transform transform, float angle)
    {
        RotateBy(transform, angle, transform.position);
    }

    /// <summary>
    /// Rotates the transform by the angle about the specified origin.
    /// </summary>
    /// <param name="transform">The transform.</param>
    /// <param name="angle">The angle in radians.</param>
    /// <param name="origin">The origin.</param>
    public static void RotateBy(this Transform transform, float angle, Vector2 origin)
    {
        transform.position = RotateBy(transform.position, angle, origin);
        transform.eulerAngles = new Vector3(0.0f, 0.0f, transform.eulerAngles.z + (angle * Mathf.Rad2Deg));
    }

    /// <summary>
    /// Rotates the point to the angle about (0, 0).
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="angle">The angle in radians.</param>
    public static Vector2 RotateTo(Vector2 point, float angle)
    {
        return RotateBy(point, angle - point.Angle());
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
    public static void RotateTo(this Transform transform, float angle)
    {
        RotateTo(transform, angle, transform.position);
    }

    /// <summary>
    /// Rotates the transform to the angle about the specified origin.
    /// </summary>
    /// <param name="transform">The transform.</param>
    /// <param name="angle">The angle in radians.</param>
    /// <param name="origin">The origin.</param>
    public static void RotateTo(this Transform transform, float angle, Vector2 origin)
    {
        transform.position = RotateTo(transform.position, angle, origin);
        transform.eulerAngles = new Vector3(0.0f, 0.0f, angle * Mathf.Rad2Deg);
    }

    /// <summary>
    /// Returns the midpoint between two points.
    /// </summary>
    /// <param name="a">The point a.</param>
    /// <param name="b">The point b.</param>
    public static Vector2 Midpoint(Vector2 a, Vector2 b)
    {
        return (a + b) / 2.0f;
    }

	/// <summary>
	/// Checks for approximate equality between two floats with an epsilon value.
	/// </summary>
	/// <param name="a">The float a.</param>
	/// <param name="b">The float b.</param>
	public static bool Equalsf(float a, float b)
	{
		return (a >= b - Epsilon && a <= b + Epsilon);
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
	public static float AngleDiffr(Vector2 a, Vector2 b)
	{
		return AngleDiffr (a.Angle (), b.Angle ());
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
	public static float AngleDiffr(float a, float b)
	{
		return AMath.Modp(b - a + Mathf.PI, Mathf.PI * 2.0f) - Mathf.PI;
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
    public static float AngleDiffd(Vector2 a, Vector2 b)
    {
        return AngleDiffd(a.Angle() * Mathf.Rad2Deg, b.Angle() * Mathf.Rad2Deg);
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
    public static float AngleDiffd(float a, float b)
    {
        return AMath.Modp(b - a + 180.0f, 360.0f) - 180.0f;
    }

	/// <summary>
	/// Inclusive floor. If the value is a whole number, the value is reduced
	/// by one. Otherwise the floor of the value is returned.
	/// </summary>
	/// <returns>The inclusive floor of the value.</returns>
	/// <param name="value">The value.</param>
	public static float IncFloor(float value)
	{
		return (Modp (value, 1.0f) == 0.0f) ? value - 1 : Mathf.Floor(value);
	}

	/// <summary>
	/// Inclusive ceiling. If the value is a whole number, the value is increased
	/// by one. Otherwise the ceiling of the value is returned.
	/// </summary>
	/// <returns>The inclusive ceiling of the value.</returns>
	/// <param name="value">The value.</param>
	public static float IncCeil(float value)
	{
		return (Modp (value, 1.0f) == 0.0f) ? value + 1 : Mathf.Ceil(value);
	}

	/// <summary>
	/// Inverse floor.
	/// </summary>
	/// <returns>The inverse floor of the value.</returns>
	/// <param name="value">The value.</param>
	public static float InvFloor(float value)
	{
		return (Modp (value, 1.0f) == 0.0f) ? value - 1 : value;
	}

	/// <summary>
	/// Inverse ceiling. Increases the value by one only if it is a whole number.
	/// </summary>
	/// <returns>The inverse ceiling of the value.</returns>
	/// <param name="value">The value.</param>
	public static float InvCeil(float value)
	{
		return (Modp (value, 1.0f) == 0.0f) ? value + 1 : value;
	}
}