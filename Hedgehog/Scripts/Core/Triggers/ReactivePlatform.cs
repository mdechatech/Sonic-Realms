using Hedgehog.Core.Actors;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Core.Triggers
{
    /// <summary>
    /// Helper class for creating platforms that react to controller events.
    /// </summary>
    [AddComponentMenu("")]
    [RequireComponent(typeof(PlatformTrigger))]
    public class ReactivePlatform : BaseReactive
    {
        protected PlatformTrigger PlatformTrigger;
        protected bool RegisteredEvents;

        public virtual void Awake()
        {
            PlatformTrigger = GetComponent<PlatformTrigger>();
            RegisteredEvents = false;
        }

        public virtual void OnEnable()
        {
            if (PlatformTrigger.SurfaceRules != null) Start();
        }

        public override void Start()
        {
            base.Start();

            if (RegisteredEvents) return;

            PlatformTrigger.CollisionRules.Add(CollidesWith);
            PlatformTrigger.OnPlatformEnter.AddListener(OnPlatformEnter);
            PlatformTrigger.OnPlatformStay.AddListener(OnPlatformStay);
            PlatformTrigger.OnPlatformExit.AddListener(OnPlatformExit);

            PlatformTrigger.SurfaceRules.Add(IsOnSurface);
            PlatformTrigger.OnSurfaceEnter.AddListener(OnSurfaceEnter);
            PlatformTrigger.OnSurfaceStay.AddListener(OnSurfaceStay);
            PlatformTrigger.OnSurfaceExit.AddListener(OnSurfaceExit);

            RegisteredEvents = true;
        }

        public virtual void OnDisable()
        {
            if (!RegisteredEvents) return;

            PlatformTrigger.CollisionRules.Remove(CollidesWith);
            PlatformTrigger.OnPlatformEnter.RemoveListener(OnPlatformEnter);
            PlatformTrigger.OnPlatformStay.RemoveListener(OnPlatformStay);
            PlatformTrigger.OnPlatformExit.RemoveListener(OnPlatformExit);

            PlatformTrigger.SurfaceRules.Remove(IsOnSurface);
            PlatformTrigger.OnSurfaceEnter.RemoveListener(OnSurfaceEnter);
            PlatformTrigger.OnSurfaceStay.RemoveListener(OnSurfaceStay);
            PlatformTrigger.OnSurfaceExit.RemoveListener(OnSurfaceExit);

            RegisteredEvents = false;
        }

        /// <summary>
        /// Default surface rule. Uses the trigger's default surface rule.
        /// </summary>
        public virtual bool IsOnSurface(HedgehogController controller, TerrainCastHit hit)
        {
            return PlatformTrigger.DefaultSurfaceRule(controller, hit);
        }

        /// <summary>
        /// Default collision rule. Uses the trigger's default collision rule.
        /// </summary>
        public virtual bool CollidesWith(TerrainCastHit hit)
        {
            return PlatformTrigger.DefaultCollisionRule(hit);
        }

        // Override these methods to react when a controller collides with the platform.
        public virtual void OnPlatformEnter(HedgehogController controller, TerrainCastHit hit)
        {

        }

        public virtual void OnPlatformStay(HedgehogController controller, TerrainCastHit hit)
        {

        }

        public virtual void OnPlatformExit(HedgehogController controller, TerrainCastHit hit)
        {

        }
        // Override these methods to react when a controller stands on the platform.
        public virtual void OnSurfaceEnter(HedgehogController controller, TerrainCastHit hit)
        {
            
        }

        public virtual void OnSurfaceStay(HedgehogController controller, TerrainCastHit hit)
        {
            
        }

        public virtual void OnSurfaceExit(HedgehogController controller, TerrainCastHit hit)
        {
            
        }
    }
}
