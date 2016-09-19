using System;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Helper class for creating platforms that react to controller events.
    /// </summary>
    [RequireComponent(typeof(PlatformTrigger))]
    public class ReactivePlatform : BaseReactive
    {
        [HideInInspector]
        public PlatformTrigger PlatformTrigger;

        protected bool RegisteredEvents;
        
        /// <summary>
        /// Name of an Animator trigger to set when a player collides with the platform.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator trigger to set when a player collides with the platform.")]
        public string CollidingTrigger;
        protected int CollidingTriggerHash;

        /// <summary>
        /// Name of an Animator bool to set to true while a player is colliding with the platform.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator bool to set to true while a player is colliding with the platform.")]
        public string CollidingBool;
        protected int CollidingBoolHash;

        /// <summary>
        /// Name of an Animator trigger to set when a player stands on the platform.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator trigger to set when a player stands on the platform.")]
        public string SurfaceTrigger;
        protected int SurfaceTriggerHash;

        /// <summary>
        /// Name of an Animator bool to set to true while a player is on the platform.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator bool to set to true while a player is on the platform.")]
        public string SurfaceBool;
        protected int SurfaceBoolHash;

        /// <summary>
        /// Name of an Animator trigger on the player to set when it collides with the platform.
        /// </summary>
        [Foldout("Player Animation")]
        [Tooltip("Name of an Animator trigger on the player to set when it collides with the platform.")]
        public string PlayerCollidingTrigger;
        protected int PlayerCollidingTriggerHash;

        /// <summary>
        /// Name of an Animator bool on the player to set to true while it's colliding with the platform.
        /// </summary>
        [Foldout("Player Animation")]
        [Tooltip("Name of an Animator bool on the player to set to true while it's colliding with the platform.")]
        public string PlayerCollidingBool;
        protected int PlayerCollidingBoolHash;

        /// <summary>
        /// Name of an Animator trigger on the player to set when it stands on the platform.
        /// </summary>
        [Foldout("Player Animation")]
        [Tooltip("Name of an Animator trigger on the player to set when it stands on the platform.")]
        public string PlayerSurfaceTrigger;
        protected int PlayerSurfaceTriggerHash;

        /// <summary>
        /// Name of an Animator bool on the player to set to true while it's on the platform.
        /// </summary>
        [Foldout("Player Animation")]
        [Tooltip("Name of an Animator bool on the player to set to true while it's on the platform.")]
        public string PlayerSurfaceBool;
        protected int PlayerSurfaceBoolHash;

        public override void Reset()
        {
            base.Reset();
            PlayerCollidingTrigger = PlayerCollidingBool = PlayerSurfaceTrigger = PlayerSurfaceBool = "";
        }

        public override void Awake()
        {
            base.Awake();
            PlatformTrigger = GetComponent<PlatformTrigger>() ?? gameObject.AddComponent<PlatformTrigger>();
            RegisteredEvents = false;

            CollidingTriggerHash = Animator.StringToHash(CollidingTrigger);
            CollidingBoolHash = Animator.StringToHash(CollidingBool);
            SurfaceTriggerHash = Animator.StringToHash(SurfaceTrigger);
            SurfaceBoolHash = Animator.StringToHash(SurfaceBool);

            PlayerCollidingTriggerHash = Animator.StringToHash(PlayerCollidingTrigger);
            PlayerCollidingBoolHash = Animator.StringToHash(PlayerCollidingBool);
            PlayerSurfaceTriggerHash = Animator.StringToHash(PlayerSurfaceTrigger);
            PlayerSurfaceBoolHash = Animator.StringToHash(PlayerSurfaceBool);
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

        /// <summary>
        /// Default surface rule. Uses the trigger's default surface rule.
        /// </summary>
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

        // Override these methods to react when a controller collides with the platform
        public virtual void OnPlatformEnter(PlatformCollision contact)
        {

        }

        public virtual void OnPlatformStay(PlatformCollision collision)
        {

        }

        public virtual void OnPlatformExit(PlatformCollision collision)
        {

        }

        // Override these methods to react when a controller stands on the platform
        public virtual void OnSurfaceEnter(SurfaceCollision collision)
        {
            
        }

        public virtual void OnSurfaceStay(SurfaceCollision collision)
        {
            
        }

        public virtual void OnSurfaceExit(SurfaceCollision collision)
        {
            
        }
        #region Notify Methods
        public void NotifyPlatformEnter(PlatformCollision collision)
        {
            OnPlatformEnter(collision);
            collision.Controller.NotifyPlatformEnter(this);
            SetAnimatorParameters(collision.Latest.HitData, SetPlatformEnterParameters, SetPlayerPlatformEnterParameters);
        }

        protected virtual void SetPlatformEnterParameters(TerrainCastHit hit)
        {
            if (CollidingTriggerHash != 0)
                Animator.SetTrigger(CollidingTriggerHash);

            if(CollidingBoolHash != 0)
                Animator.SetBool(CollidingBool, true);
        }

        protected virtual void SetPlayerPlatformEnterParameters(TerrainCastHit hit)
        {
            if (PlayerCollidingTriggerHash != 0)
                hit.Controller.Animator.SetTrigger(PlayerCollidingTriggerHash);

            if (PlayerCollidingBoolHash != 0)
                hit.Controller.Animator.SetBool(PlayerCollidingBoolHash, true);
        }

        public void NotifyPlatformStay(PlatformCollision collision)
        {
            OnPlatformStay(collision);
        }

        public void NotifyPlatformExit(PlatformCollision collision)
        {
            OnPlatformExit(collision);
            collision.Controller.NotifyPlatformExit(this);
            SetAnimatorParameters(collision.Latest.HitData, SetPlatformExitParameters, SetPlayerPlatformExitParameters);
        }

        protected virtual void SetPlatformExitParameters(TerrainCastHit hit)
        {
            if (CollidingBoolHash != 0)
                Animator.SetBool(CollidingBoolHash, false);
        }

        protected virtual void SetPlayerPlatformExitParameters(TerrainCastHit hit)
        {
            if (PlayerCollidingBoolHash != 0)
                hit.Controller.Animator.SetBool(PlayerCollidingBoolHash, false);
        }

        public void NotifySurfaceEnter(SurfaceCollision info)
        {
            OnSurfaceEnter(info);
            info.Controller.NotifySurfaceEnter(this);
            SetAnimatorParameters(info.Latest.HitData, SetSurfaceEnterParameters, SetPlayerSurfaceEnterParameters);
        }

        protected virtual void SetSurfaceEnterParameters(TerrainCastHit hit)
        {
            if (SurfaceTriggerHash != 0)
                Animator.SetTrigger(SurfaceTriggerHash);

            if (SurfaceBoolHash != 0)
                Animator.SetBool(SurfaceBoolHash, true);
        }

        protected virtual void SetPlayerSurfaceEnterParameters(TerrainCastHit hit)
        {
            if (PlayerSurfaceTriggerHash != 0)
                hit.Controller.Animator.SetTrigger(PlayerSurfaceTriggerHash);

            if (PlayerSurfaceBoolHash != 0)
                hit.Controller.Animator.SetBool(PlayerSurfaceBoolHash, true);
        }

        public void NotifySurfaceStay(SurfaceCollision info)
        {
            OnSurfaceStay(info);
        }

        public void NotifySurfaceExit(SurfaceCollision info)
        {
            OnSurfaceExit(info);
            info.Controller.NotifySurfaceExit(this);
            SetAnimatorParameters(info.Latest.HitData, SetSurfaceExitParameters, SetPlayerSurfaceExitParameters);
        }

        protected virtual void SetSurfaceExitParameters(TerrainCastHit hit)
        {
            if (SurfaceBoolHash != 0)
                Animator.SetBool(SurfaceBoolHash, false);
        }

        protected virtual void SetPlayerSurfaceExitParameters(TerrainCastHit hit)
        {
            if (PlayerSurfaceBoolHash != 0)
                hit.Controller.Animator.SetBool(PlayerSurfaceBoolHash, false);
        }

        private void SetAnimatorParameters(TerrainCastHit hit,
            Action<TerrainCastHit> setter, Action<TerrainCastHit> playerSetter)
        {
            if (Animator != null)
                setter(hit);

            var controller = hit.Controller;
            if (controller.Animator == null)
                return;

            var logWarnings = controller.Animator.logWarnings;
            controller.Animator.logWarnings = false;

            playerSetter(hit);

            controller.Animator.logWarnings = logWarnings;
        }
        #endregion

        public virtual void OnDestroy()
        {
            foreach (var contacts in PlatformTrigger.CurrentSurfaceContacts)
                NotifySurfaceExit(new SurfaceCollision(contacts.Value));

            foreach (var contacts in PlatformTrigger.CurrentPlatformContacts)
                NotifyPlatformExit(new PlatformCollision(contacts.Value));
        }
    }
}
