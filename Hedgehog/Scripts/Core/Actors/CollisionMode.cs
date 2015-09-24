namespace Hedgehog.Core.Actors
{
    public enum CollisionMode
    {
        /// <summary>
        /// The fastest collision mode and often the easiest to deal with,
        /// but since the maximum layer count is 32, this often becomes impractical.
        /// </summary>
        Layers,

        /// <summary>
        /// The recommended collision mode, slower than Layers and faster than Names.
        /// 
        /// More practical because one can have an any number of tags, but still
        /// somewhat limited because tags must be defined in projects.
        /// 
        /// If you plan to keep your scenes in a single Unity project, use Tags!
        /// </summary>
        Tags,

        /// <summary>
        /// The slowest collision mode, a bit less user-friendly, but also the most powerful.
        /// 
        /// This mode assigns layers to any game object whose name, parent's name, parent's
        /// parent's name - and so on - matches the name you specify.
        /// 
        /// You can imagine the performance overhead this incurs!
        /// </summary>
        Names,
    }
}
