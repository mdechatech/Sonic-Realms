using System;
using System.Collections.Generic;
using System.Linq;
using Hedgehog.Actors;
using UnityEngine;
using UnityEngine.Events;

namespace Hedgehog.Terrain
{
    /// <summary>
    /// Can be added to a collider to receive events when a collider enters it.
    /// </summary>
    public class AreaTrigger : BaseTrigger
    {
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
        public delegate bool CollisionPredicate(HedgehogController controller);

        /// <summary>
        /// A list of predicates which, if empty or all return true, allow the controller to collide
        /// with the area. The trigger ONLY checks if the controller is touching it!
        /// </summary>
        public List<CollisionPredicate> CollisionPredicates;

        private Dictionary<int, HedgehogController> _collisions;
        private Collider2D _collider2D;

        public void OnEnable()
        {
            if (CollisionPredicates == null) CollisionPredicates = new List<CollisionPredicate>();
        }

        public void Awake()
        {
            _collider2D = GetComponent<Collider2D>();
            _collisions = new Dictionary<int, HedgehogController>();
        }

        /// <summary>
        /// Returns whether the controller collides with the specified controller.
        /// </summary>
        /// <param name="controller">The specified controller.</param>
        /// <returns></returns>
        public bool CollidesWith(HedgehogController controller)
        {
            return !CollisionPredicates.Any() || CollisionPredicates.All(predicate => predicate(controller));
        }

        // If there are any collision predicates we must track collisions and invoke events manually
        private void CheckCustomCollision(HedgehogController controller)
        {
            if (_collisions.ContainsKey(controller.GetInstanceID()))
            {
                if (CollidesWith(controller))
                {
                    OnAreaStay.Invoke(controller);
                    OnStay.Invoke(controller);
                }
                else
                {
                    _collisions.Remove(controller.GetInstanceID());
                    OnAreaExit.Invoke(controller);
                    OnEnter.Invoke(controller);
                }
            }
            else
            {
                if (CollidesWith(controller))
                {
                    _collisions.Add(controller.GetInstanceID(), controller);
                    OnAreaEnter.Invoke(controller);
                    OnEnter.Invoke(controller);
                }
            }
        }

        public void OnTriggerEnter2D(Collider2D collider2D)
        {
            var controller = collider2D.GetComponent<HedgehogController>();
            if (controller == null) return;
            if (!TerrainUtility.CollisionModeSelector(transform, controller)) return;

            if (CollisionPredicates.Any())
            {
                CheckCustomCollision(controller);
                return;
            }

            OnAreaEnter.Invoke(controller);
            OnEnter.Invoke(controller);
        }

        public void OnTriggerStay2D(Collider2D collider2D)
        {
            var controller = collider2D.GetComponent<HedgehogController>();
            if (controller == null) return;
            if (!TerrainUtility.CollisionModeSelector(transform, controller)) return;

            if (CollisionPredicates.Any())
            {
                CheckCustomCollision(controller);
                return;
            }

            OnAreaStay.Invoke(controller);
            OnStay.Invoke(controller);
        }

        public void OnTriggerExit2D(Collider2D collider2D)
        {
            var controller = collider2D.GetComponent<HedgehogController>();
            if (controller == null) return;
            if (!TerrainUtility.CollisionModeSelector(transform, controller)) return;

            if (CollisionPredicates.Any())
            {
                CheckCustomCollision(controller);
                return;
            }

            OnAreaExit.Invoke(controller);
            OnExit.Invoke(controller);
        }
    }

    [Serializable]
    public class AreaEvent : UnityEvent<HedgehogController>
    {

    }
}
