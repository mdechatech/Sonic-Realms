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
    [AddComponentMenu("Hedgehog/Platforms/Ice")]
    public class Ice : ReactivePlatform
    {
        /// <summary>
        /// The friction coefficient; smaller than one and the surface becomes slippery.
        /// </summary>
        [SerializeField, Range(0.0f, 2.0f)]
        public float Friction;

        private Dictionary<int, HedgehogPhysicsValues> _capturedPhysicsValues;

        public void Reset()
        {
            Friction = 0.2f;
        }

        public override void Awake()
        {
            base.Awake();
            _capturedPhysicsValues = new Dictionary<int, HedgehogPhysicsValues>();
        }

        // Applies new physics values based on friction.
        public override void OnSurfaceEnter(HedgehogController controller, TerrainCastHit hit)
        {
            _capturedPhysicsValues.Add(controller.GetInstanceID(), HedgehogPhysicsValues.Capture(controller));
            if (DMath.Equalsf(Friction))
            {
                controller.DefaultGroundState.Acceleration = 0.0f;
                controller.DefaultGroundState.Deceleration = 0.0f;
                controller.GroundFriction = 0.0f;
            }
            else
            {
                controller.DefaultGroundState.Acceleration *= Friction;
                controller.DefaultGroundState.Deceleration *= Friction;
                controller.GroundFriction *= Friction;
            }
        }

        // Restores old physics values.
        public override void OnSurfaceExit(HedgehogController controller, TerrainCastHit hit)
        {
            var physicsValues = _capturedPhysicsValues[controller.GetInstanceID()];

            controller.DefaultGroundState.Acceleration = physicsValues.GroundAcceleration;
            controller.DefaultGroundState.Deceleration = physicsValues.GroundBrake;
            controller.GroundFriction = physicsValues.GroundDeceleration;

            _capturedPhysicsValues.Remove(controller.GetInstanceID());
        }
    }
}
