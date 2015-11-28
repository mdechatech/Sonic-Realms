using Hedgehog.Core.Triggers;
using Hedgehog.Level.Areas;
using UnityEngine;

namespace Hedgehog.Core.Moves
{
    /// <summary>
    /// Passive, allows custom physics values while underwater.
    /// </summary>
    public class WaterPhysics : Move
    {
        protected bool Underwater;

        public float GroundAcceleration;
        public float GroundDeceleration;
        public float GroundFriction;
        public float RollingFriction;
        public float GroundTopSpeed;
        public float AirAcceleration;
        public float AirGravity;
        public float JumpSpeed;
        public float JumpReleaseSpeed;

        public override void Reset()
        {
            base.Reset();
            GroundAcceleration = 0.84375f;
            GroundDeceleration = 0.9f;
            GroundFriction = 0.84375f;
            RollingFriction = 0.421875f;
            GroundTopSpeed = 1.8f;
            AirAcceleration = 1.6875f;
            AirGravity = 2.25f;
            JumpSpeed = 2.1f; // 1.8 for knux
            JumpReleaseSpeed = 1.2f;
        }

        public override void Awake()
        {
            base.Awake();
            Underwater = false;
        }

        public override void Start()
        {
            base.Start();
            Controller.OnReactiveEnter.AddListener(OnReactive);
            Controller.OnReactiveExit.AddListener(OnReactive);
        }

        protected void OnReactive(BaseReactive reactive)
        {
            Underwater = Controller.Inside<Water>();
            if (Underwater) Perform();
            else End();
        }

        public override void OnActiveEnter(State previousState)
        {
            Controller.GroundFriction = GroundFriction;

            if (Controller.GroundControl != null)
            {
                Controller.GroundControl.Acceleration = GroundAcceleration;
                Controller.GroundControl.Deceleration = GroundDeceleration;
                Controller.GroundControl.TopSpeed = GroundTopSpeed;
            }

            if (Controller.AirControl != null)
            {
                Controller.AirControl.Acceleration = AirAcceleration;
                Controller.AirGravity = AirGravity;
            }

            var jump = Controller.GetMove<Jump>();
            if (jump != null)
            {
                jump.ActivateSpeed = JumpSpeed;
                jump.ReleaseSpeed = JumpReleaseSpeed;
            }

            var roll = Controller.GetMove<Roll>();
            if (roll != null)
            {
                roll.Friction = RollingFriction;
            }
        }
    }
}
