using SonicRealms.Core.Actors;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Helper class for creating platforms that react to controller events.
    /// </summary>
    [AddComponentMenu("")]
    [RequireComponent(typeof(PlatformTrigger))]
    public class ReactivePlatform : BaseReactive
    {
        [HideInInspector]
        public PlatformTrigger PlatformTrigger;

        protected bool RegisteredEvents;

        /// <summary>
        /// Name of an Animator trigger on the player to set when it collides with the platform.
        /// </summary>
        [Foldout("Player Animation")]
        [Tooltip("Name of an Animator trigger on the player to set when it collides with the platform.")]
        public string CollidingTrigger;

        /// <summary>
        /// Name of an Animator bool on the player to set to true while it's colliding with the platform.
        /// </summary>
        [Foldout("Player Animation")]
        [Tooltip("Name of an Animator bool on the player to set to true while it's colliding with the platform.")]
        public string CollidingBool;

        /// <summary>
        /// Name of an Animator trigger on the player to set when it stands on the platform.
        /// </summary>
        [Foldout("Player Animation")]
        [Tooltip("Name of an Animator trigger on the player to set when it stands on the platform.")]
        public string SurfaceTrigger;

        /// <summary>
        /// Name of an Animator bool on the player to set to true while it's on the platform.
        /// </summary>
        [Foldout("Player Animation")]
        [Tooltip("Name of an Animator bool on the player to set to true while it's on the platform.")]
        public string SurfaceBool;

        public override void Reset()
        {
            base.Reset();
            CollidingTrigger = CollidingBool = SurfaceTrigger = SurfaceBool = "";
        }

        public override void Awake()
        {
            base.Awake();
            PlatformTrigger = GetComponent<PlatformTrigger>() ?? gameObject.AddComponent<PlatformTrigger>();
            RegisteredEvents = false;
        }

        public virtual void OnEnable()
        {
            if (PlatformTrigger != null && PlatformTrigger.SurfaceRules != null) Start();
        }

        public override void Start()
        {
            base.Start();

            if (RegisteredEvents) return;

            // Add listeners to the platform trigger
            PlatformTrigger.CollisionRules.Add(IsSolid);
            PlatformTrigger.OnPlatformEnter.AddListener(NotifyPlatformEnter);
            PlatformTrigger.OnPlatformStay.AddListener(NotifyPlatformStay);
            PlatformTrigger.OnPlatformExit.AddListener(NotifyPlatformExit);

            PlatformTrigger.SurfaceRules.Add(IsOnSurface);
            PlatformTrigger.OnSurfaceEnter.AddListener(NotifySurfaceEnter);
            PlatformTrigger.OnSurfaceStay.AddListener(NotifySurfaceStay);
            PlatformTrigger.OnSurfaceExit.AddListener(NotifySurfaceExit);

            RegisteredEvents = true;
        }

        public virtual void OnDisable()
        {
            if (!RegisteredEvents) return;

            // Remove listeners from the platform trigger
            PlatformTrigger.CollisionRules.Remove(IsSolid);
            PlatformTrigger.OnPlatformEnter.RemoveListener(NotifyPlatformEnter);
            PlatformTrigger.OnPlatformStay.RemoveListener(NotifyPlatformStay);
            PlatformTrigger.OnPlatformExit.RemoveListener(NotifyPlatformExit);

            PlatformTrigger.SurfaceRules.Remove(IsOnSurface);
            PlatformTrigger.OnSurfaceEnter.RemoveListener(NotifySurfaceEnter);
            PlatformTrigger.OnSurfaceStay.RemoveListener(NotifySurfaceStay);
            PlatformTrigger.OnSurfaceExit.RemoveListener(NotifySurfaceExit);

            RegisteredEvents = false;
        }

        /// <summary>
        /// Default surface rule. Uses the trigger's default surface rule.
        /// </summary>
        public virtual bool IsOnSurface(TerrainCastHit hit)
        {
            return PlatformTrigger.DefaultSurfaceRule(hit);
        }

        /// <summary>
        /// Default collision rule. Always solid to all collsions by default.
        /// </summary>
        public virtual bool IsSolid(TerrainCastHit hit)
        {
            return true;
        }

        // Override these methods to react when a controller collides with the platform.
        public virtual void OnPlatformEnter(TerrainCastHit hit)
        {

        }

        public virtual void OnPlatformStay(TerrainCastHit hit)
        {

        }

        public virtual void OnPlatformExit(TerrainCastHit hit)
        {

        }
        // Override these methods to react when a controller stands on the platform.
        public virtual void OnSurfaceEnter(TerrainCastHit hit)
        {
            
        }

        public virtual void OnSurfaceStay(TerrainCastHit hit)
        {
            
        }

        public virtual void OnSurfaceExit(TerrainCastHit hit)
        {
            
        }
        #region Notify Methods
        public void NotifyPlatformEnter(TerrainCastHit hit)
        {
            OnPlatformEnter(hit);
            hit.Controller.NotifyPlatformEnter(this);

            var controller = hit.Controller;
            if (controller.Animator == null)
                return;

            var logWarnings = controller.Animator.logWarnings;
            controller.Animator.logWarnings = false;

            SetPlatformEnterParameters(controller);

            controller.Animator.logWarnings = logWarnings;
        }

        protected virtual void SetPlatformEnterParameters(HedgehogController controller)
        {
            if (!string.IsNullOrEmpty(CollidingTrigger))
                controller.Animator.SetTrigger(CollidingTrigger);

            if (!string.IsNullOrEmpty(CollidingBool))
                controller.Animator.SetBool(CollidingBool, true);
        }

        public void NotifyPlatformStay(TerrainCastHit hit)
        {
            OnPlatformStay(hit);
            var controller = hit.Controller;
            if (controller.Animator == null)
                return;

            var logWarnings = controller.Animator.logWarnings;
            controller.Animator.logWarnings = false;

            SetPlatformStayParameters(controller);

            controller.Animator.logWarnings = logWarnings;
        }

        protected virtual void SetPlatformStayParameters(HedgehogController controller)
        {
            if(!string.IsNullOrEmpty(CollidingBool))
                controller.Animator.SetBool(CollidingBool, true);
        }

        public void NotifyPlatformExit(TerrainCastHit hit)
        {
            OnPlatformExit(hit);
            hit.Controller.NotifyPlatformExit(this);

            var controller = hit.Controller;
            if (controller.Animator == null) return;

            var logWarnings = controller.Animator.logWarnings;
            controller.Animator.logWarnings = false;

            SetPlatformExitParameters(controller);

            controller.Animator.logWarnings = logWarnings;
        }

        protected virtual void SetPlatformExitParameters(HedgehogController controller)
        {
            if (!string.IsNullOrEmpty(CollidingBool))
                controller.Animator.SetBool(CollidingBool, false);
        }

        public void NotifySurfaceEnter(TerrainCastHit hit)
        {
            OnSurfaceEnter(hit);
            hit.Controller.NotifySurfaceEnter(this);

            var controller = hit.Controller;
            if (controller.Animator == null)
                return;

            var logWarnings = controller.Animator.logWarnings;
            controller.Animator.logWarnings = false;

            SetSurfaceEnterParameters(controller);

            controller.Animator.logWarnings = logWarnings;
        }

        protected virtual void SetSurfaceEnterParameters(HedgehogController controller)
        {
            if (!string.IsNullOrEmpty(SurfaceTrigger))
                controller.Animator.SetTrigger(SurfaceTrigger);

            if (!string.IsNullOrEmpty(SurfaceBool))
                controller.Animator.SetBool(SurfaceBool, true);
        }

        public void NotifySurfaceStay(TerrainCastHit hit)
        {
            OnSurfaceStay(hit);

            var controller = hit.Controller;
            if (controller.Animator == null)
                return;

            var logWarnings = controller.Animator.logWarnings;
            controller.Animator.logWarnings = false;

            SetSurfaceStayParameters(controller);

            controller.Animator.logWarnings = logWarnings;
        }

        protected virtual void SetSurfaceStayParameters(HedgehogController controller)
        {
            if(!string.IsNullOrEmpty(SurfaceBool))
                controller.Animator.SetBool(SurfaceBool, true);
        }

        public void NotifySurfaceExit(TerrainCastHit hit)
        {
            OnSurfaceExit(hit);
            hit.Controller.NotifySurfaceExit(this);

            var controller = hit.Controller;
            if (controller.Animator == null)
                return;

            var logWarnings = controller.Animator.logWarnings;
            controller.Animator.logWarnings = false;

            SetSurfaceExitParameters(controller);

            controller.Animator.logWarnings = logWarnings;
        }

        protected virtual void SetSurfaceExitParameters(HedgehogController controller)
        {
            if (!string.IsNullOrEmpty(SurfaceBool))
                controller.Animator.SetBool(SurfaceBool, false);
        }
        #endregion

        public virtual void OnDestroy()
        {
            foreach (var hit in PlatformTrigger.SurfaceCollisions)
                NotifySurfaceExit(hit);

            foreach (var hit in PlatformTrigger.Collisions)
                NotifyPlatformExit(hit);
        }
    }
}
