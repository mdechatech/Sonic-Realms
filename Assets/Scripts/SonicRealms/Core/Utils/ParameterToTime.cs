using UnityEngine;

namespace SonicRealms.Core.Utils
{
    /// <summary>
    /// Sets the motion's normalized time to the specified parameter.
    /// </summary>
    public class ParameterToTime : StateMachineBehaviour
    {
        /// <summary>
        /// Sets the motion's normalized time to this parameter.
        /// </summary>
        [Tooltip("Sets the motion's normalized time to this parameter.")]
        public string Parameter;
        protected int Hash;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Hash = stateInfo.shortNameHash;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.Play(Hash, layerIndex, animator.GetFloat(Parameter));
        }
    }
}
