using System;

namespace Hedgehog.Core.Moves
{
    // Attributes that, when put onto the fields of moves, say in which section they should be drawn when the move
    // is viewed in the move inspector.
    
    /// <summary>
    /// Draws the move's field in the Control foldout. All fields are drawn in the Control foldout by default.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ControlFoldoutAttribute : Attribute { }

    /// <summary>
    /// Draws the move's field in the Physics foldout.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class PhysicsFoldoutAttribute : Attribute { }

    /// <summary>
    /// Draws the move's field in the Events foldout.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class EventsFoldoutAttribute : Attribute { }

    /// <summary>
    /// Draws the move's field in the Animation foldout.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class AnimationFoldoutAttribute : Attribute { }

    /// <summary>
    /// Draws the move's field in the Sound foldout.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SoundFoldoutAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public class DebugFoldoutAttribute : Attribute { }
}
