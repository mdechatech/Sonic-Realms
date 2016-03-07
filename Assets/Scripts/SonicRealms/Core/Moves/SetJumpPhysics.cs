using UnityEngine;

namespace SonicRealms.Core.Moves
{
    public class SetJumpPhysics : StateMachineBehaviour
    {
        [HideInInspector]
        public Jump Jump;

        /// <summary>
        /// Anything set to this will cause the value to stay unchanged.
        /// </summary>
        [Tooltip("Anything set to this will cause the value to stay unchanged.")]
        public float UnchangedValue;

        [Space]
        public float ActivateSpeed;
        public float ReleaseSpeed;

        public void Reset()
        {
            UnchangedValue =

            ActivateSpeed =
            ReleaseSpeed = 0.0f;
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Jump = Jump ?? animator.GetComponentInChildren<Jump>();
            if (Jump == null) return;

            if (ActivateSpeed != UnchangedValue) Jump.ActivateSpeed = ActivateSpeed;
            if (ReleaseSpeed != UnchangedValue) Jump.ReleaseSpeed = ReleaseSpeed;
        }
    }
}