using System;

namespace SonicRealms.Core.Moves
{
    // A bunch of foldout attributes specific to moves to ensure that they always appear in the same order

    public class InputFoldoutAttribute : Attribute { }
    public class PhysicsFoldoutAttribute : Attribute { }
    public class EventsFoldoutAttribute : Attribute { }
    public class AnimationFoldoutAttribute : Attribute { }
    public class SoundFoldoutAttribute : Attribute { }
    public class DebugFoldoutAttribute : Attribute { }
}
