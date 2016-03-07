using System;
using System.Collections.Generic;
using System.Linq;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace SonicRealms.Core.Actors
{
    [Serializable]
    public class HitboxEvent : UnityEvent<AreaTrigger> { }

    /// <summary>
    /// Hitbox that can decide when it wants to let an area trigger know it's been collided with.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Hitbox : MonoBehaviour
    {
        public const string MainHitboxTag = "Main Hitbox";
        public const string AttackHitboxTag = "Attack Hitbox";

        public bool IsMainHitbox
        {
            get { return CompareTag(MainHitboxTag); }
            set
            {
                if (!value) gameObject.tag = "Untagged";
                else gameObject.tag = MainHitboxTag;
            }
        }

        public bool IsAttackHitbox
        {
            get { return CompareTag(AttackHitboxTag); }
            set
            {
                if (!value) gameObject.tag = "Untagged";
                else gameObject.tag = AttackHitboxTag;
            }
        }

        /// <summary>
        /// The hitbox's collider.
        /// </summary>
        [HideInInspector]
        [Tooltip("The hitbox's collider.")]
        public Collider2D Collider2D;

        /// <summary>
        /// The controller the hitbox belongs to.
        /// </summary>
        [Tooltip("The controller the hitbox belongs to.")]
        public HedgehogController Controller;

        /// <summary>
        /// If not empty, the hitbox will only collide with objects of these tags.
        /// </summary>
        [Foldout("Collision")]
        [Tooltip("If not empty, the hitbox will only collide with objects of these tags.")]
        public List<string> TriggerTags;

        /// <summary>
        /// The number of things a hitbox can hit every frame.
        /// </summary>
        [Foldout("Collision")]
        [Tooltip("The number of things a hitbox can hit every frame.")]
        public int CollisionsPerFrame;
        protected int CollisionsThisFrame;

        /// <summary>
        /// The hitbox can only register collisions this often, in seconds.
        /// </summary>
        [Foldout("Collision")]
        [Tooltip("The hitbox can only register collisions this often, in seconds.")]
        public float TimeBetweenCollisions;
        protected float CollisionTimer;

        [Foldout("Events")]
        public HitboxEvent OnTriggerEnter;

        [Foldout("Events")]
        public HitboxEvent OnTriggerStay;

        [Foldout("Events")]
        public HitboxEvent OnTriggerExit;

        public virtual void Reset()
        {
            Controller = GetComponentInParent<HedgehogController>();
            CollisionsPerFrame = 999;
            TimeBetweenCollisions = 0f;

            TriggerTags = new List<string>();
        }

        public virtual void Awake()
        {
            Collider2D = GetComponent<Collider2D>();
            Controller = Controller ?? GetComponentInParent<HedgehogController>();
            CollisionsThisFrame = 0;
            CollisionTimer = 0f;
        }

        public virtual void Update()
        {
            CollisionsThisFrame = 0;

            if(CollisionTimer == 0f) return;
            if ((CollisionTimer -= Time.deltaTime) < 0f)
            {
                CollisionTimer = 0f;
            }
        }

        /// <summary>
        /// Whether to let the specified trigger know it's been collided with.
        /// </summary>
        /// <param name="trigger">The specified trigger.</param>
        /// <returns></returns>
        public virtual bool AllowCollision(AreaTrigger trigger)
        {
            return CollisionsThisFrame < CollisionsPerFrame &&
                   CollisionTimer == 0f &&
                   (TriggerTags.Count == 0 || TriggerTags.Any(s => trigger.CompareTag(s)));
        }

        public void NotifyCollisionEnter(AreaTrigger trigger)
        {
            ++CollisionsThisFrame;
            if (TimeBetweenCollisions > 0f) CollisionTimer = TimeBetweenCollisions;

            OnHitboxEnter(trigger);
        }

        public void NotifyCollisionStay(AreaTrigger trigger)
        {
            OnHitboxStay(trigger);
        }

        public void NotifyCollisionExit(AreaTrigger trigger)
        {
            OnHitboxExit(trigger);
        }

        public virtual void OnHitboxEnter(AreaTrigger trigger)
        {

        }

        public virtual void OnHitboxStay(AreaTrigger trigger)
        {

        }

        public virtual void OnHitboxExit(AreaTrigger trigger)
        {

        }
    }
}
