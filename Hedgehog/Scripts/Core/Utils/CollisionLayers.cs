using UnityEngine;

namespace Hedgehog.Core.Utils
{
    /// <summary>
    /// Tags and layers reserved by the library. These are automatically added in the HedgehogInit script,
    /// and they are required for many of the prefabs it comes with.
    /// </summary>
    public class CollisionLayers
    {
        public const int Layer3 = 28;
        public const int Layer2 = 29;
        public const int Layer1 = 30;
        public const int AlwaysCollide = 31;

        public const string Layer3Name = "Collision Layer 3";
        public const string Layer2Name = "Collision Layer 2";
        public const string Layer1Name = "Collision Layer 1";
        public const string AlwaysCollideName = "Always Collide";

        public const int Layer3Mask = 1 << Layer3;
        public const int Layer2Mask = 1 << Layer2;
        public const int Layer1Mask = 1 << Layer1;
        public const int AlwaysCollideMask = 1 << AlwaysCollide;

        /// <summary>
        /// Default layer mask for player collision. Collides with terrain in the "Always Collide" layer and
        /// the the first collision layer.
        /// </summary>
        public const int DefaultMask = Layer1Mask | AlwaysCollideMask;

        /// <summary>
        /// Layer mask of all collision layers except Always Collide.
        /// </summary>
        public const int LayersMask = Layer3Mask | Layer2Mask | Layer1Mask;

        /// <summary>
        /// Layer mask of all collision layers.
        /// </summary>
        public const int AllMask = Layer3Mask | Layer2Mask | Layer1Mask | AlwaysCollideMask;
    }
}
