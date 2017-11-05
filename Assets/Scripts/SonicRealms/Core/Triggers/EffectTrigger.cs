using System.Collections.Generic;
using System.Linq;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Internal;
using SonicRealms.Core.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// A generic trigger that gets activated by something external calling Activate().
    /// </summary>
    [DisallowMultipleComponent]
    public class EffectTrigger : BaseTrigger
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
        [SrFoldout("Events")]
        [FormerlySerializedAs("OnActivateEnter")]
        public EffectEvent OnActivate;

        /// <summary>
        /// Invoked while the object is activated. This is invoked only once each FixedUpdate, for the
        /// first player that activated it.
        /// </summary>
        [SrFoldout("Events")]
        public EffectEvent OnActivateStay;

        /// <summary>
        /// Invoked when the object is deactivated. This will not occur if the object still has something
        /// activating it.
        /// </summary>
        [SrFoldout("Events")]
        [FormerlySerializedAs("OnActivateExit")]
        public EffectEvent OnDeactivate;

        /// <summary>
        /// Invoked when a player becomes an activator of this trigger. This is invoked regardless of whether
        /// the player actually caused the trigger to activate, i.e. there are multiple players on the trigger.
        /// </summary>
        [SrFoldout("Events")]
        public EffectEvent OnActivatorEnter;

        /// <summary>
        /// Invoked for every player that's activating the trigger, each FixedUpdate.
        /// </summary>
        [SrFoldout("Events")]
        public EffectEvent OnActivatorStay;

        /// <summary>
        /// Invoked when a player is no longer an activator of this trigger. This is invoked regardless of whether
        /// the player actually caused the trigger to deactivate, i.e. there are multiple players on the trigger.
        /// </summary>
        [SrFoldout("Events")]
        public EffectEvent OnActivatorExit;

        [HideInInspector]
        public bool Activated;

        /// <summary>
        /// A list of things activating the object.
        /// </summary>
        [SrFoldout("Debug")]
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

        [SrFoldout("Debug")] private PlatformTrigger _platformTrigger;
        [SrFoldout("Debug")] private AreaTrigger _areaTrigger;

        public override void Reset()
        {
            base.Reset();

            AllowMultiple = true;
            OnActivate = new EffectEvent();
            OnActivateStay = new EffectEvent();
            OnDeactivate = new EffectEvent();
        }

        public override void Awake()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            base.Awake();

            Activators = new List<HedgehogController>();

            OnActivate = OnActivate ?? new EffectEvent();
            OnActivateStay = OnActivateStay ?? new EffectEvent();
            OnDeactivate = OnDeactivate ?? new EffectEvent();

            OnActivatorEnter = OnActivatorEnter ?? new EffectEvent();
            OnActivatorStay = OnActivatorStay ?? new EffectEvent();
            OnActivatorExit = OnActivatorExit ?? new EffectEvent();

            Activated = false;
        }

        public void FixedUpdate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            if (Activators.Count <= 0)
                return;
            
            if(!AllowMultiple)
                OnActivateStay.Invoke(Activator);

            foreach (var activator in new List<HedgehogController>(Activators))
            {
                if (AllowMultiple)
                {
                    OnActivateStay.Invoke(activator);
                }

                OnActivatorStay.Invoke(activator);
            }
        }

        public override bool IsAlone
        {
            get
            {
                return !GetComponent<ReactiveEffect>() &&
                       GetComponentsInParent<EffectTrigger>().All(t => t == this || !t.TriggerFromChildren) &&
                       (OnActivate == null || OnActivate.GetPersistentEventCount() == 0) &&
                       (OnActivateStay == null || OnActivateStay.GetPersistentEventCount() == 0) &&
                       (OnDeactivate == null || OnDeactivate.GetPersistentEventCount() == 0) &&
                       (OnActivatorEnter == null || OnActivatorEnter.GetPersistentEventCount() == 0) &&
                       (OnActivatorStay == null || OnActivatorStay.GetPersistentEventCount() == 0) &&
                       (OnActivatorExit == null || OnActivatorExit.GetPersistentEventCount() == 0);
            }
        }

        /// <summary>
        /// Activates the object with the ability to specify the controller that activated it, if any.
        /// </summary>
        /// <param name="controller"></param>
        public void Activate(HedgehogController controller = null)
        {
            // Do nothing if disabled
            if (!enabled || !gameObject.activeInHierarchy)
                return;

            // Do nothing if AllowMultiple isn't checked and we aren't the first activator
            if (!AllowMultiple && Activators.Count > 0)
                return;

            if (!Activators.Contains(controller))
            {
                if (AllowMultiple || Activators.Count == 0)
                {
                    OnActivate.Invoke(controller);
                    Activated = Activators.Count > 0;
                }

                Activators.Add(controller);
                OnActivatorEnter.Invoke(controller);
            }

            BubbleEvent(controller);
        }

        /// <summary>
        /// Deactivates the object with the ability to specify the controller that deactivated it, if any.
        /// </summary>
        /// <param name="controller"></param>
        public void Deactivate(HedgehogController controller = null)
        {
            // Do nothing if disabled
            if (!enabled || !gameObject.activeInHierarchy)
                return;

            if (controller != null && Activators.Remove(controller))
            {
                OnActivatorExit.Invoke(controller);
                
                if (AllowMultiple || Activators.Count == 0)
                {
                    OnDeactivate.Invoke(controller);
                    Activated = Activators.Count == 0;
                }
            }

            BubbleEvent(controller, true);
        }

        /// <summary>
        /// Activates and deactivates the object such that OnActivateStay is never called.
        /// </summary>
        /// <param name="controller"></param>
        public void Blink(HedgehogController controller = null)
        {
            Activate(controller);
            Deactivate(controller);
        }

        protected void BubbleEvent(HedgehogController controller = null, bool isExit = false)
        {
            foreach (var trigger in GetComponentsInParent<EffectTrigger>().Where(
                trigger => trigger != this && trigger.TriggerFromChildren))
            {
                if (isExit) trigger.Deactivate(controller);
                else trigger.Activate(controller);
            }
        }

        public override bool HasController(HedgehogController controller)
        {
            return Activators.Contains(controller);
        }

        public static implicit operator bool(EffectTrigger effectTrigger)
        {
            return effectTrigger != null && effectTrigger.Activated;
        }
    }
}
