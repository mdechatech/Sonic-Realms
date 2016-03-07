using UnityEngine;

namespace SonicRealms.Core.Moves
{
    public class SetAirControlPhysics : StateMachineBehaviour
    {
        [HideInInspector]
        public AirControl AirControl;

        /// <summary>
        /// Anything set to this will cause the value to stay unchanged.
        /// </summary>
        [Tooltip("Anything set to this will cause the value to stay unchanged.")]
        public float UnchangedValue;

        [Space]
        public float Acceleration;
        public float Deceleration,
            TopSpeed;

        public void Reset()
        {
            UnchangedValue =

            Acceleration =
            Deceleration = 
            TopSpeed = 0.0f;
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            AirControl = AirControl ?? animator.GetComponentInChildren<AirControl>();
            if (AirControl == null) return;

            if (Acceleration != UnchangedValue) AirControl.Acceleration = Acceleration;
            if (Deceleration != UnchangedValue) AirControl.Deceleration = Deceleration;
            if (TopSpeed != UnchangedValue) AirControl.TopSpeed = TopSpeed;
        }
    }
}