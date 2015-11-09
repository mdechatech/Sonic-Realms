using Hedgehog.Core.Actors;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Core.Moves
{
    public class Jump : Move
    {
        #region Controls
        /// <summary>
        /// Input string used for activation.
        /// </summary>
        [SerializeField]
        [Tooltip("Input string used for activation.")]
        public string ActivateInput;
        #endregion
        #region Physics
        /// <summary>
        /// Jump speed at the moment of activation.
        /// </summary>
        [SerializeField]
        [Tooltip("Jump speed at the moment of activation.")]
        public float ActivateSpeed;

        /// <summary>
        /// Jump speed at the moment of release, in units per second.
        /// </summary>
        [SerializeField]
        [Tooltip("Jump speed after moment of release, in units per second.")]
        public float ReleaseSpeed;
        #endregion

        /// <summary>
        /// Whether a jump happened. If false, the controller didn't leave the ground by jumping.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether a jump happened. If false, the controller didn't leave the ground by jumping.")]
        public bool Used;

        public override void Reset()
        {
            base.Reset();

            ActivateInput = "Jump";

            ActivateSpeed = 3.9f;
            ReleaseSpeed = 2.4f;
        }

        public override void Awake()
        {
            base.Awake();

            Used = false;
        }

        public void OnEnable()
        {
            Controller.OnAttach.AddListener(OnAttach);
        }

        public void OnDisable()
        {
            Controller.OnAttach.RemoveListener(OnAttach);
        }

        public void OnAttach()
        {
            Used = false;
        }

        public override bool Available()
        {
            return !Controller.IsActive<Duck>() && Controller.Grounded;
        }

        public override bool InputActivate()
        {
            return Input.GetButtonDown(ActivateInput);
        }

        public override bool InputDeactivate()
        {
            return Input.GetButtonUp(ActivateInput);
        }

        public override void OnActiveEnter(State previousState)
        {
            Used = true;

            Controller.Detach();
            Controller.Velocity += DMath.AngleToVector((Controller.SurfaceAngle + 90.0f)*Mathf.Deg2Rad)*ActivateSpeed;
            Controller.ForcePerformMove<Roll>();
        }

        public override void OnActiveUpdate()
        {
            if (Controller.Grounded) End();
        }

        public override void OnActiveExit()
        {
            if (Animator == null) return;
            if (Controller.Grounded) return;

            if (Controller.RelativeVelocity.y > ActivateSpeed)
            {
                Controller.RelativeVelocity = new Vector2(Controller.RelativeVelocity.x,
                    Controller.RelativeVelocity.y - (ActivateSpeed - ReleaseSpeed));
            }
            else if(Controller.RelativeVelocity.y > ReleaseSpeed)
            {
                Controller.RelativeVelocity = new Vector2(Controller.RelativeVelocity.x, ReleaseSpeed);
            }
        }
    }
}
