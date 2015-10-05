using UnityEngine;

namespace Hedgehog.Core.Triggers
{
    /// <summary>
    /// Not meant for manual addition. Added to child colliders to notify a parent AreaTrigger if
    /// it has TriggerFromChildren turned on.
    /// </summary>
    [AddComponentMenu("")]
    [RequireComponent(typeof(Collider2D))]
    public class ChildAreaTrigger : MonoBehaviour
    {
        [SerializeField]
        public AreaTrigger Target;

        public void OnTriggerEnter2D(Collider2D collider2D)
        {
            if (Target == null) return;
            Target.OnChildTriggerEnter2D(collider2D, transform);
        }

        public void OnTriggerStay2D(Collider2D collider2D)
        {
            if (Target == null) return;
            Target.OnChildTriggerStay2D(collider2D, transform);
        }

        public void OnTriggerExit2D(Collider2D collider2D)
        {
            if (Target == null) return;
            Target.OnChildTriggerExit2D(collider2D, transform);
        }
    }
}
