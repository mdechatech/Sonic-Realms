using System;
using Hedgehog.Actors;
using UnityEditor;
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

        public override void Reset()
        {
            OnSurfaceEnter = new PlatformSurfaceEvent();
            OnSurfaceStay = new PlatformSurfaceEvent();
            OnSurfaceExit = new PlatformSurfaceEvent();
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
