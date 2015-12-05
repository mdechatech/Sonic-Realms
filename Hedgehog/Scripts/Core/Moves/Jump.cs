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
        public string ActivateButton;

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

        private Transform ClearanceSensorLeft;
        private Transform ClearanceSensorRight;

        public override void Reset()
        {
            base.Reset();

            ActivateButton = "Jump";
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
            Controller.OnAttach.AddListener(OnAttach);
        }

        protected void CreateClearanceSensors()
        {
            if(ClearanceSensorLeft) Destroy(ClearanceSensorLeft.gameObject);
            if (ClearanceSensorRight) Destroy(ClearanceSensorRight.gameObject);

            var offset = (Controller.Sensors.TopCenter.position - Controller.Sensors.Center.position).normalized
                         *ClearanceHeight;
            ClearanceSensorLeft = new GameObject {name = "Clearance Left"}.transform;
            ClearanceSensorLeft.transform.SetParent(Controller.Sensors.transform);
            ClearanceSensorLeft.transform.position = Controller.Sensors.TopLeftStart.position + offset;

            ClearanceSensorRight = new GameObject {name = "Clearance Right"}.transform;
            ClearanceSensorRight.transform.SetParent(Controller.Sensors.transform);
            ClearanceSensorRight.transform.position = Controller.Sensors.TopRightStart.position + offset;
        }

        public void OnDestroy()
        {
            Destroy(ClearanceSensorRight.gameObject);
            Destroy(ClearanceSensorLeft.gameObject);
        }

        public void OnAttach()
        {
            Used = false;
            End();
        }

        public override bool Available()
        {
            return !Manager.IsActive<Duck>() && Controller.Grounded &&
                   !Controller.TerrainCast(ClearanceSensorLeft.position, ClearanceSensorRight.transform.position,
                       ControllerSide.Top);
        }

        public override bool InputActivate()
        {
            return Input.GetButtonDown(ActivateButton);
        }

        public override bool InputDeactivate()
        {
            return Input.GetButtonUp(ActivateButton);
        }

        public override void OnActiveEnter(State previousState)
        {
            Used = true;

            Controller.Detach();
            Controller.Velocity += DMath.AngleToVector((Controller.SurfaceAngle + 90.0f)*Mathf.Deg2Rad)*ActivateSpeed;

            var roll = Manager.GetMove<Roll>();
            if (roll == null) return;

            if (roll.Active)
            {
                // Disable air control if jumping while rolling
                Manager.End<AirControl>();
            }
            else
            {
                Manager.Perform<Roll>(true);
            }
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
