using System;
using UnityEngine;
using UnityEngine.Events;

namespace SonicRealms.Core.Internal
{
    [Serializable]
    public class SrTriggerEvent2D : UnityEvent<Collider2D> {}

    /// <summary>
    /// Links OnTriggerEnter2D, OnTriggerStay2D, and OnTriggerExit2D to UnityEvents.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SrTriggerCallback2D : MonoBehaviour
    {
        public SrTriggerEvent2D TriggerEnter2D;
        public SrTriggerEvent2D TriggerStay2D;
        public SrTriggerEvent2D TriggerExit2D;

        public void Reset()
        {
            TriggerEnter2D = new SrTriggerEvent2D();
            TriggerStay2D = new SrTriggerEvent2D();
            TriggerExit2D = new SrTriggerEvent2D();
        }

        public void Awake()
        {
            TriggerEnter2D = TriggerEnter2D ?? new SrTriggerEvent2D();
            TriggerStay2D = TriggerStay2D ?? new SrTriggerEvent2D();
            TriggerExit2D = TriggerExit2D ?? new SrTriggerEvent2D();
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
