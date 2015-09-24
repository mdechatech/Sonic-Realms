using System.Collections.Generic;
using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level.Platforms
{
    /// <summary>
    /// Gives friction to a platform, allowing slippery surfaces.
    /// </summary>
    [RequireComponent(typeof(PlatformTrigger))]
    public class Ice : MonoBehaviour
    {
        /// <summary>
        /// The friction coefficient; smaller than one and the surface becomes slippery.
        /// </summary>
        [SerializeField, Range(0.0f, 2.0f)]
        public float Friction;

        private PlatformTrigger _trigger;
        private Dictionary<int, HedgehogPhysicsValues> _capturedPhysicsValues;

        public void Reset()
        {
            Friction = 0.2f;
        }

        public void Awake()
        {
            _capturedPhysicsValues = new Dictionary<int, HedgehogPhysicsValues>();
            _trigger = GetComponent<PlatformTrigger>();
        }

        public void OnEnable()
        {
            _trigger.OnSurfaceEnter.AddListener(ApplyFriction);
            _trigger.OnSurfaceExit.AddListener(RemoveFriction);
        }

        public void OnDisable()
        {
            _trigger.OnSurfaceEnter.RemoveListener(ApplyFriction);
            _trigger.OnSurfaceExit.RemoveListener(RemoveFriction);
        }

        // Applies new physics values based on friction.
        public void ApplyFriction(HedgehogController controller, TerrainCastHit hit, SurfacePriority priority)
        {
            if (priority == SurfacePriority.Secondary) return;

            _capturedPhysicsValues.Add(controller.GetInstanceID(), HedgehogPhysicsValues.Capture(controller));
            if (DMath.Equalsf(Friction))
            {
                controller.GroundAcceleration = 0.0f;
                controller.GroundDeceleration = 0.0f;
                controller.GroundBrake = 0.0f;
            }
            else
            {
                controller.GroundAcceleration *= Friction;
                controller.GroundDeceleration *= Friction;
                controller.GroundBrake *= Friction;
            }
        }

        // Restores old physics values.
        public void RemoveFriction(HedgehogController controller, TerrainCastHit hit, SurfacePriority priority)
        {
            if (priority == SurfacePriority.Secondary) return;
            var physicsValues = _capturedPhysicsValues[controller.GetInstanceID()];

            controller.GroundAcceleration = physicsValues.GroundAcceleration;
            controller.GroundDeceleration = physicsValues.GroundDeceleration;
            controller.GroundBrake = physicsValues.GroundBrake;

            _capturedPhysicsValues.Remove(controller.GetInstanceID());
        }
    }
}
