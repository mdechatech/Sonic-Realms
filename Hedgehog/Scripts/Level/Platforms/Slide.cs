using System.Collections.Generic;
using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level.Platforms
{
    // TODO disable jumping while on slide
    /// <summary>
    /// Slide (or tunnel? rail?) that increases the controller's slope gravity.
    /// </summary>
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

        public void Reset()
        {
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

        public override bool CollidesWith(TerrainCastHit hit)
        {
            if (hit.Source == null) return true;
            return base.CollidesWith(hit) && (!RequireGroundEntry ||
                                              (RequireGroundEntry && hit.Source.Grounded &&
                                               !DMath.Equalsf(hit.Hit.fraction)));
        }

        public override void OnSurfaceEnter(HedgehogController controller, TerrainCastHit hit)
        {
            _originalSlopeGravities[controller.GetInstanceID()] = controller.SlopeGravity;
        }

        public override void OnSurfaceStay(HedgehogController controller, TerrainCastHit hit)
        {
            var instanceID = controller.GetInstanceID();
            if (!_originalSlopeGravities.ContainsKey(instanceID)) return;

            var result = _originalSlopeGravities[instanceID];
            if (-DMath.ScalarProjectionAbs(controller.Velocity, controller.GravityDirection*Mathf.Deg2Rad) < 0.0f)
                result += DownhillSlopeGravity;
            else
                result += UphillSlopeGravity;

            controller.SlopeGravity = result;
        }

        public override void OnSurfaceExit(HedgehogController controller, TerrainCastHit hit)
        {
            var instanceID = controller.GetInstanceID();
            if (!_originalSlopeGravities.ContainsKey(instanceID)) return;

            controller.SlopeGravity = _originalSlopeGravities[instanceID];
            _originalSlopeGravities.Remove(instanceID);
        }
    }
}
