using System;
using UnityEngine;
using UnityEngine.Events;

namespace SonicRealms.Core.Utils
{
    [Serializable]
    public class TriggerEvent2D : UnityEvent<Collider2D> {}

    /// <summary>
    /// Links OnTriggerEnter2D, OnTriggerStay2D, and OnTriggerExit2D to UnityEvents.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TriggerCallback2D : MonoBehaviour
    {
        public TriggerEvent2D TriggerEnter2D;
        public TriggerEvent2D TriggerStay2D;
        public TriggerEvent2D TriggerExit2D;

        public void Reset()
        {
            TriggerEnter2D = new TriggerEvent2D();
            TriggerStay2D = new TriggerEvent2D();
            TriggerExit2D = new TriggerEvent2D();
        }

        public void Awake()
        {
            TriggerEnter2D = TriggerEnter2D ?? new TriggerEvent2D();
            TriggerStay2D = TriggerStay2D ?? new TriggerEvent2D();
            TriggerExit2D = TriggerExit2D ?? new TriggerEvent2D();
        }

        protected void OnTriggerEnter2D(Collider2D other)
        {
            TriggerEnter2D.Invoke(other);
        }

        protected void OnTriggerStay2D(Collider2D other)
        {
            TriggerStay2D.Invoke(other);
        }

        protected void OnTriggerExit2D(Collider2D other)
        {
            TriggerExit2D.Invoke(other);
        }
    }
}
