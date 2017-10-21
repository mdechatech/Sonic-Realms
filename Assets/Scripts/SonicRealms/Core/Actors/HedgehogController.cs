using System.Collections.Generic;
using System.Linq;
using System.Text;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using SonicRealms.Level.Areas;
using UnityEngine;
using UnityEngine.Events;

namespace SonicRealms.Core.Actors
{
    /// <summary>
    /// Implements Sonic-style physics and lets platform triggers know when they're touched.
    /// </summary>
    [SelectionBase]
    [RequireComponent(typeof(Rigidbody2D))]
    public class HedgehogController : MonoBehaviour
    {
        #region Components
        /// <summary>
        /// Game object that contains the renderer.
        /// </summary>
        [Tooltip("Game object that contains the renderer.")]
        public GameObject RendererObject;

        /// <summary>
        /// The controller's animator, if any.
        /// </summary>
        [Tooltip("The controller's animator, if any.")]
        public Animator Animator;

        /// <summary>
        /// Contains sensor data for hit detection.
        /// </summary>
        [Tooltip("Contains sensor data for hit detection.")]
        public HedgehogSensors Sensors;
        #endregion

        #region Events
        /// <summary>
        /// Invoked when the controller lands on something.
        /// </summary>
        public UnityEvent OnAttach;

        /// <summary>
        /// Invoked right before the controller will collide with a platform. Listeners of this event
        /// may call IgnoreThisCollision() on the controller to prevent the collision from occurring.
        /// </summary>
        public PlatformPreCollisionEvent OnPreCollide;

        /// <summary>
        /// Invoked when the controller collides with a platform.
        /// </summary>
        public PlatformCollisionEvent OnCollide;

        /// <summary>
        /// Invoked when the controller detaches from the ground for any reason.
        /// </summary>
        public UnityEvent OnDetach;

        /// <summary>
        /// Invoked when the controller detaches from the ground because it was moving too slowly on a steep
        /// surface.
        /// </summary>
        public UnityEvent OnSteepDetach;

        /// <summary>
        /// Invoked when the controller enters a reactive area.
        /// </summary>
        public ReactiveAreaEvent OnAreaEnter;

        /// <summary>
        /// Invoked when the controller exits a reactive area.
        /// </summary>
        public ReactiveAreaEvent OnAreaExit;

        /// <summary>
        /// Invoked when the controller begins colliding with a reactive platform.
        /// </summary>
        public ReactivePlatformEvent OnPlatformCollisionEnter;

        /// <summary>
        /// Invoked when the controller stops colliding with a reactive platform.
        /// </summary>
        public ReactivePlatformEvent OnPlatformCollisionExit;

        /// <summary>
        /// Invoked when the controller lands on the surface of a reactive platform.
        /// </summary>
        public ReactivePlatformEvent OnPlatformSurfaceEnter;

        /// <summary>
        /// Invoked when the controller leaves on the surface of a reactive platform.
        /// </summary>
        public ReactivePlatformEvent OnPlatformSurfaceExit;

        /// <summary>
        /// Invoked when the controller activates a reactive object.
        /// </summary>
        public ReactiveEffectEvent OnEffectActivate;

        /// <summary>
        /// Invoked when the controller deactivates a reactive object.
        /// </summary>
        public ReactiveEffectEvent OnEffectDeactivate;
        #endregion

        #region Trigger Stuff
        /// <summary>
        /// What reactives the controller is on.
        /// </summary>
        [Tooltip("What reactives the controller is on.")]
        public List<BaseReactive> Reactives;
        #endregion

        #region Physics Stuff

        #region Physics Values
        /// <summary>
        /// The default direction of gravity, in degrees.
        /// </summary>
        private const float DefaultGravityDirection = 270.0f;

        /// <summary>
        /// The player's friction on the ground in units per second per second.
        /// </summary>
        [Tooltip("Ground friction in units per second squared.")]
        public float GroundFriction;

        /// <summary>
        /// This coefficient is applied to horizontal speed when horizontal and vertical speed
        /// requirements are met.
        /// </summary>
        [Tooltip("Air drag coefficient applied if horizontal speed is greater than air drag speed.")]
        public float AirDrag;

        /// <summary>
        /// The speed above which air drag begins.
        /// </summary>
        [Tooltip("The speed above which air drag begins.")]
        public Vector2 AirDragRequiredSpeed;

        /// <summary>
        /// The acceleration by gravity in units per second per second.
        /// </summary>
        [Tooltip("Acceleration in the air, in the downward direction, in units per second squared.")]
        public float AirGravity;

        /// <summary>
        /// The magnitude of the force applied to the player when going up or down slopes.
        /// </summary>
        [Tooltip("Acceleration on downward slopes, the maximum being this value, in units per second squared")]
        public float SlopeGravity;

        /// <summary>
        /// Direction of gravity. 0/360 is right, 90 is up, 180 is left, 270 is down.
        /// </summary>
        [Tooltip("Direction of gravity in degrees. 0/360 is right, 90 is up, 180 is left, 270 is down.")]
        public float GravityDirection;

        /// <summary>
        /// Maximum speed in units per second. The controller can NEVER go faster than this.
        /// </summary>
        [Tooltip("Maximum speed in units per second. The controller can NEVER go faster than this.")]
        public float MaxSpeed;

        /// <summary>
        /// The maximum height of a surface above the player's feet that it can snap onto without hindrance.
        /// </summary>
        [Tooltip("The maximum height of a surface above the player's feet that it can snap onto without hindrance.")]
        public float LedgeClimbHeight;

        /// <summary>
        /// The maximum depth of a surface below the player's feet that it can drop to without hindrance.
        /// </summary>
        [Tooltip("The maximum depth of a surface below the player's feet that it can drop to without hindrance.")]
        public float LedgeDropHeight;

        /// <summary>
        /// The minimum speed in units per second the player must be moving at to stagger each physics update,
        /// processing the movement in fractions.
        /// </summary>
        [Tooltip("The minimum speed in units per second the player must be moving at to stagger each physics update, " +
                 "processing the movement in fractions.")]
        public float AntiTunnelingSpeed;

        /// <summary>
        /// The minimum angle of an incline at which slope gravity is applied.
        /// </summary>
        [Tooltip("The minimum angle of an incline at which slope gravity is applied.")]
        public float SlopeGravityBeginAngle;

        /// <summary>
        /// The controller falls off walls and ceilings if moving below this speed.
        /// </summary>
        [Tooltip("The controller falls off walls and ceilings if moving below this speed.")]
        public float DetachSpeed;

        /// <summary>
        /// The controller can't latch onto walls if they're this much steeper than the floor it's on, in degrees.
        /// </summary>
        [Tooltip("The controller can't latch onto walls if they're this much steeper than the floor it's on, in degrees.")]
        public float MaxClimbAngle;

        /// <summary>
        /// After the controller switches wall mode, it can't revert the action until this much time has passed, in seconds.
        /// This prevents glitches at 45 degree angles that are the result of using non tile-based collision.
        /// </summary>
        [Tooltip("After the controller switches wall mode, it can't revert the action until this much time has passed, in seconds. " +
                 "This prevents glitches at 45 degree angles that are the result of using non tile-based collision.")]
        public float WallModeRevertBufferTime;

        /// <summary>
        /// The maximum ground speed at which the wall mode revert buffer activates, in units per second.
        /// </summary>
        [Tooltip("The maximum ground speed at which the wall mode revert buffer activates, in units per second.")]
        public float RevertBufferMaxSpeed;
        #endregion

        #region Physics Flags/Performance
        /// <summary>
        /// Whether the controller has collisions and physics turned off. Often used for set pieces.
        /// </summary>
        [Tooltip("Whether the controller has collisions and physics turned off. Often used for set pieces.")]
        public bool Interrupted;

        /// <summary>
        /// Time left on the current interruption, if any.
        /// </summary>
        [Tooltip("Time left on the current interruption, if any.")]
        public float InterruptTimer;

        /// <summary>
        /// If true, platform triggers are not notified when touched.
        /// </summary>
        [Tooltip("If true, platform triggers are not notified when touched.")]
        public bool DisableNotifyPlatforms;

        /// <summary>
        /// If true, events are not invoked.
        /// </summary>
        [Tooltip("If true, events are not invoked.")]
        public bool DisableEvents;

        /// <summary>
        /// If true, no ceiling checks are made.
        /// </summary>
        [Tooltip("If checked, ceiling collisions do not occur.")]
        public bool DisableCeilingCheck;

        /// <summary>
        /// If true, no ground checks are made.
        /// </summary>
        [Tooltip("If checked, ground collisions do not occur.")]
        public bool DisableGroundCheck;

        /// <summary>
        /// If true, no side checks are made.
        /// </summary>
        [Tooltip("If checked, side collisions do not occur.")]
        public bool DisableSideCheck;

        /// <summary>
        /// If true, no solid object checks are made.
        /// </summary>
        [Tooltip("If checked, solid object collisions do not occur.")]
        public bool DisableSolidObjectCheck;

        /// <summary>
        /// Whether to move the controller based on current velocity.
        /// </summary>
        public bool DisableMovement;

        /// <summary>
        /// Whether to disable forces acting on the controller.
        /// </summary>
        public bool DisableForces;

        /// <summary>
        /// Whether to apply slope gravity on the controller when it's on steep ground.
        /// </summary>
        public bool DisableSlopeGravity;

        /// <summary>
        /// Whether to apply ground friction on the controller when it's on the ground.
        /// </summary>
        public bool DisableGroundFriction;

        /// <summary>
        /// Whether to fall off walls and loops if ground speed is below DetachSpeed.
        /// </summary>
        public bool DisableWallDetach;

        /// <summary>
        /// When true, prevents the controller from falling off surfaces.
        /// </summary>
        public bool DisableDetach;

        /// <summary>
        /// When true, prevents the controller from attaching to surfaces.
        /// </summary>
        public bool DisableAttach;

        /// <summary>
        /// Whether to apply air gravity on the controller when it's in the air.
        /// </summary>
        public bool DisableAirGravity;

        /// <summary>
        /// Whether to apply air drag on the controller when it's in the air.
        /// </summary>
        public bool DisableAirDrag;

        /// <summary>
        /// Whether to ignore collisions with EVERYTHING.
        /// </summary>
        public bool DisablePlatformCollisions;
        #endregion

