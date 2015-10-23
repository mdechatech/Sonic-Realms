using Hedgehog.Core.Actors;
using UnityEngine;

namespace Hedgehog.Core.Triggers
{
    /// <summary>
    /// Helper class for creating platforms that can also deactivate and become areas
    /// (like water you can run across).
    /// </summary>
    [RequireComponent(typeof(PlatformTrigger), typeof(AreaTrigger))]
    public class ReactivePlatformArea : ReactivePlatform
    {
        protected AreaTrigger AreaTrigger;

        public override void Awake()
        {
            base.Awake();

            AreaTrigger = GetComponent<AreaTrigger>();
        }

        public override void OnEnable()
        {
            base.OnEnable();

            if (AreaTrigger.InsideRules != null) Start();
        }

        public override void Start()
        {
            if (RegisteredEvents) return;

            if (!AreaTrigger.InsideRules.Contains(IsInsideArea))
                AreaTrigger.InsideRules.Add(IsInsideArea);
            AreaTrigger.OnAreaEnter.AddListener(OnAreaEnter);
            AreaTrigger.OnAreaStay.AddListener(OnAreaStay);
            AreaTrigger.OnAreaExit.AddListener(OnAreaExit);

            base.Start();
        }

        public override void OnDisable()
        {
            if (!RegisteredEvents) return;

            AreaTrigger.InsideRules.Remove(IsInsideArea);
            AreaTrigger.OnAreaEnter.RemoveListener(OnAreaEnter);
            AreaTrigger.OnAreaStay.RemoveListener(OnAreaStay);
            AreaTrigger.OnAreaExit.RemoveListener(OnAreaExit);

            base.OnDisable();
        }

        public virtual bool IsInsideArea(HedgehogController controller)
        {
            return AreaTrigger.DefaultCollisionRule(controller);
        }

        public virtual void OnAreaEnter(HedgehogController controller)
        {

        }

        public virtual void OnAreaStay(HedgehogController controller)
        {

        }

        public virtual void OnAreaExit(HedgehogController controller)
        {

        }
    }
}
