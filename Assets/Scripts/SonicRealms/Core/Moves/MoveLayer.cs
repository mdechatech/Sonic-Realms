namespace SonicRealms.Core.Moves
{
    /// <summary>
    /// Defines the layers in the MoveManager FSM. One layer may only have one active move at one time.
    /// Moves on the None layer have no such limitation.
    /// 
    /// Besides None, the layers have no special significance to the MoveManager, so adding more is fine.
    /// </summary>
    public enum MoveLayer
    {
        /// <summary>
        /// The default layer. There is no limit to how many moves can be active on this layer.
        /// </summary>
        None,

        /// <summary>
        /// Controls the player in some way - these tend to stay active for awhile.
        /// For example: GroundControl and AirControl.
        /// </summary>
        Control = 1,

        /// <summary>
        /// Thanks to the ubiquity of rolling, it's on its own layer.
        /// </summary>
        Roll = 2,

        /// <summary>
        /// The main action being performed. For example: duck, spindash, instashield
        /// </summary>
        Action = 3,
    }
}
