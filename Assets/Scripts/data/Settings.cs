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

    /// <summary>
    /// The name of the layer which contains terrain colliders. Terrain is separated
    /// into a layer x based on their name, for example "Terrain x" where x is the
    /// layer number.
    /// </summary>
    public static string LayerTerrain = "Terrain";

    /// <summary>
    /// The name of the tag which contains moving platforms.
    /// </summary>
    public static string TagMovingPlatform = "Moving Platform";
}