using System.Collections.Generic;
using System.Linq;
using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level.Platforms
{
    /// <summary>
    /// Functions the same as the trippy gravity surfaces in Death Egg mk ii. Sticks the controller
    /// onto the platform using gravity.
    /// </summary>
    public class GravityMagnet : ReactivePlatform
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
            RestoreOnExit = false;
        }

        public override void Awake()
        {
            base.Awake();
            _oldGravities = new Dictionary<int, float>();
        }

        public override void OnSurfaceEnter(HedgehogController controller, TerrainCastHit hit)
        {
            if (!RestoreOnExit) return;
            _oldGravities[controller.GetInstanceID()] = controller.GravityDirection;
        }

        public override void OnSurfaceStay(HedgehogController controller, TerrainCastHit hit)
        {
            controller.GravityDirection = DMath.PositiveAngle_d(controller.SurfaceAngle - 90.0f);
        }

        public override void OnSurfaceExit(HedgehogController controller, TerrainCastHit hit)
        {
            if (!RestoreOnExit) return;
            var instanceID = controller.GetInstanceID();
            controller.GravityDirection = _oldGravities[instanceID];
            _oldGravities.Remove(instanceID);
        }
    }
}
