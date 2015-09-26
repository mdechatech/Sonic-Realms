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
            if (!AreaTrigger.CollisionRules.Contains(IsInsideArea))
                AreaTrigger.CollisionRules.Add(IsInsideArea);
            AreaTrigger.OnAreaEnter.AddListener(OnAreaEnter);
            AreaTrigger.OnAreaStay.AddListener(OnAreaStay);
            AreaTrigger.OnAreaExit.AddListener(OnAreaExit);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            AreaTrigger.CollisionRules.Remove(IsInsideArea);
            AreaTrigger.OnAreaEnter.RemoveListener(OnAreaEnter);
            AreaTrigger.OnAreaStay.RemoveListener(OnAreaStay);
            AreaTrigger.OnAreaExit.RemoveListener(OnAreaExit);
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
