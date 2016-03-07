using UnityEngine;

namespace SonicRealms.Core.Moves
{
    public class SetGroundControlPhysics : StateMachineBehaviour
    {
        [HideInInspector]
        public GroundControl GroundControl;

        /// <summary>
        /// Anything set to this will cause the value to stay unchanged.
        /// </summary>
        [Tooltip("Anything set to this will cause the value to stay unchanged.")]
        public float UnchangedValue;
        [Space]
        public float Acceleration;
        public float
            Deceleration,
            TopSpeed,
            MinSlopeGravitySpeed;

        public void Reset()
        {
            UnchangedValue =

            Acceleration =
            Deceleration =
            TopSpeed =
            MinSlopeGravitySpeed = 0.0f;
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            GroundControl = GroundControl ?? animator.GetComponentInChildren<GroundControl>();
            if (GroundControl == null) return;

            if (Acceleration != UnchangedValue) GroundControl.Acceleration = Acceleration;
            if (Deceleration != UnchangedValue) GroundControl.Deceleration = Deceleration;
            if (TopSpeed != UnchangedValue) GroundControl.TopSpeed = TopSpeed;
            if (MinSlopeGravitySpeed != UnchangedValue) GroundControl.MinSlopeGravitySpeed = MinSlopeGravitySpeed;
        }
    }
}
