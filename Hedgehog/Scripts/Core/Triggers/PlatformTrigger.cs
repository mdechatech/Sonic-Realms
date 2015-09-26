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
    /// Can be added to a collider to receive events when a controller stands on it.
    /// </summary>
    [AddComponentMenu("Hedgehog/Platforms/Platform Trigger")]
    public class PlatformTrigger : BaseTrigger
    {
        /// <summary>
        /// Called when a controller lands on the surface of the platform.
        /// </summary>
        [SerializeField]
        public PlatformSurfaceEvent OnSurfaceEnter;

        /// <summary>
        /// Called while a controller is on the surface of the platform.
        /// </summary>
        [SerializeField]
        public PlatformSurfaceEvent OnSurfaceStay;

        /// <summary>
        /// Called when a controller exits the surface of the platform.
        /// </summary>
        [SerializeField]
        public PlatformSurfaceEvent OnSurfaceExit;

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
        public ICollection<CollisionPredicate> CollisionRules;

        /// <summary>
        /// Returns whether the controller is considered to be on the surface.
        /// </summary>
        /// <returns></returns>
        public delegate bool SurfacePredicate(
            HedgehogController controller, TerrainCastHit hit, SurfacePriority priority);

        /// <summary>
        /// A list of predicates which invokes surface events based on whether it is empty or all
        /// return true.
        /// </summary>
        public ICollection<SurfacePredicate> SurfaceRules;

        private List<HedgehogController> _surfaceCollisions;  

        public override void Reset()
        {
            base.Reset();

            OnSurfaceEnter = new PlatformSurfaceEvent();
            OnSurfaceStay = new PlatformSurfaceEvent();
            OnSurfaceExit = new PlatformSurfaceEvent();
        }

        public void Awake()
        {
            CollisionRules = new List<CollisionPredicate>();
            SurfaceRules = new List<SurfacePredicate>();
            _surfaceCollisions = new List<HedgehogController>();
        }

        /// <summary>
        /// Invokes the platform's surface events based on SurfaceRules.
        /// </summary>
        public void InvokeSurfaceEvents(HedgehogController controller, TerrainCastHit hit,
            SurfacePriority priority = SurfacePriority.Primary, bool invokeStay = true)
        {
            if (_surfaceCollisions.Contains(controller))
            {
                if (IsOnSurface(controller, hit, priority))
                {
                    if (invokeStay)
                    {
                        OnSurfaceStay.Invoke(controller, hit, priority);
                        OnStay.Invoke(controller);
                    }
                }
                else
                {
                    _surfaceCollisions.Remove(controller);
                    OnSurfaceExit.Invoke(controller, hit, priority);
                    OnExit.Invoke(controller);
                }
            }
            else if (IsOnSurface(controller, hit, priority))
            {
                _surfaceCollisions.Add(controller);
                OnSurfaceEnter.Invoke(controller, hit, priority);
                OnEnter.Invoke(controller);
            }
        }

        public bool IsOnSurface(HedgehogController controller, TerrainCastHit hit, SurfacePriority priority)
        {
            return (!SurfaceRules.Any() && DefaultSurfaceRule(controller, hit, priority))
                || SurfaceRules.All(predicate => predicate(controller, hit, priority));
        }

        /// <summary>
        /// Returns whether the platform can be collided with based on its list of collision predicates
        /// and the specified results of a terrain cast.
        /// </summary>
        /// <param name="hit">The specified results of a terrain cast.</param>
        /// <returns></returns>
        public bool CollidesWith(TerrainCastHit hit)
        {
            return (!CollisionRules.Any() && DefaultCollisionRule(hit))
                || CollisionRules.All(predicate => predicate(hit));
        }

        public bool DefaultSurfaceRule(HedgehogController controller, TerrainCastHit hit,
            SurfacePriority priority)
        {
            return priority != SurfacePriority.Secondary && controller.PrimarySurface == transform;
        }

        public bool DefaultCollisionRule(TerrainCastHit hit)
        {
            return true;
        }
    }

    /// <summary>
    /// Represents the priority the platform's surface holds to the controller.
    /// </summary>
    public enum SurfacePriority
    {
        /// <summary>
        /// Represents a null value.
        /// </summary>
        None,

        /// <summary>
        /// The platform's surface defines the controller's position and rotation. Usually given
        /// to the higher surface beneath the controller's left and right sensors.
        /// </summary>
        Primary,

        /// <summary>
        /// The platform's surface partially defines the controller's rotation. Usually given to
        /// the lower surface beneath the controller's left and right sensors.
        /// </summary>
        Secondary
    }

    /// <summary>
    /// An event for when a controller lands on a platform, invoked with the offending controller,
    /// the offending platform, and the priority the surface holds.
    /// </summary>
    [Serializable]
    public class PlatformSurfaceEvent : UnityEvent<HedgehogController, TerrainCastHit, SurfacePriority>
    {
        
    }
}
