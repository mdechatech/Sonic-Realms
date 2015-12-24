using System;
using System.Collections.Generic;
using System.Linq;
using Hedgehog.Core.Actors;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Hedgehog.Core.Triggers
{
    /// <summary>
    /// UnityEvents that pass in a controller. Used by OnAreaEnter, OnAreaStay, and OnAreaExit.
    /// </summary>
    [Serializable]
    public class AreaEvent : UnityEvent<HedgehogController> { }

    /// <summary>
    /// Hook up to these events to react when a controller enters the area.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Hedgehog/Triggers/Area Trigger")]
    public class AreaTrigger : BaseTrigger
    {
        /// <summary>
        /// Whether to always collide regardless of a controller's path.
        /// </summary>
        [FormerlySerializedAs("IgnoreLayers")]
        [Tooltip("Whether to always collide regardless of a controller's path.")]
        public bool AlwaysCollide;

        /// <summary>
        /// Invoked when a controller enters the area.
        /// </summary>
        public AreaEvent OnAreaEnter;

        /// <summary>
        /// Invoked when a controller stays in the area.
        /// </summary>
        public AreaEvent OnAreaStay;

        /// <summary>
        /// Invoked when a controller exits the area.
        /// </summary>
        public AreaEvent OnAreaExit;

        /// <summary>
        /// Defines whether the controller should collide with the area. The trigger ONLY checks
        /// if the controller is touching it!
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public delegate bool InsidePredicate(HedgehogController controller);

        /// <summary>
        /// A list of predicates which, if empty or all return true, allow the controller to collide
        /// with the area. The trigger ONLY checks if the controller is touching it!
        /// </summary>
        public List<InsidePredicate> InsideRules;

        /// <summary>
        /// Maps a controller to the areas it is colliding with (can collide with multiple child areas).
        /// </summary>
        protected Dictionary<HedgehogController, List<Transform>> Collisions;

        protected HashSet<Collider2D> MiscCollisions; 

        public override void Reset()
        {
            base.Reset();

            AlwaysCollide = true;
            OnAreaEnter = new AreaEvent();
            OnAreaStay = new AreaEvent();
            OnAreaExit = new AreaEvent();
        }

        public override void Awake()
        {
            base.Awake();

            OnAreaEnter = OnAreaEnter ?? new AreaEvent();
            OnAreaStay = OnAreaStay ?? new AreaEvent();
            OnAreaExit = OnAreaExit ?? new AreaEvent();
            Collisions = new Dictionary<HedgehogController, List<Transform>>();
            MiscCollisions = new HashSet<Collider2D>();
            InsideRules = new List<InsidePredicate>();
        }

        public virtual void Start()
        {
            var collider2D = GetComponent<Collider2D>();
            collider2D.isTrigger = true;

            if (!TriggerFromChildren)
                return;

            foreach (var childCollider in transform.GetComponentsInChildren<Collider2D>())
            {
                if (childCollider.transform == transform ||
                    childCollider.GetComponent<PlatformTrigger>() != null ||
                    childCollider.GetComponent<ObjectTrigger>() != null)
                    continue;

                var childTrigger = childCollider.gameObject.GetComponent<AreaTrigger>() ??
                                   childCollider.gameObject.AddComponent<AreaTrigger>();
                childTrigger.AlwaysCollide |= AlwaysCollide;
            }
        }

        public void FixedUpdate()
        {
            if (Collisions.Count == 0)
            {
                enabled = false;
                return;
            }

            foreach (var controller in new List<HedgehogController>(Collisions.Keys))
            {
                OnAreaStay.Invoke(controller);
                OnStay.Invoke(controller);
            }
        }

        public override bool HasController(HedgehogController controller)
        {
            return Collisions.ContainsKey(controller);
        }

        /// <summary>
        /// Returns whether the specified controller collides with the collider.
        /// </summary>
        /// <param name="controller">The specified controller.</param>
        /// <returns></returns>
        public bool CollidesWith(HedgehogController controller)
        {
            if (!InsideRules.Any()) DefaultCollisionRule(controller);
            return InsideRules.All(predicate => predicate(controller));
        }

        public bool DefaultCollisionRule(HedgehogController controller)
        {
            return true;
        }

        public void NotifyCollision(HedgehogController controller, Transform hit, bool isExit = false)
        {
            if (controller == null) return;

            List<Transform> hits;
            if (Collisions.TryGetValue(controller, out hits))
            {
                if (isExit || !CollidesWith(controller))
                {
                    hits.Remove(hit);
                    if (hits.Count > 0) return;
                    
                    Collisions.Remove(controller);
                    OnAreaExit.Invoke(controller);
                    OnExit.Invoke(controller);

                    if (Collisions.Count == 0) enabled = false;

                } else if (!hits.Contains(hit))
                {
                    hits.Add(hit);
                }
            }
            else
            {
                if (isExit) return;
                if (!CollidesWith(controller)) return;

                Collisions[controller] = new List<Transform> {hit};
                OnAreaEnter.Invoke(controller);
                OnEnter.Invoke(controller);

                if (Collisions.Count == 1) enabled = true;
            }
        }

        /// <summary>
        /// Used by children triggers to bubble their events up to parent triggers.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="area"></param>
        /// <param name="isExit"></param>
        public void BubbleEvent(HedgehogController controller, Transform area, bool isExit = false)
        {
            foreach (var trigger in GetComponentsInParent<AreaTrigger>().Where(
                trigger => trigger != this && trigger.TriggerFromChildren))
            {
                trigger.NotifyCollision(controller, area, isExit);
            }
        }

        public void OnTriggerEnter2D(Collider2D collider2D)
        {
            if (MiscCollisions.Contains(collider2D)) return;

            var controller = collider2D.GetComponentInParent<HedgehogController>();
            if (controller == null)
            {
                MiscCollisions.Add(collider2D);
            }

            NotifyCollision(controller, transform);
            BubbleEvent(controller, transform);
        }

        public void OnTriggerStay2D(Collider2D collider2D)
        {
            if (MiscCollisions.Contains(collider2D)) return;

            var controller = collider2D.GetComponentInParent<HedgehogController>();
            if (controller == null) return;

            NotifyCollision(controller, transform);
            BubbleEvent(controller, transform);
        }

        public void OnTriggerExit2D(Collider2D collider2D)
        {
            var controller = collider2D.GetComponentInParent<HedgehogController>();
            if (controller == null)
            {
                MiscCollisions.Remove(collider2D);
            }

            NotifyCollision(controller, transform, true);
            BubbleEvent(controller, transform, true);
        }
    }
}
