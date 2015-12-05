using System.Linq;
using UnityEngine;

namespace Hedgehog.Core.Utils
{
    /// <summary>
    /// Helper component with useful methods that can be called from the Unity UI.
    /// </summary>
    public class AnimationEvents : MonoBehaviour
    {
        public void DestroyGameObject(GameObject gameObject)
        {
            Destroy(gameObject);
        }

        public void DestroyComponent(Component component)
        {
            Destroy(component);
        }

        public void DestroySelf()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// Disables the component with the specified type name.
        /// </summary>
        /// <param name="type">The specified type name.</param>
        public void DisableComponent(string type)
        {
            var component = GetComponents<Behaviour>().
                FirstOrDefault(component1 => component1.GetType().Name == type);
            if (component != null) component.enabled = false;
        }

        /// <summary>
        /// Enables the component with the specified type name.
        /// </summary>
        /// <param name="type">The specified type name.</param>
        public void EnableComponent(string type)
        {
            var component = GetComponents<Behaviour>().
                FirstOrDefault(component1 => component1.GetType().Name == type);
            if (component != null)
                component.enabled = true;
        }

        /// <summary>
        /// Sets the sorting order of the sprite.
        /// </summary>
        public void SetSortingOrder(int sortingOrder)
        {
            var sprite = GetComponentInChildren<SpriteRenderer>();
            if (sprite == null) return;
            sprite.sortingOrder = sortingOrder;
        }
    }
}
