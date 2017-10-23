using System;
using SonicRealms.Core.Internal;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Sets animation parameters based on platform trigger events.
    /// </summary>
    public class AnimatePlatformTrigger : ReactivePlatform
    {
        #region Animation
        [SrFoldout("Animation")]
        public Animator Animator;

        /// <summary>
        /// Name of an Animator trigger to set when a player collides with the platform.
        /// </summary>
        [SrFoldout("Animation")]
        [Tooltip("Name of an Animator trigger to set when a player collides with the platform.")]
        public string CollidingTrigger;
        protected int CollidingTriggerHash;

        /// <summary>
        /// Name of an Animator bool to set to true while a player is colliding with the platform.
        /// </summary>
        [SrFoldout("Animation")]
        [Tooltip("Name of an Animator bool to set to true while a player is colliding with the platform.")]
        public string CollidingBool;
        protected int CollidingBoolHash;

        /// <summary>
        /// Name of an Animator trigger to set when a player stands on the platform.
        /// </summary>
        [SrFoldout("Animation")]
        [Tooltip("Name of an Animator trigger to set when a player stands on the platform.")]
        public string SurfaceTrigger;
        protected int SurfaceTriggerHash;

        /// <summary>
        /// Name of an Animator bool to set to true while a player is on the platform.
        /// </summary>
        [SrFoldout("Animation")]
        [Tooltip("Name of an Animator bool to set to true while a player is on the platform.")]
        public string SurfaceBool;
        protected int SurfaceBoolHash;
        #endregion
        #region Player Animation
        /// <summary>
        /// Name of an Animator trigger on the player to set when it collides with the platform.
        /// </summary>
        [SrFoldout("Player Animation")]
        [Tooltip("Name of an Animator trigger on the player to set when it collides with the platform.")]
        public string PlayerCollidingTrigger;
        protected int PlayerCollidingTriggerHash;

        /// <summary>
        /// Name of an Animator bool on the player to set to true while it's colliding with the platform.
        /// </summary>
        [SrFoldout("Player Animation")]
        [Tooltip("Name of an Animator bool on the player to set to true while it's colliding with the platform.")]
        public string PlayerCollidingBool;
        protected int PlayerCollidingBoolHash;

        /// <summary>
        /// Name of an Animator trigger on the player to set when it stands on the platform.
        /// </summary>
        [SrFoldout("Player Animation")]
        [Tooltip("Name of an Animator trigger on the player to set when it stands on the platform.")]
        public string PlayerSurfaceTrigger;
        protected int PlayerSurfaceTriggerHash;

        /// <summary>
        /// Name of an Animator bool on the player to set to true while it's on the platform.
        /// </summary>
        [SrFoldout("Player Animation")]
        [Tooltip("Name of an Animator bool on the player to set to true while it's on the platform.")]
        public string PlayerSurfaceBool;
        protected int PlayerSurfaceBoolHash;
        #endregion

        public override void Reset()
        {
            base.Reset();

            Animator = GetComponent<Animator>();
        }

        public override void Awake()
        {
            base.Awake();

            Animator = Animator ?? GetComponent<Animator>();

            CollidingTriggerHash = Animator.StringToHash(CollidingTrigger);
            CollidingBoolHash = Animator.StringToHash(CollidingBool);
            SurfaceTriggerHash = Animator.StringToHash(SurfaceTrigger);
            SurfaceBoolHash = Animator.StringToHash(SurfaceBool);

            PlayerCollidingTriggerHash = Animator.StringToHash(PlayerCollidingTrigger);
            PlayerCollidingBoolHash = Animator.StringToHash(PlayerCollidingBool);
            PlayerSurfaceTriggerHash = Animator.StringToHash(PlayerSurfaceTrigger);
            PlayerSurfaceBoolHash = Animator.StringToHash(PlayerSurfaceBool);
        }

        public override void OnPlatformEnter(PlatformCollision collision)
        {
            SetAnimatorParameters(collision.Latest.HitData, SetPlatformEnterParameters, SetPlayerPlatformEnterParameters);
        }

        public override void OnPlatformExit(PlatformCollision collision)
        {
            SetAnimatorParameters(collision.Latest.HitData, SetPlatformExitParameters, SetPlayerPlatformExitParameters);
        }

        public override void OnSurfaceEnter(SurfaceCollision collision)
        {
            SetAnimatorParameters(collision.Latest.HitData, SetSurfaceEnterParameters, SetPlayerSurfaceEnterParameters);
        }

        public override void OnSurfaceExit(SurfaceCollision collision)
        {
            SetAnimatorParameters(collision.Latest.HitData, SetSurfaceExitParameters, SetPlayerSurfaceExitParameters);
        }

        protected void SetPlatformEnterParameters(TerrainCastHit hit)
        {
            if (CollidingTriggerHash != 0)
                Animator.SetTrigger(CollidingTriggerHash);

            if (CollidingBoolHash != 0)
                Animator.SetBool(CollidingBool, true);
        }

        protected void SetPlayerPlatformEnterParameters(TerrainCastHit hit)
        {
            if (PlayerCollidingTriggerHash != 0)
                hit.Controller.Animator.SetTrigger(PlayerCollidingTriggerHash);

            if (PlayerCollidingBoolHash != 0)
                hit.Controller.Animator.SetBool(PlayerCollidingBoolHash, true);
        }

        protected void SetPlatformExitParameters(TerrainCastHit hit)
        {
            if (CollidingBoolHash != 0)
                Animator.SetBool(CollidingBoolHash, false);
        }

        protected void SetPlayerPlatformExitParameters(TerrainCastHit hit)
        {
            if (PlayerCollidingBoolHash != 0)
                hit.Controller.Animator.SetBool(PlayerCollidingBoolHash, false);
        }

        protected void SetSurfaceEnterParameters(TerrainCastHit hit)
        {
            if (SurfaceTriggerHash != 0)
                Animator.SetTrigger(SurfaceTriggerHash);

            if (SurfaceBoolHash != 0)
                Animator.SetBool(SurfaceBoolHash, true);
        }

        protected void SetPlayerSurfaceEnterParameters(TerrainCastHit hit)
        {
            if (PlayerSurfaceTriggerHash != 0)
                hit.Controller.Animator.SetTrigger(PlayerSurfaceTriggerHash);

            if (PlayerSurfaceBoolHash != 0)
                hit.Controller.Animator.SetBool(PlayerSurfaceBoolHash, true);
        }

        protected void SetSurfaceExitParameters(TerrainCastHit hit)
        {
            if (SurfaceBoolHash != 0)
                Animator.SetBool(SurfaceBoolHash, false);
        }

        protected void SetPlayerSurfaceExitParameters(TerrainCastHit hit)
        {
            if (PlayerSurfaceBoolHash != 0)
                hit.Controller.Animator.SetBool(PlayerSurfaceBoolHash, false);
        }

        protected void SetAnimatorParameters(TerrainCastHit hit, Action<TerrainCastHit> setter,
            Action<TerrainCastHit> playerSetter)
        {
            if (Animator != null && setter != null)
                setter(hit);

            SetPlayerAnimatorParameters(hit, playerSetter);
        }
    }
}
