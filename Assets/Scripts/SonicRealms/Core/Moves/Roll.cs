using SonicRealms.Core.Actors;
using SonicRealms.Core.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace SonicRealms.Core.Moves
{
    /// <summary>
    /// Traditional Sonic rolling. Makes you not as slow going uphill and even faster going downhill.
    /// </summary>
    public class Roll : Move
    {
        #region Public Fields & Properties

        /// <summary>
        /// Input string used for activation.
        /// </summary>
        public string ActivateAxis { get { return _activateAxis; } set { _activateAxis = value; } }

        /// <summary>
        /// Minimum axis input required to start rolling.
        /// </summary>
        public float MinimumInput { get { return _minimumInput; } set { _minimumInput = value; } }

        /// <summary>
        /// Whether to activate when the input is in the opposite direction (if ActivateButton is "Vertical" and this
        /// is true, activates when input moves down instead of up).
        /// </summary>
        public bool InvertAxis { get { return _invertAxis; } set { _invertAxis = value; } }

        /// <summary>
        /// Minimum ground speed required to start rolling, in units per second.
        /// </summary>
        public float MinActivateSpeed { get { return _minActivateSpeed; } set { _minActivateSpeed = value; } }

        /// <summary>
        /// If true, the roll will end when the player stops moving on the ground.
        /// </summary>
        public bool EndOnStop { get { return _endOnStop; } set { _endOnStop = value; } }

        /// <summary>
        /// If true, the roll will end when the player lands on the ground.
        /// </summary>
        public bool EndOnLanding { get { return _endOnLanding; } set { _endOnLanding = value; } }

        /// <summary>
        /// This hitbox becomes harmful while rolling, allowing the player to destroy enemies while rolling.
        /// </summary>
        public SonicHitbox Hitbox { get { return _hitbox; } set { _hitbox = value; } }

        /// <summary>
        /// Change in width (usually negative) while rolling, in units.
        /// </summary>
        public float WidthChange { get { return _widthChange; } set { _widthChange = value; } }

        /// <summary>
        /// Change in sensor height (usually negative) while rolling, in units. The script makes sure that
        /// the controller's ground sensors are still on the ground after the height change.
        /// </summary>
        public float HeightChange { get { return _heightChange; } set { _heightChange = value; } }

        /// <summary>
        /// Slope gravity when rolling uphill, in units per second squared.
        /// </summary>
        public float UphillGravity { get { return _uphillGravity; } set { _uphillGravity = value; } }

        /// <summary>
        /// Slope gravity when rolling downhill, in units per second squared.
        /// </summary>
        public float DownhillGravity { get { return _downhillGravity; } set { _downhillGravity = value; } }

        /// <summary>
        /// Deceleration while rolling, in units per second squared.
        /// </summary>
        public float Deceleration { get { return _deceleration; } set { _deceleration = value; } }
        
        /// <summary>
        /// Friction while rolling, in units per second squared.
        /// </summary>
        public float Friction { get { return _friction; } set { _friction = value; } }

        /// <summary>
        /// Hash value of an Animator bool set to whether the player is going uphill.
        /// Use <see cref="Animator.StringToHash"/>.
        /// </summary>
        public int UphillBoolHash { get; set; }

        public bool IsGoingUphill { get; private set; }

        #endregion

        #region Inspector & Private Fields

        // Input
        [SerializeField, InputFoldout, FormerlySerializedAs("ActivateAxis")]
        [Tooltip("Input string used for activation.")]
        private string _activateAxis;
        
        [Range(0, 1)]
        [SerializeField, InputFoldout, FormerlySerializedAs("MinimumInput")]
        [Tooltip("Minimum axis input required to start rolling.")]
        private float _minimumInput;
        
        [SerializeField, InputFoldout, FormerlySerializedAs("InvertAxis")]
        [Tooltip("Whether to activate when the input is in the opposite direction (if ActivateButton is \"Vertical\" " +
                 "and this is true, activates when input moves down instead of up.")]
        private bool _invertAxis;
        
        [SerializeField, InputFoldout, FormerlySerializedAs("MinActivateSpeed")]
        [Tooltip("Minimum ground speed required to start rolling, in units per second.")]
        private float _minActivateSpeed;

        // Physics
        [SerializeField, PhysicsFoldout]
        [Tooltip("If true, the roll will end when the player stops moving on the ground.")]
        private bool _endOnStop;
        
        [SerializeField, PhysicsFoldout]
        [Tooltip("If true, the roll will end when the player lands on the ground.")]
        private bool _endOnLanding;
        
        [Space]
        [SerializeField, PhysicsFoldout, FormerlySerializedAs("Hitbox")]
        [Tooltip("A hitbox that will become harmful while rolling, allowing the player to destroy " +
                 "enemies while rolling.")]
        private SonicHitbox _hitbox;
        
        [Space]
        [SerializeField, PhysicsFoldout, FormerlySerializedAs("WidthChange")]
        [Tooltip("Change in sensor width (usually negative) while rolling, in units.")]
        private float _widthChange;
        
        [SerializeField, PhysicsFoldout, FormerlySerializedAs("HeightChange")]
        [Tooltip("Change in sensor height (usually negative) while rolling, in units. The script makes sure " +
                 "that the controller's ground sensors are still on the ground after the height change.")]
        private float _heightChange;
        
        [SerializeField, PhysicsFoldout, FormerlySerializedAs("UphillGravity")]
        [Tooltip("Slope gravity when rolling uphill, in units per second squared.")]
        private float _uphillGravity;
        
        [SerializeField, PhysicsFoldout, FormerlySerializedAs("DownhillGravity")]
        [Tooltip("Slope gravity when rolling downhill, in units per second squared.")]
        private float _downhillGravity;
        
        [SerializeField, PhysicsFoldout, FormerlySerializedAs("Deceleration")]
        [Tooltip("Deceleration while rolling, in units per second squared.")]
        private float _deceleration;

        [SerializeField, PhysicsFoldout, FormerlySerializedAs("Friction")]
        [Tooltip("Friction while rolling, in units per second squared.")]
        private float _friction;

        // Animation
        [SerializeField, AnimationFoldout, FormerlySerializedAs("UphillBool")]
        [Tooltip("Name of an Animator bool set to whether the controller is going uphill.")]
        private string _uphillBool;


        private bool _rightDirection;

        private float _originalSlopeGravity;
        private float _originalFriction;
        private float _originalDeceleration;

        #endregion

        protected ScoreCounter Score;
        protected GroundControl GroundControl;

        public override int Layer { get { return (int) MoveLayer.Roll; } }

        public override bool Available
        {
            get { return Controller.Grounded && Mathf.Abs(Controller.GroundVelocity) > MinActivateSpeed; }
        }

        public override bool ShouldEnd
        {
            get
            {
                if (!Controller.Grounded)
                    return false;

                if (!EndOnStop)
                    return false;

                var switchedDirection = (_rightDirection && Controller.GroundVelocity <= 0.0f &&
                                         Controller.GroundVelocity > -MinActivateSpeed) ||
                                        (!_rightDirection && Controller.GroundVelocity >= 0.0f &&
                                         Controller.GroundVelocity < MinActivateSpeed);

                return switchedDirection;
            }
        }

        public override bool ShouldPerform
        {
            get
            {
                var axis = Input.GetAxis(ActivateAxis) * (InvertAxis ? -1 : 1);

                if (axis <= -MinimumInput)
                    axis = -1;
                else if (axis >= MinimumInput)
                    axis = 1;
                else
                    axis = 0;

                return axis == 1;
            }
        }

        public override void Reset()
        {
            base.Reset();

            _uphillBool = "";

            ActivateAxis = "Vertical";
            InvertAxis = true;
            MinimumInput = 0.4f;
            MinActivateSpeed = 0.61875f;

            Hitbox = Controller.GetComponentInChildren<SonicHitbox>();
            HeightChange = -0.10f;
            WidthChange = -0.04f;
            UphillGravity = 2.8125f;
            DownhillGravity = 11.25f;
            Friction = 0.8451f;
            Deceleration = 4.5f;
        }

        public override void Awake()
        {
            base.Awake();
            _rightDirection = false;
            IsGoingUphill = false;

            UphillBoolHash = string.IsNullOrEmpty(_uphillBool) ? 0 : Animator.StringToHash(_uphillBool);
        }

        public override void Start()
        {
            base.Start();
            Controller.OnAttach.AddListener(OnAttach);
            Score = Controller.GetComponent<ScoreCounter>();
        }

        public override void OnManagerAdd()
        {
            GroundControl = Manager.Get<GroundControl>();
        }

        protected void OnAttach()
        {
            if (EndOnLanding)
                End();
        }

        public override void SetAnimatorParameters()
        {
            base.SetAnimatorParameters();
            
            if (!string.IsNullOrEmpty(_uphillBool))
                Controller.Animator.SetBool(_uphillBool, IsGoingUphill);
        }

        public override void OnActiveEnter(State previousState)
        {
            // Store original physics values to restore after leaving the roll
            _rightDirection = Controller.GroundVelocity > 0.0f;

            _originalSlopeGravity = Controller.SlopeGravity;
            _originalFriction = Controller.GroundFriction;
            _originalDeceleration = GroundControl.Deceleration;

            Controller.GroundFriction = Friction;
            GroundControl.DisableAcceleration = true;
            GroundControl.Deceleration = Deceleration;

            // Change player size
            Controller.Sensors.TopOffset += HeightChange/2;
            Controller.Sensors.BottomOffset -= HeightChange/2;
            Controller.Sensors.SolidOffset -= HeightChange/2;

            Controller.Sensors.TrueCenterOffset = Vector2.down*HeightChange*0.5f;

            Controller.Sensors.LedgeWidth += WidthChange;
            Controller.Sensors.BottomWidth += WidthChange;
            Controller.Sensors.TopWidth += WidthChange;

            // Correct the player's position based on how much its height changed to keep it on the ground
            Controller.transform.position += (Vector3)GetPositionOffset();

            // Since sensors have changed so drastically, we should resolve collisions immediately
            Controller.HandleCollisions();

            Hitbox.Harmful = true;
        }

        public override void OnActiveFixedUpdate()
        {
            var previousUphill = IsGoingUphill;

            if (Controller.GroundVelocity > 0.0f)
            {
                IsGoingUphill = DMath.AngleInRange_d(Controller.RelativeSurfaceAngle, 0.0f, 180.0f);
            } else if (Controller.GroundVelocity < 0.0f)
            {
                IsGoingUphill = DMath.AngleInRange_d(Controller.RelativeSurfaceAngle, 180.0f, 360.0f);
            }

            // If physics was set by something else, we should store those values to restore later
            // and then set the them back to the values we wanted
            if (Controller.SlopeGravity != (previousUphill ? UphillGravity : _downhillGravity))
            {
                _originalSlopeGravity = Controller.SlopeGravity;
            }

            if (Controller.GroundFriction != Friction)
            {
                _originalFriction = Controller.GroundFriction;
                Controller.GroundFriction = Friction;
            }

            if (GroundControl.Deceleration != Deceleration)
            {
                _originalDeceleration = GroundControl.Deceleration;
                GroundControl.Deceleration = Deceleration;
            }

            Controller.SlopeGravity = IsGoingUphill ? UphillGravity : _downhillGravity;
        }

        public override void OnActiveExit()
        {
            // Restore physics values the player had before rolling
            Controller.SlopeGravity = _originalSlopeGravity;
            Controller.GroundFriction = _originalFriction;
            GroundControl.DisableAcceleration = false;
            GroundControl.Deceleration = _originalDeceleration;

            // Undo changes to player width and height
            Controller.Sensors.TopOffset -= HeightChange/2;
            Controller.Sensors.BottomOffset += HeightChange/2;
            Controller.Sensors.SolidOffset += HeightChange/2;

            Controller.Sensors.TrueCenterOffset = Vector2.zero;

            Controller.Sensors.LedgeWidth -= WidthChange;
            Controller.Sensors.BottomWidth -= WidthChange;
            Controller.Sensors.TopWidth -= WidthChange;

            // Correct the player's position based on how much its height changed to keep it on the ground
            Controller.transform.position -= (Vector3)GetPositionOffset();

            // Since sensors have changed so drastically, we should resolve collisions immediately
            Controller.HandleCollisions();

            Hitbox.Harmful = false;

            if(Score) Score.EndCombo();
        }

        /// <summary>
        /// Returns the vector that is added to the player's position when it rolls and subtracted when it exits a roll.
        /// </summary>
        public Vector2 GetPositionOffset()
        {
            if (Controller.WallMode != WallMode.None)
            {
                return DMath.UnitVector(Controller.RelativeAngle(Controller.WallMode.ToNormal())*Mathf.Deg2Rad)*
                       HeightChange/2;
            }

            return DMath.UnitVector((Controller.GravityDirection + 180)*Mathf.Deg2Rad)*HeightChange/2;
        }
    }
}
