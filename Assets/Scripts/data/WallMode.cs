using UnityEngine;

/// <summary>
/// Represents the four sides based on the orientation of a character standing on the mask.
/// This means that RIGHT is the mask's left side and that LEFT is the mask's right side.
/// </summary>
public enum WallMode : int {
	Floor, Right, Ceiling, Left, None,
};

/// <summary>
/// Contains extension and utility methods for WallMode.
/// </summary>
public static class WallModeUtils
{

    /// <summary>
    /// Returns the wall mode of a surface with the specified angle.
    /// </summary>
    /// <returns>The wall mode.</returns>
    /// <param name="angleRadians">The surface angle in radians.</param>
    public static WallMode FromSurfaceAngle(float angleRadians)
    {
        float angle = AMath.Modp(angleRadians, AMath.DOUBLE_PI);

        if (angle <= Mathf.PI * 0.25f || angle > Mathf.PI * 1.75f)
            return WallMode.Floor;
        else if (angle > Mathf.PI * 0.25f && angle <= Mathf.PI * 0.75f)
            return WallMode.Right;
        else if (angle > Mathf.PI * 0.75f && angle <= Mathf.PI * 1.25f)
            return WallMode.Ceiling;
        else
            return WallMode.Left;
    }

	/// <summary>
	/// Returns a unit vector which represents the direction in which a wall mode points.
	/// </summary>
	/// <returns>The vector which represents the direction in which a wall mode points.</returns>
	/// <param name="wallMode">The wall mode.</param>
	public static Vector2 UnitVector(this WallMode wallMode)
	{
		switch (wallMode)
		{
    		case WallMode.Floor : 
    			return new Vector2(0.0f, -1.0f);
    			
    		case WallMode.Ceiling : 
    			return new Vector2(0.0f, 1.0f);
    			
    		case WallMode.Left :
    			return new Vector2(-1.0f, 0.0f);
    			
    		case WallMode.Right : 
    			return new Vector2(1.0f, 0.0f);
    			
    		default:
    			return default(Vector2);
		}
	}

	/// <summary>
	/// Returns the normal angle in radians (-pi to pi) of a flat wall in the specified wall mode.
	/// </summary>
	/// <param name="wallMode">The wall mode.</param>
	public static float Normal(this WallMode wallMode)
	{
		switch(wallMode)
		{
    		case WallMode.Floor : 
    			return AMath.HALF_PI;
    		
    		case WallMode.Right : 
    			return Mathf.PI;

    		case WallMode.Ceiling : 
    			return -AMath.HALF_PI;

    		case WallMode.Left : 
    			return 0.0f;

    		default:
    			return default(float);
		}
	}

	/// <summary>
	/// Returns the wall mode which points in the direction opposite to this one.
	/// </summary>
	/// <param name="wallMode">The wall mode.</param>
	public static WallMode Opposite(this WallMode wallMode)
	{
		switch(wallMode)
		{
    		case WallMode.Floor : 
    			return WallMode.Ceiling;
    			
    		case WallMode.Right : 
    			return WallMode.Left;
    			
    		case WallMode.Ceiling : 
    			return WallMode.Floor;
    			
    		case WallMode.Left : 
    			return WallMode.Right;
    			
    		default:
    			return WallMode.None;
		}
	}

    /// <summary>
    /// Returns the wall mode adjacent to this traveling clockwise.
    /// </summary>
    /// <returns>The adjacent wall mode.</returns>
    /// <param name="wallMode">The given wall mode.</param>
    public static WallMode AdjacentCW(this WallMode wallMode)
    {
        switch(wallMode)
        {
            case WallMode.Floor : 
                return WallMode.Left;
                
            case WallMode.Right : 
                return WallMode.Floor;
                
            case WallMode.Ceiling : 
                return WallMode.Right;
                
            case WallMode.Left : 
                return WallMode.Ceiling;
                
            default:
                return WallMode.None;
        }
    }

    /// <summary>
    /// Returns the wall mode adjacent to this traveling counter-clockwise.
    /// </summary>
    /// <returns>The adjacent wall mode.</returns>
    /// <param name="wallMode">The given wall mode.</param>
    public static WallMode AdjacentCCW(this WallMode wallMode)
    {
        switch(wallMode)
        {
            case WallMode.Floor : 
                return WallMode.Right;
                
            case WallMode.Right : 
                return WallMode.Ceiling;
                
            case WallMode.Ceiling : 
                return WallMode.Left;
                
            case WallMode.Left : 
                return WallMode.Floor;
                
            default:
                return WallMode.None;
        }
    }
}