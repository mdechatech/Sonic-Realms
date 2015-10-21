using System.Collections.Generic;
using System.Linq;
using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using UnityEngine;

namespace Hedgehog.Level.Areas
{
    /// <summary>
    /// When the player collides, this switches the player's collision mask/tag/name.
    /// </summary>
    [RequireComponent(typeof(AreaTrigger))]
    public class PathSwitcher : ReactiveArea
    {
        /// <summary>
        /// Whether the player must be grounded to switch layers.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether the player must be on the ground for the path switcher to activate.")]
        public bool MustBeGrounded;

        /// <summary>
        /// The path switcher's collision mode.
        /// </summary>
        [SerializeField]
        public CollisionMode CollisionMode;
        #region If CollisionMode.Layers
        /// <summary>
        /// The path switcher activates if it is in CollisionMode.Layers and
        /// the controller's terrain mask has ANY of the layers in this layer mask.
        /// </summary>
        [SerializeField]
        [Tooltip("Activates if the controller's terrain mask has ANY of the layers in this layer mask.")]
        public LayerMask IfTerrainMaskHas;

        /// <summary>
        /// These layers are added to the player's terrain mask.
        /// </summary>
        [SerializeField]
        [Tooltip("These layers are added to the player's terrain mask.")]
        public LayerMask AddLayers;

        /// <summary>
        /// These layers are removed from the player's terrain mask.
        /// </summary>
        [SerializeField]
        [Tooltip("These layers are removed from the player's terrain mask.")]
        public LayerMask RemoveLayers;
        #endregion
        #region If CollisionMode.Tags
        /// <summary>
        /// The path switcher activates if it is in CollisionMode.Tags and
        /// the controller's terrain tag list has ANY of the tags in this tag list.
        /// </summary>
        [SerializeField]
        public List<string> IfTerrainTagsHas;

        /// <summary>
        /// These tags are added to the player's terrain tag list.
        /// </summary>
        [SerializeField]
        public List<string> AddTags;

        /// <summary>
        /// These tags are removed from the player's terrain tag list.
        /// </summary>
        [SerializeField]
        public List<string> RemoveTags;
        #endregion
        #region If CollisionMode.Names
        /// <summary>
        /// The path switcher activates if it is in CollisionMode.Names and
        /// the controller's terrain names list has ANY of the name in this name list.
        /// </summary>
        [SerializeField]
        public List<string> IfTerrainNamesHas;

        /// <summary>
        /// These names are added to the player's terrain name list.
        /// </summary>
        [SerializeField]
        public List<string> AddNames;
        /// <summary>
        /// These names are removed from the player's terrain name list.
        /// </summary>
        [SerializeField]
        public List<string> RemoveNames;

        #endregion

        // The controller must be grounded if MustBeGrounded is true.
        public override bool IsInsideArea(HedgehogController controller)
        {
            return base.IsInsideArea(controller) && AppliesTo(controller) && (!MustBeGrounded || controller.Grounded);
        }

        public override void OnAreaEnter(HedgehogController controller)
        {
            Apply(controller);
        }

        public void Apply(HedgehogController controller)
        {
            switch (CollisionMode)
            {
                case CollisionMode.Layers:
                    controller.TerrainMask |= AddLayers;
                    controller.TerrainMask &= ~RemoveLayers;
                    break;

                case CollisionMode.Tags:
                    foreach (var tag in AddTags) controller.TerrainTags.Add(tag);
                    foreach (var tag in RemoveTags) controller.TerrainTags.Remove(tag);
                    break;

                case CollisionMode.Names:
                    foreach (var name in AddNames) controller.TerrainNames.Add(name);
                    foreach (var name in RemoveNames) controller.TerrainNames.Remove(name);
                    break;
            }
        }

        public bool AppliesTo(HedgehogController player)
        {
            if (MustBeGrounded && !player.Grounded) return false;

            switch (CollisionMode)
            {
                case CollisionMode.Layers:
                    return (player.TerrainMask | IfTerrainMaskHas) > 0;

                case CollisionMode.Tags:
                    return IfTerrainTagsHas.Any(tag => player.TerrainTags.Contains(tag));

                case CollisionMode.Names:
                    return IfTerrainNamesHas.Any(name => player.TerrainNames.Contains(name));

                default:
                    return true;
            }
        }
    }
}
