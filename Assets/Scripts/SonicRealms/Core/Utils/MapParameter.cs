using UnityEngine;

namespace SonicRealms.Core.Utils
{
    /// <summary>
    /// Maps one Animator float to another Animator float using the given curve.
    /// </summary>
    public class MapParameter : StateMachineBehaviour
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
