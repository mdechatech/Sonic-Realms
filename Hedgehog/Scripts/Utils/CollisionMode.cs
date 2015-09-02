namespace Hedgehog.Utils
{
    public enum CollisionMode
    {
        /// <summary>
        /// The fastest collision mode, but since the maximum layer count is 32,
        /// this often becomes impractical.
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
        /// This mode assigns layers to any game object whose name ends in whatever you specify.
        /// 
        /// For example, if your actor's terrain mask is "Terrain",
        /// it will collide with any game object whose name ends in "Terrain".
        /// 
        /// This mode is used in example scenes because, unlike Tags and Layers, it is not
        /// dependent on project settings.
        /// 
        /// If you want to be able to use your scenes in different projects, use Names!
        /// </summary>
        Names,
    }
}
