using System.Linq;
using Hedgehog.Core.Moves;
using UnityEngine;

namespace Hedgehog.Core.Actors
{
    public class SetPhysics : StateMachineBehaviour
    {
        protected HedgehogController Controller;

        /// <summary>
        /// Anything set to this will cause the value to stay unchanged.
        /// </summary>
        [Tooltip("Anything set to this will cause the value to stay unchanged.")]
        public float UnchangedValue;

        public float GroundAcceleration;
        public float GroundDeceleration;
        public float GroundFriction;
        public float GroundTopSpeed;
        public float SlopeGravity;

        public float AirAcceleration;
        public float AirGravity;
        public float AirTopSpeed;

        public float JumpSpeed;
        public float JumpReleaseSpeed;

        public float RollingFriction;
        public float RollingDeceleration;
        public float RollingUphillGravity;
        public float RollingDownhillGravity;

        public void Reset()
        {
            UnchangedValue = GroundAcceleration = GroundDeceleration = GroundFriction = GroundTopSpeed =
                AirAcceleration = JumpSpeed = JumpReleaseSpeed = RollingFriction = RollingDeceleration =
                AirGravity = SlopeGravity = RollingUphillGravity = RollingDownhillGravity =
                AirTopSpeed = -1.0f;
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Controller = Controller ?? animator.GetComponentInParent<HedgehogController>();

            if (GroundFriction != UnchangedValue)
                Controller.GroundFriction = GroundFriction;

            if (AirGravity != UnchangedValue)
                Controller.AirGravity = AirGravity;

            if (SlopeGravity != UnchangedValue)
                Controller.SlopeGravity = SlopeGravity;

            if (Controller.GroundControl != null)
            {
                if (GroundAcceleration != UnchangedValue)
                    Controller.GroundControl.Acceleration = GroundAcceleration;

                if (GroundDeceleration != UnchangedValue)
                    Controller.GroundControl.Deceleration = GroundDeceleration;

                if (GroundTopSpeed != UnchangedValue)
                    Controller.GroundControl.TopSpeed = GroundTopSpeed;
            }

            if (Controller.AirControl != null)
            {
                if (AirAcceleration != UnchangedValue)
                    Controller.AirControl.Acceleration = AirAcceleration;

                if (AirTopSpeed != UnchangedValue)
                    Controller.AirControl.TopSpeed = AirTopSpeed;
            }

            var jump = Controller.GetMove<Jump>();
            if (jump != null)
            {
                if (JumpSpeed != UnchangedValue)
                    jump.ActivateSpeed = JumpSpeed;

                if (JumpReleaseSpeed != UnchangedValue)
                    jump.ReleaseSpeed = JumpReleaseSpeed;
            }

            var roll = Controller.GetMove<Roll>();
            if (roll != null)
            {
                if (RollingFriction != UnchangedValue)
                    roll.Friction = RollingFriction;

                if (RollingDeceleration != UnchangedValue)
                    roll.Deceleration = RollingDeceleration;

                if (RollingUphillGravity != UnchangedValue)
                    roll.UphillGravity = RollingUphillGravity;

                if (RollingDownhillGravity != UnchangedValue)
                    roll.DownhillGravity = RollingDownhillGravity;
            }
        }
    }
}
