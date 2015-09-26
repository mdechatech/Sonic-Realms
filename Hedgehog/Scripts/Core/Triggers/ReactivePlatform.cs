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

        public virtual void Awake()
        {
            PlatformTrigger = GetComponent<PlatformTrigger>();
        }

        public virtual void OnEnable()
        {
            if(!PlatformTrigger.SurfaceRules.Contains(IsOnSurface))
                PlatformTrigger.SurfaceRules.Add(IsOnSurface);
            PlatformTrigger.CollisionRules.Add(CollidesWith);
            PlatformTrigger.OnSurfaceEnter.AddListener(OnSurfaceEnter);
            PlatformTrigger.OnSurfaceStay.AddListener(OnSurfaceStay);
            PlatformTrigger.OnSurfaceExit.AddListener(OnSurfaceExit);
        }

        public virtual void OnDisable()
        {
            PlatformTrigger.SurfaceRules.Remove(IsOnSurface);
            PlatformTrigger.CollisionRules.Remove(CollidesWith);
            PlatformTrigger.OnSurfaceEnter.RemoveListener(OnSurfaceEnter);
            PlatformTrigger.OnSurfaceStay.RemoveListener(OnSurfaceStay);
            PlatformTrigger.OnSurfaceExit.RemoveListener(OnSurfaceExit);
        }

        /// <summary>
        /// Default surface rule. Uses the trigger's default surface rule.
        /// </summary>
        public virtual bool IsOnSurface(HedgehogController controller, TerrainCastHit hit, SurfacePriority priority)
        {
            return PlatformTrigger.DefaultSurfaceRule(controller, hit, priority);
        }

        /// <summary>
        /// Default collision rule. Uses the trigger's default collision rule.
        /// </summary>
        public virtual bool CollidesWith(TerrainCastHit hit)
        {
            return PlatformTrigger.DefaultCollisionRule(hit);
        }

        // Override these methods to react when a controller enters, stays on, and exits a platform!
        public virtual void OnSurfaceEnter(HedgehogController controller, TerrainCastHit hit, SurfacePriority priority)
        {
            
        }

        public virtual void OnSurfaceStay(HedgehogController controller, TerrainCastHit hit, SurfacePriority priority)
        {

        }

        public virtual void OnSurfaceExit(HedgehogController controller, TerrainCastHit hit, SurfacePriority priority)
        {

        }
    }
}
