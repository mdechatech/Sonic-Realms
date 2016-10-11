using System;
using System.Collections.Generic;
using System.Linq;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Utils;
using SonicRealms.Level;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Hook up to these events to react when a controller lands on the object.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlatformTrigger : BaseTrigger
    {
        #region Events
        /// <summary>
        /// Called right before a controller is going to collide with the platform. Listeners of this event
        /// may call IgnoreThisCollision() on the controller to prevent the collision from occurring.
        /// </summary>
        [Foldout("Platform Events")]
        public PlatformTriggerPreCollisionEvent OnPreCollide;

        /// <summary>
        /// Called when a controller begins colliding with the platform.
        /// </summary>
        [Foldout("Platform Events")]
        public PlatformTriggerCollisionEvent OnPlatformEnter;

        /// <summary>
        /// Called while a controller is colliding with the platform.
        /// </summary>
        [Foldout("Platform Events")]
        public PlatformTriggerCollisionEvent OnPlatformStay;

        /// <summary>
        /// Called when a controller stops colliding with the platform.
        /// </summary>
        [Foldout("Platform Events")]
        public PlatformTriggerCollisionEvent OnPlatformExit;

        /// <summary>
        /// Called when a controller lands on the surface of the platform.
        /// </summary>
        [Foldout("Surface Events")]
        public PlatformTriggerSurfaceEvent OnSurfaceEnter;

        /// <summary>
        /// Called while a controller is on the surface of the platform.
        /// </summary>
        [Foldout("Surface Events")]
        public PlatformTriggerSurfaceEvent OnSurfaceStay;

        /// <summary>
        /// Called when a controller exits the surface of the platform.
        /// </summary>
        [Foldout("Surface Events")]
        public PlatformTriggerSurfaceEvent OnSurfaceExit;
        #endregion
        /// <summary>
        /// A list of controllers currently colliding with the platform.
        /// </summary>
        [Foldout("Debug")]
        public List<HedgehogController> ControllersOnPlatform;

        /// <summary>
        /// A list of controllers currently on the surface of the platform.
        /// </summary>
        [Foldout("Debug")]
        public List<HedgehogController> ControllersOnSurface;

        /// <summary>
        /// Returns whether the platform should be solid based on the given terrain cast.
        /// </summary>
        public delegate bool SolidityPredicate(TerrainCastHit data);

        /// <summary>
        /// A list of predicates which, if empty or all return true, allows terrain casts to hit the platform.
        /// </summary>
        public List<SolidityPredicate> SolidityRules;

        /// <summary>
        /// Returns whether the controller is considered to be on the surface based on the given contact data.
        /// </summary>
        /// <returns></returns>
        public delegate bool SurfacePredicate(SurfaceCollision.Contact hit);

        /// <summary>
        /// A list of predicates which, if empty or all return true, allows the trigger to invoke surface events.
        /// </summary>
        public List<SurfacePredicate> SurfaceRules;
        
        protected List<PlatformTrigger> Parents;

        public Dictionary<HedgehogController, List<PlatformCollision.Contact>> CurrentPlatformContacts;
        private Dictionary<HedgehogController, List<PlatformCollision.Contact>> _pendingPlatformContacts;

        public Dictionary<HedgehogController, List<SurfaceCollision.Contact>> CurrentSurfaceContacts;
        private Dictionary<HedgehogController, List<SurfaceCollision.Contact>> _pendingSurfaceContacts;

        public override void Awake()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            base.Awake();

            OnPreCollide = OnPreCollide ?? new PlatformTriggerPreCollisionEvent();

            OnPlatformEnter = OnPlatformEnter ?? new PlatformTriggerCollisionEvent();
            OnPlatformStay = OnPlatformStay ?? new PlatformTriggerCollisionEvent();
            OnPlatformExit = OnPlatformExit ?? new PlatformTriggerCollisionEvent();

            OnSurfaceEnter = OnSurfaceEnter ?? new PlatformTriggerSurfaceEvent();
            OnSurfaceStay = OnSurfaceStay ?? new PlatformTriggerSurfaceEvent();
            OnSurfaceExit = OnSurfaceExit ?? new PlatformTriggerSurfaceEvent();

            SolidityRules = new List<SolidityPredicate>();
            SurfaceRules = new List<SurfacePredicate>();

            ControllersOnSurface = new List<HedgehogController>();
            ControllersOnPlatform = new List<HedgehogController>();

            CurrentPlatformContacts = new Dictionary<HedgehogController, List<PlatformCollision.Contact>>();
            _pendingPlatformContacts = new Dictionary<HedgehogController, List<PlatformCollision.Contact>>();

            CurrentSurfaceContacts = new Dictionary<HedgehogController, List<SurfaceCollision.Contact>>();
            _pendingSurfaceContacts = new Dictionary<HedgehogController, List<SurfaceCollision.Contact>>();

            Parents = new List<PlatformTrigger>();
            GetComponentsInParent(true, Parents);

            for (var i = 0; i < Parents.Count; ++i)
            {
                if (Parents[i] == this)
                {
                    Parents.RemoveAt(i);
                    break;
                }
            }
        }

        public void Start()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            if (!TriggerFromChildren)
                return;

            foreach (var child in GetComponentsInChildren<Collider2D>())
            {
                if (((1 << child.gameObject.layer) & CollisionLayers.AllMask) == 0)
                    continue;

                if (child.GetComponent<PlatformTrigger>() || child.GetComponent<AreaTrigger>())
                    continue;

                child.gameObject.AddComponent<PlatformTrigger>().TriggerFromChildren = true;
            }
        }

        public override bool IsAlone
        {
            get
            {
                return !GetComponent<ReactivePlatform>() &&
                       GetComponentsInParent<PlatformTrigger>().All(t => t == this || !t.TriggerFromChildren) &&
                       OnPlatformEnter.GetPersistentEventCount() == 0 &&
                       OnPlatformStay.GetPersistentEventCount() == 0 &&
                       OnPlatformExit.GetPersistentEventCount() == 0 &&
                       OnSurfaceEnter.GetPersistentEventCount() == 0 &&
                       OnSurfaceStay.GetPersistentEventCount() == 0 &&
                       OnSurfaceExit.GetPersistentEventCount() == 0;
            }
        }

        public override bool HasController(HedgehogController controller)
        {
            return HasControllerOnPlatform(controller) ||
                   HasControllerOnSurface(controller);
        }

        public bool HasControllerOnPlatform(HedgehogController controller)
        {
            return CurrentPlatformContacts.ContainsKey(controller);
        }

        public bool HasControllerOnSurface(HedgehogController controller)
        {
            return CurrentSurfaceContacts.ContainsKey(controller);
        }

        #region Collision Rules
        /// <summary>
        /// Whether the controller is on the surface with the given terrain cast. This will not affect
        /// the controller's physics, but rather will not invoke surface events if false.
        /// </summary>
        public bool IsOnSurface(SurfaceCollision.Contact contact)
        {
            for (var i = 0; i < SurfaceRules.Count; ++i)
            {
                if (!SurfaceRules[i](contact))
                    return false;
            }

            for (var i = 0; i < Parents.Count; ++i)
            {
                var parent = Parents[i];
                if (!parent.TriggerFromChildren)
                    continue;

                for (var j = 0; j < parent.SurfaceRules.Count; ++j)
                {
                    if (!parent.SurfaceRules[j](contact))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns whether the platform should be solid against the given terrain cast. This check
        /// shouldn't have any side effects on the platform.
        /// </summary>
        public bool IsSolid(TerrainCastHit hit)
        {
            for (var i = 0; i < SolidityRules.Count; ++i)
            {
                if (!SolidityRules[i](hit))
                    return false;
            }

            for (var i = 0; i < Parents.Count; ++i)
            {
                var parent = Parents[i];
                if (!parent.TriggerFromChildren)
                    continue;

                for (var j = 0; j < parent.SolidityRules.Count; ++j)
                {
                    if (!parent.SolidityRules[j](hit))
                        return false;
                }
            }

            return true;
        }
        #endregion  

        #region Notify Platform Methods
        public void NotifyPreCollision(PlatformCollision.Contact contact, bool bubble = true)
        {
            OnPreCollide.Invoke(contact);

            if (bubble)
                BubblePreCollision(contact);
        }

        private void BubblePreCollision(PlatformCollision.Contact contact)
        {
            for (var i = 0; i < Parents.Count; ++i)
            {
                var parent = Parents[i];
                if (parent.TriggerFromChildren)
                {
                    parent.NotifyPreCollision(contact, false);
                }
            }
        }

        public void NotifyPlatformCollision(PlatformCollision.Contact contact, bool bubble = true)
        {
            var controller = contact.Controller;

            var pending = _pendingPlatformContacts.ContainsKey(controller)
                ? _pendingPlatformContacts[controller]
                : _pendingPlatformContacts[controller] = new List<PlatformCollision.Contact>();

            if (pending.Count == 0 && !CurrentPlatformContacts.ContainsKey(controller) &&
                !ControllersOnSurface.Contains(controller))
            {
                controller.AddCommandBuffer(HandlePlatformCollisions,
                    HedgehogController.BufferEvent.AfterMovement);

                ControllersOnPlatform.Add(controller);
            }

            pending.Add(contact);

            if (bubble)
                BubblePlatformCollision(contact);
        }

        private void BubblePlatformCollision(PlatformCollision.Contact contact)
        {
            for (var i = 0; i < Parents.Count; ++i)
            {
                var parent = Parents[i];
                if (parent.TriggerFromChildren)
                {
                    parent.NotifyPlatformCollision(contact, false);
                }
            }
        }

        private void HandlePlatformCollisions(HedgehogController controller)
        {
            // Get current and pending contacts for the controller
            List<PlatformCollision.Contact> current;
            List<PlatformCollision.Contact> pending;

            CurrentPlatformContacts.TryGetValue(controller, out current);
            _pendingPlatformContacts.TryGetValue(controller, out pending);

            // If there's a pending contact and no current, the controller has just entered the surface
            if (current == null && pending != null)
            {
                _pendingPlatformContacts.Remove(controller);
                CurrentPlatformContacts.Add(controller, pending);

                OnPlatformEnter.Invoke(new PlatformCollision(pending));

                return;
            }

            // If there's a pending and current contact, the controller has stayed on the surface
            if (current != null && pending != null)
            {
                _pendingPlatformContacts.Remove(controller);
                CurrentPlatformContacts[controller] = pending;

                OnPlatformStay.Invoke(new PlatformCollision(pending));

                return;
            }

            // If there's no pending contact but there is a current, the controller has just exited the surface
            if (current != null)
            {
                CurrentPlatformContacts.Remove(controller);

                ControllersOnPlatform.Remove(controller);

                controller.RemoveCommandBuffer(HandlePlatformCollisions,
                    HedgehogController.BufferEvent.AfterMovement);

                OnPlatformExit.Invoke(new PlatformCollision(current));

                return;
            }

            // If there's no pending or current contact, we shouldn't be here
            controller.RemoveCommandBuffer(HandlePlatformCollisions, HedgehogController.BufferEvent.AfterMovement);
        }
        #endregion
        #region Notify Surface Methods
        public void NotifySurfaceCollision(SurfaceCollision.Contact contact, bool bubble = true)
        {
            var controller = contact.Controller;

            var pending = _pendingSurfaceContacts.ContainsKey(controller)
                ? _pendingSurfaceContacts[controller]
                : _pendingSurfaceContacts[controller] = new List<SurfaceCollision.Contact>();

            if (pending.Count == 0 && !CurrentSurfaceContacts.ContainsKey(controller) &&
                !ControllersOnSurface.Contains(controller))
            {
                controller.AddCommandBuffer(HandleSurfaceCollisions,
                    HedgehogController.BufferEvent.AfterMovement);

                ControllersOnSurface.Add(controller);
            }

            pending.Add(contact);
            
            if(bubble)
                BubbleSurfaceCollision(contact);
        }

        private void BubbleSurfaceCollision(SurfaceCollision.Contact contact)
        {
            for (var i = 0; i < Parents.Count; ++i)
            {
                var parent = Parents[i];
                if (parent.TriggerFromChildren)
                {
                    parent.NotifySurfaceCollision(contact, false);
                }
            }
        }

        private void HandleSurfaceCollisions(HedgehogController controller)
        {
            // Get current and pending contacts for the controller
            List<SurfaceCollision.Contact> current;
            List<SurfaceCollision.Contact> pending;

            CurrentSurfaceContacts.TryGetValue(controller, out current);
            _pendingSurfaceContacts.TryGetValue(controller, out pending);

            if (pending != null)
            {
                // Remove pending contacts that don't fulfill surface rules
                for (var j = pending.Count - 1; j >= 0; --j)
                {
                    if (!IsOnSurface(pending[j]))
                        pending.RemoveAt(j);
                }

                if (pending.Count == 0)
                {
                    _pendingSurfaceContacts.Remove(controller);
                    pending = null;
                }
            }

            // If there's a pending contact and no current, the controller has just entered the surface
            if (current == null && pending != null)
            {
                _pendingSurfaceContacts.Remove(controller);
                CurrentSurfaceContacts.Add(controller, pending);

                OnSurfaceEnter.Invoke(new SurfaceCollision(pending));

                return;
            }

            // If there's a pending and current contact, the controller has stayed on the surface
            if (current != null && pending != null)
            {
                _pendingSurfaceContacts.Remove(controller);
                CurrentSurfaceContacts[controller] = pending;

                OnSurfaceStay.Invoke(new SurfaceCollision(pending));

                return;
            }

            // If there's no pending contact but there is a current, the controller has just exited the surface
            if (current != null)
            {
                CurrentSurfaceContacts.Remove(controller);

                ControllersOnSurface.Remove(controller);

                controller.RemoveCommandBuffer(HandleSurfaceCollisions,
                    HedgehogController.BufferEvent.AfterMovement);

                OnSurfaceExit.Invoke(new SurfaceCollision(current));

                return;
            }

            // If there's no pending or current contact, we shouldn't be here
            controller.RemoveCommandBuffer(HandleSurfaceCollisions, HedgehogController.BufferEvent.AfterMovement);
        }
        #endregion

    }
}
