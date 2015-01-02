using UnityEngine;

/// <summary>
/// A list of values for key bindings and other preferences.
/// </summary>
public class Settings
{
	public static KeyCode LeftKey = KeyCode.A;
	public static KeyCode RightKey = KeyCode.D;
	public static KeyCode JumpKey = KeyCode.W;

    /// <summary>
    /// The normal desired interval between fixed updates.
    /// </summary>
    public static float DefaultFixedDeltaTime = 0.02f;
}