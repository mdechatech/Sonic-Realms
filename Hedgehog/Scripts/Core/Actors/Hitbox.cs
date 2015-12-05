using UnityEngine;

namespace Hedgehog.Core.Actors
{
    /// <summary>
    /// Hitbox that can decide when it wants to let an area trigger know it's been collided with.
    /// Works well for preventing Sonic from collecting far away rings when using his
    /// Insta-Shield move.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Hitbox : MonoBehaviour
    {

        
    }
}
