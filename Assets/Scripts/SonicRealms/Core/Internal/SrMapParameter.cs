using UnityEngine;

namespace SonicRealms.Core.Internal
{
    /// <summary>
    /// Maps one Animator float to another Animator float using the given curve.
    /// </summary>
    public class SrMapParameter : StateMachineBehaviour
    {
        public string InParameter;
        public AnimationCurve Curve;
        public string OutParameter;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            OnStateUpdate(animator, stateInfo, layerIndex);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetFloat(OutParameter, Curve.Evaluate(animator.GetFloat(InParameter)));
        }
    }
}
