using UnityEngine;
using UnityEngine.Analytics;

namespace Hedgehog.Terrain
{
    /// <summary>
    /// Can be added to bits of terrain to give them special properties.
    /// </summary>
    public class TerrainProperties : MonoBehaviour
    {
        public const TerrainSide LedgeSolidMask = TerrainSide.Bottom;
        #region Inspector Fields
        /// <summary>
        /// How far down these properties apply to the object's children. For example
        /// if this is set to 1, it only applies to immediate children. If 2, children
        /// and grandchildren.
        /// </summary>
        [SerializeField]
        public int AppliesToChildrenLevel = 0;

        /// <summary>
        /// If 1, this has no effect. If less than 1, the platform will become slippery,
        /// like ice. If greater than 1, the player will find it harder to move around
        /// and come to a stop more quickly.
        /// </summary>
        [SerializeField]
        public float Friction = 1.0f;

        /// <summary>
        /// If true, the player will move with any movements in the terrain.
        /// </summary>
        [SerializeField]
        public bool MovingPlatform;
        
        /// <summary>
        /// Whether to ignore collisions with the player's left side.
        /// </summary>
        [SerializeField] public bool IgnoreLeft;
        
        /// <summary>
        /// Whether to ignore collisions with the player's right side.
        /// </summary>
        [SerializeField] public bool IgnoreRight;

        /// <summary>
        /// Whether to ignore collisions with the player's top side. Perfect for ledges!
        /// </summary>
        [SerializeField] public bool IgnoreTop;

        /// <summary>
        /// Whether to ignore collisions with the player's bottom side.
        /// </summary>
        [SerializeField] public bool IgnoreBottom;
        #endregion
        #region Side Masks
        /// <summary>
        /// A mask representation of the solid sides of the terrain.
        /// </summary>
        public TerrainSide SolidSides
        {
            get
            {
                var value = TerrainSide.None;
                
                if(!IgnoreBottom) value |= TerrainSide.Bottom;
                if(!IgnoreLeft) value |= TerrainSide.Left;
                if(!IgnoreTop) value |= TerrainSide.Top;
                if(!IgnoreRight) value |= TerrainSide.Right;

                return value;
            }

            set
            {
                IgnoreBottom = (value & TerrainSide.Bottom) == 0;
                IgnoreLeft = (value & TerrainSide.Left) == 0;
                IgnoreTop = (value & TerrainSide.Top) == 0;
                IgnoreRight = (value & TerrainSide.Right) == 0;
            }
        }

        /// <summary>
        /// A mask representation of the sides which the terrain ignores.
        /// </summary>
        public TerrainSide IgnoreSides
        {
            get
            {
                var value = TerrainSide.None;

                if (IgnoreBottom) value |= TerrainSide.Bottom;
                if (IgnoreLeft) value |= TerrainSide.Left;
                if (IgnoreTop) value |= TerrainSide.Top;
                if (IgnoreRight) value |= TerrainSide.Right;

                return value;
            }

            set
            {
                IgnoreBottom = (value & TerrainSide.Bottom) > 0;
                IgnoreLeft = (value & TerrainSide.Left) > 0;
                IgnoreTop = (value & TerrainSide.Top) > 0;
                IgnoreRight = (value & TerrainSide.Right) > 0;
            }
        }
        #endregion
        #region Side Mask Utilities
        /// <summary>
        /// Whether the terrain is a ledge. A ledge only checks for collision with
        /// the player's bottom side and therefore has only a floor.
        /// </summary>
        public bool Ledge
        {
            get { return SolidSides == LedgeSolidMask; }
            set { SolidSides = value ? LedgeSolidMask : TerrainSide.All; }
        }

        /// <summary>
        /// Returns whether the terrain collides with the specified side.
        /// </summary>
        /// <param name="side">The specified side.</param>
        /// <returns></returns>
        public bool CollidesWithSide(TerrainSide side)
        {
            return (SolidSides & side) > 0;
        }

        /// <summary>
        /// Returns whether the terrain ignores the specified side.
        /// </summary>
        /// <param name="side">The specified side.</param>
        /// <returns></returns>
        public bool IgnoresSide(TerrainSide side)
        {
            return (IgnoreSides & side) > 0;
        }
        #endregion
        /// <summary>
        /// Returns whether these properties apply to the specified transform.
        /// </summary>
        /// <param name="terrain">The specified transform.</param>
        /// <returns></returns>
        public bool AppliesTo(Transform terrain)
        {
            var check = terrain;
            int childrenLevel = AppliesToChildrenLevel;

            while (check != null && childrenLevel >= 0)
            {
                if (check == this) return true;

                childrenLevel--;
                check = check.parent;
            }

            return false;
        }
    }
}