        #region Physics State Variables
        #region Sensor State Variables
        /// <summary>
        /// The rotation angle of the sensors in degrees.
        /// </summary>
        public float SensorsRotation
        {
            get { return Sensors.transform.eulerAngles.z; }
            set
            {
                if (value == Sensors.transform.eulerAngles.z) return;

                Sensors.transform.eulerAngles = new Vector3(
                    Sensors.transform.eulerAngles.x,
                    Sensors.transform.eulerAngles.y,
                    value);
            }
        }
        #endregion
        #region Movement State Variables
        /// <summary>
        /// Whether the controller is facing forward or backward. This doesn't actually affect graphics, but it's
        /// used for making dashes go in the right direction.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether the controller is facing forward or backward. This doesn't actually affect graphics, but it's " +
                 "used for making dashes go in the right direction..")]
        private bool _isFacingForward;
        public bool IsFacingForward
        {
            get { return _isFacingForward; }
            set
            {
                _isFacingForward = value;
                if (Animator != null && FacingForwardBoolHash != 0)
                    Animator.SetBool(FacingForwardBoolHash, value);
            }
        }

        /// <summary>
        /// The controller's velocity as a Vector2. If set while on the ground, the controller will not
        /// fall off and instead set ground velocity to the specified value's scalar projection onto the surface.
        /// </summary>
        public Vector2 Velocity
        {
            get { return new Vector2(Vx, Vy); }
            set
            {
                if (Grounded)
                {
                    SetGroundVelocity(value);
                }
                else
                {
                    Vx = value.x;
                    Vy = value.y;
                }
            }
        }

        /// <summary>
        /// The controller's velocity relative to the direction of gravity. If gravity is down (270),
        /// relative velocity is the same as velocity.
        /// </summary>
        public Vector2 RelativeVelocity
        {
            get { return SrMath.RotateBy(Velocity, (DefaultGravityDirection - GravityDirection) * Mathf.Deg2Rad); }
            set { Velocity = SrMath.RotateBy(value, (GravityDirection - DefaultGravityDirection) * Mathf.Deg2Rad); }
        }

        /// <summary>
        /// Direction of "right" given the current direction of gravity, in degrees.
        /// </summary>
        public float GravityRight
        {
            get { return GravityDirection + 90.0f; }
            set { GravityDirection = value - 90.0f; }
        }

        /// <summary>
        /// A unit vector pointing in the "downward" direction based on the controller's GravityDirection.
        /// </summary>
        public Vector2 GravityDownVector
        {
            get { return SrMath.UnitVector(GravityDirection * Mathf.Deg2Rad); }
            set { GravityDirection = SrMath.Modp(SrMath.Angle(value) * Mathf.Rad2Deg, 360.0f); }
        }

        /// <summary>
        /// The player's horizontal velocity in units per second.
        /// </summary>
        [Tooltip("The player's horizontal velocity in units per second.")]
        public float Vx;

        /// <summary>
        /// The player's vertical velocity in units per second.
        /// </summary>
        [Tooltip("The player's vertical velocity in units per second.")]
        public float Vy;

        /// <summary>
        /// The controller's velocity on the ground in units per second. Positive if forward, negative if backward.
        /// </summary>
        [Tooltip("The controller's velocity on the ground in units per second. Positive if forward, negative if backward.")]
        public float GroundVelocity;
        #endregion
        #region Collision State Variables
        /// <summary>
        /// Mask representing the layers the controller collides with.
        /// </summary>
        [Tooltip("Mask representing the layers the controller collides with.")]
        public LayerMask CollisionMask;

        /// <summary>
        /// If grounded, the surface which is currently defining the controller's position
        /// and rotation.
        /// </summary>
        [HideInInspector]
        public Transform PrimarySurface;

        /// <summary>
        /// The results from the terrain cast which found the primary surface, if any.
        /// </summary>
        public TerrainCastHit PrimarySurfaceHit;

        /// <summary>
        /// The surface that the controller's secondary sensor is on, if any. This is set only if on a
        /// flat surface, where both ground sensors would be planted firmly on the ground.
        /// </summary>
        [HideInInspector]
        public Transform SecondarySurface;

        /// <summary>
        /// The results from the terrain cast which found the secondary surface, if any. This is set even if
        /// the secondary sensor isn't planted firmly on the ground.
        /// 
        /// SecondarySurface could be null and this value not if the secondary sensors find a surface but had to
        /// dip below PrimarySurfaceHit to find it.
        /// </summary>
        public TerrainCastHit SecondarySurfaceHit;

        public Transform LeftSurface
        {
            get
            {
                if (Side == GroundSensorType.Left)
                    return PrimarySurface;

                if (Side == GroundSensorType.Right)
                    return SecondarySurface;

                return null;
            }
        }

        public TerrainCastHit LeftSurfaceHit
        {
            get
            {
                if (Side == GroundSensorType.Left)
                    return PrimarySurfaceHit;

                if (Side == GroundSensorType.Right)
                    return SecondarySurfaceHit;

                return null;
            }
        }

        public Transform RightSurface
        {
            get
            {
                if (Side == GroundSensorType.Right)
                    return PrimarySurface;

                if (Side == GroundSensorType.Left)
                    return SecondarySurface;

                return null;
            }
        }

        public TerrainCastHit RightSurfaceHit
        {
            get
            {
                if (Side == GroundSensorType.Right)
                    return PrimarySurfaceHit;

                if (Side == GroundSensorType.Left)
                    return SecondarySurfaceHit;

                return null;
            }
        }

        /// <summary>
        /// The surface to the left of the player, if any.
        /// </summary>
        [Tooltip("The surface to the left of the player, if any.")]
        public Transform LeftWall;

        /// <summary>
        /// The results from the terrain cast which found the surface to the left of the player, if any.
        /// </summary>
        [Tooltip("The results from the terrain cast which found the surface to the left of the player, if any.")]
        public TerrainCastHit LeftWallHit;

        /// <summary>
        /// The surface to the right of the player, if any.
        /// </summary>
        [Tooltip("The surface to the right of the player, if any.")]
        public Transform RightWall;

        /// <summary>
        /// The results from the terrain cast which found the surface to the right of the player, if any.
        /// </summary>
        [Tooltip("The results from the terrain cast which found the surface to the right of the player, if any.")]
        public TerrainCastHit RightWallHit;

        /// <summary>
        /// The ceiling at the left side of the player, if any.
        /// </summary>
        [Tooltip("The ceiling at the left side of the player, if any.")]
        public Transform LeftCeiling;

        /// <summary>
        /// The results from the terrain cast which found the surface at the left ceiling of the player, if any.
        /// </summary>
        [Tooltip("The results from the terrain cast which found the surface at the left ceiling of the player, if any.")]
        public TerrainCastHit LeftCeilingHit;

        /// <summary>
        /// The ceiling at the right side of the player, if any.
        /// </summary>
        [Tooltip("The ceiling at the right side of the player, if any.")]
        public Transform RightCeiling;

        /// <summary>
        /// The results from the terrain cast which found the surface at the right ceiling of the player, if any.
        /// </summary>
        [Tooltip("The results from the terrain cast which found the surface at the right ceiling of the player, if any.")]
        public TerrainCastHit RightCeilingHit;
        #endregion
        #region Surface State Variables
        /// <summary>
        /// Whether the player is touching the ground.
        /// </summary>
        [Tooltip("Whether the player is touching the ground.")]
        public bool Grounded;

        /// <summary>
        /// If grounded, angle of the surface the controller is on. It is smoothed between both sensors -
        /// true surface angles can be found from PrimarySurfaceHit and SecondarySurfaceHit.
        /// </summary>
        public float SurfaceAngle
        {
            get { return _surfaceAngle; }
            set { _surfaceAngle = SrMath.PositiveAngle_d(value); }
        }

        [SerializeField]
        [Tooltip("If grounded, angle of the surface the controller is on.")]
        private float _surfaceAngle;

        /// <summary>
        /// If grounded, the angle's surface relative to the direction of gravity. If gravity points down, this is
        /// the same as SurfaceAngle.
        /// </summary>
        public float RelativeSurfaceAngle
        {
            get { return RelativeAngle(SurfaceAngle); }
            set { SurfaceAngle = AbsoluteAngle(value); }
        }

        /// <summary>
        /// If grounded, which sensor on the player defines the primary surface.
        /// </summary>
        [HideInInspector]
        public GroundSensorType Side;

        /// <summary>
        /// If grounded, the current wall mode (determines which way sensors are oriented).
        /// </summary>
        [HideInInspector]
        public WallMode WallMode;

        /// <summary>
        /// <see cref="WallModeRevertBufferTime"/>
        /// If active, the time remaining on the wall mode revert buffer in seconds.
        /// </summary>
        public float WallModeRevertBufferTimer;

        /// <summary>
        /// <see cref="WallModeRevertBufferTime"/>
        /// The controller's previous wall mode as stored by the revert buffer.
        /// </summary>
        public WallMode WallModeRevertBuffer;

        /// <summary>
        /// Whether the player attached to the gorund in the previous FixedUpdate.
        /// </summary>
        [HideInInspector]
        public bool JustAttached;

        /// <summary>
        /// Whether the player detached from the ground in the previous FixedUpdate.
        /// </summary>
        [HideInInspector]
        public bool JustDetached;

        private ISurfaceSolver _surfaceSolver;

        private SurfaceSolverMediator _surfaceSolverMediator;
        #endregion
        #endregion

        #endregion

        #region Animation Stuff
        /// <summary>
        /// Name of an Animator trigger set when the controller falls off a surface.
        /// </summary>
        [Tooltip("Name of an Animator trigger set when the controller falls off a surface.")]
        public string DetachTrigger;
        protected int DetachTriggerHash;

        /// <summary>
        /// Name of an Animator trigger set when the controller lands on a surface.
        /// </summary>
        [Tooltip("Name of an Animator trigger set when the controller lands on a surface.")]
        public string AttachTrigger;
        protected int AttachTriggerHash;

        /// <summary>
        /// Name of an Animator bool set to whether the controller is facing forward.
        /// </summary>
        [Tooltip("Name of an Animator bool set to whether the controller is facing forward.")]
        public string FacingForwardBool;
        protected int FacingForwardBoolHash;

        /// <summary>
        /// Name of an Animator float set to horizontal air speed, in units per second.
        /// </summary>
        [Tooltip("Name of an Animator float set to horizontal air speed, in units per second.")]
        public string AirSpeedXFloat;
        protected int AirSpeedXFloatHash;

        /// <summary>
        /// Name of an Animator float set to vertical air speed, in units per second.
        /// </summary>
        [Tooltip("Name of an Animator float set to vertical air speed, in units per second.")]
        public string AirSpeedYFloat;
        protected int AirSpeedYFloatHash;

        /// <summary>
        /// Name of Animator float set to whether the controller is grounded.
        /// </summary>
        [Tooltip("Name of Animator float set to whether the controller is grounded.")]
        public string GroundedBool;
        protected int GroundedBoolHash;

        /// <summary>
        /// Name of an Animator float set to the controller's ground speed, in units per second.
        /// </summary>
        [Tooltip("Name of an Animator float set to the controller's ground speed, in units per second.")]
        public string GroundSpeedFloat;
        protected int GroundSpeedFloatHash;

        /// <summary>
        /// Name of an Animator float set to the absolute value of the controller's ground speed, in units per second.
        /// </summary>
        [Tooltip("Name of an Animator float set to the absolute value of the controller's ground speed, in units per " +
                 "second.")]
        public string AbsGroundSpeedFloat;
        protected int AbsGroundSpeedFloatHash;

        /// <summary>
        /// Name of an Animator bool set to whether the controller has any ground speed.
        /// </summary>
        [Tooltip("Name of an Animator bool set to whether the controller has any ground speed.")]
        public string GroundSpeedBool;
        protected int GroundSpeedBoolHash;

        /// <summary>
        /// Name of an Animator float set to the surface angle, in degrees.
        /// </summary>
        [Tooltip("Name of an Animator float set to the surface angle, in degrees.")]
        public string SurfaceAngleFloat;
        protected int SurfaceAngleFloatHash;
        #endregion

        #region Command Buffer Stuff
        public enum BufferEvent
        {
            BeforeForces,
            AfterForces,
            BeforeMovement,
            AfterMovement,
            BeforeCollisions,
            AfterCollisions
        }

        public delegate void CommandBuffer(HedgehogController controller);
        protected Dictionary<BufferEvent, List<CommandBuffer>> CommandBuffers;
        #endregion

        #region Debug Stuff
        public bool ShowDebugGraphics;
        #endregion


        #region Lifecycle Functions
        public virtual void Reset()
        {
            RendererObject = GetComponentInChildren<Renderer>().gameObject;
            Animator = RendererObject.GetComponent<Animator>() ?? GetComponentInChildren<Animator>();

            CollisionMask = CollisionLayers.DefaultMask;
            Reactives = new List<BaseReactive>();

            Sensors = GetComponentInChildren<HedgehogSensors>();

            // Set default physics values to those of Sonic's
            MaxSpeed = 9001.0f;
            GroundFriction = 1.6785f;
            GravityDirection = 270.0f;
            SlopeGravity = 4.5f;
            AirGravity = 7.857f;
            AirDrag = 0.1488343f;   // Air drag is 0.96875 per frame @ 60fps, 0.96875^60 ~= 0.1488343 per second
            AirDragRequiredSpeed = new Vector2(0.075f, 2.4f);
            AntiTunnelingSpeed = 4.0f;
            DetachSpeed = 1.5f;
            SlopeGravityBeginAngle = 10.0f;
            LedgeClimbHeight = 0.16f;
            LedgeDropHeight = 0.16f;
            MaxClimbAngle = 60.0f; // This might be a bit too high, we'll see
            WallModeRevertBufferTime = 0.5f;
            RevertBufferMaxSpeed = 1.5f;
        }

        public void Awake()
        {
            IsFacingForward = true;

            _surfaceSolverMediator = new SurfaceSolverMediator(this);

            OnAttach = OnAttach ?? new UnityEvent();
            OnPreCollide = OnPreCollide ?? new PlatformPreCollisionEvent();
            OnCollide = OnCollide ?? new PlatformCollisionEvent();
            OnDetach = OnDetach ?? new UnityEvent();
            OnSteepDetach = OnSteepDetach ?? new UnityEvent();
            
            OnAreaEnter = OnAreaEnter ?? new ReactiveAreaEvent();
            OnAreaExit = OnAreaExit ?? new ReactiveAreaEvent();
            OnPlatformCollisionEnter = OnPlatformCollisionEnter ?? new ReactivePlatformEvent();
            OnPlatformCollisionExit = OnPlatformCollisionExit ?? new ReactivePlatformEvent();
            OnPlatformSurfaceEnter = OnPlatformSurfaceEnter ?? new ReactivePlatformEvent();
            OnPlatformSurfaceExit = OnPlatformSurfaceExit ?? new ReactivePlatformEvent();
            OnEffectActivate = OnEffectActivate ?? new ReactiveEffectEvent();
            OnEffectDeactivate = OnEffectDeactivate ?? new ReactiveEffectEvent();

            CommandBuffers = new Dictionary<BufferEvent, List<CommandBuffer>>();

            if (Animator == null) return;
            DetachTriggerHash = Animator.StringToHash(DetachTrigger);
            AttachTriggerHash = Animator.StringToHash(AttachTrigger);
            FacingForwardBoolHash = Animator.StringToHash(FacingForwardBool);
            AirSpeedXFloatHash = Animator.StringToHash(AirSpeedXFloat);
            AirSpeedYFloatHash = Animator.StringToHash(AirSpeedYFloat);
            GroundedBoolHash = Animator.StringToHash(GroundedBool);
            GroundSpeedFloatHash = Animator.StringToHash(GroundSpeedFloat);
            GroundSpeedBoolHash = Animator.StringToHash(GroundSpeedBool);
            AbsGroundSpeedFloatHash = Animator.StringToHash(AbsGroundSpeedFloat);
            SurfaceAngleFloatHash = Animator.StringToHash(SurfaceAngleFloat);
        }

        public void Start()
        {
            if (!Sensors.HasBottomSensors && !DisableGroundCheck)
            {
                Debug.LogWarning(string.Format("HedgehogController {0} has no bottom sensors, so Disable Ground Check has been set.",
                    this));
                DisableGroundCheck = true;
            }

            if (!Sensors.HasLedgeSensors && !DisableGroundCheck)
            {
                Debug.LogWarning(string.Format("HedgehogController {0} has no ledge sensors, so Disable Ground Check has been set.",
                    this));
                DisableGroundCheck = true;
            }

            if (!Sensors.HasCeilingSensors && !DisableCeilingCheck)
            {
                Debug.LogWarning(string.Format("HedgehogController {0} has no ceiling sensors, so Disable Ceiling Check has been set.",
                    this));
                DisableCeilingCheck = true;
            }

            if (!Sensors.HasSideSensors && !DisableSideCheck)
            {
                Debug.LogWarning(string.Format("HedgehogController {0} has no side sensors, so Disable Side Check has been set.",
                    this));
                DisableSideCheck = true;
            }

            if (!Sensors.HasSolidSensors && !DisableSolidObjectCheck)
            {
                Debug.LogWarning(string.Format("HedgehogController {0} has no solid object sensors, so Disable Solid Object Check " +
                                               "has been set.",
                                               this));
                DisableSolidObjectCheck = true;
            }
        }

        public void Update()
        {
            if (Animator != null) SetAnimatorParameters();

            if (Interrupted)
            {
                HandleInterrupt();
                return;
            }
        }

        public void FixedUpdate()
        {
            LeftWall = RightWall = LeftCeiling = RightCeiling = null;
            LeftWallHit = RightWallHit = LeftCeilingHit = RightCeilingHit = null;

            if (Interrupted) return;

            UpdateGroundVelocity();

            if (!DisableForces)
            {
                HandleForces();
                UpdateGroundVelocity();
            }

            if (!DisableMovement)
            {
                HandleMovement();
                UpdateGroundVelocity();
            }

            if (ShowDebugGraphics)
            {
                HandleDebugGraphics();
            }
        }
        #endregion


        #region Lifecycle Subroutines
        /// <summary>
        /// Handles the interruption timer, if any.
        /// </summary>
        public void HandleInterrupt()
        {
            HandleInterrupt(Time.deltaTime);
        }

        /// <summary>
        /// Uses Time.deltaTime as the timestep. Handles the interruption timer, if any.
        /// </summary>
        /// <param name="timestep"></param>
        public void HandleInterrupt(float timestep)
        {
            if (!Interrupted || InterruptTimer <= 0.0f) return;
            if ((InterruptTimer -= timestep) <= 0.0f)
            {
                Interrupted = false;
                InterruptTimer = 0.0f;
            }
        }

        /// <summary>
        /// Uses Time.fixedDeltaTime as the timestep. Handles movement caused by velocity.
        /// </summary>
        public void HandleMovement()
        {
            HandleMovement(Time.fixedDeltaTime);
        }

        /// <summary>
        /// Handles movement caused by velocity, including anti-tunneling routines.
        /// </summary>
        public void HandleMovement(float timestep)
        {
            HandleBuffers(BufferEvent.BeforeMovement);
            UpdateWallModeRevertBuffer(Time.fixedDeltaTime);

            bool usedSurfaceSolver = false;

            if (_surfaceSolver != null)
            {
                usedSurfaceSolver = HandleSurfaceSolver(timestep);
            }

            if (usedSurfaceSolver)
            {
                HandleCollisions();
                UpdateGroundVelocity();
            }
            else
            {
                var vt = Velocity.magnitude;
                if (DisablePlatformCollisions)
                {
                    // If there's no collision, we don't have to care about tunneling through walls - 
                    // so just add velocity to position
                    transform.position += new Vector3(Vx * timestep, Vy * timestep);
                    UpdateGroundVelocity();
                }
                else if (SrMath.Equalsf(vt))
                {
                    HandleCollisions();
                    UpdateGroundVelocity();
                }
                else
                {
                    // If the controller's gotta go fast, split up collision into steps so we don't glitch through walls.
                    // The speed limit becomes however many steps your computer can handle!

                    // vc = current velocity - we will subtract from this for each iteration of movement
                    var vc = vt;

                    const int CollisionStepLimit = 100;
                    var steps = 0;
                    while (vc > 0.0f)
                    {
                        if (vc > AntiTunnelingSpeed)
                        {
                            var delta = new Vector3(Vx * timestep, Vy * timestep) * (AntiTunnelingSpeed / vt);

                            // One movement iteration - the movement is limited by AntiTunnelingSpeed
                            transform.position += delta;
                            vc -= AntiTunnelingSpeed;
                        }
                        else
                        {
                            var delta = new Vector3(Vx * timestep, Vy * timestep) * (vc / vt);

                            // If we're less than AntiTunnelingSpeed, just apply the remaining velocity
                            transform.position += delta;// * Mathf.Min(1f, distanceMultiplier);
                            vc = 0f;
                        }

                        HandleCollisions();
                        UpdateGroundVelocity();

                        // Leave early if we've hit the hard limit, come to a complete stop, or gotten interrupted
                        if (++steps > CollisionStepLimit || SrMath.Equalsf(Velocity.magnitude) || Interrupted)
                            break;

                        // If the player's speed changes while we're doing this (ex: the player hits a spring),
                        // recalculate the current 
                        var vn = Mathf.Sqrt(Vx * Vx + Vy * Vy);
                        vc *= vn / vt;
                        vt *= vn / vt;
                    }
                }
            }

            HandleBuffers(BufferEvent.AfterMovement);
        }

        /// <summary>
        /// Uses Time.fixedDeltaTime as the timestep. Applies forces on the player and also handles speed-based conditions,
        /// such as detaching the player if it is too slow on an incline.
        /// </summary>
        private void HandleForces()
        {
            HandleForces(Time.fixedDeltaTime);
        }

        /// <summary>
        /// Applies forces on the player and also handles speed-based conditions, such as detaching the player if it is too slow on
        /// an incline.
        /// </summary>
        private void HandleForces(float timestep)
        {
            HandleBuffers(BufferEvent.BeforeForces);

            if (Grounded)
            {
                var oldGroundVelocity = GroundVelocity;

                // Slope gravity
                if (!DisableSlopeGravity &&
                    !SrMath.AngleInRange_d(RelativeSurfaceAngle, -SlopeGravityBeginAngle, SlopeGravityBeginAngle))
                {
                    var previous = GroundVelocity;
                    GroundVelocity -= SlopeGravity*Mathf.Sin(RelativeSurfaceAngle*Mathf.Deg2Rad)*timestep;
                    if ((previous < 0f && GroundVelocity > 0f) ||
                        (previous > 0f && GroundVelocity < 0f))
                        GroundVelocity = 0f;
                }

                // Ground friction
                if (!DisableGroundFriction)
                    GroundVelocity -= Mathf.Min(Mathf.Abs(GroundVelocity), GroundFriction * timestep) * Mathf.Sign(GroundVelocity);

                // Speed limit
                GroundVelocity = Mathf.Clamp(GroundVelocity, -MaxSpeed, MaxSpeed);

                // Detachment if we're running on a wall and speed is too low
                if (!DisableWallDetach && Mathf.Abs(GroundVelocity) < DetachSpeed &&
                    SrMath.AngleInRange_d(RelativeSurfaceAngle, 89.9f, 270.1f)
                    )
                {
                    var result = Detach();
                    if (!DisableEvents && result)
                    {
                        OnSteepDetach.Invoke();
                    }
                }

                // If we switched directions on the ground, allow the wall mode to revert
                if (oldGroundVelocity*GroundVelocity <= 0)
                    ClearWallModeRevertBuffer();
            }
            else
            {
                // Air gravity
                if (!DisableAirGravity)
                    Velocity += SrMath.UnitVector(GravityDirection*Mathf.Deg2Rad)*AirGravity*timestep;

                // Air drag
                if (!DisableAirDrag &&
                    Vy > 0f && Vy < AirDragRequiredSpeed.y && Mathf.Abs(Vx) > AirDragRequiredSpeed.x)
                {
                    Vx *= Mathf.Pow(AirDrag, timestep);
                }

                // Rotate sensors to direction of gravity
                UpdateSensorRotation();
            }

            HandleBuffers(BufferEvent.AfterForces);
        }

        /// <summary>
        /// Checks for collision with all sensors, changing position, velocity, and rotation if necessary. This should be called each time
        /// the player changes position. This method does not require a timestep because it only resolves overlaps in the player's collision.
        /// </summary>
        public void HandleCollisions()
        {
            if (DisablePlatformCollisions)
                return;

            HandleBuffers(BufferEvent.BeforeCollisions);

            if (!Grounded)
            {
                if (!DisableSideCheck)
                    AirSideCheck();

                if (!DisableCeilingCheck)
                    AirCeilingCheck();

                if (!DisableGroundCheck)
                    AirGroundCheck();

                UpdateSensorRotation();

                JustDetached = false;
            }

            if (Grounded)
            {
                if (!DisableSideCheck)
                    GroundSideCheck();

                if (!DisableSolidObjectCheck && !JustAttached)
                    GroundSolidCheck();

                if (!DisableCeilingCheck)
                    GroundCeilingCheck();

                UpdateGroundVelocity();

                if (!DisableGroundCheck && _surfaceSolver == null)
                    GroundSurfaceCheck();

                UpdateGroundVelocity();

                JustAttached = false;
            }

            HandleBuffers(BufferEvent.AfterCollisions);
        }

        public bool HandleSurfaceSolver()
        {
            return HandleSurfaceSolver(Time.fixedDeltaTime);
        }

        public bool HandleSurfaceSolver(float timestep)
        {
            var solver = _surfaceSolver;
            var mediator = _surfaceSolverMediator;

            if (solver == null)
                return false;

            var translation = SrMath.UnitVector(SurfaceAngle*Mathf.Deg2Rad)*GroundVelocity*timestep;
            mediator.Reset(translation);

            solver.Solve(mediator);

            var error = mediator.Validate();
            if (error != SurfaceSolverMediator.ValidationError.None)
            {
                var errorString = new StringBuilder("Surface solver did not return a valid solution: ");

                if ((error & SurfaceSolverMediator.ValidationError.Position) > 0)
                    errorString.Append("No position specified. ");

                if ((error & SurfaceSolverMediator.ValidationError.Surface) > 0)
                    errorString.Append("No surface specified. ");

                if ((error & SurfaceSolverMediator.ValidationError.SurfaceAngle) > 0)
                    errorString.Append("No surface angle specified. ");

                Debug.LogError(errorString);
                return false;
            }

            if (mediator.CalledDetach)
            {
                _surfaceSolver = null;
                solver.OnSolverRemoved(this);
                return false;
            }

            transform.position = mediator.Position;
            SurfaceAngle = mediator.SurfaceAngle;
            PrimarySurface = mediator.Surface;

            return true;
        }

        /// <summary>
        /// Sets the controller's animator parameters.
        /// </summary>
        public void SetAnimatorParameters()
        {
            if (FacingForwardBoolHash != 0)
                Animator.SetBool(FacingForwardBoolHash, IsFacingForward);

            if (AirSpeedXFloatHash != 0)
                Animator.SetFloat(AirSpeedXFloatHash, Velocity.x);

            if (AirSpeedYFloatHash != 0)
                Animator.SetFloat(AirSpeedYFloatHash, Velocity.y);

            if (GroundedBoolHash != 0)
                Animator.SetBool(GroundedBoolHash, Grounded);

            if (GroundSpeedFloatHash != 0)
                Animator.SetFloat(GroundSpeedFloatHash, GroundVelocity);

            if (GroundSpeedBoolHash != 0)
                Animator.SetBool(GroundSpeedBool, GroundVelocity != 0.0f);

            if (AbsGroundSpeedFloatHash != 0)
                Animator.SetFloat(AbsGroundSpeedFloatHash, Mathf.Abs(GroundVelocity));

            if (SurfaceAngleFloatHash != 0)
                Animator.SetFloat(SurfaceAngleFloatHash, SurfaceAngle);
        }

        /// <summary>
        /// If grounded, sets x and y velocity based on ground velocity.
        /// </summary>
        public void UpdateGroundVelocity()
        {
            if (!Grounded)
                return;

            Vx = GroundVelocity*Mathf.Cos(SurfaceAngle*Mathf.Deg2Rad);
            Vy = GroundVelocity*Mathf.Sin(SurfaceAngle*Mathf.Deg2Rad);
        }

        public void UpdateSensorRotation()
        {
            if (!Grounded)
            {
                SensorsRotation = GravityDirection + 90f;
                return;
            }

            UpdateWallMode();
            SensorsRotation = AbsoluteAngle(WallMode.ToSurface());
        }

        public void UpdateWallMode()
        {
            if (WallMode == WallMode.None)
            {
                WallMode = WallModeUtility.FromSurface(RelativeSurfaceAngle);
                return;
            }

            var relativeSurfaceAngle = RelativeSurfaceAngle;
            const float wallModeTolerance = 5;
            if (WallMode == WallMode.Floor)
            {
                if (relativeSurfaceAngle < 45f + wallModeTolerance ||
                    relativeSurfaceAngle > 315f - wallModeTolerance) return;

                if (relativeSurfaceAngle < 135f)
                    TrySetGroundedWallMode(WallMode.Right);
                else if (relativeSurfaceAngle > 225f)
                    TrySetGroundedWallMode(WallMode.Left);
            }
            else if(WallMode == WallMode.Right)
            {
                if (relativeSurfaceAngle > 135f + wallModeTolerance)
                    TrySetGroundedWallMode(WallMode.Ceiling);
                else if (relativeSurfaceAngle < 45f - wallModeTolerance)
                    TrySetGroundedWallMode(WallMode.Floor);
            }
            else if (WallMode == WallMode.Ceiling)
            {
                if (relativeSurfaceAngle > 225f + wallModeTolerance)
                    TrySetGroundedWallMode(WallMode.Left);
                else if (relativeSurfaceAngle < 135f - wallModeTolerance)
                    TrySetGroundedWallMode(WallMode.Right);
            }
            else
            {
                if (relativeSurfaceAngle > 315f + wallModeTolerance)
                    TrySetGroundedWallMode(WallMode.Floor);
                else if (relativeSurfaceAngle < 225f - wallModeTolerance)
                    TrySetGroundedWallMode(WallMode.Ceiling);
            }
        }

        public void UpdateWallModeRevertBuffer(float deltaTime)
        {
            if (WallModeRevertBufferTimer <= 0)
                return;

            if (Mathf.Abs(GroundVelocity) > RevertBufferMaxSpeed ||
                (WallModeRevertBufferTimer -= deltaTime) <= 0)
                ClearWallModeRevertBuffer();
        }
        #endregion

        #region Collision Subroutines
        /// <summary>
        /// Collision check with side sensors for when player is in the air.
        /// </summary>
        /// <returns><c>true</c>, if a collision was found, <c>false</c> otherwise.</returns>
        private bool AirSideCheck()
        {
            // Get our collision results
            var leftCheck = this.TerrainCast(Sensors.Center.position, Sensors.CenterLeft.position,
                ControllerSide.Left);
            var rightCheck = this.TerrainCast(Sensors.Center.position, Sensors.CenterRight.position,
                ControllerSide.Right);

            if (leftCheck)
            {
                LeftWallHit = leftCheck;
                LeftWall = leftCheck.Transform;

                if (CheckPreCollision(SensorType.CenterLeft, leftCheck))
                {
                    // Push out by the difference between the sensor location and hit location,
                    // And also plus a tiny amount to prevent sticky collisions
                    var push = (Vector3)(leftCheck.Raycast.point - (Vector2)Sensors.CenterLeft.position);
                    transform.position += push + push.normalized * 0.0002f;

                    // Stop the player
                    if (RelativeVelocity.x < 0.0f)
                        RelativeVelocity = new Vector2(0.0f, RelativeVelocity.y);

                    // Invoke events and let platform triggers know we collided
                    NotifyPlatformCollision(SensorType.CenterLeft, leftCheck);

                    return true;
                }
            }

            if (rightCheck)
            {
                RightWallHit = rightCheck;
                RightWall = rightCheck.Transform;

                if (CheckPreCollision(SensorType.CenterRight, rightCheck))
                {
                    // Push out by the difference between the sensor location and hit location,
                    // And also plus a tiny amount to prevent sticky collisions
                    var push = (Vector3)(rightCheck.Raycast.point - (Vector2)Sensors.CenterRight.position);
                    transform.position += push + push.normalized * 0.0002f;

                    // Stop the player
                    if (RelativeVelocity.x > 0.0f)
                        RelativeVelocity = new Vector2(0.0f, RelativeVelocity.y);

                    // Invoke events and let platform triggers know we collided
                    NotifyPlatformCollision(SensorType.CenterRight, rightCheck);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Collision check with air sensors for when player is in the air.
        /// </summary>
        /// <returns><c>true</c>, if a collision was found, <c>false</c> otherwise.</returns>
        private bool AirCeilingCheck()
        {
            // Get our collision results
            var leftCheck = this.TerrainCast(Sensors.TopLeftStart.position, Sensors.TopLeft.position, ControllerSide.Top);
            var rightCheck = this.TerrainCast(Sensors.TopRightStart.position, Sensors.TopRight.position,
                ControllerSide.Top);

            if (!leftCheck && !rightCheck)
                return false;

            var firstCheck = default(TerrainCastHit);
            var firstSensor = SensorType.None;

            var secondCheck = default(TerrainCastHit);
            var secondSensor = SensorType.None;

            if (leftCheck && rightCheck)
            {
                if (SrMath.HeightDifference(leftCheck.Raycast.point, rightCheck.Raycast.point, GravityDirection * Mathf.Deg2Rad) >= 0f)
                {
                    firstCheck = leftCheck;
                    firstSensor = SensorType.TopLeft;

                    secondCheck = rightCheck;
                    secondSensor = SensorType.TopRight;
                }
                else
                {
                    firstCheck = rightCheck;
                    firstSensor = SensorType.TopRight;

                    secondCheck = leftCheck;
                    secondSensor = SensorType.TopLeft;
                }
            }
            else if (leftCheck)
            {
                firstCheck = leftCheck;
                firstSensor = SensorType.TopLeft;
            }
            else // rightCheck must exist here
            {
                firstCheck = rightCheck;
                firstSensor = SensorType.TopRight;
            }

            if (CheckPreCollision(firstSensor, firstCheck))
            {
                transform.position += (Vector3)(firstCheck.Raycast.point - (Vector2)Sensors.Get(firstSensor).position);

                NotifyPlatformCollision(firstSensor, firstCheck);

                var impact = GetImpactResult(firstCheck);
                if (impact.ShouldAttach)
                {
                    Attach(impact);
                }
                else
                {
                    if (RelativeVelocity.y > 0)
                        RelativeVelocity = new Vector2(RelativeVelocity.x, 0);
                }
            }
            else
            {
                if (!secondCheck || !CheckPreCollision(secondSensor, secondCheck))
                    return false;

                transform.position += (Vector3) (secondCheck.Raycast.point - (Vector2) Sensors.Get(secondSensor).position);

                if (RelativeVelocity.y > 0)
                    RelativeVelocity = new Vector2(RelativeVelocity.x, 0);

                NotifyPlatformCollision(secondSensor, secondCheck);

                var impact = GetImpactResult(secondCheck);
                if (impact.ShouldAttach)
                {
                    Attach(impact);
                }
                else
                {
                    if (RelativeVelocity.y > 0)
                        RelativeVelocity = new Vector2(RelativeVelocity.x, 0);
                }
            }

            return true;
        }

        /// <summary>
        /// Collision check with ground sensors for when player is in the air.
        /// </summary>
        /// <returns><c>true</c>, if a collision was found, <c>false</c> otherwise.</returns>
        private bool AirGroundCheck()
        {
            // Get our collision results
            var leftCheck = this.TerrainCast(Sensors.LedgeClimbLeft.position, Sensors.BottomLeft.position,
                ControllerSide.Bottom);
            var rightCheck = this.TerrainCast(Sensors.LedgeClimbRight.position, Sensors.BottomRight.position,
                ControllerSide.Bottom);

            if (!leftCheck && !rightCheck)
                return false;

            var firstCheck = default(TerrainCastHit);
            var firstSensor = SensorType.None;

            var secondCheck = default(TerrainCastHit);
            var secondSensor = SensorType.None;

            if (leftCheck && rightCheck)
            {
                if (SrMath.HeightDifference(leftCheck.Raycast.point, rightCheck.Raycast.point, GravityDirection * Mathf.Deg2Rad) <= 0)
                {
                    firstCheck = leftCheck;
                    firstSensor = SensorType.BottomLeft;

                    secondCheck = rightCheck;
                    secondSensor = SensorType.BottomRight;
                }
                else
                {
                    firstCheck = rightCheck;
                    firstSensor = SensorType.BottomRight;

                    secondCheck = leftCheck;
                    secondSensor = SensorType.BottomLeft;
                }
            }
            else if (leftCheck)
            {
                firstCheck = leftCheck;
                firstSensor = SensorType.BottomLeft;
            }
            else // rightCheck must exist here
            {
                firstCheck = rightCheck;
                firstSensor = SensorType.BottomRight;
            }

            if (CheckPreCollision(firstSensor, firstCheck))
            {
                var impact = GetImpactResult(firstCheck);

                if (impact.ShouldAttach)
                {
                    NotifyPlatformCollision(firstSensor, firstCheck);

                    transform.position += (Vector3)firstCheck.Raycast.point - Sensors.Get(firstSensor).position;

                    SetSurface(firstSensor.ToGroundSensor(), firstCheck);
                    Attach(impact);
                }
                else if (RelativeVelocity.y < 0)
                {
                    transform.position += (Vector3)firstCheck.Raycast.point - Sensors.Get(firstSensor).position;
                }
            }
            else
            {
                if (!secondCheck || !CheckPreCollision(secondSensor, secondCheck))
                    return false;

                var impact = GetImpactResult(secondCheck);

                if (impact.ShouldAttach)
                {
                    NotifyPlatformCollision(secondSensor, secondCheck);

                    transform.position += (Vector3)secondCheck.Raycast.point - Sensors.Get(secondSensor).position;

                    SetSurface(secondSensor.ToGroundSensor(), secondCheck);
                    Attach(impact);
                }
                else if (RelativeVelocity.y < 0)
                {
                    transform.position += (Vector3)secondCheck.Raycast.point - Sensors.Get(secondSensor).position;
                }
            }

            return true;
        }

        /// <summary>
        /// Collision check with side sensors for when player is on the ground.
        /// </summary>
        /// <returns><c>true</c>, if a collision was found, <c>false</c> otherwise.</returns>
        private bool GroundSideCheck()
        {
            // Get collision results
            var leftCheck = this.TerrainCast(Sensors.Center.position, Sensors.CenterLeft.position, ControllerSide.Left);
            var rightCheck = this.TerrainCast(Sensors.Center.position, Sensors.CenterRight.position,
                ControllerSide.Right);

            if (leftCheck && CheckPreCollision(SensorType.CenterLeft, leftCheck))
            {
                LeftWallHit = leftCheck;
                LeftWall = LeftWallHit ? leftCheck.Transform : null;

                // Push out by the difference between the sensor location and hit location,
                // And also plus a tiny amount to prevent sticky collisions
                var push = (Vector3)(leftCheck.Raycast.point - (Vector2)Sensors.CenterLeft.position);
                transform.position += push + push.normalized * 0.0002f;

                var fixedAngle = RelativeAngle(leftCheck.SurfaceAngle*Mathf.Rad2Deg);
                var diff = Mathf.Abs(SrMath.ShortestArc_d(fixedAngle, RelativeSurfaceAngle));

                if (diff < MaxClimbAngle)
                {
                    SurfaceAngle = leftCheck.SurfaceAngle * Mathf.Rad2Deg;
                }
                else
                {
                    if (diff < 95 && (fixedAngle > 315 || fixedAngle < 45))
                    {
                        SurfaceAngle = leftCheck.SurfaceAngle * Mathf.Rad2Deg;
                    }

                    // Stop the player
                    if (GroundVelocity < 0.0f)
                        GroundVelocity = 0.0f;
                }

                UpdateGroundVelocity();
                UpdateSensorRotation();

                NotifyPlatformCollision(SensorType.CenterLeft, leftCheck);

                return true;
            }

            if (rightCheck && CheckPreCollision(SensorType.CenterRight, rightCheck))
            {
                RightWallHit = rightCheck;
                RightWall = RightWallHit ? rightCheck.Transform : null;
                
                var push = (Vector3)(rightCheck.Raycast.point - (Vector2)Sensors.CenterRight.position);
                transform.position += push + push.normalized * 0.0002f;

                var fixedAngle = RelativeAngle(rightCheck.SurfaceAngle * Mathf.Rad2Deg);
                var diff = Mathf.Abs(SrMath.ShortestArc_d(fixedAngle, RelativeSurfaceAngle));

                if (diff < MaxClimbAngle)
                {
                    SurfaceAngle = rightCheck.SurfaceAngle * Mathf.Rad2Deg;
                }
                else
                {
                    if (diff < 95 && (fixedAngle > 315 || fixedAngle < 45))
                    {
                        SurfaceAngle = rightCheck.SurfaceAngle * Mathf.Rad2Deg;
                    }

                    // Stop the player
                    if (GroundVelocity > 0.0f)
                        GroundVelocity = 0.0f;
                }

                UpdateGroundVelocity();
                UpdateSensorRotation();

                HandleDebugGraphics();
                    
                NotifyPlatformCollision(SensorType.CenterRight, rightCheck);

                return true;
            }

            UpdateSensorRotation();
            return false;
        }

        /// <summary>
        /// Collision check with ceiling sensors for when player is on the ground.
        /// </summary>
        /// <returns><c>true</c>, if a collision was found, <c>false</c> otherwise.</returns>
        private bool GroundCeilingCheck()
        {
            var leftCheck = this.TerrainCast(Sensors.CenterLeft.position, Sensors.TopLeft.position, ControllerSide.Left);
            var rightCheck = this.TerrainCast(Sensors.CenterRight.position, Sensors.TopRight.position,
                ControllerSide.Right);

            // TODO invoke ceiling events?

            // Sonic doesn't actually react to ceilings while on the ground!
            // There is nothing to do here but set variables. This information is used for crushers.
            if (leftCheck && CheckPreCollision(SensorType.TopLeft, leftCheck))
            {
                LeftCeilingHit = leftCheck;
                LeftCeiling = leftCheck.Transform;
            }

            if (rightCheck && CheckPreCollision(SensorType.TopRight, rightCheck))
            {
                RightCeilingHit = rightCheck;
                RightCeiling = rightCheck.Transform;
            }

            return false;
        }

        /// <summary>
        /// Collision check with ground sensors for when player is on the ground. This algorithm is the heart
        /// of the engine!
        /// </summary>
        /// <returns><c>true</c>, if a collision was found, <c>false</c> otherwise.</returns>
        private bool GroundSurfaceCheck()
        {
            // Get the surfaces at the left and right sensors
            var left = this.TerrainCast(Sensors.LedgeClimbLeft.position, Sensors.LedgeDropLeft.position,
                ControllerSide.Bottom);

            var right = this.TerrainCast(Sensors.LedgeClimbRight.position, Sensors.LedgeDropRight.position,
                    ControllerSide.Bottom);

            var ledgeClimbRatio = LedgeClimbHeight / (LedgeClimbHeight + LedgeDropHeight);

            if (left != null && left.Raycast.fraction == 0)
                left = null;

            if (right != null && right.Raycast.fraction == 0)
                right = null;
                
            // Get the angles of their surfaces in degrees
            var leftAngle = left ? left.SurfaceAngle * Mathf.Rad2Deg : 0.0f;
            var rightAngle = right ? right.SurfaceAngle * Mathf.Rad2Deg : 0.0f;

            // Get the difference between the two surfaces
            var diff = SrMath.ShortestArc_d(leftAngle, rightAngle);

            // Get the differences between the two surfaces and the angle of the last surface
            var leftDiff = Mathf.Abs(SrMath.ShortestArc_d(SurfaceAngle, leftAngle));
            var rightDiff = Mathf.Abs(SrMath.ShortestArc_d(SurfaceAngle, rightAngle));

            // Get easy checks out of the way

            // If we've found no surface, or they're all too steep, detach from the ground
            if ((!left || leftDiff > MaxClimbAngle) &&
                (!right || rightDiff > MaxClimbAngle))
            {
                goto detach;
            }

            // If only one sensor found a surface we don't need any further checks
            if (!left && right)
                goto orientRight;

            if (left && !right)
                goto orientLeft;

            // If one surface is too steep, use the other (as long as that surface isn't too different from the current)
            const float SteepnessTolerance = 0.25f;
            if (diff > MaxClimbAngle || diff < -MaxClimbAngle)
            {
                if (leftDiff < rightDiff && leftDiff < MaxClimbAngle*SteepnessTolerance)
                    goto orientLeft;

                if (rightDiff < MaxClimbAngle*SteepnessTolerance)
                    goto orientRight;
            }

            // Both sensors have possible surfaces at this point

            // The controller may have both ground sensors register a surface if the secondary sensor's surface is
            // at ground-level plus this much leeway as a fraction of the ledge sensors' total height.
            const float FlatSurfaceTolerance = 0.01f;

            // Choose the sensor to use based on which one found the "higher" floor (based on the wall mode)
            if (SrMath.HeightDifference(left.Raycast.point, right.Raycast.point, AbsoluteAngle(WallMode.ToNormal())*Mathf.Deg2Rad) >= 0f)
            {
                goto orientLeftRight;
            }
            
            goto orientRightLeft;
            
            #region Orientation Goto's
            orientLeft:
            // Use the left sensor as the ground
            if (CheckPreCollision(SensorType.BottomLeft, left))
            {
                Side = GroundSensorType.Left;
                SurfaceAngle = left.SurfaceAngle*Mathf.Rad2Deg;
                transform.position += (Vector3)(left.Raycast.point - (Vector2)Sensors.BottomLeft.position);

                NotifyPlatformCollision(SensorType.BottomLeft, left);

                SetSurface(GroundSensorType.Left, left);
            }

            goto finish;

            orientRight:
            // Use the right sensor as the ground
            if (CheckPreCollision(SensorType.BottomRight, right))
            {
                Side = GroundSensorType.Right;
                SurfaceAngle = right.SurfaceAngle * Mathf.Rad2Deg;
                transform.position += (Vector3)(right.Raycast.point - (Vector2)Sensors.BottomRight.position);

                NotifyPlatformCollision(SensorType.BottomRight, right);

                SetSurface(GroundSensorType.Right, right);
            }

            goto finish;

            orientLeftRight:
            // Use the left sensor as the ground, then see if the right sensor surface should be notified of a collision
            if (CheckPreCollision(SensorType.BottomLeft, left))
            {
                Side = GroundSensorType.Left;
                SurfaceAngle = left.SurfaceAngle * Mathf.Rad2Deg;
                transform.position += (Vector3)(left.Raycast.point - (Vector2)Sensors.BottomLeft.position);

                NotifyPlatformCollision(SensorType.BottomLeft, left);

                if (right.Raycast.fraction < ledgeClimbRatio + FlatSurfaceTolerance &&
                    CheckPreCollision(SensorType.BottomRight, right))
                    NotifyPlatformCollision(SensorType.BottomRight, right);

                SetSurface(GroundSensorType.Left, left, right, right.Raycast.fraction < ledgeClimbRatio + FlatSurfaceTolerance);
            }

            goto finish;

            orientRightLeft:
            // Use the right sensor as the ground, then see if the left sensor surface should be notified of a collision
            if (CheckPreCollision(SensorType.BottomRight, right))
            {
                Side = GroundSensorType.Right;
                SurfaceAngle = right.SurfaceAngle * Mathf.Rad2Deg;
                transform.position += (Vector3)(right.Raycast.point - (Vector2)Sensors.BottomRight.position);

                NotifyPlatformCollision(SensorType.BottomRight, right);

                if (left.Raycast.fraction < ledgeClimbRatio + FlatSurfaceTolerance &&
                    CheckPreCollision(SensorType.BottomLeft, left))
                    NotifyPlatformCollision(SensorType.BottomLeft, left);

                SetSurface(GroundSensorType.Right, right, left, left.Raycast.fraction < ledgeClimbRatio + FlatSurfaceTolerance);
            }

            goto finish;

            detach:

            Detach();
            return false;

            #endregion

            finish:

            UpdateSensorRotation();

            return true;
        }

        /// <summary>
        /// Collision check with solid sensors against objects with the special Solid Object component for when the player is on
        /// the ground.
        /// </summary>
        /// <returns></returns>
        private bool GroundSolidCheck()
        {
            // Get collision results
            var leftCheck = this.TerrainCast(Sensors.SolidCenter.position, Sensors.SolidLeft.position, ControllerSide.Left);
            var rightCheck = this.TerrainCast(Sensors.SolidCenter.position, Sensors.SolidRight.position,
                ControllerSide.Right);

            // TODO This method is a copy of GroundSideCheck other than this part, find some way to merge the parts in common
            if (!leftCheck || !leftCheck.Transform.GetComponent<PreventLedgeClimb>())
            {
                leftCheck = null;
            }

            if (!rightCheck || !rightCheck.Transform.GetComponent<PreventLedgeClimb>())
            {
                rightCheck = null;
            }

            if (leftCheck && CheckPreCollision(SensorType.SolidLeft, leftCheck))
            {
                LeftWallHit = leftCheck;
                LeftWall = LeftWallHit ? leftCheck.Transform : null;

                // Push out by the difference between the sensor location and hit location,
                // And also plus a tiny amount to prevent sticky collisions
                var push = (Vector3)(leftCheck.Raycast.point - (Vector2)Sensors.SolidLeft.position);
                transform.position += push + push.normalized * 0.0002f;

                var fixedAngle = RelativeAngle(leftCheck.SurfaceAngle * Mathf.Rad2Deg);
                var diff = Mathf.Abs(SrMath.ShortestArc_d(fixedAngle, RelativeSurfaceAngle));

                if (diff < MaxClimbAngle)
                {
                    SurfaceAngle = leftCheck.SurfaceAngle * Mathf.Rad2Deg;
                }
                else
                {
                    if (diff < 95 && (fixedAngle > 315 || fixedAngle < 45))
                    {
                        SurfaceAngle = leftCheck.SurfaceAngle * Mathf.Rad2Deg;
                    }

                    // Stop the player
                    if (GroundVelocity < 0.0f)
                        GroundVelocity = 0.0f;
                }

                UpdateGroundVelocity();
                UpdateSensorRotation();

                NotifyPlatformCollision(SensorType.SolidLeft, leftCheck);

                return true;
            }

            if (rightCheck && CheckPreCollision(SensorType.SolidRight, rightCheck))
            {
                RightWallHit = rightCheck;
                RightWall = RightWallHit ? rightCheck.Transform : null;

                var push = (Vector3)(rightCheck.Raycast.point - (Vector2)Sensors.SolidRight.position);
                transform.position += push + push.normalized * 0.0002f;

                var fixedAngle = RelativeAngle(rightCheck.SurfaceAngle * Mathf.Rad2Deg);
                var diff = Mathf.Abs(SrMath.ShortestArc_d(fixedAngle, RelativeSurfaceAngle));

                if (diff < MaxClimbAngle)
                {
                    SurfaceAngle = rightCheck.SurfaceAngle * Mathf.Rad2Deg;
                }
                else
                {
                    if (diff < 95 && (fixedAngle > 315 || fixedAngle < 45))
                    {
                        SurfaceAngle = rightCheck.SurfaceAngle * Mathf.Rad2Deg;
                    }

                    // Stop the player
                    if (GroundVelocity > 0.0f)
                        GroundVelocity = 0.0f;
                }

                UpdateGroundVelocity();
                UpdateSensorRotation();

                NotifyPlatformCollision(SensorType.SolidRight, rightCheck);

                return true;
            }

            UpdateSensorRotation();
            return false;
        }
        #endregion

        #region Debug Subroutines
        protected void HandleDebugGraphics()
        {
            if (Grounded)
            {
                if (PrimarySurfaceHit)
                {
                    Debug.DrawLine(PrimarySurfaceHit.Start, PrimarySurfaceHit.Raycast.point);
                    Debug.DrawLine(PrimarySurfaceHit.Raycast.point, PrimarySurfaceHit.End, Color.gray);
                    DebugUtility.DrawCircle(PrimarySurfaceHit.Raycast.point, 0.03f);

                    Debug.DrawLine(
                        PrimarySurfaceHit.Raycast.point - SrMath.RotateBy(Vector2.right, SurfaceAngle*Mathf.Deg2Rad)*0.1f,
                        PrimarySurfaceHit.Raycast.point + SrMath.RotateBy(Vector2.right, SurfaceAngle*Mathf.Deg2Rad)*0.1f);
                }

                if (SecondarySurfaceHit)
                {
                    if (SecondarySurface)
                    {
                        Debug.DrawLine(SecondarySurfaceHit.Start, SecondarySurfaceHit.Raycast.point);
                        Debug.DrawLine(SecondarySurfaceHit.Raycast.point, SecondarySurfaceHit.End, Color.gray);
                    }
                    else
                    {
                        Debug.DrawLine(SecondarySurfaceHit.Start, SecondarySurfaceHit.End, Color.gray);
                    }

                    DebugUtility.DrawCircle(SecondarySurfaceHit.Raycast.point, 0.03f,
                        SecondarySurface ? Color.white : Color.gray);
                }
                else
                {
                    if(Side == GroundSensorType.Left)
                        Debug.DrawLine(Sensors.LedgeClimbRight.position, Sensors.LedgeDropRight.position, Color.gray);

                    if (Side == GroundSensorType.Right)
                        Debug.DrawLine(Sensors.LedgeClimbLeft.position, Sensors.LedgeDropLeft.position, Color.gray);
                }
            }
        }
        #endregion


        #region Command Buffer Functions
        /// <summary>
        /// Adds a command buffer at the given buffer event. Command buffers can be used to insert code
        /// at certain points in the player's physics routines.
        /// 
        /// For example, one may add a function to be called right after the player has applied forces to 
        /// itself by calling:
        /// 
        /// AddCommandBuffer(theFunction, HedgehogController.BufferEvent.AfterForces);
        /// 
        /// The function must take in a HedgehogController and have no return value.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="evt"></param>
        public void AddCommandBuffer(CommandBuffer buffer, BufferEvent evt)
        {
            List<CommandBuffer> buffers;
            if (!CommandBuffers.TryGetValue(evt, out buffers))
                CommandBuffers[evt] = buffers = new List<CommandBuffer>();
            buffers.Add(buffer);
        }

        public bool HasCommandBuffer(CommandBuffer buffer, BufferEvent evt)
        {
            List<CommandBuffer> buffers;
            if (CommandBuffers.TryGetValue(evt, out buffers))
                return buffers.Contains(buffer);

            return false;
        }

        /// <summary>
        /// Removes the command buffer at the given buffer event.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="evt"></param>
        public void RemoveCommandBuffer(CommandBuffer buffer, BufferEvent evt)
        {
            List<CommandBuffer> buffers;
            if (CommandBuffers.TryGetValue(evt, out buffers))
                buffers.Remove(buffer);
        }

        /// <summary>
        /// Goes through the command buffers for the given buffer event.
        /// </summary>
        /// <param name="evt"></param>
        protected void HandleBuffers(BufferEvent evt)
        {
            List<CommandBuffer> buffers;
            if (CommandBuffers.TryGetValue(evt, out buffers))
            {
                for (var i = buffers.Count - 1; i >= 0; --i)
                    buffers[i](this);
            }
        }
        #endregion

        #region Move and Control Functions
        /// <summary>
        /// Turns off the controller's physics, usually so it can be modified by set pieces.
        /// </summary>
        /// <param name="endMoves">Whether to stop all moves the controller is performing.</param>
        public void Interrupt(bool endMoves = false)
        {
            Interrupted = true;
            InterruptTimer = 0.0f;
        }

        /// <summary>
        /// Turns off the controller's physics, usually so it can be modified by set pieces.
        /// </summary>
        /// <param name="time">The physics turns back on after this time, in seconds.</param>
        /// <param name="endMoves">Whether to stop all moves the controller is performing.</param>
        public void Interrupt(float time, bool endMoves = false)
        {
            Interrupt(endMoves);
            InterruptTimer = Mathf.Max(0.0002f, time);
        }

        /// <summary>
        /// Turns on the controller's physics, usually after leaving a set piece.
        /// </summary>
        public void Resume()
        {
            Interrupted = false;
            InterruptTimer = 0.0f;
        }

        /// <summary>
        /// If grounded, sets ground velocity to the scalar projection of the specified
        /// velocity against the angle of the surface the controller is standing on.
        /// 
        /// For example, if the controller is on a flat horizontal surface and the specified velocity 
        /// points straight up, there will be no change in ground velocity.
        /// </summary>
        /// <param name="velocity">The specified velocity as a vector with x and y components.</param>
        /// <returns>The resulting ground velocity.</returns>
        public float SetGroundVelocity(Vector2 velocity)
        {
            if (!Grounded)
                return 0.0f;

            GroundVelocity = SrMath.ScalarProjectionAbs(velocity, SurfaceAngle * Mathf.Deg2Rad);

            var v = SrMath.UnitVector(SurfaceAngle * Mathf.Deg2Rad) * GroundVelocity;
            Vx = v.x;
            Vy = v.y;

            return GroundVelocity;
        }

        /// <summary>
        /// If grounded, adds to the controller's ground velocity the scalar projection of the specified
        /// velocity against the angle of the surface the controller is standing on.
        /// 
        /// For example, if the controller is on a flat horizontal surface and the specified velocity 
        /// points straight up, there will be no change in ground velocity.
        /// </summary>
        /// <param name="velocity">The specified velocity as a vector with x and y components.</param>
        /// <returns>The resulting change in ground velocity.</returns>
        public float PushOnGround(Vector2 velocity)
        {
            if (!Grounded)
                return 0.0f;

            GroundVelocity += SrMath.ScalarProjectionAbs(velocity, SurfaceAngle * Mathf.Deg2Rad);

            var v = SrMath.UnitVector(SurfaceAngle * Mathf.Deg2Rad) * GroundVelocity;
            Vx = v.x;
            Vy = v.y;

            return GroundVelocity;
        }

        /// <summary>
        /// Applies one step of gravity. Often used to increase bounce speed.
        /// </summary>
        public void ApplyGravityOnce()
        {
            if (Grounded)
                return;
            Velocity += GravityDownVector.normalized * AirGravity * (1f / 60f);
        }

        /// <summary>
        /// Prevents the controller from entering the given wall mode for the duration of the wall mode buffer.
        /// </summary>
        public void StartWallModeBuffer(WallMode wallMode)
        {
            WallModeRevertBuffer = wallMode;
            WallModeRevertBufferTimer = WallModeRevertBufferTime;
        }

        /// <summary>
        /// When on the ground, sets the wall mode if it isn't blocked by the current wallmode buffer.
        /// </summary>
        public bool TrySetGroundedWallMode(WallMode wallMode)
        {
            if (wallMode == WallMode.None || Mathf.Abs(GroundVelocity) > RevertBufferMaxSpeed || 
                wallMode != WallModeRevertBuffer)
            {
                StartWallModeBuffer(WallMode);
                WallMode = wallMode;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Immediately ends the wall mode revert buffer.
        /// </summary>
        private void ClearWallModeRevertBuffer()
        {
            if (WallModeRevertBuffer != WallMode.None)
            {
                WallModeRevertBuffer = WallMode.None;
                WallModeRevertBufferTimer = 0;
            }
        }
        #endregion

        #region Surface Acquisition Functions
        /// <summary>
        /// Detach the player from whatever surface it is on.
        /// <returns>Whether the controller detached successfully.</returns>
        /// </summary>
        public bool Detach()
        {
            if (DisableDetach) return false;
            var wasGrounded = Grounded;

            ClearWallModeRevertBuffer();

            Grounded = false;
            JustDetached = true;
            Side = GroundSensorType.None;
            WallMode = WallMode.None;

            UseCustomSurface(null);

            SetSurface(GroundSensorType.None, null);
            UpdateSensorRotation();

            // Don't invoke the event if the controller was already off the ground
            if (wasGrounded)
            {
                if (!DisableEvents) OnDetach.Invoke();
                if (Animator != null && DetachTriggerHash != 0)
                    Animator.SetTrigger(DetachTriggerHash);
            }
            
            return true;
        }

        public bool Attach(ImpactResult impact)
        {
            return Attach(impact.GroundSpeed, impact.SurfaceAngle);
        }

        /// <summary>
        /// Attaches the player to a surface within the reach of its surface sensors. The angle of attachment
        /// need not be perfect; the method works reliably for angles within 45 degrees of the one specified.
        /// </summary>
        /// <param name="groundSpeed">The ground speed of the player after attaching.</param>
        /// <param name="angleRadians">The angle of the surface, in radians.</param>
        /// <returns>Whether the controller attached successfully.</returns>
        public bool Attach(float groundSpeed, float angleRadians)
        {
            if (DisableAttach) return false;
            var wasGrounded = Grounded;

            // Set the appropriate physics variables
            var angleDegrees = SrMath.PositiveAngle_d(angleRadians*Mathf.Rad2Deg);
            GroundVelocity = groundSpeed;
            SurfaceAngle = angleDegrees;
            JustAttached = true;
            Grounded = true;
            
            if (angleDegrees >= 310 || angleDegrees <= 50)
                WallMode = WallMode.Floor;
            else if(angleDegrees <= 130)
                WallMode = WallMode.Right;
            else if (angleDegrees <= 220)
                WallMode = WallMode.Ceiling;
            else
                WallMode = WallMode.Left;

            UpdateSensorRotation();

            // If we already were on the ground, that's all we needed to do
            if (wasGrounded) return true;

            if (!DisableEvents) OnAttach.Invoke();
            if (Animator != null && AttachTriggerHash != 0)
                Animator.SetTrigger(AttachTriggerHash);

            return true;
        }

        /// <summary>
        /// Calculates the ground velocity as the result of an impact on the specified surface angle.
        /// 
        /// Uses the algorithm here: https://info.sonicretro.org/SPG:Solid_Tiles#Reacquisition_Of_The_Ground
        /// adapted for 360° gravity.
        /// </summary>
        /// <returns>The value the controller's ground velocity should be set to, or null if it should not attach.</returns>
        /// <param name="hit">The impact data as th result of a terrain cast.</param>
        public ImpactResult GetImpactResult(TerrainCastHit hit)
        {
            if (hit == null || hit.Raycast.fraction == 0) return default(ImpactResult);

            var surfaceDegrees = SrMath.PositiveAngle_d(hit.SurfaceAngle * Mathf.Rad2Deg);

            // The player can't land on (static) platforms if he's traveling 90 degrees within their normal
            var playerAngle = SrMath.Angle(Velocity)*Mathf.Rad2Deg;

            const float normalTolerance = 90f;

            if (!SrMath.AngleInRange_d(playerAngle + 180f, 
                surfaceDegrees + 90f - normalTolerance, surfaceDegrees + 90f + normalTolerance))
                return default(ImpactResult);

            var result = 0f;
            var shouldAttach = true;
            var relativeSurfaceAngle = RelativeAngle(surfaceDegrees);

            if (RelativeVelocity.y <= 0.0f)
            {
                if (relativeSurfaceAngle < 22.5f || relativeSurfaceAngle > 337.5f)
                {
                    result = RelativeVelocity.x;
                }
                else if (relativeSurfaceAngle < 45.0f)
                {
                    result = Mathf.Abs(RelativeVelocity.x) > -RelativeVelocity.y
                        ? RelativeVelocity.x
                        : 0.5f * RelativeVelocity.y;
                }
                else if (relativeSurfaceAngle > 315.0f)
                {
                    result = Mathf.Abs(RelativeVelocity.x) > -RelativeVelocity.y
                        ? RelativeVelocity.x
                        : -0.5f * RelativeVelocity.y;
                }
                else if (relativeSurfaceAngle < 90.0f)
                {
                    result = Mathf.Abs(RelativeVelocity.x) > -RelativeVelocity.y
                        ? RelativeVelocity.x
                        : RelativeVelocity.y;
                }
                else if (relativeSurfaceAngle > 265.0f)
                {
                    result = Mathf.Abs(RelativeVelocity.x) > -RelativeVelocity.y
                        ? RelativeVelocity.x
                        : -RelativeVelocity.y;
                }
                else
                {
                    shouldAttach = false;
                }
            }
            else
            {
                if (relativeSurfaceAngle > 90.0f && relativeSurfaceAngle < 135.0f)
                {
                    result = RelativeVelocity.y;
                }
                else if (relativeSurfaceAngle > 220.0f && relativeSurfaceAngle < 275.0f)
                {
                    result = -RelativeVelocity.y;
                }
                else
                {
                    shouldAttach = false;
                }
            }
            
            // If we're on a wall and the proposed impact speed is too low to stay on it, don't attach at all
            if (!DisableWallDetach &&
                (Mathf.Abs(result) < DetachSpeed &&
                 relativeSurfaceAngle > 89.9f && relativeSurfaceAngle < 270.1f))
            {
                return default(ImpactResult);
            }
            
            return new ImpactResult {GroundSpeed = result, ShouldAttach = shouldAttach, SurfaceAngle = hit.SurfaceAngle};
        }

        /// <summary>
        /// Sets the controller's primary and secondary surfaces and triggers their platform events, if any.
        /// </summary>
        /// <param name="primarySurfaceHit">The new primary surface.</param>
        /// <param name="secondarySurfaceHit">The new secondary surface.</param>
        /// <param name="notifySecondary">If true, secondary surface will be stored and notified of collisions.</param>
        public void SetSurface(GroundSensorType primarySide, TerrainCastHit primarySurfaceHit, TerrainCastHit secondarySurfaceHit = null,
            bool notifySecondary = false)
        {
            PrimarySurfaceHit = primarySurfaceHit;

            PrimarySurface = PrimarySurfaceHit
                ? PrimarySurfaceHit.Transform
                : null;

            SecondarySurfaceHit = secondarySurfaceHit;

            SecondarySurface = notifySecondary && SecondarySurfaceHit
                ? SecondarySurfaceHit.Transform
                : null;

            if (DisableNotifyPlatforms)
                return;

            var primaryTrigger = PrimarySurface == null
                ? null
                : PrimarySurface.GetComponent<PlatformTrigger>();

            if (primaryTrigger != null)
                primaryTrigger.NotifySurfaceCollision(
                    new SurfaceCollision.Contact(primarySide, primarySurfaceHit, primaryTrigger));

            if (notifySecondary)
            {
                var secondaryTrigger = SecondarySurface == null
                    ? null
                    : SecondarySurface.GetComponent<PlatformTrigger>();

                if (secondaryTrigger != null)
                    secondaryTrigger.NotifySurfaceCollision(
                        new SurfaceCollision.Contact(primarySide.Flip(), secondarySurfaceHit, secondaryTrigger));
            }
        }

        /// <summary>
        /// Returns the given absolute angle as an angle relative to the controller's gravity.
        /// For example, 0 (flat surface angle) returns 180 (upside-down surface) when gravity
        /// points upward and 0 when gravity points downward.
        /// </summary>
        /// <param name="absoluteAngle">The absolute angle, in degrees.</param>
        /// <returns></returns>
        public float RelativeAngle(float absoluteAngle)
        {
            return SrMath.PositiveAngle_d(absoluteAngle - GravityDirection + 270.0f);
        }

        /// <summary>
        /// Returns the given absolute angle as an angle relative to the controller's gravity.
        /// For example, 0 (flat surface angle) returns 180 (upside-down surface) when gravity
        /// points upward and 0 when gravity points downward.
        /// </summary>
        /// <param name="absoluteAngle">The absolute angle, in degrees.</param>
        /// <param name="gravityDirection">The controller's direction of gravity, in degrees.</param>
        /// <returns></returns>
        public static float RelativeAngle(float absoluteAngle, float gravityDirection)
        {
            return SrMath.PositiveAngle_d(absoluteAngle - gravityDirection + 270.0f);
        }

        /// <summary>
        /// Returns the given relative angle corrected for physics where gravity points straight down.
        /// For example, 0 (flat) while gravity is 90 (upside-down) will return 180, an
        /// upside-down surface.
        /// </summary>
        /// <param name="relativeAngle">The relative angle, in degrees.</param>
        /// <returns></returns>
        public float AbsoluteAngle(float relativeAngle)
        {
            return SrMath.PositiveAngle_d(relativeAngle + GravityDirection - 270.0f);
        }

        /// <summary>
        /// Returns the given relative angle corrected for physics where gravity points straight down.
        /// For example, 0 (flat) while gravity is 90 (upside-down) will return 180, an
        /// upside-down surface.
        /// </summary>
        /// <param name="relativeAngle">The relative angle, in degrees.</param>
        /// <param name="gravityDirection">The controller's direction of gravity, in degrees.</param>
        /// <returns></returns>
        public static float AbsoluteAngle(float relativeAngle, float gravityDirection)
        {
            return SrMath.PositiveAngle_d(relativeAngle + gravityDirection - 270.0f);
        }
        #endregion

        #region Surface Solver Functions
        public void UseCustomSurface(ISurfaceSolver surfaceSolver)
        {
            if (_surfaceSolver != null)
            {
                var solver = _surfaceSolver;
                _surfaceSolver = null;

                solver.OnSolverRemoved(this);
            }

            _surfaceSolver = surfaceSolver;

            if (_surfaceSolver != null)
                _surfaceSolver.OnSolverAdded(this);
        }
        #endregion

        #region Trigger Functions
        /// <summary>
        /// Lets a platform's trigger know about a collision, if it has one.
        /// </summary>
        private void NotifyPlatformCollision(SensorType sensor, TerrainCastHit data)
        {
            if (!data || !data.Transform || (DisableNotifyPlatforms && DisableEvents))
                return;

            var contact = new PlatformCollision.Contact(sensor, data, data.Transform.GetComponent<PlatformTrigger>());

            if (!DisableNotifyPlatforms && contact.PlatformTrigger)
            {
                contact.PlatformTrigger.NotifyPlatformCollision(contact);
            }

            if (!DisableEvents)
                OnCollide.Invoke(contact);
        }

        // We're using a semaphore in case recursive calls to CheckPreCollision occur
        // IgnoreThisCollision() can't be called unless the current call stack has entered CheckPreCollision at least once
        private int _preCollisionSemaphore;

        private bool _willIgnoreThisCollision;

        public bool IsPreColliding { get { return _preCollisionSemaphore > 0; } }

        private bool CheckPreCollision(SensorType sensor, TerrainCastHit data)
        {
            if (!data || !data.Transform || (DisableEvents && DisableNotifyPlatforms))
                return true;

            ++_preCollisionSemaphore;

            try
            {
                var contact = new PlatformCollision.Contact(sensor, data, data.Transform.GetComponent<PlatformTrigger>());

                if (!DisableNotifyPlatforms)
                {
                    var trigger = data.Transform.GetComponent<PlatformTrigger>();
                    if (trigger)
                        trigger.NotifyPreCollision(contact);
                }

                if (!DisableEvents)
                    OnPreCollide.Invoke(contact);

                return !_willIgnoreThisCollision;
            }
            finally
            {
                _willIgnoreThisCollision = false;
                --_preCollisionSemaphore;
            }
        }

        /// <summary>
        /// <para>
        /// Call this inside an OnPreCollide event to prevent the controller from reacting to the upcoming
        /// collision. This is useful for breakable and disappearing objects.
        /// </para>
        /// <para>
        /// You can call this by subscribing to the controller's OnPreCollide event, a PlatformTrigger's OnPreCollide event,
        /// or you can override ReactivePlatform's OnPreCollide method.
        /// </para>
        /// </summary>
        public void IgnoreThisCollision()
        {
            if (!IsPreColliding)
            {
                Debug.LogError("IgnoreThisCollision() may only be called inside an OnPreCollide event. " +
                               "Either subscribe to the OnPreCollide event on the controller, or " +
                               "inherit from ReactivePlatform and override OnPreCollide().");

                return;
            }

            _willIgnoreThisCollision = true;
        }

        #region Notify Reactive Methods
        public void NotifyAreaEnter(ReactiveArea area)
        {
            Reactives.Add(area);

            if (!DisableEvents)
                OnAreaEnter.Invoke(area);
        }

        public void NotifyAreaExit(ReactiveArea area)
        {
            Reactives.Remove(area);

            if (!DisableEvents)
                OnAreaExit.Invoke(area);
        }

        public void NotifyPlatformEnter(ReactivePlatform platform)
        {
            Reactives.Add(platform);

            if (!DisableEvents)
                OnPlatformCollisionEnter.Invoke(platform);
        }

        public void NotifyPlatformExit(ReactivePlatform platform)
        {
            Reactives.Remove(platform);

            if (!DisableEvents)
                OnPlatformCollisionExit.Invoke(platform);
        }

        public void NotifySurfaceEnter(ReactivePlatform platform)
        {
            Reactives.Add(platform);

            if (!DisableEvents)
                OnPlatformSurfaceEnter.Invoke(platform);
        }

        public void NotifySurfaceExit(ReactivePlatform platform)
        {
            Reactives.Remove(platform);

            if (!DisableEvents)
                OnPlatformSurfaceExit.Invoke(platform);
        }

        public void NotifyActivateObject(ReactiveEffect obj)
        {
            Reactives.Add(obj);

            if (!DisableEvents)
                OnEffectActivate.Invoke(obj);
        }

        public void NotifyDeactivateObject(ReactiveEffect obj)
        {
            Reactives.Remove(obj);

            if (!DisableEvents)
                OnEffectDeactivate.Invoke(obj);
        }
        #endregion

        // TODO this stuff should prolly go into extension methods

        /// <summary>
        /// Finds the first reactive of the specified type the controller is interacting with, if any.
        /// </summary>
        /// <typeparam name="T">The specified type.</typeparam>
        /// <returns></returns>
        public T GetReactive<T>() where T : BaseReactive
        {
            for (var i = 0; i < Reactives.Count; ++i)
            {
                var reactive = Reactives[i] as T;
                if (reactive != null)
                    return reactive;
            }

            return null;
        }

        /// <summary>
        /// Returns whether the controller is interacting with a reactive of the specified type.
        /// </summary>
        /// <typeparam name="T">The specified type.</typeparam>
        /// <returns></returns>
        public bool IsInteractingWith<T>() where T : BaseReactive
        {
            for (var i = 0; i < Reactives.Count; ++i)
            {
                if (Reactives[i] is T)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns whether the controller is interacting with the specified reactive.
        /// </summary>
        /// <param name="reactive">The specified reactive.</param>
        /// <returns></returns>
        public bool IsInteractingWith(BaseReactive reactive)
        {
            return Reactives.Contains(reactive);
        }

        /// <summary>
        /// Returns whether the controller is inside a reactive area of the specified type.
        /// </summary>
        /// <typeparam name="T">The specified type.</typeparam>
        /// <returns></returns>
        public bool IsInside<T>() where T : ReactiveArea
        {
            for (var i = 0; i < Reactives.Count; ++i)
            {
                if (Reactives[i] is T)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns whether the controller is inside the specified reactive area.
        /// </summary>
        /// <param name="area">The specified reactive area.</param>
        /// <returns></returns>
        public bool IsInside(ReactiveArea area)
        {
            return area.AreaTrigger.HasController(this);
        }

        /// <summary>
        /// Returns whether the controller is standing on the specified reactive platform.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsStandingOn<T>() where T : ReactivePlatform
        {
            
            for (var i = 0; i < Reactives.Count; ++i)
            {
                var platform = Reactives[i] as T;
                if (!platform)
                    continue;

                if (!platform.PlatformTrigger.HasControllerOnSurface(this))
                    continue;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns whether the controller is standing on the specified object.
        /// </summary>
        /// <param name="platform">The specified object.</param>
        /// <param name="checkParents">Whether to also check if the object is a parent of what the controller
        /// is standing on.</param>
        /// <returns></returns>
        public bool IsStandingOn(Transform platform, bool checkParents = false)
        {
            if (!Grounded)
                return false;

            if (checkParents)
            {
                return (PrimarySurface && PrimarySurface.IsChildOf(platform)) ||
                       (SecondarySurface && SecondarySurface.IsChildOf(platform));
            }
            else
            {
                return platform == PrimarySurface || platform == SecondarySurface;
            }
        }

        /// <summary>
        /// Returns whether the controller is standing on the specified reactive platform.
        /// </summary>
        /// <param name="platform">The specified platform.</param>
        /// <returns></returns>
        public bool IsStandingOn(ReactivePlatform platform)
        {
            return IsStandingOn(platform.transform) || platform.PlatformTrigger.HasControllerOnSurface(this);
        }

        #endregion
    }
}