using System.Collections.Generic;
using System.Linq;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Utils;
using SonicRealms.Level;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Hook up to these events to react when a controller enters the area.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Hedgehog/Triggers/Area Trigger")]
    public class AreaTrigger : BaseTrigger
    {
        /// <summary>
        /// If true, a player can trigger the area multiple times from multiple hitboxes.
        /// </summary>
        [Tooltip("If true, a player can trigger the area multiple times from multiple hitboxes.")]
        public bool AllowMultiple;

        /// <summary>
        /// Invoked when a controller enters the area.
        /// </summary>
        [Foldout("Events")]
        public AreaEvent OnAreaEnter;

        /// <summary>
        /// Invoked when a controller stays in the area.
        /// </summary>
        [Foldout("Events")]
        public AreaEvent OnAreaStay;

        /// <summary>
        /// Invoked when a controller exits the area.
        /// </summary>
        [Foldout("Events")]
        public AreaEvent OnAreaExit;

        /// <summary>
        /// An audio clip to play when a controller enters the area.
        /// </summary>
        [Foldout("Sound")]
        [Tooltip("An audio clip to play when a controller enters the area.")]
        public AudioClip AreaEnterSound;

        /// <summary>
        /// An audio clip to loop while a controller is inside the area.
        /// </summary>
        [Foldout("Sound")]
        [Tooltip("An audio clip to loop while a controller is inside the area.")]
        public AudioClip AreaLoopSound;

        /// <summary>
        /// An audio clip to play when a controller leaves the area.
        /// </summary>
        [Foldout("Sound")]
        [Tooltip("An audio clip to play when a controller leaves the area.")]
        public AudioClip AreaExitSound;

        /// <summary>
        /// Defines whether the controller should collide with the area. The trigger ONLY checks
        /// if the controller is touching it!
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public delegate bool InsidePredicate(Hitbox hitbox);

        /// <summary>
        /// A list of predicates which, if empty or all return true, allow the controller to collide
        /// with the area. The trigger ONLY checks if the controller is touching it!
        /// </summary>
        ///
        public List<InsidePredicate> InsideRules;

        /// <summary>
        /// Maps a controller to the areas it is colliding with (can collide with multiple child areas).
        /// </summary>
        [Foldout("Debug")]
        [Tooltip("Maps a controller to the areas it is colliding with (can collide with multiple child areas).")]
        public List<Hitbox> Collisions;

        protected HashSet<Collider2D> MiscCollisions;

        public override void Reset()
        {
            base.Reset();

            AllowMultiple = false;

            OnAreaEnter = new AreaEvent();
            OnAreaStay = new AreaEvent();
            OnAreaExit = new AreaEvent();

            AreaEnterSound = AreaLoopSound = AreaExitSound = null;
        }

        public override void Awake()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            base.Awake();

            OnAreaEnter = OnAreaEnter ?? new AreaEvent();
            OnAreaStay = OnAreaStay ?? new AreaEvent();
            OnAreaExit = OnAreaExit ?? new AreaEvent();
            Collisions = new List<Hitbox>();
            MiscCollisions = new HashSet<Collider2D>();
            InsideRules = new List<InsidePredicate>();
        }

        public virtual void Start()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            if (!TriggerFromChildren)
                return;

            foreach (var childCollider in transform.GetComponentsInChildren<Collider2D>())
            {
                if (childCollider.transform == transform ||
                    childCollider.GetComponent<PlatformTrigger>() != null ||
                    childCollider.GetComponent<ObjectTrigger>() != null)
                    continue;

                var callback = childCollider.GetComponent<TriggerCallback2D>() ??
                               childCollider.gameObject.AddComponent<TriggerCallback2D>();


                callback.TriggerEnter2D.AddListener(OnTriggerEnter2D);
                callback.TriggerStay2D.AddListener(OnTriggerStay2D);
                callback.TriggerExit2D.AddListener(OnTriggerExit2D);
            }
        }

        public void FixedUpdate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            if (Collisions.Count == 0)
            {
                enabled = false;
                return;
            }

            foreach (var hitbox in Collisions)
            {
                OnAreaStay.Invoke(hitbox);
                hitbox.NotifyCollisionStay(this);
            }
        }

        public override bool HasController(HedgehogController controller)
        {
            return controller != null && Collisions.Any(hitbox => hitbox.Controller == controller);
        }

        protected bool HasController(HedgehogController controller, Hitbox excludeHitbox)
        {
            return controller != null &&
                   Collisions.Any(hitbox => hitbox != excludeHitbox && hitbox.Controller == controller);
        }

        /// <summary>
        /// Returns whether the specified controller collides with the collider.
        /// </summary>
        /// <param name="controller">The specified controller.</param>
        /// <returns></returns>
        public virtual bool CollidesWith(Hitbox hitbox)
        {
            if (!InsideRules.Any()) return DefaultInsideRule(hitbox);
            return InsideRules.All(predicate => predicate(hitbox));
        }

        public bool DefaultInsideRule(Hitbox hitbox)
        {
            return hitbox.CompareTag("Untagged") || hitbox.CompareTag(Hitbox.MainHitboxTag);
        }

        public void NotifyCollision(Hitbox hitbox, bool isExit = false)
        {
            if (Collisions.Contains(hitbox))
            {
                if (isExit || !CollidesWith(hitbox))
                {
                    Collisions.Remove(hitbox);

                    if (AreaExitSound != null)
                        SoundManager.Instance.PlayClipAtPoint(AreaExitSound, transform.position);
                    Collisions.Remove(hitbox);
                    OnAreaExit.Invoke(hitbox);
                    hitbox.NotifyCollisionExit(this);

                    if (Collisions.Count == 0) enabled = false;
                }
            }
            else
            {
                if (isExit) return;
                if (!CollidesWith(hitbox)) return;

                if (AreaEnterSound != null)
                    SoundManager.Instance.PlayClipAtPoint(AreaEnterSound, transform.position);
                Collisions.Add(hitbox);
                OnAreaEnter.Invoke(hitbox);
                hitbox.NotifyCollisionEnter(this);

                if (Collisions.Count == 1) enabled = true;
            }
        }

        /// <summary>
        /// Used by children triggers to bubble their events up to parent triggers.
        /// </summary>
        /// <param name="hitbox"></param>
        /// <param name="area"></param>
        /// <param name="isExit"></param>
        public void BubbleEvent(Hitbox hitbox, bool isExit = false)
        {
            foreach (var trigger in GetComponentsInParent<AreaTrigger>().Where(
                trigger => trigger != this && trigger.TriggerFromChildren))
            {
                trigger.NotifyCollision(hitbox, isExit);
            }
        }

        public void OnTriggerEnter2D(Collider2D collider2D)
        {
            if (MiscCollisions.Contains(collider2D)) return;

            var hitbox = collider2D.GetComponent<Hitbox>();
            if (hitbox == null)
            {
                MiscCollisions.Add(collider2D);
                return;
            }

            if ((!AllowMultiple && HasController(hitbox.Controller, hitbox)) || 
                !hitbox.AllowCollision(this) || !CollidesWith(hitbox))
                return;

            NotifyCollision(hitbox);
            BubbleEvent(hitbox);
        }

        public void OnTriggerStay2D(Collider2D collider2D)
        {
            if (MiscCollisions.Contains(collider2D)) return;

            var hitbox = collider2D.GetComponent<Hitbox>();
            if (hitbox == null) return;

            if ((AllowMultiple || !HasController(hitbox.Controller, hitbox)) &&
                hitbox.AllowCollision(this) && CollidesWith(hitbox))
            {
                NotifyCollision(hitbox);
                BubbleEvent(hitbox);
            }
            else
            {
                NotifyCollision(hitbox, true);
                BubbleEvent(hitbox, true);
            }
        }

        public void OnTriggerExit2D(Collider2D collider2D)
        {
            var hitbox = collider2D.GetComponent<Hitbox>();
            if (hitbox == null)
            {
                MiscCollisions.Remove(collider2D);
                return;
            }

            NotifyCollision(hitbox, true);
            BubbleEvent(hitbox, true);
        }
    }
}
