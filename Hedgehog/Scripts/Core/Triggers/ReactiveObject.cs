using System.Linq;
using Hedgehog.Core.Actors;
using UnityEngine;

namespace Hedgehog.Core.Triggers
{
    /// <summary>
    /// Helper class for creating objects that react to activation.
    /// </summary>
    [AddComponentMenu("")]
    [RequireComponent(typeof(ObjectTrigger))]
    public class ReactiveObject : BaseReactive
    {
        protected ObjectTrigger ObjectTrigger;
        protected bool RegisteredEvents;

        public override void Awake()
        {
            base.Awake();
            ObjectTrigger = GetComponent<ObjectTrigger>();

            if (ObjectTrigger.AutoActivate || GetComponents<BaseReactive>().Any(reactive => reactive.ActivatesObject))
                return;

            // If there's no activator, we can guess whether we want to act like a platform or an area
            var collider2D = GetComponent<Collider2D>();
            if (collider2D == null)
                return;

            ObjectTrigger.AutoActivate = true;
            if (collider2D.isTrigger)
            {
                if (GetComponent<AreaTrigger>() == null)
                    gameObject.AddComponent<AreaTrigger>();
            }
            else if (GetComponent<PlatformTrigger>() == null)
            {
                gameObject.AddComponent<PlatformTrigger>();
            }
        }

        public virtual void OnEnable()
        {
            if (ObjectTrigger.OnActivateEnter != null) Start();
        }

        public override void Start()
        {
            base.Start();

            if (RegisteredEvents) return;

            ObjectTrigger.OnActivateEnter.AddListener(OnActivateEnter);
            ObjectTrigger.OnActivateStay.AddListener(OnActivateStay);
            ObjectTrigger.OnActivateExit.AddListener(OnActivateExit);

            RegisteredEvents = true;
        }

        public virtual void OnDisable()
        {
            if (!RegisteredEvents) return;

            ObjectTrigger.OnActivateEnter.RemoveListener(OnActivateEnter);
            ObjectTrigger.OnActivateStay.RemoveListener(OnActivateStay);
            ObjectTrigger.OnActivateExit.RemoveListener(OnActivateExit);

            RegisteredEvents = false;
        }

        public virtual void OnActivateEnter(HedgehogController controller)
        {
            
        }

        public virtual void OnActivateStay(HedgehogController controller)
        {
            
        }

        public virtual void OnActivateExit(HedgehogController controller)
        {
            
        }
    }
}
