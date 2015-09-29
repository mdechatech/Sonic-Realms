using System.Collections.Generic;
using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level.Platforms
{
    /// <summary>
    /// Functions the same as the trippy gravity surfaces in Death Egg mk ii.
    /// Automatically sets a controller's direction of gravity opposite to the surface's normal.
    /// The surface basically becomes "flat" (no gravity applied) no matter how steep it is.
    /// </summary>
    public class AutoGravitySurface : ReactivePlatform
    {
        /// <summary>
        /// Whether to restore the controller's old gravity direction when it leaves the platform.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to restore the controller's old gravity direction when it leaves the platform.")]
        public bool RestoreOnExit;

        private Dictionary<int, float> _oldGravities; 

        public void Reset()
        {
            RestoreOnExit = true;
        }

        public override void Awake()
        {
            base.Awake();
            _oldGravities = new Dictionary<int, float>();
        }

        public override void OnSurfaceEnter(HedgehogController controller, TerrainCastHit hit, SurfacePriority priority)
        {
            if (!RestoreOnExit) return;
            _oldGravities[controller.GetInstanceID()] = controller.GravityDirection;
        }

        public override void OnSurfaceStay(HedgehogController controller, TerrainCastHit hit, SurfacePriority priority)
        {
            controller.GravityDirection = DMath.PositiveAngle_d(controller.SurfaceAngle - 90.0f);
        }

        public override void OnSurfaceExit(HedgehogController controller, TerrainCastHit hit, SurfacePriority priority)
        {
            if (!RestoreOnExit) return;
            var instanceID = controller.GetInstanceID();
            controller.GravityDirection = _oldGravities[instanceID];
            _oldGravities.Remove(instanceID);
        }
    }
}
