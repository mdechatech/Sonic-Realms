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

        /// <summary>
        /// Height above the controller's center that must be clear to allow jumping, in units.
        /// </summary>
        [Tooltip("Height above the controller's center that must be clear to allow jumping, in units.")]
        public float ClearanceHeight;
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

        private Transform _clearanceSensorLeft;
        private Transform _clearanceSensorRight;

        public override void Reset()
        {
            base.Reset();

            ActivateInput = "Jump";
            ClearanceHeight = 0.25f;

            ActivateSpeed = 3.9f;
            ReleaseSpeed = 2.4f;
        }

        public override void Awake()
        {
            base.Awake();
            Used = false;
        }

        public override void Start()
        {
            base.Start();
            CreateClearanceSensors();
        }

        protected void CreateClearanceSensors()
        {
            if(_clearanceSensorLeft) Destroy(_clearanceSensorLeft.gameObject);
            if (_clearanceSensorRight) Destroy(_clearanceSensorRight.gameObject);

            var offset = (Controller.Sensors.TopCenter.position - Controller.Sensors.Center.position).normalized
                         *ClearanceHeight;
            _clearanceSensorLeft = new GameObject {name = "Clearance Left"}.transform;
            _clearanceSensorLeft.transform.SetParent(Controller.Sensors.transform);
            _clearanceSensorLeft.transform.position = Controller.Sensors.TopLeftStart.position + offset;

            _clearanceSensorRight = new GameObject {name = "Clearance Right"}.transform;
            _clearanceSensorRight.transform.SetParent(Controller.Sensors.transform);
            _clearanceSensorRight.transform.position = Controller.Sensors.TopRightStart.position + offset;
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
            End();
        }

        public override bool Available()
        {
            return !Controller.IsActive<Duck>() && Controller.Grounded &&
                   !Controller.TerrainCast(_clearanceSensorLeft.position, _clearanceSensorRight.transform.position,
                       ControllerSide.Top);
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

        public override void OnActiveExit()
        {
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
