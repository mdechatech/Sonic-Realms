using SonicRealms.Core.Actors;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Moves
{
    public class Jump : Move
    {
        #region Controls
        /// <summary>
        /// Input string used for activation.
        /// </summary>
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
        [PhysicsFoldout]
        [Tooltip("Jump speed at the moment of activation.")]
        public float ActivateSpeed;

        /// <summary>
        /// Jump speed at the moment of release, in units per second.
        /// </summary>
        [PhysicsFoldout]
        [Tooltip("Jump speed after moment of release, in units per second.")]
        public float ReleaseSpeed;
        #endregion

        /// <summary>
        /// Whether a jump happened. If false, the controller didn't leave the ground by jumping.
        /// </summary>
        [Tooltip("Whether a jump happened. If false, the controller didn't leave the ground by jumping.")]
        public bool Used;

        private Transform ClearanceSensorLeft;
        private Transform ClearanceSensorRight;

        public bool HeldDown
        {
            get { return Input.GetButton(ActivateButton); }
        }

        public override MoveLayer Layer
        {
            get { return MoveLayer.Action; }
        }

        public override bool Available
        {
            get
            {
                var currentAction = Manager[MoveLayer.Action];

                return Controller.Grounded &&
                       (currentAction == null || currentAction is LookUp || currentAction is Push ||
                        currentAction is Skid) &&
                       HasClearance();
            }
        }

        public override bool ShouldPerform
        {
            get { return Input.GetButtonDown(ActivateButton); }
        }

        public override bool ShouldEnd
        {
            get { return Input.GetButtonUp(ActivateButton); }
        }

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

        public bool HasClearance()
        {
            return !Controller.TerrainCast(ClearanceSensorLeft.position, ClearanceSensorRight.position, ControllerSide.Top);
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

        public override void OnActiveEnter(State previousState)
        {
            Used = true;

            Controller.Detach();
            Controller.Velocity += DMath.AngleToVector((Controller.SurfaceAngle + 90.0f)*Mathf.Deg2Rad)*ActivateSpeed;

            var roll = Manager.Get<Roll>();
            if (roll == null) return;

            if (roll.Active)
            {
                // Disable air control if jumping while rolling
                Manager.End<AirControl>();
            }
            else
            {
                Manager.Perform<Roll>(true, true);
            }
        }

        public override void OnActiveExit()
        {
            if (Controller.Grounded) return;

            // Set vertical speed to release speed if it's greater
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
