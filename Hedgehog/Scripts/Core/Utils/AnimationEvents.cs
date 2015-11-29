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
    }
}
