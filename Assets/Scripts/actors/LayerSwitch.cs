using UnityEngine;
using System.Collections;

/// <summary>
/// Must have a collider2D attached. When the player collides (has the tag "Player"),
/// this switches the player's layer from layerFrom to layerTo.
/// </summary>
public class LayerSwitch : MonoBehaviour {

    /// <summary>
    /// Whether the player must be grounded to switch layers.
    /// </summary>
    [SerializeField]
    private bool mustBeGrounded;

    /// <summary>
    /// The layer the player must be on to be able to switch layer.
    /// </summary>
    [SerializeField]
    private int layerFrom;

    /// <summary>
    /// The layer the player switches layer to.
    /// </summary>
    [SerializeField]
    private int layerTo;

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == "Player")
        {
            PlayerController player = collider.gameObject.GetComponent<PlayerController>();
            if(player.Layer == layerFrom)
            {
                if(!mustBeGrounded || (mustBeGrounded && player.Grounded))
                {
                    player.Layer = layerTo;
                }
            }
        }
    }
}
