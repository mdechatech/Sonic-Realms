using System.Collections.Generic;
using System.Linq;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Utils;
using SonicRealms.Level;
using UnityEngine;
using UnityEngine.Serialization;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Hook up to these events to react when the object is activated. Activation must be
    /// performed by other triggers.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Hedgehog/Triggers/Object Trigger")]
    public class ObjectTrigger : BaseTrigger
    {
        /// <summary>
        /// Whether the trigger can be activated if it is already on. If true, OnActivateEnter and
        /// OnActivateExit will be invoked for any number of players that trigger it.
        /// </summary>
        [FormerlySerializedAs("AllowReactivation")]
        [Tooltip("Whether the trigger can be activated again even if it is already on.")]
        public bool AllowMultiple;

        /// <summary>
        /// Invoked when the object is activated. This will not occur if the object is already activated.
        /// </summary>
        [Foldout("Events")]
        [FormerlySerializedAs("OnActivateEnter")]
        public ObjectEvent OnActivate;

        /// <summary>
        /// Invoked while the object is activated. This is invoked only once each FixedUpdate, for the
        /// first player that activated it.
        /// </summary>
        [Foldout("Events")]
        public ObjectEvent OnActivateStay;

        /// <summary>
        /// Invoked when the object is deactivated. This will not occur if the object still has something
        /// activating it.
        /// </summary>
        [Foldout("Events")]
        [FormerlySerializedAs("OnActivateExit")]
        public ObjectEvent OnDeactivate;

        /// <summary>
        /// Invoked when a player becomes an activator of this trigger. This is invoked regardless of whether
        /// the player actually caused the trigger to activate, i.e. there are multiple players on the trigger.
        /// </summary>
        [Foldout("Events")]
        public ObjectEvent OnActivatorEnter;

        /// <summary>
        /// Invoked for every player that's activating the trigger, each FixedUpdate.
        /// </summary>
        [Foldout("Events")]
        public ObjectEvent OnActivatorStay;

        /// <summary>
        /// Invoked when a player is no longer an activator of this trigger. This is invoked regardless of whether
        /// the player actually caused the trigger to deactivate, i.e. there are multiple players on the trigger.
        /// </summary>
        [Foldout("Events")]
        public ObjectEvent OnActivatorExit;

        [HideInInspector]
        public bool Activated;

        #region Animation
        /// <summary>
        /// Reference to the target animator.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Reference to the target animator.")]
        public Animator Animator;

        /// <summary>
        /// Name of an Animator trigger set when the object is activated.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator trigger set when the object is activated.")]
        public string ActivatedTrigger;
        protected int ActivatedTriggerHash;

        /// <summary>
        /// Name of an Animator bool set to whether the object is activated.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator bool set to whether the object is activated.")]
        public string ActivatedBool;
        protected int ActivatedBoolHash;
        #endregion

        #region Sound
        /// <summary>
        /// An audio clip to play when the object trigger is activated.
        /// </summary>
        [Foldout("Sound")]
        [Tooltip("An audio clip to play when the object trigger is activated.")]
        public AudioClip ActivateSound;

        /// <summary>
        /// An audio clip to loop while the object trigger is activated.
        /// </summary>
        [Foldout("Sound")]
        [Tooltip("An audio clip to loop while the object trigger is activated.")]
        public AudioClip LoopSound;

        /// <summary>
        /// An audio clip to play when the object trigger is deactivated.
        /// </summary>
        [Foldout("Sound")]
        [Tooltip("An audio clip to play when the object trigger is deactivated.")]
        public AudioClip DeactivateSound;
        #endregion

        /// <summary>
        /// A list of things activating the object.
        /// </summary>
        [Foldout("Debug")]
        [Tooltip("A list of things activating the object.")]
        public List<HedgehogController> Activators;

        /// <summary>
        /// If activated, the first player than activated it.
        /// </summary>
        public HedgehogController Activator
        {
            get { return Activators.FirstOrDefault(); }
            set { if (!Activators.Contains(value)) Activate(value); }
        }

        [Foldout("Debug")] private PlatformTrigger _platformTrigger;
        [Foldout("Debug")] private AreaTrigger _areaTrigger;

        public override void Reset()
        {
            base.Reset();

            AllowMultiple = true;
            OnActivate = new ObjectEvent();
            OnActivateStay = new ObjectEvent();
            OnDeactivate = new ObjectEvent();

            Animator = GetComponentInChildren<Animator>();
            ActivatedTrigger = "";
            ActivatedBool = "";

            ActivateSound = LoopSound = DeactivateSound = null;
        }

        public override void Awake()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            base.Awake();

            Activators = new List<HedgehogController>();

            OnActivate = OnActivate ?? new ObjectEvent();
            OnActivateStay = OnActivateStay ?? new ObjectEvent();
            OnDeactivate = OnDeactivate ?? new ObjectEvent();

            OnActivatorEnter = OnActivatorEnter ?? new ObjectEvent();
            OnActivatorStay = OnActivatorStay ?? new ObjectEvent();
            OnActivatorExit = OnActivatorExit ?? new ObjectEvent();

            Activated = false;

            Animator = Animator ?? GetComponentInChildren<Animator>();
            if (Animator == null) return;

            ActivatedTriggerHash = string.IsNullOrEmpty(ActivatedTrigger) ? 0 : Animator.StringToHash(ActivatedTrigger);
            ActivatedBoolHash = string.IsNullOrEmpty(ActivatedBool) ? 0 : Animator.StringToHash(ActivatedBool);
        }

        public void FixedUpdate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            if (Animator != null && ActivatedBoolHash != 0)
                Animator.SetBool(ActivatedBoolHash, Activated);

            if (Activators.Count <= 0) return;

            OnActivateStay.Invoke(Activator);

            foreach (var activator in new List<HedgehogController>(Activators))
                OnActivatorStay.Invoke(activator);
        }

        /// <summary>
        /// Activates the object with the ability to specify the controller that activated it, if any.
        /// </summary>
        /// <param name="controller"></param>
        public void Activate(HedgehogController controller = null)
        {
            if (!enabled || !gameObject.activeInHierarchy) return;
            if (Activators.Count > 0 && !AllowMultiple) return;

            if (controller != null && !Activators.Contains(controller))
            {
                Activators.Add(controller);
                OnActivatorEnter.Invoke(controller);
            }

            var any = Activators.Any();
            if (!AllowMultiple && any) return;
            
            Activated = true;

            if (ActivateSound != null)
                SoundManager.Instance.PlayClipAtPoint(ActivateSound, transform.position);

            OnActivate.Invoke(controller);
            BubbleEvent(controller);

            if (Animator != null && ActivatedTriggerHash != 0)
                Animator.SetTrigger(ActivatedTriggerHash);
        }

        /// <summary>
        /// Deactivates the object with the ability to specify the controller that deactivated it, if any.
        /// </summary>
        /// <param name="controller"></param>
        public void Deactivate(HedgehogController controller = null)
        {
            if (controller != null)
            {
                Activators.Remove(controller);
                OnActivatorExit.Invoke(controller);
            }

            var any = Activators.Any();
            if (!AllowMultiple && any) return;

            if(!any) Activated = false;

            if (DeactivateSound != null)
                SoundManager.Instance.PlayClipAtPoint(DeactivateSound, transform.position);

            OnDeactivate.Invoke(controller);
            BubbleEvent(controller, true);
        }

        /// <summary>
        /// Activates and deactivates the object such that OnActivateStay is never called.
        /// </summary>
        /// <param name="controller"></param>
        public void Trigger(HedgehogController controller = null)
        {
            Activate(controller);
            Deactivate(controller);
        }

        protected void BubbleEvent(HedgehogController controller = null, bool isExit = false)
        {
            foreach (var trigger in GetComponentsInParent<ObjectTrigger>().Where(
                trigger => trigger != this && trigger.TriggerFromChildren))
            {
                if (isExit) trigger.Activate(controller);
                else trigger.Deactivate(controller);
            }
        }

        public override bool HasController(HedgehogController controller)
        {
            return Activators.Contains(controller);
        }

        public static implicit operator bool(ObjectTrigger objectTrigger)
        {
            return objectTrigger != null && objectTrigger.Activated;
        }
    }
}
