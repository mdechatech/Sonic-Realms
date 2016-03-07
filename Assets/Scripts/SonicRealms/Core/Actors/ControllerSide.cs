using System;

namespace SonicRealms.Core.Actors
{
    /// <summary>
    /// Represents the four sides of a controller. Each value is a flag, similar to
    /// Unity's layer masks, so you MUST check for equality with bitwise AND.
    /// </summary>
    [Flags]
    public enum ControllerSide
    {
        None    = 0,

        Right   = 1 << 0,
        Top     = 1 << 1,
        Left    = 1 << 2,
        Bottom  = 1 << 3,

        All     = Right | Top | Left | Bottom,
    }
}
