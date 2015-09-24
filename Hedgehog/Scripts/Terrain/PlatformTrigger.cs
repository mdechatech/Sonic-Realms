using System;
using System.Collections.Generic;
using System.Linq;
using Hedgehog.Actors;
using UnityEngine;
using UnityEngine.Events;

namespace Hedgehog.Terrain
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
        public ICollection<CollisionPredicate> CollisionPredicates;

        public override void Reset()
        {
            OnSurfaceEnter = new PlatformSurfaceEvent();
            OnSurfaceStay = new PlatformSurfaceEvent();
            OnSurfaceExit = new PlatformSurfaceEvent();
        }

        public void OnEnable()
        {
            // We instantiate here to let collision predicates be easily added and removed
            if(CollisionPredicates == null) CollisionPredicates = new List<CollisionPredicate>();
        }

        /// <summary>
        /// Returns whether the platform can be collided with based on its list of collision predicates
        /// and the specified results of a terrain cast.
        /// </summary>
        /// <param name="hit">The specified results of a terrain cast.</param>
        /// <returns></returns>
        public bool CollidesWith(TerrainCastHit hit)
        {
            return !CollisionPredicates.Any() || CollisionPredicates.All(predicate => predicate(hit));
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
