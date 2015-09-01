using UnityEngine;
using System.Collections;

/// <summary>
/// Must have a collider2D attached. When the player collides (has the tag "Player"),
/// this switches the player's layer from LayerFrom to LayerTo.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PathSwitcher : MonoBehaviour
{
    /// <summary>
    /// The path switcher activates if the controller's terrain mask has ANY
    /// of the layers in this layer mask.
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

    /// <summary>
    /// Whether the player must be grounded to switch layers.
    /// </summary>
    [SerializeField]
    [Tooltip("Whether the player must be on the ground for the path switcher to activate.")]
    public bool MustBeGrounded;

    public void OnTriggerEnter2D(Collider2D collider)
    {
        HedgehogController player = collider.gameObject.GetComponent<HedgehogController>();
        if (player == null) return;

        if ((player.TerrainMask | IfTerrainMaskHas) > 0)
        {
            if (!MustBeGrounded || (MustBeGrounded && player.Grounded))
            {
                Apply(player);
            }
        }
    }

    public void OnTriggerStay2D(Collider2D collider)
    {
        if (!MustBeGrounded) return;

        HedgehogController player = collider.gameObject.GetComponent<HedgehogController>();
        if (player == null) return;

        if ((player.TerrainMask | IfTerrainMaskHas) > 0 && player.Grounded)
        {
            Apply(player);
        }
    }

    public void Apply(HedgehogController player)
    {
        player.TerrainMask |= AddLayers;
        player.TerrainMask &= ~RemoveLayers;
    }
}
