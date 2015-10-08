using Hedgehog.Core.Actors;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Core.Triggers
{
    /// <summary>
    /// Helper class for creating platforms that react to player events.
    /// </summary>
    [AddComponentMenu("")]
    [RequireComponent(typeof(PlatformTrigger))]
    public class ReactivePlatform : MonoBehaviour
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

        public virtual void Start()
        {
            if (RegisteredEvents) return;

            if (!PlatformTrigger.SurfaceRules.Contains(IsOnSurface))
                PlatformTrigger.SurfaceRules.Add(IsOnSurface);
            PlatformTrigger.CollisionRules.Add(CollidesWith);
            PlatformTrigger.OnSurfaceEnter.AddListener(OnSurfaceEnter);
            PlatformTrigger.OnSurfaceStay.AddListener(OnSurfaceStay);
            PlatformTrigger.OnSurfaceExit.AddListener(OnSurfaceExit);

            RegisteredEvents = true;
        }

        public virtual void OnDisable()
        {
            if (!RegisteredEvents) return;

            PlatformTrigger.SurfaceRules.Remove(IsOnSurface);
            PlatformTrigger.CollisionRules.Remove(CollidesWith);
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

        // Override these methods to react when a controller enters, stays on, and exits a platform!
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
