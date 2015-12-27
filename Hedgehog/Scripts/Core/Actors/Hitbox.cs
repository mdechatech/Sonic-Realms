using Hedgehog.Core.Triggers;
using UnityEngine;

namespace Hedgehog.Core.Actors
{
    /// <summary>
    /// Hitbox that can decide when it wants to let an area trigger know it's been collided with.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Hitbox : MonoBehaviour
    {
        /// <summary>
        /// The controller the hitbox belongs to.
        /// </summary>
        [Tooltip("The controller the hitbox belongs to.")]
        public HedgehogController Source;

        public void Reset()
        {
            Source = GetComponentInParent<HedgehogController>();
        }

        public void Awake()
        {
            Source = Source ?? GetComponentInParent<HedgehogController>();
        }

        /// <summary>
        /// Whether to let the specified trigger know it's been collided with.
        /// </summary>
        /// <param name="trigger">The specified trigger.</param>
        /// <returns></returns>
        public virtual bool AllowCollision(AreaTrigger trigger)
        {
            return true;
        }
    }
}
