using System;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Internal;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Sets animation parameters based on area trigger events.
    /// </summary>
    public class AnimateAreaTrigger : ReactiveArea
    {
        #region Animation
        [SrFoldout("Animation")]
        public Animator Animator;

        /// <summary>
        /// Name of an Animator trigger to set when a player enters the area.
        /// </summary>
        [SrFoldout("Animation")]
        [Tooltip("Name of an Animator trigger to set when a player enters the area.")]
        public string InsideTrigger;
        protected int InsideTriggerHash;

        /// <summary>
        /// Name of an Animator bool to set to true while a player is inside the area.
        /// </summary>
        [SrFoldout("Animation")]
        [Tooltip("Name of an Animator bool to set to true while a player is inside the area.")]
        public string InsideBool;
        protected int InsideBoolHash;
        #endregion
        #region Player Animation
        /// <summary>
        /// Name of an Animator trigger on a player to set when it enters the area.
        /// </summary>
        [SrFoldout("Player Animation")]
        [Tooltip("Name of an Animator trigger on the controller to set when it enters the area.")]
        public string PlayerInsideTrigger;
        protected int PlayerInsideTriggerHash;

        /// <summary>
        /// Name of an Animator bool on a player to set to true while it's inside the area.
        /// </summary>
        [SrFoldout("Player Animation")]
        [Tooltip("Name of an Animator bool on the controller to set to true while it's inside the area.")]
        public string PlayerInsideBool;
        protected int PlayerInsideBoolHash;
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

            InsideTriggerHash = Animator.StringToHash(InsideTrigger);
            InsideBoolHash = Animator.StringToHash(InsideBool);

            PlayerInsideTriggerHash = Animator.StringToHash(PlayerInsideTrigger);
            PlayerInsideBoolHash = Animator.StringToHash(PlayerInsideBool);
        }

        public override void OnAreaEnter(AreaCollision collision)
        {
            SetAnimatorParameters(collision.Latest.Hitbox, SetAreaEnterParameters, SetPlayerAreaEnterParameters);
        }

        public override void OnAreaExit(AreaCollision collision)
        {
            SetAnimatorParameters(collision.Latest.Hitbox, SetAreaExitParameters, SetPlayerAreaExitParameters);
        }

        protected void SetAreaEnterParameters(Hitbox hitbox)
        {
            if (InsideTriggerHash != 0)
                Animator.SetTrigger(InsideTriggerHash);

            if (InsideBoolHash != 0)
                Animator.SetBool(InsideBoolHash, true);
        }

        protected void SetPlayerAreaEnterParameters(Hitbox hitbox)
        {
            if (PlayerInsideTriggerHash != 0)
                hitbox.Controller.Animator.SetTrigger(PlayerInsideTriggerHash);

            if (PlayerInsideBoolHash != 0)
                hitbox.Controller.Animator.SetBool(PlayerInsideBoolHash, true);
        }

        protected void SetAreaExitParameters(Hitbox hitbox)
        {
            if (InsideBoolHash != 0)
                Animator.SetBool(InsideBoolHash, false);
        }

        protected void SetPlayerAreaExitParameters(Hitbox hitbox)
        {
            if (PlayerInsideBoolHash != 0)
                hitbox.Controller.Animator.SetBool(PlayerInsideBoolHash, false);
        }

        protected void SetAnimatorParameters(Hitbox hitbox, Action<Hitbox> setter, Action<Hitbox> playerSetter)
        {
            if (Animator != null && setter != null)
                setter(hitbox);

            var controller = hitbox.Controller;

            if (controller.Animator == null || playerSetter == null)
                return;

            var logWarnings = controller.Animator.logWarnings;
            controller.Animator.logWarnings = false;

            playerSetter(hitbox);

            controller.Animator.logWarnings = logWarnings;
        }
    }
}
