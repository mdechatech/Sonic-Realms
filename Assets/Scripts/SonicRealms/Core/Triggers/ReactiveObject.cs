using System;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Helper class for creating objects that react to activation.
    /// </summary>
    [RequireComponent(typeof(ObjectTrigger))]
    public class ReactiveObject : BaseReactive
    {
        protected bool RegisteredEvents;

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
                 "(check ObjectTrigger's tooltips to see the difference)")]
        public string PlayerActivatorBool;
        protected int PlayerActivatorBoolHash;

        public override void Awake()
        {
            base.Awake();
            ObjectTrigger = GetComponent<ObjectTrigger>();

            ActivateTriggerHash = Animator.StringToHash(ActivateTrigger);
            ActivateBoolHash = Animator.StringToHash(ActivateBool);
            ActivatorTriggerHash = Animator.StringToHash(ActivatorTrigger);

            PlayerActivateTriggerHash = Animator.StringToHash(PlayerActivateTrigger);
            PlayerActivateBoolHash = Animator.StringToHash(PlayerActivateBool);
            PlayerActivatorTriggerHash = Animator.StringToHash(PlayerActivatorTrigger);
            PlayerActivatorBoolHash = Animator.StringToHash(PlayerActivatorBool);
        }

        public virtual void OnEnable()
        {
            if (ObjectTrigger.OnActivate != null) Start();
        }

        public override void Start()
        {
            base.Start();

            if (RegisteredEvents) return;

            ObjectTrigger.OnActivate.AddListener(NotifyActivate);
            ObjectTrigger.OnActivateStay.AddListener(NotifyActivateStay);
            ObjectTrigger.OnDeactivate.AddListener(NotifyDeactivate);
            ObjectTrigger.OnActivatorEnter.AddListener(NotifyActivatorEnter);
            ObjectTrigger.OnActivatorStay.AddListener(NotifyActivatorStay);
            ObjectTrigger.OnActivatorExit.AddListener(NotifyActivatorExit);

            RegisteredEvents = true;
        }

        public virtual void OnDisable()
        {
            if (!RegisteredEvents) return;

            ObjectTrigger.OnActivate.RemoveListener(NotifyActivate);
            ObjectTrigger.OnActivateStay.RemoveListener(NotifyActivateStay);
            ObjectTrigger.OnDeactivate.RemoveListener(NotifyDeactivate);
            ObjectTrigger.OnActivatorEnter.RemoveListener(NotifyActivatorEnter);
            ObjectTrigger.OnActivatorStay.RemoveListener(NotifyActivatorStay);
            ObjectTrigger.OnActivatorExit.RemoveListener(NotifyActivatorEnter);

            RegisteredEvents = false;
        }

        public virtual void OnActivate(HedgehogController controller)
        {
            
        }

        public virtual void OnActivateStay(HedgehogController controller)
        {
            
        }

        public virtual void OnDeactivate(HedgehogController controller)
        {
            
        }

        public virtual void OnActivatorEnter(HedgehogController controller)
        {

        }

        public virtual void OnActivatorStay(HedgehogController controller)
        {

        }

        public virtual void OnActivatorExit(HedgehogController controller)
        {

        }
        #region Notify Methods
        public void NotifyActivate(HedgehogController controller)
        {
            OnActivate(controller);
            SetAnimatorParameters(controller, SetActivateParameters, SetPlayerActivateParameters);
        }

        protected virtual void SetActivateParameters(HedgehogController controller)
        {
            if (ActivateTriggerHash != 0)
                Animator.SetTrigger(ActivateTriggerHash);

            if (ActivateBoolHash != 0)
                Animator.SetBool(ActivateBoolHash, true);
        }

        protected virtual void SetPlayerActivateParameters(HedgehogController controller)
        {
            if (PlayerActivateTriggerHash != 0)
                controller.Animator.SetTrigger(PlayerActivateTriggerHash);

            if (PlayerActivateBoolHash != 0)
                controller.Animator.SetBool(PlayerActivateBoolHash, true);
        }

        public void NotifyActivateStay(HedgehogController controller)
        {
            OnActivateStay(controller);
        }

        public void NotifyDeactivate(HedgehogController controller)
        {
            OnDeactivate(controller);
            SetAnimatorParameters(controller, SetDeactivateParameters, SetPlayerDeactivateParameters);
        }

        protected virtual void SetDeactivateParameters(HedgehogController controller)
        {
            if (ActivateBoolHash != 0)
                Animator.SetBool(ActivateBoolHash, false);
        }

        protected virtual void SetPlayerDeactivateParameters(HedgehogController controller)
        {
            if (PlayerActivateBoolHash != 0)
                controller.Animator.SetBool(PlayerActivateBoolHash, false);
        }

        public void NotifyActivatorEnter(HedgehogController controller)
        {
            OnActivatorEnter(controller);
            controller.NotifyActivateObject(this);
            SetAnimatorParameters(controller, SetActivatorEnterParameters, SetPlayerActivatorEnterParameters);
        }

        protected virtual void SetActivatorEnterParameters(HedgehogController controller)
        {
            if (ActivatorTriggerHash != 0)
                Animator.SetTrigger(ActivatorTriggerHash);
        }

        protected virtual void SetPlayerActivatorEnterParameters(HedgehogController controller)
        {
            if (PlayerActivatorTriggerHash != 0)
                controller.Animator.SetTrigger(PlayerActivatorTriggerHash);

            if (PlayerActivatorBoolHash != 0)
                controller.Animator.SetBool(PlayerActivateBoolHash, true);
        }

        public void NotifyActivatorStay(HedgehogController controller)
        {
            // here for consistency, may add something later
            OnActivatorStay(controller);
        }

        public void NotifyActivatorExit(HedgehogController controller)
        {
            controller.NotifyDeactivateObject(this);
            OnActivatorExit(controller);
            SetAnimatorParameters(controller, SetActivatorExitParameters, SetPlayerActivatorExitParameters);
        }

        protected virtual void SetActivatorExitParameters(HedgehogController controller)
        {
            // Nothing here
        }

        protected virtual void SetPlayerActivatorExitParameters(HedgehogController controller)
        {
            if (PlayerActivatorBoolHash != 0)
                controller.Animator.SetBool(PlayerActivateBoolHash, false);
        }

        private void SetAnimatorParameters(HedgehogController controller,
            Action<HedgehogController> setter, Action<HedgehogController> playerSetter)
        {
            if (Animator != null)
                setter(controller);

            if (controller.Animator == null)
                return;

            var logWarnings = controller.Animator.logWarnings;
            controller.Animator.logWarnings = false;

            playerSetter(controller);

            controller.Animator.logWarnings = logWarnings;
        }
        #endregion

        public virtual void OnDestroy()
        {
            foreach (var controller in ObjectTrigger.Activators)
                NotifyDeactivate(controller);
        }
    }
}
