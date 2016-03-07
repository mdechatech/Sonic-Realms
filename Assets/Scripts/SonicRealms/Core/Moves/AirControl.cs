using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Moves
{
    public class AirControl : Move
    {
        #region Control Fields
        /// <summary>
        /// The name of the input axis.
        /// </summary>
        [ControlFoldout]
        [Tooltip("The name of the input axis.")]
        public string MovementAxis;

        /// <summary>
        /// Whether to invert the axis input.
        /// </summary>
        [ControlFoldout]
        [Tooltip("Whether to invert the axis input.")]
        public bool InvertAxis;

        #endregion
        #region Physics Fields
        /// <summary>
        /// Air acceleration in units per second squared.
        /// </summary>
        [SerializeField, PhysicsFoldout]
        [Tooltip("Air acceleration in units per second squared.")]
        public float Acceleration;

        /// <summary>
        /// Air deceleration in units per second squared.
        /// </summary>
        [SerializeField, PhysicsFoldout]
        [Tooltip("Air deceleration in units per second squared.")]
        public float Deceleration;

        /// <summary>
        /// Top air speed in units per second.
        /// </summary>
        [SerializeField, PhysicsFoldout]
        [Tooltip("Top air speed in units per second.")]
        public float TopSpeed;
        #endregion

        /// <summary>
        /// If true, disables input.
        /// </summary>
        [HideInInspector]
        public bool ControlLock;

        private float _axis;

        public override MoveLayer Layer
        {
            get { return MoveLayer.Control; }
        }

        public override void Reset()
        {
            base.Reset();

            MovementAxis = "Horizontal";
            InvertAxis = false;

            Acceleration = 3.375f;
            Deceleration = 3.375f;
            TopSpeed = 3.6f;
        }

        public override void Awake()
        {
            base.Awake();
            _axis = 0.0f;
        }

        public override void OnManagerAdd()
        {
            if (!Controller.Grounded) Perform(true);
            Controller.OnDetach.AddListener(OnDetach);
        }

        public void OnDetach()
        {
            Perform();
        }

        public override void OnActiveEnter()
        {
            Manager.End<GroundControl>();
        }

        public override void OnActiveUpdate()
        {
            if (ControlLock) _axis = 0.0f;
            else _axis = InvertAxis ? -Input.GetAxis(MovementAxis) : Input.GetAxis(MovementAxis);

            if (DMath.Equalsf(_axis)) return;
            Controller.FacingForward = _axis > 0.0f;
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
            if(DMath.Equalsf(magnitude)) return;
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
