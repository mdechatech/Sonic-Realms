using System.Linq;
using UnityEngine;

namespace Hedgehog.Core.Utils
{
    /// <summary>
    /// Helper component with useful methods that can be called from the Unity UI.
    /// </summary>
    public class AnimationEvents : MonoBehaviour
    {
        /// <summary>
        /// Destroys the specified game object.
        /// </summary>
        /// <param name="gameObject">The specified game object.</param>
        public void DestroyGameObject(GameObject gameObject)
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// Destroys the specified component.
        /// </summary>
        /// <param name="component">The specified component.</param>
        public void DestroyComponent(Component component)
        {
            Destroy(component);
        }

        /// <summary>
        /// Destroys this component's game object.
        /// </summary>
        public void DestroySelf()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// Copies the specified object at the transform's position, at the top level of the hierarchy
        /// </summary>
        /// <param name="original">The specified object.</param>
        public void AddObject(GameObject original)
        {
            Instantiate(original, transform.position, transform.rotation);
        }

        /// <summary>
        /// Copies the specified object at the transform's position, at a child of the transform.
        /// </summary>
        /// <param name="original">The specified object.</param>
        public void AddObjectAsChild(GameObject original)
        {
            var copy = (GameObject)Instantiate(original, transform.position, transform.rotation);
            copy.transform.position = transform.position;
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
