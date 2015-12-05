namespace Hedgehog.Core.Moves
{
    public enum MoveGroup
    {
        All = 0,

        Air = 1000,
        Ground,

        DoubleJump,
        Control,
        Shield,

        Harmful,
        Death = 0xDEAD,
    }
}
