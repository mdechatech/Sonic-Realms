using UnityEngine;

namespace Hedgehog.Core.Utils
{
    /// <summary>
    /// Just a quick component that allows animation events to call Destroy.
    /// </summary>
    public class AnimationDestroy : MonoBehaviour
    {
        public void DestroyGameObject(GameObject gameObject)
        {
            Destroy(gameObject);
        }

        public void DestroySelf()
        {
            Destroy(gameObject);
        }
    }
}
