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

        private List<HedgehogController> _collisions;

        public override void Reset()
        {
            base.Reset();

            IgnoreLayers = true;
            OnAreaEnter = new AreaEvent();
            OnAreaStay = new AreaEvent();
            OnAreaExit = new AreaEvent();
        }

        public void Awake()
        {
            _collisions = new List<HedgehogController>();
            CollisionRules = new List<CollisionPredicate>();
        }

        /// <summary>
        /// Returns whether the specified controller collides with the collider.
        /// </summary>
        /// <param name="controller">The specified controller.</param>
        /// <returns></returns>
        public bool CollidesWith(HedgehogController controller)
        {
            return (!CollisionRules.Any() && DefaultCollisionRule(controller)) 
                || CollisionRules.All(predicate => predicate(controller));
        }

        private void CheckCollision(HedgehogController controller)
        {
            if (_collisions.Contains(controller))
            {
                if (CollidesWith(controller))
                {
                    OnAreaStay.Invoke(controller);
                    OnStay.Invoke(controller);
                }
                else
                {
                    _collisions.Remove(controller);
                    OnAreaExit.Invoke(controller);
                    OnEnter.Invoke(controller);
                }
            }
            else
            {
                if (CollidesWith(controller))
                {
                    _collisions.Add(controller);
                    OnAreaEnter.Invoke(controller);
                    OnEnter.Invoke(controller);
                }
            }
        }

        public void OnTriggerEnter2D(Collider2D collider2D)
        {
            var controller = collider2D.GetComponent<HedgehogController>();
            if (controller == null) return;
            if (!IgnoreLayers && !TerrainUtility.CollisionModeSelector(transform, controller)) return;

            CheckCollision(controller);
        }

        public void OnTriggerStay2D(Collider2D collider2D)
        {
            var controller = collider2D.GetComponent<HedgehogController>();
            if (controller == null) return;
            if (!IgnoreLayers && !TerrainUtility.CollisionModeSelector(transform, controller)) return;

            CheckCollision(controller);
        }

        public void OnTriggerExit2D(Collider2D collider2D)
        {
            var controller = collider2D.GetComponent<HedgehogController>();
            if (controller == null) return;
            if (!IgnoreLayers && !TerrainUtility.CollisionModeSelector(transform, controller)) return;

            CheckCollision(controller);
        }

        public bool DefaultCollisionRule(HedgehogController controller)
        {
            return true;
        }
    }

    [Serializable]
    public class AreaEvent : UnityEvent<HedgehogController>
    {

    }
}
