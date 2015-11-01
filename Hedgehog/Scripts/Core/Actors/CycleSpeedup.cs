using UnityEngine;

namespace Hedgehog.Core.Actors
{
    /// <summary>
    /// Speeds up an animation based on an animator float. Great for making walk cycles faster
    /// when a controller picks up speed.
    /// </summary>
    public class CycleSpeedup : StateMachineBehaviour
    {
        /// <summary>
        /// Name of an Animator float to change animation speed with.
        /// </summary>
        [SerializeField]
        [Tooltip("Name of an Animator float to change animation speed with.")]
        public string SpeedParameter;

        /// <summary>
        /// Whether to use the parameter's absolute value.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to use the parameter's absolute value.")]
        public bool UseAbsoluteValue;

        /// <summary>
        /// Animator speed vs speed parameter.
        /// </summary>
        [SerializeField]
        [Tooltip("Animator speed vs speed parameter.")]
        public AnimationCurve AnimationSpeedCurve;

        private float _originalSpeed;

        public void Reset()
        {
            SpeedParameter = "";
            UseAbsoluteValue = true;
            AnimationSpeedCurve = AnimationCurve.Linear(0.0f, 1.0f, 3.15f, 4.0f);
        }

        public void Awake()
        {
            _originalSpeed = 1.0f;
        }

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _originalSpeed = animator.speed;
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.speed = UseAbsoluteValue
                ? Mathf.Max(0.0f, Mathf.Abs(AnimationSpeedCurve.Evaluate(animator.GetFloat(SpeedParameter))))
                : Mathf.Max(0.0f, AnimationSpeedCurve.Evaluate(animator.GetFloat(SpeedParameter)));
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.speed = _originalSpeed;
        }
    }
}
