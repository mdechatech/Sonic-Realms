using UnityEngine;

namespace SonicRealms.Core.Internal
{
    /// <summary>
    /// Sets the motion's normalized time to the specified parameter.
    /// </summary>
    public class SrParameterToTime : StateMachineBehaviour
    {
        /// <summary>
        /// Sets the motion's normalized time to this parameter.
        /// </summary>
        [Tooltip("Sets the motion's normalized time to this parameter.")]
        public string Parameter;
        protected int ParameterHash;

        protected int StateHash;

        private float _originalSpeed;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            StateHash = stateInfo.shortNameHash;

            if (ParameterHash == 0)
                ParameterHash = Animator.StringToHash(Parameter);

            _originalSpeed = animator.speed;
            animator.speed = 0;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.Play(StateHash, layerIndex, animator.GetFloat(ParameterHash));
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {   
            animator.speed = _originalSpeed;
        }
    }
}
