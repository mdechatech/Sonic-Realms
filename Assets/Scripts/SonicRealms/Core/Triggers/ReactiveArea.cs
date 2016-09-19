using System;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Helper class for creating areas that react to player events.
    /// </summary>
    [RequireComponent(typeof(AreaTrigger))]
    public class ReactiveArea : BaseReactive
    {
        [HideInInspector]
        public AreaTrigger AreaTrigger;

        protected bool RegisteredEvents;

        /// <summary>
        /// Name of an Animator trigger to set when a player enters the area.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator trigger to set when a player enters the area.")]
        public string InsideTrigger;
        protected int InsideTriggerHash;

        /// <summary>
        /// Name of an Animator bool to set to true while a player is inside the area.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator bool to set to true while a player is inside the area.")]
        public string InsideBool;
        protected int InsideBoolHash;

        /// <summary>
        /// Name of an Animator trigger on a player to set when it enters the area.
        /// </summary>
        [Foldout("Player Animation")]
        [Tooltip("Name of an Animator trigger on the controller to set when it enters the area.")]
        public string PlayerInsideTrigger;
        protected int PlayerInsideTriggerHash;

        /// <summary>
        /// Name of an Animator bool on a player to set to true while it's inside the area.
        /// </summary>
        [Foldout("Player Animation")]
        [Tooltip("Name of an Animator bool on the controller to set to true while it's inside the area.")]
        public string PlayerInsideBool;
        protected int PlayerInsideBoolHash;

        public override void Reset()
        {
            base.Reset();

            AreaTrigger = GetComponent<AreaTrigger>();
            PlayerInsideTrigger = PlayerInsideBool = "";
        }

        public override void Awake()
        {
            base.Awake();
            AreaTrigger = GetComponent<AreaTrigger>() ?? gameObject.AddComponent<AreaTrigger>();

            InsideTriggerHash = Animator.StringToHash(InsideTrigger);
            InsideBoolHash = Animator.StringToHash(InsideBool);

            PlayerInsideTriggerHash = Animator.StringToHash(PlayerInsideTrigger);
            PlayerInsideBoolHash = Animator.StringToHash(PlayerInsideBool);
        }

        public virtual void OnEnable()
        {
            if (AreaTrigger != null && AreaTrigger.TouchRules != null) Start();
        }

        public override void Start()
        {
            base.Start();

            if (RegisteredEvents) return;
            
            AreaTrigger.TouchRules.Add(CanTouch);
            AreaTrigger.OnAreaEnter.AddListener(NotifyAreaEnter);
            AreaTrigger.OnAreaStay.AddListener(NotifyAreaStay);
            AreaTrigger.OnAreaExit.AddListener(NotifyAreaExit);

            RegisteredEvents = true;
        }

        public virtual void OnDisable()
        {
            if (!RegisteredEvents) return;

            AreaTrigger.TouchRules.Remove(CanTouch);
            AreaTrigger.OnAreaEnter.RemoveListener(NotifyAreaEnter);
            AreaTrigger.OnAreaStay.RemoveListener(NotifyAreaStay);
            AreaTrigger.OnAreaExit.RemoveListener(NotifyAreaExit);

            RegisteredEvents = false;
        }

        public virtual bool CanTouch(AreaCollision.Contact contact)
        {
            return AreaTrigger.DefaultInsideRule(contact);
        }
        
        public virtual void OnAreaEnter(AreaCollision collision)
        {
            
        }

        public virtual void OnAreaStay(AreaCollision collision)
        {
            
        }

        public virtual void OnAreaExit(AreaCollision collision)
        {
            
        }
        #region Notify Methods
        public void NotifyAreaEnter(AreaCollision collision)
        {
            OnAreaEnter(collision);
            collision.Controller.NotifyAreaEnter(this);
            SetAnimatorParameters(collision.Latest.Hitbox, SetAreaEnterParameters, SetPlayerAreaEnterParameters);
        }

        protected virtual void SetAreaEnterParameters(Hitbox hitbox)
        {
            if (InsideTriggerHash != 0)
                Animator.SetTrigger(InsideTriggerHash);

            if (InsideBoolHash != 0)
                Animator.SetBool(InsideBoolHash, true);
        }

        protected virtual void SetPlayerAreaEnterParameters(Hitbox hitbox)
        {
            if (PlayerInsideTriggerHash != 0)
                hitbox.Controller.Animator.SetTrigger(PlayerInsideTriggerHash);

            if (PlayerInsideBoolHash != 0)
                hitbox.Controller.Animator.SetBool(PlayerInsideBoolHash, true);
        }

        public void NotifyAreaStay(AreaCollision collision)
        {
            // here for consistency, may add something later
            OnAreaStay(collision);
        }

        public void NotifyAreaExit(AreaCollision collision)
        {
            collision.Controller.NotifyAreaExit(this);
            OnAreaExit(collision);
            SetAnimatorParameters(collision.Latest.Hitbox, SetAreaExitParameters, SetPlayerAreaExitParameters);
        }

        protected virtual void SetAreaExitParameters(Hitbox hitbox)
        {
            if (InsideBoolHash != 0)
                Animator.SetBool(InsideBoolHash, false);
        }

        protected virtual void SetPlayerAreaExitParameters(Hitbox hitbox)
        {
            if (PlayerInsideBoolHash != 0)
                hitbox.Controller.Animator.SetBool(PlayerInsideBoolHash, false);
        }

        private void SetAnimatorParameters(Hitbox hitbox,
            Action<Hitbox> setter, Action<Hitbox> playerSetter)
        {
            if (Animator != null)
                setter(hitbox);

            var controller = hitbox.Controller;

            if (controller.Animator == null)
                return;

            var logWarnings = controller.Animator.logWarnings;
            controller.Animator.logWarnings = false;

            playerSetter(hitbox);

            controller.Animator.logWarnings = logWarnings;
        }
        #endregion

        private bool _isQuitting;
        protected virtual void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (_isQuitting)
                return;

            foreach (var pair in AreaTrigger.CurrentContacts)
            {
                if(pair.Value.Count > 0)
                    NotifyAreaExit(new AreaCollision(pair.Value));
            }
        }
    }
}
