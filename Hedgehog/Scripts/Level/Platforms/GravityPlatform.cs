using System.Collections.Generic;
using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level.Platforms
{
    /// <summary>
    /// Changes the direction and magnitude of a controller's gravity when entered. Functions same as GravityArea.
    /// </summary>
    public class GravityPlatform : ReactivePlatform
    {
        /// <summary>
        /// Whether to restore the controller's old gravity after leaving.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to restore the controller's old gravity after leaving.")]
        public bool RestoreOnExit;

        /// <summary>
        /// Whether to change the controller's current direction of gravity.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to change the controller's current direction of gravity.")]
        public bool ModifyDirection;

        /// <summary>
        /// New direction of gravity, in degrees. 0/360 is right, 90 is up, 180 is down, 270 is right.
        /// </summary>
        [SerializeField]
        [Tooltip("New direction of gravity, in degrees. 0/360 is right, 90 is up, 180 is down, 270 is right.")]
        public float Direction;

        /// <summary>
        /// Whether change the controller's current strength of gravity.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether change the controller's current strength of gravity.")]
        public bool ModifyStrength;

        /// <summary>
        /// New air strength of gravity, in units per second squared.
        /// </summary>
        [SerializeField]
        [Tooltip("New air strength of gravity, in units per second squared.")]
        public float AirStrength;

        /// <summary>
        /// New ground strength of gravity, in units per second squared.
        /// </summary>
        [SerializeField]
        [Tooltip("New ground strength of gravity, in units per second squared.")]
        public float GroundStrength;

        private class GravityData
        {
            public float Direction;
            public float AirStrength;
            public float GroundStrength;

            public GravityData(float direction, float airStrength, float groundStrength)
            {
                Direction = direction;
                AirStrength = airStrength;
                GroundStrength = groundStrength;
            }
        }

        private Dictionary<int, GravityData> _oldGravities;

        public void Reset()
        {
            Direction = 90.0f;
            ModifyDirection = true;
            AirStrength = GroundStrength = 0.0f;
            ModifyStrength = false;
            RestoreOnExit = false;
        }

        public override void Awake()
        {
            base.Awake();
            _oldGravities = new Dictionary<int, GravityData>();
        }

        public override void OnSurfaceEnter(HedgehogController controller, TerrainCastHit hit, SurfacePriority priority)
        {
            _oldGravities[controller.GetInstanceID()] = new GravityData(controller.GravityDirection,
                controller.AirGravity, controller.SlopeGravity);

            if (ModifyDirection) controller.GravityDirection = Direction;
            if (!ModifyStrength) return;
            controller.AirGravity = AirStrength;
            controller.SlopeGravity = GroundStrength;
        }

        public override void OnSurfaceExit(HedgehogController controller, TerrainCastHit hit, SurfacePriority priority)
        {
            var instanceID = controller.GetInstanceID();
            if (!RestoreOnExit || !_oldGravities.ContainsKey(instanceID)) return;

            var old = _oldGravities[instanceID];
            _oldGravities.Remove(instanceID);

            if (ModifyDirection) controller.GravityDirection = old.Direction;
            if (!ModifyStrength) return;
            controller.AirGravity = old.AirStrength;
            controller.SlopeGravity = old.GroundStrength;

        }
    }
}
