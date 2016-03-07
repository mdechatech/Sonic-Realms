using UnityEngine;

namespace SonicRealms.Core.Utils
{
    /// <summary>
    /// Tags and layers reserved by the library.
    /// </summary>
    public static class CollisionLayers
    {
        public const int Layer7 = 24;
        public const int Layer6 = 25;
        public const int Layer5 = 26;
        public const int Layer4 = 27;
        public const int Layer3 = 28;
        public const int Layer2 = 29;
        public const int Layer1 = 30;
        public const int AlwaysCollide = 31;

        public const string Layer7Name = "Terrain Layer 7";
        public const string Layer6Name = "Terrain Layer 6";
        public const string Layer5Name = "Terrain Layer 5";
        public const string Layer4Name = "Terrain Layer 4";
        public const string Layer3Name = "Terrain Layer 3";
        public const string Layer2Name = "Terrain Layer 2";
        public const string Layer1Name = "Terrain Layer 1";
        public const string AlwaysCollideName = "Terrain";

        public const int Layer7Mask = 1 << Layer7;
        public const int Layer6Mask = 1 << Layer6;
        public const int Layer5Mask = 1 << Layer5;
        public const int Layer4Mask = 1 << Layer4;
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
        public const int LayersMask =  Layer7Mask | Layer6Mask | Layer5Mask | Layer4Mask
            | Layer3Mask | Layer2Mask | Layer1Mask;

        /// <summary>
        /// Layer mask of all collision layers.
        /// </summary>
        public const int AllMask = Layer7Mask | Layer6Mask | Layer5Mask | Layer4Mask | 
            Layer3Mask | Layer2Mask | Layer1Mask | AlwaysCollideMask;

        /// <summary>
        /// Returns the first found collision layer in the mask. The number is normalized such that 
        /// the first collision layer is 1, the second is 2, etc.
        /// </summary>
        /// <param name="mask"></param>
        /// <returns></returns>
        public static int GetLayerNumber(LayerMask mask)
        {
            for (int i = Layer1; i >= Layer7; --i)
            {
                if (((mask >> i) & 1) == 1) return -(i - Layer1) + 1;
            }

            return -1;
        }
    }
}
