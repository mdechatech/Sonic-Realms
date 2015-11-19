using UnityEngine;

namespace Hedgehog.Core.Moves
{
    public class AirControl : Move
    {
        #region Control Fields
        /// <summary>
        /// The name of the input axis.
        /// </summary>
        [SerializeField]
        [Tooltip("The name of the input axis.")]
        public string MovementAxis;

        /// <summary>
        /// Whether to invert the axis input.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to invert the axis input.")]
        public bool InvertAxis;

        #endregion
        #region Physics Fields

        /// <summary>
        /// Air acceleration in units per second squared.
        /// </summary>
        [SerializeField]
        [Tooltip("Air acceleration in units per second squared.")]
        public float Acceleration;

        /// <summary>
        /// Air deceleration in units per second squared.
        /// </summary>
        [SerializeField]
        [Tooltip("Air deceleration in units per second squared.")]
        public float Deceleration;

        /// <summary>
        /// Top air speed in units per second.
        /// </summary>
        [SerializeField]
        [Tooltip("Top air speed in units per second.")]
        public float TopSpeed;

        #endregion
        #region Animation Fields
        /// <summary>
        /// Name of an Animator float set to horizontal ground speed in units per second.
        /// </summary>
        [SerializeField]
        [Tooltip("Name of an Animator float set to horizontal ground speed in units per second.")]
        public string HorizontalSpeedFloat;

        /// <summary>
        /// Name of an Animator float set to vertical ground speed in units per second.
        /// </summary>
        [SerializeField]
        [Tooltip("Name of an Animator float set to vertical ground speed in units per second.")]
        public string VerticalSpeedFloat;
        #endregion

        private float _axis;

        public override void Reset()
        {
            base.Reset();

            MovementAxis = "Horizontal";
            InvertAxis = false;

            Acceleration = 3.375f;
            Deceleration = 3.375f;
            TopSpeed = 3.6f;

            HorizontalSpeedFloat = VerticalSpeedFloat = "";
        }

        public override void Awake()
        {
            base.Awake();

            _axis = 0.0f;
        }

        public override void OnEnable()
        {
            Controller.OnAttach.AddListener(OnAttach);
            Controller.OnDetach.AddListener(OnDetach);
        }

        public override void OnDisable()
        {
            Controller.OnAttach.RemoveListener(OnAttach);
            Controller.OnDetach.RemoveListener(OnDetach);
        }

        private void OnDetach()
        {
            Perform(true);
        }

        private void OnAttach()
        {
            End();
        }

        public override void SetAnimatorParameters()
        {
            if(!string.IsNullOrEmpty(HorizontalSpeedFloat))
                Animator.SetFloat(HorizontalSpeedFloat, Controller.Velocity.x);

            if(!string.IsNullOrEmpty(VerticalSpeedFloat))
                Animator.SetFloat(VerticalSpeedFloat, Controller.Velocity.y);
        }

        public override void OnActiveUpdate()
        {
            _axis = InvertAxis ? -Input.GetAxis(MovementAxis) : Input.GetAxis(MovementAxis);
        }

        public override void OnActiveFixedUpdate()
        {
            Accelerate(_axis);
        }

        /// <summary>
        /// Accelerates the controller forward.
        /// </summary>
        /// <param name="magnitude">A value between 0 and 1, 0 being no acceleration and 1 being the amount
        /// defined by Acceleration.</param>
        public void AccelerateForward(float magnitude = 1.0f)
        {
            Accelerate(magnitude);
        }

        /// <summary>
        /// Accelerates the controller backward.
        /// </summary>
        /// <param name="magnitude">A value between 0 and 1, 0 being no acceleration and 1 being the amount
        /// defined by Acceleration.</param>
        public void AccelerateBackward(float magnitude = 1.0f)
        {
            Accelerate(-magnitude);
        }

        /// <summary>
        /// Accelerates the controller using Time.fixedDeltaTime as the timestep.
        /// </summary>
        /// <param name="magnitude">A value between -1 and 1, positive moving it forward and negative moving
        /// it back.</param>
        public void Accelerate(float magnitude)
        {
            Accelerate(magnitude, Time.deltaTime);
        }

        /// <summary>
        /// Accelerates the controller.
        /// </summary>
        /// <param name="magnitude">A value between -1 and 1, positive moving it forward and negative moving
        /// it back.</param>
        /// <param name="timestep">The timestep, in seconds</param>
        public void Accelerate(float magnitude, float timestep)
        {
            magnitude = Mathf.Clamp(magnitude, -1.0f, 1.0f);

            if (magnitude < 0.0f)
            {
                var xNew = Controller.RelativeVelocity.x;

                if (Controller.RelativeVelocity.x > 0.0f)
                {
                    xNew += Deceleration*magnitude*timestep;
                }
                else if (Controller.RelativeVelocity.x > -TopSpeed)
                {
                    xNew += Acceleration*magnitude*timestep;
                }

                Controller.RelativeVelocity = new Vector2(xNew, Controller.RelativeVelocity.y);
            }
            else if (magnitude > 0.0f)
            {
                var xNew = Controller.RelativeVelocity.x;

                if (Controller.RelativeVelocity.x < 0.0f)
                {
                    xNew += Deceleration*magnitude*timestep;
                }
                else if (Controller.RelativeVelocity.x < TopSpeed)
                {
                    xNew += Acceleration*magnitude*timestep;
                }

                Controller.RelativeVelocity = new Vector2(xNew, Controller.RelativeVelocity.y);
            }
        }
    }
}
