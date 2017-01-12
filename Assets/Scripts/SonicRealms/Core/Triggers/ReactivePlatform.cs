using System;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Base class for platforms that do something when touched by the player.
    /// </summary>
    [RequireComponent(typeof(PlatformTrigger))]
    public abstract class ReactivePlatform : BaseReactive
    {
        [HideInInspector]
        public PlatformTrigger PlatformTrigger;

        protected bool RegisteredEvents;

        #region Modifier Functions

        // Override this to determine when the trigger should raise surface events for the given contact
        public virtual bool IsOnSurface(SurfaceCollision.Contact contact)
        {
            return true;
        }

        // Override this to change when a terrain cast should register with the platform
        // Return false if terrain casts should ignore this platform
        public virtual bool IsSolid(TerrainCastHit data)
        {
            return true;
        }

        // Override this to react when a controller is about to collide with the platform
        public virtual void OnPreCollide(PlatformCollision.Contact contact)
        {
            // Call contact.Controller.IgnoreThisCollision() here to prevent the collision from happening
        }

        #endregion

        #region Platform Collision Functions

        // Override these methods to react when a controller collides with the platform

        /// <summary>
        /// Override this to react when a player starts colliding with the platform.
        /// </summary>
        public virtual void OnPlatformEnter(PlatformCollision collision)
        {

        }

        /// <summary>
        /// Override this to react on every FixedUpdate for each player colliding with the player.
        /// </summary>
        public virtual void OnPlatformStay(PlatformCollision collision)
        {

        }

        /// <summary>
        /// Override this to react when a player stops colliding with the platform.
        /// </summary>
        public virtual void OnPlatformExit(PlatformCollision collision)
        {

        }
        #endregion
        #region Platform Surface Functions
        // Override these methods to react when a controller stands on the platform

        /// <summary>
        /// Override this to react when a player starts standing on the platform.
        /// </summary>
        public virtual void OnSurfaceEnter(SurfaceCollision collision)
        {

        }

        /// <summary>
        /// Override this to react on every FixedUpdate for each player on the platform.
        /// </summary>
        public virtual void OnSurfaceStay(SurfaceCollision collision)
        {

        }

        /// <summary>
        /// Override this to react when a player stops standing on the platform.
        /// </summary>
        public virtual void OnSurfaceExit(SurfaceCollision collision)
        {

        }

        #endregion

        #region Helper Functions
        protected void SetPlayerAnimatorParameters(TerrainCastHit hit, Action<TerrainCastHit> setter)
        {
            var controller = hit.Controller;
            if (controller.Animator == null || setter == null)
                return;

            var logWarnings = controller.Animator.logWarnings;
            controller.Animator.logWarnings = false;

            setter(hit);

            controller.Animator.logWarnings = logWarnings;
        }
        #endregion

        #region Lifecycle Functions
        public override void Reset()
        {
            base.Reset();

            GetComponent<PlatformTrigger>().KeepWhenAlone = false;
        }

        public override void Awake()
        {
            base.Awake();

            PlatformTrigger = GetComponent<PlatformTrigger>() ?? gameObject.AddComponent<PlatformTrigger>();
            PlatformTrigger.KeepWhenAlone = false;

            RegisteredEvents = false;
        }

        public virtual void OnEnable()
        {
            if (PlatformTrigger != null && PlatformTrigger.SurfaceRules != null)
                Start();
        }

        public override void Start()
        {
            base.Start();

            if (RegisteredEvents)
                return;

            // Add listeners to the platform trigger
            PlatformTrigger.SolidityRules.Add(IsSolid);
            PlatformTrigger.OnPreCollide.AddListener(OnPreCollide);
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
            PlatformTrigger.SolidityRules.Remove(IsSolid);
            PlatformTrigger.OnPreCollide.RemoveListener(OnPreCollide);
            PlatformTrigger.OnPlatformEnter.RemoveListener(NotifyPlatformEnter);
            PlatformTrigger.OnPlatformStay.RemoveListener(NotifyPlatformStay);
            PlatformTrigger.OnPlatformExit.RemoveListener(NotifyPlatformExit);

            PlatformTrigger.SurfaceRules.Remove(IsOnSurface);
            PlatformTrigger.OnSurfaceEnter.RemoveListener(NotifySurfaceEnter);
            PlatformTrigger.OnSurfaceStay.RemoveListener(NotifySurfaceStay);
            PlatformTrigger.OnSurfaceExit.RemoveListener(NotifySurfaceExit);

            RegisteredEvents = false;
        }

        public virtual void OnDestroy()
        {
            foreach (var contacts in PlatformTrigger.CurrentSurfaceContacts)
                NotifySurfaceExit(new SurfaceCollision(contacts.Value));

            foreach (var contacts in PlatformTrigger.CurrentPlatformContacts)
                NotifyPlatformExit(new PlatformCollision(contacts.Value));
        }
        #endregion

        #region Notify Functions
        public void NotifyPlatformEnter(PlatformCollision collision)
        {
            OnPlatformEnter(collision);
            collision.Controller.NotifyPlatformEnter(this);
        }

        public void NotifyPlatformStay(PlatformCollision collision)
        {
            OnPlatformStay(collision);
        }

        public void NotifyPlatformExit(PlatformCollision collision)
        {
            OnPlatformExit(collision);
            collision.Controller.NotifyPlatformExit(this);
        }


        public void NotifySurfaceEnter(SurfaceCollision info)
        {
            OnSurfaceEnter(info);
            info.Controller.NotifySurfaceEnter(this);
        }

        public void NotifySurfaceStay(SurfaceCollision info)
        {
            OnSurfaceStay(info);
        }

        public void NotifySurfaceExit(SurfaceCollision info)
        {
            OnSurfaceExit(info);
            info.Controller.NotifySurfaceExit(this);
        }
        #endregion
    }
}
