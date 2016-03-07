using UnityEngine;
using UnityEngine.Events;

namespace SonicRealms.Core.Utils
{
    public class OnBecameVisibleCallback : MonoBehaviour
    {
        public UnityEvent BecameVisible;
        public UnityEvent BecameInvisible;

        public void Reset()
        {
            BecameVisible = new UnityEvent();
            BecameInvisible = new UnityEvent();
        }

        public void Awake()
        {
            BecameVisible = BecameVisible ?? new UnityEvent();
            BecameInvisible = BecameInvisible ?? new UnityEvent();
        }

        public void OnBecameVisible()
        {
            BecameVisible.Invoke();
        }

        public void OnBecameInvisible()
        {
            BecameInvisible.Invoke();
        }
    }
}
