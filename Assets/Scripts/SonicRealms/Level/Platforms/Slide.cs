using System.Collections.Generic;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Platforms
{
    /// <summary>
    /// Slide (or tunnel? rail?) that increases the controller's slope gravity.
    /// </summary>
    [AddComponentMenu("Hedgehog/Platforms/Slide")]
    public class Slide : ReactivePlatform
    {
        /// <summary>
        /// The controller's bonus slope gravity when going downhill.
        /// </summary>
        [SerializeField] public float DownhillSlopeGravity;

        /// <summary>
        /// The controller's bonus slope gravity (usually negative!) when going uphill.
        /// </summary>
        [SerializeField] public float UphillSlopeGravity;

        /// <summary>
        /// Whether the controller must enter the slide from a surface. Disables collision
        /// when trying to land on it from the air, making it similar to a rail.
        /// </summary>
        [SerializeField] public bool RequireGroundEntry;

        private Dictionary<int, float> _originalSlopeGravities;

        public override void Reset()
        {
            base.Reset();
            DownhillSlopeGravity = 5.0f;
            UphillSlopeGravity = -5.0f;
            RequireGroundEntry = false;

            if (GetComponent<Ledge>() == null)
                gameObject.AddComponent<Ledge>();
        }

        public override void Awake()
        {
            base.Awake();
            _originalSlopeGravities = new Dictionary<int, float>();
        }

        public override bool IsSolid(TerrainCastHit data)
        {
            if (data.Controller == null) return true;
            return base.IsSolid(data) && (!RequireGroundEntry ||
                                              (RequireGroundEntry && data.Controller.Grounded &&
                                               !DMath.Equalsf(data.Raycast.fraction)));
        }

        public override void OnSurfaceEnter(SurfaceCollision collision)
        {
            _originalSlopeGravities[collision.Controller.GetInstanceID()] = collision.Controller.SlopeGravity;
        }

        public override void OnSurfaceStay(SurfaceCollision collision)
        {
            var instanceID = collision.Controller.GetInstanceID();
            if (!_originalSlopeGravities.ContainsKey(instanceID)) return;

            var result = _originalSlopeGravities[instanceID];
            if (-DMath.ScalarProjectionAbs(collision.Controller.Velocity, collision.Controller.GravityDirection*Mathf.Deg2Rad) < 0.0f)
                result += DownhillSlopeGravity;
            else
                result += UphillSlopeGravity;

            collision.Controller.SlopeGravity = result;
        }

        public override void OnSurfaceExit(SurfaceCollision collision)
        {
            var instanceID = collision.Controller.GetInstanceID();
            if (!_originalSlopeGravities.ContainsKey(instanceID)) return;

            collision.Controller.SlopeGravity = _originalSlopeGravities[instanceID];
            _originalSlopeGravities.Remove(instanceID);
        }
    }
}
