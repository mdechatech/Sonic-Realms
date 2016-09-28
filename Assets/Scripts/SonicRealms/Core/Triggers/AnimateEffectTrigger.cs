using System;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Sets animation parameters based on effect trigger events.
    /// </summary>
    public class AnimateEffectTrigger : ReactiveEffect
    {
        #region Animation
        [Foldout("Animation")]
        public Animator Animator;

        /// <summary>
        /// Name of an Animator trigger to set when the object is activated.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator trigger to set when the object is activated.")]
        public string ActivateTrigger;
        protected int ActivateTriggerHash;

        /// <summary>
        /// Name of an Animator bool to set to true while the object is activated.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator bool to set to true while the object is activated.")]
        public string ActivateBool;
        protected int ActivateBoolHash;

        /// <summary>
        /// Name of an Animator trigger to set when an activator enters the object.
        /// </summary>
        [Foldout("Animation")]
        public string ActivatorTrigger;
        protected int ActivatorTriggerHash;

        // No need for ActivatorBool as it would be the same as ActivateBool
        #endregion
        #region Player Animation
        /// <summary>
        /// Name of an Animator trigger on a player to set when they activate the object.
        /// </summary>
        [Foldout("Player Animation")]
        [Tooltip("Name of an Animator trigger on a player to set when they activate the object.")]
        public string PlayerActivateTrigger;
        protected int PlayerActivateTriggerHash;

        /// <summary>
        /// Name of an Animator bool on a player to set while they are activating the object.
        /// </summary>
        [Foldout("Player Animation")]
        [Tooltip("Name of an Animator bool on a player to set while they are activating the object.")]
        public string PlayerActivateBool;
        protected int PlayerActivateBoolHash;

        /// <summary>
        /// Name of an Animator trigger on a player to set when they become the activator of the object
        /// (check ObjectTrigger's comments to see the difference)
        /// </summary>
        [Foldout("Player Animation")]
        [Tooltip("Name of an Animator trigger on a player to set when they become the activator of the object " +
                 "(check ObjectTrigger's tooltips to see the difference")]
        public string PlayerActivatorTrigger;
        protected int PlayerActivatorTriggerHash;

        /// <summary>
        /// Name of an Animator bool on a player to set to true while they are the activator of the object
        /// (check ObjectTrigger's comments to see the difference)
        /// </summary>
        [Foldout("Player Animation")]
        [Tooltip("Name of an Animator bool on a player to set to true while they are the activator of the object " +
                 "(check EffectTrigger's tooltips to see the difference)")]
        public string PlayerActivatorBool;
        protected int PlayerActivatorBoolHash;
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

            ActivateTriggerHash = Animator.StringToHash(ActivateTrigger);
            ActivateBoolHash = Animator.StringToHash(ActivateBool);
            ActivatorTriggerHash = Animator.StringToHash(ActivatorTrigger);

            PlayerActivateTriggerHash = Animator.StringToHash(PlayerActivateTrigger);
            PlayerActivateBoolHash = Animator.StringToHash(PlayerActivateBool);
            PlayerActivatorTriggerHash = Animator.StringToHash(PlayerActivatorTrigger);
            PlayerActivatorBoolHash = Animator.StringToHash(PlayerActivatorBool);
        }

        public override void OnActivate(HedgehogController controller)
        {
            SetAnimatorParameters(controller, SetActivateParameters, SetPlayerActivateParameters);
        }

        public override void OnDeactivate(HedgehogController controller)
        {
            SetAnimatorParameters(controller, SetDeactivateParameters, SetPlayerDeactivateParameters);
        }

        public override void OnActivatorEnter(HedgehogController controller)
        {
            SetAnimatorParameters(controller, SetActivatorEnterParameters, SetPlayerActivatorEnterParameters);
        }

        public override void OnActivatorExit(HedgehogController controller)
        {
            SetAnimatorParameters(controller, SetActivatorExitParameters, SetPlayerActivatorExitParameters);
        }

        protected void SetActivateParameters(HedgehogController controller)
        {
            if (ActivateTriggerHash != 0)
                Animator.SetTrigger(ActivateTriggerHash);

            if (ActivateBoolHash != 0)
                Animator.SetBool(ActivateBoolHash, true);
        }

        protected void SetPlayerActivateParameters(HedgehogController controller)
        {
            if (PlayerActivateTriggerHash != 0)
                controller.Animator.SetTrigger(PlayerActivateTriggerHash);

            if (PlayerActivateBoolHash != 0)
                controller.Animator.SetBool(PlayerActivateBoolHash, true);
        }

        protected void SetDeactivateParameters(HedgehogController controller)
        {
            if (ActivateBoolHash != 0)
                Animator.SetBool(ActivateBoolHash, false);
        }

        protected void SetPlayerDeactivateParameters(HedgehogController controller)
        {
            if (PlayerActivateBoolHash != 0)
                controller.Animator.SetBool(PlayerActivateBoolHash, false);
        }

        protected void SetActivatorEnterParameters(HedgehogController controller)
        {
            if (ActivatorTriggerHash != 0)
                Animator.SetTrigger(ActivatorTriggerHash);
        }

        protected void SetPlayerActivatorEnterParameters(HedgehogController controller)
        {
            if (PlayerActivatorTriggerHash != 0)
                controller.Animator.SetTrigger(PlayerActivatorTriggerHash);

            if (PlayerActivatorBoolHash != 0)
                controller.Animator.SetBool(PlayerActivateBoolHash, true);
        }

        protected void SetActivatorExitParameters(HedgehogController controller)
        {
            // Nothing here
        }

        protected void SetPlayerActivatorExitParameters(HedgehogController controller)
        {
            if (PlayerActivatorBoolHash != 0)
                controller.Animator.SetBool(PlayerActivateBoolHash, false);
        }

        protected void SetAnimatorParameters(HedgehogController controller,
            Action<HedgehogController> setter, Action<HedgehogController> playerSetter)
        {
            if (Animator != null && setter != null)
                setter(controller);

            if (controller.Animator == null || playerSetter == null)
                return;

            var logWarnings = controller.Animator.logWarnings;
            controller.Animator.logWarnings = false;

            playerSetter(controller);

            controller.Animator.logWarnings = logWarnings;
        }
    }
}
