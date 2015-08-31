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
    /// Whether the player must be grounded to switch layers.
    /// </summary>
    [SerializeField]
    public bool MustBeGrounded;

    /// <summary>
    /// The layer the player must be on to be able to switch layer.
    /// </summary>
    [SerializeField]
    public int LayerFrom;

    /// <summary>
    /// The layer the player switches layer to.
    /// </summary>
    [SerializeField]
    public int LayerTo;

    public void OnTriggerEnter2D(Collider2D collider)
    {
        HedgehogController player = collider.gameObject.GetComponent<HedgehogController>();
        if (player == null) return;

        if (player.Layer == LayerFrom)
        {
            if (!MustBeGrounded || (MustBeGrounded && player.Grounded))
            {
                player.Layer = LayerTo;
            }
        }
    }

    public void OnTriggerStay2D(Collider2D collider)
    {
        if (!MustBeGrounded) return;

        HedgehogController player = collider.gameObject.GetComponent<HedgehogController>();
        if (player == null) return;

        if (player.Layer == LayerFrom && player.Grounded)
            player.Layer = LayerTo;
    }
}
