using UnityEngine;

/// <summary>
/// Represents the four sides based on the orientation of a character standing on the mask.
/// This means that RIGHT is the mask's left side and that LEFT is the mask's right side.
/// </summary>
public enum WallMode : int {
	Floor, Right, Ceiling, Left, None,
};

/// <summary>
/// Contains extension methods for WallMode.
/// </summary>
public static class WallModeExtensions
{
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
	/// Returns the normal of a flat wall in the specified wall mode.
	/// </summary>
	/// <param name="wallMode">The wall mode.</param>
	public static float Normal(this WallMode wallMode)
	{
		switch(wallMode)
		{
		case WallMode.Floor : 
			return Mathf.PI / 2;
		
		case WallMode.Right : 
			return Mathf.PI;

		case WallMode.Ceiling : 
			return -Mathf.PI / 2;

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
}