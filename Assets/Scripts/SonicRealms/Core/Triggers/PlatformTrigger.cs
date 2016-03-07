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
    [AddComponentMenu("Hedgehog/Triggers/Platform Trigger")]
    public class PlatformTrigger : BaseTrigger
    {
        #region Events
        /// <summary>
        /// Called when a controller lands on the surface of the platform.
        /// </summary>
        [Foldout("Surface Events")]
        public PlatformSurfaceEvent OnSurfaceEnter;

        /// <summary>
        /// Called while a controller is on the surface of the platform.
        /// </summary>
        [Foldout("Surface Events")]
        public PlatformSurfaceEvent OnSurfaceStay;

        /// <summary>
        /// Called when a controller exits the surface of the platform.
        /// </summary>
        [Foldout("Surface Events")]
        public PlatformSurfaceEvent OnSurfaceExit;

        /// <summary>
        /// Called when a controller begins colliding with the platform.
        /// </summary>
        [Foldout("Platform Events")]
        public PlatformCollisionEvent OnPlatformEnter;

        /// <summary>
        /// Called while a controller is colliding with the platform.
        /// </summary>
        [Foldout("Platform Events")]
        public PlatformCollisionEvent OnPlatformStay;

        /// <summary>
        /// Called when a controller stops colliding with the platform.
        /// </summary>
        [Foldout("Platform Events")]
        public PlatformCollisionEvent OnPlatformExit;
        #endregion
        #region Sounds
        /// <summary>
        /// An audio clip to play when a controller starts colliding with the platform.
        /// </summary>
        [Foldout("Sound")]
        [Tooltip("An audio clip to play when a controller starts colliding with the platform.")]
        public AudioClip PlatformEnterSound;

        /// <summary>
        /// An audio clip to loop while the platform is being collided with.
        /// </summary>
        [Foldout("Sound")]
        [Tooltip("An audio clip to loop while the platform is being collided with.")]
        // TODO Work this out later
        public AudioClip PlatformLoopSound;

        /// <summary>
        /// An audio clip to play when a controller stops colliding with the platform.
        /// </summary>
        [Foldout("Sound")]
        [Tooltip("An audio clip to play when a controller stops colliding with the platform.")]
        public AudioClip PlatformExitSound;

        /// <summary>
        /// An audio clip to play when a controller stands on the platform.
        /// </summary>
        [Space, Foldout("Sound")]
        [Tooltip("An audio clip to play when a controller stands on the platform.")]
        public AudioClip SurfaceEnterSound;

        /// <summary>
        /// An audio clip to loop while a controller is on the platform.
        /// </summary>
        [Foldout("Sound")]
        [Tooltip("An audio clip to loop while a controller is on the platform.")]
        public AudioClip SurfaceLoopSound;

        /// <summary>
        /// An audio clip to play when a controller exits the platform.
        /// </summary>
        [Foldout("Sound")]
        [Tooltip("An audio clip to play when a controller exits the platform.")]
        public AudioClip SurfaceExitSound;
        #endregion

        /// <summary>
        /// Returns whether the platform should be collided with based on the result of the specified
        /// terrain cast.
        /// </summary>
        /// <param name="hit">The specified terrain cast.</param>
        /// <returns></returns>
        public delegate bool CollisionPredicate(TerrainCastHit hit);

        /// <summary>
        /// A list of predicates which, if empty or all return true, allow collision with the
        /// platform.
        /// </summary>
        public List<CollisionPredicate> CollisionRules;

        /// <summary>
        /// Returns whether the controller is considered to be on the surface.
        /// </summary>
        /// <returns></returns>
        public delegate bool SurfacePredicate(TerrainCastHit hit);

        /// <summary>
        /// A list of predicates which invokes surface events based on whether it is empty or all
        /// return true.
        /// </summary>
        public List<SurfacePredicate> SurfaceRules;

        /// <summary>
        /// A list of current collisions;
        /// </summary>
        public List<TerrainCastHit> Collisions;
        private List<TerrainCastHit> _notifiedCollisions;

        /// <summary>
        /// A list of current surface collisions.
        /// </summary>
        public List<TerrainCastHit> SurfaceCollisions;
        private List<TerrainCastHit> _notifiedSurfaceCollisions;

        protected List<PlatformTrigger> Parents;

        public override void Reset()
        {
            base.Reset();

            OnPlatformEnter = new PlatformCollisionEvent();
            OnPlatformStay = new PlatformCollisionEvent();
            OnPlatformExit = new PlatformCollisionEvent();

            OnSurfaceEnter = new PlatformSurfaceEvent();
            OnSurfaceStay = new PlatformSurfaceEvent();
            OnSurfaceExit = new PlatformSurfaceEvent();

            PlatformEnterSound = PlatformLoopSound = PlatformExitSound =
                SurfaceEnterSound = SurfaceLoopSound = SurfaceExitSound = null;
        }

        public override void Awake()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            base.Awake();

            OnPlatformEnter = OnPlatformEnter ?? new PlatformCollisionEvent();
            OnPlatformStay = OnPlatformStay ?? new PlatformCollisionEvent();
            OnPlatformExit = OnPlatformExit ?? new PlatformCollisionEvent();

            OnSurfaceEnter = OnSurfaceEnter ?? new PlatformSurfaceEvent();
            OnSurfaceStay = OnSurfaceStay ?? new PlatformSurfaceEvent();
            OnSurfaceExit = OnSurfaceExit ?? new PlatformSurfaceEvent();

            CollisionRules = new List<CollisionPredicate>();
            Collisions = new List<TerrainCastHit>();
            _notifiedCollisions = new List<TerrainCastHit>();

            SurfaceRules = new List<SurfacePredicate>();
            SurfaceCollisions = new List<TerrainCastHit>();
            _notifiedSurfaceCollisions = new List<TerrainCastHit>();

            Parents = new List<PlatformTrigger>();
            GetComponentsInParent(true, Parents);
        }

        public void Start()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            if (!TriggerFromChildren) return;
            foreach (var child in GetComponentsInChildren<Collider2D>())
            {
                if (((1 << child.gameObject.layer) & CollisionLayers.AllMask) == 0) continue;
                if (child.GetComponent<PlatformTrigger>() || child.GetComponent<AreaTrigger>()) continue;

                var trigger = child.gameObject.AddComponent<PlatformTrigger>();
                trigger.TriggerFromChildren = true;
            }
        }

        public virtual void FixedUpdate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            if (Collisions.Count == 0 && SurfaceCollisions.Count == 0 &&
                _notifiedCollisions.Count == 0 && _notifiedSurfaceCollisions.Count == 0)
            {
                enabled = false;
                return;
            }

            // Remove collisions that were recorded last update but not this one and invoke their "exit" events
            Collisions.RemoveAll(CollisionsRemover);

            // Move this update's collision list to the last update's
            Collisions = _notifiedCollisions;

            // Invoke their "stay" events if they still fulfill IsSolid
            for (var i = Collisions.Count - 1; i >= 0; i = --i >= Collisions.Count ? Collisions.Count - 1 : i)
            {
                var collision = Collisions[i];
                if (IsSolid(collision)) OnPlatformStay.Invoke(collision);
            }

            // Make room in the collision list for the next update
            _notifiedCollisions = new List<TerrainCastHit>();

            // Remove surface collisions that were recorded last update but not this one. Invoke their "exit" events
            SurfaceCollisions.RemoveAll(SurfaceCollisionsRemover);

            // Move this update's surface collision list to the last update's
            SurfaceCollisions = _notifiedSurfaceCollisions;

            // Invoke their "stay" events if they still fulfill IsOnSurface
            for (var i = SurfaceCollisions.Count - 1; i >= 0; 
                i = --i >= SurfaceCollisions.Count ? SurfaceCollisions.Count - 1 : i)
            {
                var collision = SurfaceCollisions[i];
                if (IsOnSurface(collision)) OnSurfaceStay.Invoke(collision);
            }

            // Make room in the surface collision list for the next update
            _notifiedSurfaceCollisions = new List<TerrainCastHit>();

            if (Collisions.Count == 0 && SurfaceCollisions.Count == 0) enabled = false;
        }

        public override bool HasController(HedgehogController controller)
        {
            return Collisions.Any(hit => hit.Controller == controller);
        }

        public bool HasControllerOnSurface(HedgehogController controller)
        {
            return SurfaceCollisions.Any(hit => hit.Controller == controller);
        }
        #region Collision List Removers
        private bool CollisionsRemover(TerrainCastHit hit)
        {
            for (var i = 0; i < _notifiedCollisions.Count; ++i)
            {
                if (_notifiedCollisions[i].Controller == hit.Controller) return false;
            }

            if(PlatformExitSound != null) SoundManager.Instance.PlayClipAtPoint(PlatformExitSound, transform.position);
            OnPlatformExit.Invoke(hit);
            return true;
        }

        private bool SurfaceCollisionsRemover(TerrainCastHit hit)
        {
            for (var i = 0; i < _notifiedSurfaceCollisions.Count; ++i)
            {
                if (_notifiedSurfaceCollisions[i].Controller == hit.Controller)
                    return false;
            }

            if(SurfaceExitSound != null) SoundManager.Instance.PlayClipAtPoint(SurfaceExitSound, transform.position);
            OnSurfaceExit.Invoke(hit);
            return true;
        }
        #endregion
        #region Notify Functions
        public void NotifyCollision(TerrainCastHit hit)
        {
            NotifyCollision(hit, true);
        }

        /// <summary>
        /// Lets the trigger know about a collision with a controller.
        /// </summary>
        /// <param name="hit">The collision data.</param>
        /// <param name="bubble"></param>
        public void NotifyCollision(TerrainCastHit hit, bool bubble)
        {
            if (!IsSolid(hit)) return;

            for (var i = 0; i < Collisions.Count; ++i)
            {
                var collision = Collisions[i];
                if (collision.Controller == hit.Controller)
                {
                    for (i = 0; i < _notifiedCollisions.Count; ++i)
                    {
                        var notifiedCollision = _notifiedCollisions[i];
                        if (notifiedCollision.Controller == hit.Controller) goto bubble;
                    }

                    _notifiedCollisions.Add(hit);
                    goto bubble;
                }
            }

            if(PlatformEnterSound != null)
                SoundManager.Instance.PlayClipAtPoint(PlatformEnterSound, transform.position);

            Collisions.Add(hit);
            _notifiedCollisions.Add(hit);
            OnPlatformEnter.Invoke(hit);

            if (Collisions.Count == 1 && !enabled) enabled = true;

            bubble:
            if (!bubble) return;
            for (var i = 0; i < Parents.Count; ++i)
            {
                var parent = Parents[i];
                if (!ReceivesEvents(parent, transform)) continue;
                parent.NotifyCollision(hit, false);
            }
        }

        public void NotifySurfaceCollision(TerrainCastHit hit)
        {
            NotifySurfaceCollision(hit, true);
        }

        /// <summary>
        /// Lets the trigger know about a controller standing on its surface.
        /// </summary>
        /// <param name="hit">The collision data.</param>
        public void NotifySurfaceCollision(TerrainCastHit hit, bool bubble)
        {
            if (!IsOnSurface(hit)) return;

            for (var i = 0; i < SurfaceCollisions.Count; ++i)
            {
                var collision = SurfaceCollisions[i];
                if (collision.Controller != hit.Controller) continue;

                for (i = 0; i < _notifiedSurfaceCollisions.Count; ++i)
                {
                    var notifiedCollision = _notifiedSurfaceCollisions[i];
                    if (notifiedCollision.Controller == hit.Controller)
                        goto bubble;
                }

                _notifiedSurfaceCollisions.Add(hit);
                goto bubble;
            }

            if(SurfaceEnterSound != null) SoundManager.Instance.PlayClipAtPoint(SurfaceEnterSound, transform.position);
            SurfaceCollisions.Add(hit);
            _notifiedSurfaceCollisions.Add(hit);
            OnSurfaceEnter.Invoke(hit);

            if (SurfaceCollisions.Count == 1 && !enabled) enabled = true;

            bubble:
            if (!bubble) return;
            for (var i = 0; i < Parents.Count; ++i)
            {
                var parent = Parents[i];
                if (!ReceivesEvents(parent, transform)) return;
                parent.NotifySurfaceCollision(hit, false);
            }
        }

        #endregion
        #region Collision Rules
        /// <summary>
        /// Whether the controller is on the surface given the terrain cast results. This doesn't affect
        /// the controller's physics, but rather will not invoke surface events if false.
        /// </summary>
        /// <param name="hit">The terrain cast results.</param>
        /// <returns></returns>
        public bool IsOnSurface(TerrainCastHit hit)
        {
            if (SurfaceRules.Count == 0) return DefaultSurfaceRule(hit);

            for(var i = 0; i < SurfaceRules.Count; ++i)
                if (!SurfaceRules[i](hit)) return false;

            if (Parents.Count == 0) return true;

            for (var i = 0; i < Parents.Count; ++i)
            {
                var parent = Parents[i];
                if (!ReceivesEvents(parent, transform)) return false;
            }

            return true;
        }

        /// <summary>
        /// Returns whether the platform can be collided with based on its list of collision predicates
        /// and the specified results of a terrain cast.
        /// </summary>
        /// <param name="hit">The specified results of a terrain cast.</param>
        /// <returns></returns>
        public bool IsSolid(TerrainCastHit hit)
        {
            if (CollisionRules.Count == 0 && Parents.Count == 0) return true;

            for (var i = 0; i < CollisionRules.Count; ++i)
                if (!CollisionRules[i](hit)) return false;

            if (Parents.Count == 0) return true;

            for (var i = 0; i < Parents.Count; ++i)
            {
                var parent = Parents[i];
                if (!ReceivesEvents(parent, transform)) continue;

                for(var j = 0; j < parent.CollisionRules.Count; ++j)
                    if (!parent.CollisionRules[j](hit)) return false;
            }

            return true;
        }

        public bool DefaultSurfaceRule(TerrainCastHit hit)
        {
            return hit.Controller.Grounded;
        }
        #endregion
    }
}
