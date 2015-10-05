using System;
using System.Collections.Generic;
using System.Linq;
using Hedgehog.Core.Actors;
using Hedgehog.Core.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Hedgehog.Core.Triggers
{
    /// <summary>
    /// UnityEvents that pass in a controller. Used by OnAreaEnter, OnAreaStay, and OnAreaExit.
    /// </summary>
    [Serializable]
    public class AreaEvent : UnityEvent<HedgehogController>
    {

    }

    /// <summary>
    /// Can be added to a collider to receive events when a collider enters it.
    /// </summary>
    public class AreaTrigger : BaseTrigger
    {
        /// <summary>
        /// Whether to ignore the controller's collision mask/tags/names and always
        /// collide with the controller.
        /// </summary>
        [SerializeField] public bool IgnoreLayers;

        /// <summary>
        /// Invoked when a controller enters the area.
        /// </summary>
        [SerializeField] public AreaEvent OnAreaEnter;

        /// <summary>
        /// Invoked when a controller stays in the area.
        /// </summary>
        [SerializeField] public AreaEvent OnAreaStay;

        /// <summary>
        /// Invoked when a controller exits the area.
        /// </summary>
        [SerializeField] public AreaEvent OnAreaExit;

        /// <summary>
        /// Defines whether the controller should collide with the area. The trigger ONLY checks
        /// if the controller is touching it!
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public delegate bool CollisionPredicate(HedgehogController controller);

        /// <summary>
        /// A list of predicates which, if empty or all return true, allow the controller to collide
        /// with the area. The trigger ONLY checks if the controller is touching it!
        /// </summary>
        public List<CollisionPredicate> CollisionRules;

        private Dictionary<HedgehogController, List<Transform>> _collisions; 

        public override void Reset()
        {
            base.Reset();

            IgnoreLayers = true;
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
            _collisions = new Dictionary<HedgehogController, List<Transform>>();
            CollisionRules = new List<CollisionPredicate>();
        }

        public void FixedUpdate()
        {
            foreach (var controller in _collisions.Keys)
            {
                OnAreaStay.Invoke(controller);
                OnStay.Invoke(controller);
            }
        }

        public void OnEnable()
        {
            if (!TriggerFromChildren) return;
            foreach (var childCollider in transform.GetComponentsInChildren<Collider2D>())
            {
                if (childCollider.transform == transform) continue;
                var childTrigger = childCollider.gameObject.GetComponent<AreaTrigger>() ??
                                   childCollider.gameObject.AddComponent<AreaTrigger>();
                childTrigger.IgnoreLayers |= IgnoreLayers;
            }
        }

        /// <summary>
        /// Returns whether the specified controller collides with the collider.
        /// </summary>
        /// <param name="controller">The specified controller.</param>
        /// <returns></returns>
        public bool CollidesWith(HedgehogController controller)
        {
            return !CollisionRules.Any()
                ? DefaultCollisionRule(controller)
                : CollisionRules.All(predicate => predicate(controller));
        }

        public bool DefaultCollisionRule(HedgehogController controller)
        {
            return controller != null && (IgnoreLayers || TerrainUtility.CollisionModeSelector(transform, controller));
        }

        private void CheckCollision(HedgehogController controller, Transform hit, bool isExit = false)
        {
            if (!enabled || controller == null) return;

            List<Transform> hits;
            if (_collisions.TryGetValue(controller, out hits))
            {
                if (isExit || !CollidesWith(controller))
                {
                    hits.Remove(hit);
                    if (hits.Any()) return;
                    
                    _collisions.Remove(controller);
                    OnAreaExit.Invoke(controller);
                    OnExit.Invoke(controller);
                } else if (!hits.Contains(hit))
                {
                    hits.Add(hit);
                }
            }
            else
            {
                if (isExit) return;
                if (!CollidesWith(controller)) return;
                _collisions[controller] = new List<Transform> {hit};
                OnAreaEnter.Invoke(controller);
                OnEnter.Invoke(controller);
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
                trigger.CheckCollision(controller, area, isExit);
            }
        }

        public void OnTriggerEnter2D(Collider2D collider2D)
        {
            var controller = collider2D.GetComponent<HedgehogController>();
            if (controller == null) return;
            CheckCollision(controller, transform);
            BubbleEvent(controller, transform);
        }

        public void OnTriggerStay2D(Collider2D collider2D)
        {
            var controller = collider2D.GetComponent<HedgehogController>();
            if (controller == null) return;
            CheckCollision(controller, transform);
            BubbleEvent(controller, transform);
        }

        public void OnTriggerExit2D(Collider2D collider2D)
        {
            var controller = collider2D.GetComponent<HedgehogController>();
            if (controller == null) return;
            CheckCollision(controller, transform, true);
            BubbleEvent(controller, transform, true);
        }
    }
}
