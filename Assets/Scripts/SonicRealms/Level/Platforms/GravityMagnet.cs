using System.Collections.Generic;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Platforms
{
    /// <summary>
    /// Functions the same as the trippy gravity surfaces in Death Egg mk ii. Sticks the controller
    /// onto the platform using gravity.
    /// </summary>
    [AddComponentMenu("Hedgehog/Platforms/Gravity Magnet")]
    public class GravityMagnet : ReactivePlatform
    {
        /// <summary>
        /// Whether to restore the controller's old gravity direction when it leaves the platform.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to restore the controller's old gravity direction when it leaves the platform.")]
        public bool RestoreOnExit;

        private Dictionary<int, float> _oldGravities; 

        public override void Reset()
        {
            base.Reset();
            RestoreOnExit = false;
        }

        public override void Awake()
        {
            base.Awake();
            _oldGravities = new Dictionary<int, float>();
        }

        public override void OnSurfaceEnter(TerrainCastHit hit)
        {
            if (!RestoreOnExit) return;
            _oldGravities[hit.Controller.GetInstanceID()] = hit.Controller.GravityDirection;
        }

        public override void OnSurfaceStay(TerrainCastHit hit)
        {
            hit.Controller.GravityDirection = DMath.PositiveAngle_d(hit.Controller.SurfaceAngle - 90.0f);
        }

        public override void OnSurfaceExit(TerrainCastHit hit)
        {
            if (!RestoreOnExit) return;
            var instanceID = hit.Controller.GetInstanceID();
            hit.Controller.GravityDirection = _oldGravities[instanceID];
            _oldGravities.Remove(instanceID);
        }
    }
}
