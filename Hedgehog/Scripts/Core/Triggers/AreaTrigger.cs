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

        public void Awake()
        {
            _collisions = new Dictionary<HedgehogController, List<Transform>>();
            CollisionRules = new List<CollisionPredicate>();
        }

#if UNITY_EDITOR
        public void Start()
        {
            foreach (var rigidbody2D in GetComponentsInChildren<Rigidbody2D>())
            {
                if (rigidbody2D.GetComponent<ChildAreaTrigger>() != null && TriggerFromChildren)
                {
                    Debug.LogWarning(rigidbody2D.name + " is the child of area trigger " + name +" and has a" +
                                     " rigidbody2D. This can cause problems with the trigger.");
                }

                if (rigidbody2D.GetComponent<AreaTrigger>() != null && 
                    rigidbody2D.GetComponentsInChildren<Collider2D>().Any(d => d.transform != rigidbody2D.transform))
                {
                    Debug.LogWarning(rigidbody2D.name + " is an area trigger with a rigidbody2D." +
                                     " This can cause problems with the trigger if it has children.");
                }
            }
        }
#endif
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
                var childTrigger = childCollider.gameObject.AddComponent<ChildAreaTrigger>();
                childTrigger.Target = this;
            }
        }

        public void OnDisable()
        {
            foreach (var childTrigger in transform.GetComponentsInChildren<ChildAreaTrigger>())
            {
                if(childTrigger.transform != transform && childTrigger.Target == this) 
                    Destroy(childTrigger);
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
                if (isExit)
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
                _collisions[controller] = new List<Transform> {hit};
                OnAreaEnter.Invoke(controller);
                OnEnter.Invoke(controller);
            }
        }
        #region Trigger Functions
        public void OnTriggerEnter2D(Collider2D collider2D)
        {
            CheckCollision(collider2D.GetComponent<HedgehogController>(), transform);
        }

        public void OnTriggerStay2D(Collider2D collider2D)
        {
            CheckCollision(collider2D.GetComponent<HedgehogController>(), transform);
        }

        public void OnTriggerExit2D(Collider2D collider2D)
        {
            CheckCollision(collider2D.GetComponent<HedgehogController>(), transform, true);
        }

        // These functions are for when TriggerFromChildren is on
        public void OnChildTriggerEnter2D(Collider2D collider2D, Transform child)
        {
            CheckCollision(collider2D.GetComponent<HedgehogController>(), child);
        }

        public void OnChildTriggerStay2D(Collider2D collider2D, Transform child)
        {
            CheckCollision(collider2D.GetComponent<HedgehogController>(), child);
        }

        public void OnChildTriggerExit2D(Collider2D collider2D, Transform child)
        {
            CheckCollision(collider2D.GetComponent<HedgehogController>(), child, true);
        }
        #endregion
    }
}
