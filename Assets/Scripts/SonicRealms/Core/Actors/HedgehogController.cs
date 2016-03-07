using System.Collections.Generic;
using System.Linq;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace SonicRealms.Core.Actors
{
    /// <summary>
    /// Implements Sonic-style physics and lets platform triggers know when they're touched.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class HedgehogController : MonoBehaviour
    {
        #region Inspector Fields
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
        #endregion
        #region Collision
        /// <summary>
        /// Mask representing the layers the controller collides with.
        /// </summary>
        [Tooltip("Mask representing the layers the controller collides with.")]
        public LayerMask CollisionMask;

        /// <summary>
        /// What reactives the controller is on.
        /// </summary>
        [Tooltip("What reactives the controller is on.")]
        public List<BaseReactive> Reactives;
        #endregion
        #region Sensors
        /// <summary>
        /// Contains sensor data for hit detection.
        /// </summary>
        [Tooltip("Contains sensor data for hit detection.")]
        public HedgehogSensors Sensors;

        /// <summary>
        /// The rotation angle of the sensors in degrees.
        /// </summary>
        public float SensorsRotation
        {
            get
            {
                return Sensors.transform.eulerAngles.z;
            }
            set
            {
                Sensors.transform.eulerAngles = new Vector3(
              Sensors.transform.eulerAngles.x,
              Sensors.transform.eulerAngles.y,
              value);
            }
        }

        #endregion
        #region Physics
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
        #endregion
        #region Performance
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
        #endregion
        #region Events
        /// <summary>
        /// Invoked when the controller lands on something.
        /// </summary>
        public UnityEvent OnAttach;

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
        public ReactiveObjectEvent OnObjectActivate;

        /// <summary>
        /// Invoked when the controller deactivates a reactive object.
        /// </summary>
        public ReactiveObjectEvent OnObjectDeactivate;
        #endregion
        #endregion
        #region Control Variables
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
        #endregion
        #region Physics Variables
        /// <summary>
        /// The default direction of gravity, in degrees.
        /// </summary>
        private const float DefaultGravityDirection = 270.0f;

        /// <summary>
        /// The maximum number of steps a controller's movement can be divided into.
        /// This is in place to prevent the controller from crashing the game if it
        /// tries to do a ton of steps.
        /// </summary>
        private const int CollisionStepLimit = 100;

        /// <summary>
        /// Angle difference used when rotating between two surfaces, in radians.
        /// </summary>
        private const float SurfaceAngleTolerance = 0.087222f;

        /// <summary>
        /// Whether to move the controller based on current velocity.
        /// </summary>
        public bool ApplyVelocity;

        /// <summary>
        /// Whether to apply slope gravity on the controller when it's on steep ground.
        /// </summary>
        public bool ApplySlopeGravity;

        /// <summary>
        /// Whether to apply ground friction on the controller when it's on the ground.
        /// </summary>
        public bool ApplyGroundFriction;

        /// <summary>
        /// Whether to fall off walls and loops if ground speed is below DetachSpeed.
        /// </summary>
        public bool DetachWhenSlow;

        /// <summary>
        /// When true, prevents the controller from falling off surfaces.
        /// </summary>
        public bool DetachLock;

        /// <summary>
        /// When true, prevents the controller from attaching to surfaces.
        /// </summary>
        public bool DisableAttach;

        /// <summary>
        /// Whether to apply air gravity on the controller when it's in the air.
        /// </summary>
        public bool ApplyAirGravity;

        /// <summary>
        /// Whether to apply air drag on the controller when it's in the air.
        /// </summary>
        public bool ApplyAirDrag;

        /// <summary>
        /// Whether the controller is facing forward or backward. This doesn't actually affect graphics, but it's
        /// used for making dashes go in the right direction. Doing something such as flipping the sprite can be
        /// handled using FacingForwardBool with animtion.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether the controller is facing forward or backward. This doesn't actually affect graphics, but it's " +
                 "used for making dashes go in the right direction. Doing something such as flipping the sprite can be " +
                 "handled using FacingForwardBool with animtion.")]
        private bool _facingForward;
        public bool FacingForward
        {
            get
            {
                return _facingForward;
            }
            set
            {
                _facingForward = value;
                if (Animator != null && FacingForwardBoolHash != 0)
                    Animator.SetBool(FacingForwardBoolHash, value);
            }
        }


        /// <summary>
        /// Whether to ignore collisions with EVERYTHING.
        /// </summary>
        public bool IgnoreCollision;

        /// <summary>
        /// Whether the controller is ignoring the result of its current collision check. Set by triggers during
        /// HandleCollisions.
        /// </summary>
        public bool IgnoringThisCollision;

        /// <summary>
        /// The controller's velocity as a Vector2. If set while on the ground, the controller will not
        /// fall off and instead set ground velocity to the specified value's scalar projection onto the surface.
        /// </summary>
        public Vector2 Velocity
        {
            get
            {
                return new Vector2(Vx, Vy);
            }
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
            get
            {
                return DMath.RotateBy(Velocity, (DefaultGravityDirection - GravityDirection) * Mathf.Deg2Rad);
            }
            set
            {
                Velocity = DMath.RotateBy(value, (GravityDirection - DefaultGravityDirection) * Mathf.Deg2Rad);
            }
        }

        /// <summary>
        /// Direction of "right" given the current direction of gravity.
        /// </summary>
        public float GravityRight
        {
            get
            {
                return GravityDirection + 90.0f;
            }
            set
            {
                GravityDirection = value - 90.0f;
            }
        }

        /// <summary>
        /// A unit vector pointing in the "downward" direction based on the controller's GravityDirection.
        /// </summary>
        public Vector2 GravityDownVector
        {
            get
            {
                return DMath.AngleToVector(GravityDirection * Mathf.Deg2Rad);
            }
            set
            {
                GravityDirection = DMath.Modp(DMath.Angle(value) * Mathf.Rad2Deg, 360.0f);
            }
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

        /// <summary>
        /// Whether the player is touching the ground.
        /// </summary>
        [Tooltip("Whether the player is touching the ground.")]
        public bool Grounded;

        /// <summary>
        /// If grounded, the angle of incline the controller is walking on in degrees. Goes hand-in-hand
        /// with rotation.
        /// </summary>
        [Tooltip("If grounded, the angle of incline the controller is walking on in degrees.")]
        public float SurfaceAngle;

        /// <summary>
        /// Whether to bypass surface angle calculations the next FixedUpdate. Allows you to set your own.
        /// </summary>
        [Tooltip("Whether to bypass surface angle calculations the next FixedUpdate. Allows you to set your own.")]
        public bool ForceSurfaceAngle;

        /// <summary>
        /// If grounded, the angle's surface relative to the direction of gravity. If gravity points down, this is
        /// the same as SurfaceAngle.
        /// </summary>
        public float RelativeSurfaceAngle
        {
            get
            {
                return RelativeAngle(SurfaceAngle);
            }
            set
            {
                SurfaceAngle = AbsoluteAngle(value);
            }
        }

        /// <summary>
        /// If grounded, which sensor on the player defines the primary surface.
        /// </summary>
        [HideInInspector]
        public Footing Footing;

        /// <summary>
        /// Whether the player has just detached. Is used to avoid reattachments right after.
        /// </summary>
        [HideInInspector]
        public bool JustDetached;

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
        /// The surface which is currently partially defining the controller's rotation, if any.
        /// </summary>
        [HideInInspector]
        public Transform SecondarySurface;

        /// <summary>
        /// The results from the terrain cast which found the secondary surface, if any.
        /// </summary>
        public TerrainCastHit SecondarySurfaceHit;

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

        /// <summary>
        /// The controller will move this much in its next FixedUpdate. It is set using the Translate function,
        /// which guarantees that movement from outside sources is applied before collision checks.
        /// </summary>
        public Vector3 QueuedTranslation;
        #endregion
        #region Animation Variables
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
            AirDrag = 0.1488343f;   // Air drag is 0.96875 per frame @ 60fps, 0.96875^60 converts it to seconds
            AirDragRequiredSpeed = new Vector2(0.075f, 2.4f);
            AntiTunnelingSpeed = 4.0f;
            DetachSpeed = 1.5f;
            SlopeGravityBeginAngle = 10.0f;
            LedgeClimbHeight = 0.16f;
            LedgeDropHeight = 0.16f;
            MaxClimbAngle = 70.0f;

            ApplyAirDrag = true;
            ApplyAirGravity = true;
            ApplyGroundFriction = true;
            ApplySlopeGravity = true;
            DisableAttach = true;
            DetachLock = true;
            DetachWhenSlow = true;

            DisableEvents = false;
            DisableNotifyPlatforms = false;
            DisableGroundCheck = false;
            DisableCeilingCheck = false;
            DisableSideCheck = false;

            DetachTrigger = AttachTrigger = FacingForwardBool = AirSpeedXFloat =
                AirSpeedYFloat = GroundedBool = GroundSpeedFloat = AbsGroundSpeedFloat =
                SurfaceAngleFloat = GroundSpeedBool = "";
        }

        public void Awake()
        {
            Footing = Footing.None;
            Grounded = false;
            Vx = Vy = GroundVelocity = 0.0f;
            JustDetached = false;
            QueuedTranslation = default(Vector3);

            ApplyAirDrag = ApplyAirGravity = ApplyGroundFriction = ApplySlopeGravity = DetachWhenSlow = true;
            FacingForward = true;
            DisableAttach = DetachLock = false;
            IgnoreCollision = IgnoringThisCollision = false;

            OnAttach = OnAttach ?? new UnityEvent();
            OnCollide = OnCollide ?? new PlatformCollisionEvent();
            OnDetach = OnDetach ?? new UnityEvent();
            OnSteepDetach = OnSteepDetach ?? new UnityEvent();
            
            OnAreaEnter = OnAreaEnter ?? new ReactiveAreaEvent();
            OnAreaExit = OnAreaExit ?? new ReactiveAreaEvent();
            OnPlatformCollisionEnter = OnPlatformCollisionEnter ?? new ReactivePlatformEvent();
            OnPlatformCollisionExit = OnPlatformCollisionExit ?? new ReactivePlatformEvent();
            OnPlatformSurfaceEnter = OnPlatformSurfaceEnter ?? new ReactivePlatformEvent();
            OnPlatformSurfaceExit = OnPlatformSurfaceExit ?? new ReactivePlatformEvent();
            OnObjectActivate = OnObjectActivate ?? new ReactiveObjectEvent();
            OnObjectDeactivate = OnObjectDeactivate ?? new ReactiveObjectEvent();

            if (Animator == null)
                return;

            DetachTriggerHash = string.IsNullOrEmpty(DetachTrigger) ? 0 : Animator.StringToHash(DetachTrigger);
            AttachTriggerHash = string.IsNullOrEmpty(AttachTrigger) ? 0 : Animator.StringToHash(AttachTrigger);
            FacingForwardBoolHash = string.IsNullOrEmpty(FacingForwardBool) ? 0 : Animator.StringToHash(FacingForwardBool);
            AirSpeedXFloatHash = string.IsNullOrEmpty(AirSpeedXFloat) ? 0 : Animator.StringToHash(AirSpeedXFloat);
            AirSpeedYFloatHash = string.IsNullOrEmpty(AirSpeedYFloat) ? 0 : Animator.StringToHash(AirSpeedYFloat);
            GroundedBoolHash = string.IsNullOrEmpty(GroundedBool) ? 0 : Animator.StringToHash(GroundedBool);
            GroundSpeedFloatHash = string.IsNullOrEmpty(GroundSpeedFloat) ? 0 : Animator.StringToHash(GroundSpeedFloat);
            GroundSpeedBoolHash = string.IsNullOrEmpty(GroundSpeedBool) ? 0 : Animator.StringToHash(GroundSpeedBool);
            AbsGroundSpeedFloatHash = string.IsNullOrEmpty(AbsGroundSpeedFloat) ? 0 : Animator.StringToHash(AbsGroundSpeedFloat);
            SurfaceAngleFloatHash = string.IsNullOrEmpty(SurfaceAngleFloat) ? 0 : Animator.StringToHash(SurfaceAngleFloat);
        }

        public void Update()
        {
            if (Animator != null)
                SetAnimatorParameters();

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

            if (Interrupted)
                return;

            UpdateGroundVelocity();

            HandleForces();

            UpdateGroundVelocity();

            HandleQueuedTranslation();
            HandleMovement();

            UpdateGroundVelocity();
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
            if (!Interrupted || InterruptTimer <= 0.0f)
                return;
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
        /// Handles movement caused by velocity.
        /// </summary>
        public void HandleMovement(float timestep)
        {
            var vt = Mathf.Sqrt(Vx * Vx + Vy * Vy);

            var surfaceAngle = SurfaceAngle;

            if (IgnoreCollision)
            {
                transform.position += new Vector3(Vx * timestep, Vy * timestep);
                UpdateGroundVelocity();
            }
            else if (vt < AntiTunnelingSpeed)
            {
                transform.position += new Vector3(Vx * timestep, Vy * timestep);
                HandleCollisions();
                UpdateGroundVelocity();
            }
            else
            {
                // If the controller's gotta go fast, split up collision into steps so we don't glitch through walls.
                // The speed limit becomes however many steps your computer can handle!
                var vc = vt;
                var steps = 0;
                while (vc > 0.0f)
                {
                    if (vc > AntiTunnelingSpeed)
                    {
                        transform.position +=
                            (new Vector3(Vx * timestep, Vy * timestep)) * (AntiTunnelingSpeed / vt);
                        vc -= AntiTunnelingSpeed;
                    }
                    else
                    {
                        transform.position +=
                            (new Vector3(Vx * timestep, Vy * timestep)) * (vc / vt);
                        vc = 0.0f;
                    }

                    HandleCollisions();
                    UpdateGroundVelocity();

                    if (++steps > CollisionStepLimit || DMath.Equalsf(Velocity.magnitude) || Interrupted)
                        break;

                    // If the player's speed changes mid-stagger recalculate current velocity and total velocity
                    var vn = Mathf.Sqrt(Vx * Vx + Vy * Vy);
                    vc *= vn / vt;
                    vt *= vn / vt;
                }
            }

            if (ForceSurfaceAngle)
            {
                SurfaceAngle = surfaceAngle;
                ForceSurfaceAngle = false;
            }
        }

        /// <summary>
        /// Handles translations requested from outside sources.
        /// </summary>
        public void HandleQueuedTranslation()
        {
            if (QueuedTranslation == Vector3.zero)
                return;

            transform.position += QueuedTranslation;
            QueuedTranslation = Vector3.zero;
        }

        /// <summary>
        /// Sets the controller's animator parameters.
        /// </summary>
        public void SetAnimatorParameters()
        {
            if (FacingForwardBoolHash != 0)
                Animator.SetBool(FacingForwardBoolHash, FacingForward);

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
            if (Grounded)
            {
                // Slope gravity
                if (ApplySlopeGravity &&
                    !DMath.AngleInRange_d(RelativeSurfaceAngle, -SlopeGravityBeginAngle, SlopeGravityBeginAngle))
                {
                    GroundVelocity -= SlopeGravity * Mathf.Sin(RelativeSurfaceAngle * Mathf.Deg2Rad) * timestep;
                }

                // Ground friction
                if (ApplyGroundFriction)
                    GroundVelocity -= Mathf.Min(Mathf.Abs(GroundVelocity), GroundFriction * timestep) * Mathf.Sign(GroundVelocity);

                // Speed limit
                if (GroundVelocity > MaxSpeed)
                    GroundVelocity = MaxSpeed;
                else if (GroundVelocity < -MaxSpeed)
                    GroundVelocity = -MaxSpeed;

                // Detachment from walls if speed is too low
                if (DetachWhenSlow &&
                    Mathf.Abs(GroundVelocity) < DetachSpeed &&
                    DMath.AngleInRange_d(RelativeSurfaceAngle, 85.0f, 275.0f))
                {
                    if (!DisableEvents && Detach())
                        OnSteepDetach.Invoke();
                }
            }
            else
            {
                // Rotate sensors to direction of gravity
                SensorsRotation = GravityDirection + 90.0f;

                // Air gravity
                if (ApplyAirGravity)
                    Velocity += DMath.AngleToVector(GravityDirection * Mathf.Deg2Rad) * AirGravity * timestep;

                // Air drag
                if (ApplyAirDrag &&
                    Vy > 0f && Vy < AirDragRequiredSpeed.y && Mathf.Abs(Vx) > AirDragRequiredSpeed.x)
                {
                    Vx *= Mathf.Pow(AirDrag, timestep);
                }
            }
        }

        /// <summary>
        /// Checks for collision with all sensors, changing position, velocity, and rotation if necessary. This should be called each time
        /// the player changes position. This method does not require a timestep because it only resolves overlaps in the player's collision.
        /// </summary>
        public void HandleCollisions()
        {
            if (!Grounded)
            {
                if (!DisableSideCheck) AirSideCheck();
                if (!DisableCeilingCheck) AirCeilingCheck();
                if (!DisableGroundCheck) AirGroundCheck();
                SensorsRotation = GravityDirection + 90.0f;
            }

            if (Grounded)
            {
                if (!DisableSideCheck) GroundSideCheck();
                if (!DisableCeilingCheck) GroundCeilingCheck();
                UpdateGroundVelocity();

                if (!DisableGroundCheck) GroundSurfaceCheck();
                UpdateGroundVelocity();
            }
        }

        /// <summary>
        /// If grounded, sets x and y velocity based on ground velocity.
        /// </summary>
        public void UpdateGroundVelocity()
        {
            if (!Grounded)
                return;

            Vx = GroundVelocity * Mathf.Cos(SurfaceAngle * Mathf.Deg2Rad);
            Vy = GroundVelocity * Mathf.Sin(SurfaceAngle * Mathf.Deg2Rad);
        }
        #endregion
        #region Collision Subroutines
        /// <summary>
        /// Collision check with side sensors for when player is in the air.
        /// </summary>
        /// <returns><c>true</c>, if a collision was found, <c>false</c> otherwise.</returns>
        private bool AirSideCheck()
        {
            var leftCheck = this.TerrainCast(Sensors.Center.position, Sensors.CenterLeft.position,
                ControllerSide.Left);
            var rightCheck = this.TerrainCast(Sensors.Center.position, Sensors.CenterRight.position,
                ControllerSide.Right);

            if (leftCheck)
            {
                LeftWallHit = leftCheck;
                LeftWall = leftCheck.Transform;

                NotifyTriggers(leftCheck);

                if (!IgnoringThisCollision)
                {
                    var push = (Vector3)(leftCheck.Hit.point - (Vector2)Sensors.CenterLeft.position);
                    transform.position += push + push.normalized * DMath.Epsilon;

                    if (RelativeVelocity.x < 0.0f)
                        RelativeVelocity = new Vector2(0.0f, RelativeVelocity.y);

                    IgnoringThisCollision = false;
                }

                return true;
            }

            if (rightCheck)
            {
                RightWallHit = rightCheck;
                RightWall = rightCheck.Transform;

                NotifyTriggers(rightCheck);

                if (!IgnoringThisCollision)
                {
                    var push = (Vector3)(rightCheck.Hit.point - (Vector2)Sensors.CenterRight.position);
                    transform.position += push + push.normalized * DMath.Epsilon;

                    if (RelativeVelocity.x > 0.0f)
                        RelativeVelocity = new Vector2(0.0f, RelativeVelocity.y);

                    IgnoringThisCollision = false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Collision check with air sensors for when player is in the air.
        /// </summary>
        /// <returns><c>true</c>, if a collision was found, <c>false</c> otherwise.</returns>
        private bool AirCeilingCheck()
        {
            var leftCheck = this.TerrainCast(Sensors.TopLeftStart.position, Sensors.TopLeft.position, ControllerSide.Top);
            var rightCheck = this.TerrainCast(Sensors.TopRightStart.position, Sensors.TopRight.position,
                ControllerSide.Top);

            if (leftCheck || rightCheck)
            {
                if (leftCheck && rightCheck)
                {
                    NotifyTriggers(leftCheck);
                    NotifyTriggers(rightCheck);
                    if (!IgnoringThisCollision)
                    {
                        if (DMath.Highest(leftCheck.Hit.point, rightCheck.Hit.point,
                            GravityDirection * Mathf.Deg2Rad) >= 0f)
                        {
                            var impact = CheckImpact(leftCheck);
                            if (impact.ShouldAttach)
                            {
                                transform.position += (Vector3)(leftCheck.Hit.point - (Vector2)Sensors.TopLeft.position);
                                Attach(impact);
                            }
                            else
                            {
                                impact = CheckImpact(rightCheck);
                                if (impact.ShouldAttach)
                                {
                                    transform.position += (Vector3)(rightCheck.Hit.point - (Vector2)Sensors.TopRight.position);
                                    Attach(impact);
                                }
                            }
                        }
                        else
                        {
                            var impact = CheckImpact(rightCheck);
                            if (impact.ShouldAttach)
                            {
                                transform.position += (Vector3)(rightCheck.Hit.point - (Vector2)Sensors.TopRight.position);
                                Attach(impact);
                            }
                            else
                            {
                                impact = CheckImpact(leftCheck);
                                if (impact.ShouldAttach)
                                {
                                    transform.position += (Vector3)(leftCheck.Hit.point - (Vector2)Sensors.TopLeft.position);
                                    Attach(impact);
                                }
                            }
                        }
                    }
                }
                else if (leftCheck)
                {
                    NotifyTriggers(leftCheck);
                    if (!IgnoringThisCollision)
                    {
                        var impact = CheckImpact(leftCheck);
                        if (impact.ShouldAttach)
                        {
                            transform.position += (Vector3)(leftCheck.Hit.point - (Vector2)Sensors.TopLeft.position);
                            Attach(impact);
                        }
                    }
                }
                else if (rightCheck)
                {
                    NotifyTriggers(rightCheck);
                    if (!IgnoringThisCollision)
                    {
                        var impact = CheckImpact(rightCheck);
                        if (impact.ShouldAttach)
                        {
                            transform.position += (Vector3)(rightCheck.Hit.point - (Vector2)Sensors.TopRight.position);
                            Attach(impact);
                        }
                    }
                }

                IgnoringThisCollision = false;
                return true;
            }

            IgnoringThisCollision = false;
            return false;
        }

        /// <summary>
        /// Collision check with ground sensors for when player is in the air.
        /// </summary>
        /// <returns><c>true</c>, if a collision was found, <c>false</c> otherwise.</returns>
        private bool AirGroundCheck()
        {
            var groundLeftCheck = this.TerrainCast(Sensors.LedgeClimbLeft.position, Sensors.BottomLeft.position,
                ControllerSide.Bottom);
            var groundRightCheck = this.TerrainCast(Sensors.LedgeClimbRight.position, Sensors.BottomRight.position,
                ControllerSide.Bottom);
            
            if (groundLeftCheck || groundRightCheck)
            {
                if (groundLeftCheck && groundRightCheck)
                {
                    if (DMath.Highest(groundLeftCheck.Hit.point, groundRightCheck.Hit.point,
                            GravityDirection*Mathf.Deg2Rad) >= 0.0f)
                    {
                        var impact = CheckImpact(groundLeftCheck);

                        if (impact.ShouldAttach)
                        {
                            NotifyTriggers(groundLeftCheck);
                            if (!IgnoringThisCollision)
                            {
                                transform.position += (Vector3)groundLeftCheck.Hit.point - Sensors.BottomLeft.position;
                                Attach(impact);
                            }
                        }
                        else
                        {
                            transform.position += (Vector3)groundLeftCheck.Hit.point - Sensors.BottomLeft.position;
                        }
                    }
                    else
                    {
                        var impact = CheckImpact(groundRightCheck);

                        if (impact.ShouldAttach)
                        {
                            NotifyTriggers(groundRightCheck);
                            if (!IgnoringThisCollision)
                            {
                                transform.position += (Vector3)groundRightCheck.Hit.point - Sensors.BottomRight.position;
                                Attach(impact);
                            }
                        }
                        else
                        {
                            transform.position += (Vector3)groundRightCheck.Hit.point - Sensors.BottomRight.position;
                        }
                    }
                }
                else if (groundLeftCheck)
                {
                    var impact = CheckImpact(groundLeftCheck);
                    if (impact.ShouldAttach)
                    {
                        NotifyTriggers(groundLeftCheck);
                        if (!IgnoringThisCollision)
                        {
                            transform.position += (Vector3)groundLeftCheck.Hit.point - Sensors.BottomLeft.position;
                            Attach(impact);
                        }
                    }
                    else
                    {
                        transform.position += (Vector3)groundLeftCheck.Hit.point - Sensors.BottomLeft.position;
                    }
                }
                else
                {
                    var impact = CheckImpact(groundRightCheck);

                    if (impact.ShouldAttach)
                    {
                        NotifyTriggers(groundRightCheck);
                        if (!IgnoringThisCollision)
                        {
                            transform.position += (Vector3)groundRightCheck.Hit.point - Sensors.BottomRight.position;
                            Attach(impact);
                        }
                    }
                    else
                    {
                        transform.position += (Vector3)groundRightCheck.Hit.point - Sensors.BottomRight.position;
                    }
                }

                IgnoringThisCollision = false;
                return true;
            }

            IgnoringThisCollision = false;
            return false;
        }

        /// <summary>
        /// Collision check with side sensors for when player is on the ground.
        /// </summary>
        /// <returns><c>true</c>, if a collision was found, <c>false</c> otherwise.</returns>
        private bool GroundSideCheck()
        {
            var leftCheck = this.TerrainCast(Sensors.Center.position, Sensors.CenterLeft.position, ControllerSide.Left);
            var rightCheck = this.TerrainCast(Sensors.Center.position, Sensors.CenterRight.position,
                ControllerSide.Right);

            LeftWallHit = leftCheck;
            LeftWall = LeftWallHit ? leftCheck.Transform : null;
            RightWallHit = rightCheck;
            RightWall = RightWallHit ? rightCheck.Transform : null;

            if (leftCheck)
            {
                NotifyTriggers(leftCheck);

                if (!IgnoringThisCollision)
                {
                    if (GroundVelocity < 0.0f)
                        GroundVelocity = 0.0f;

                    var push = (Vector3)(leftCheck.Hit.point - (Vector2)Sensors.CenterLeft.position);
                    transform.position += push + push.normalized * DMath.Epsilon;

                    // If running down a wall and hits the floor, orient the player onto the floor
                    if (DMath.AngleInRange_d(RelativeSurfaceAngle,
                        MaxClimbAngle, 180.0f - MaxClimbAngle))
                    {
                        SurfaceAngle -= 90.0f;
                    }

                    SensorsRotation = SurfaceAngle;
                }

                IgnoringThisCollision = false;
                return true;
            }

            if (rightCheck)
            {
                NotifyTriggers(rightCheck);

                if (!IgnoringThisCollision)
                {
                    if (GroundVelocity > 0.0f)
                        GroundVelocity = 0.0f;

                    var push = (Vector3)(rightCheck.Hit.point - (Vector2)Sensors.CenterRight.position);
                    transform.position += push + push.normalized * DMath.Epsilon;

                    // If running down a wall and hits the floor, orient the player onto the floor
                    if (DMath.AngleInRange_d(RelativeSurfaceAngle,
                        180.0f + MaxClimbAngle, 360.0f - MaxClimbAngle))
                    {
                        SurfaceAngle += 90.0f;
                    }

                    SensorsRotation = SurfaceAngle;
                }

                IgnoringThisCollision = false;
                return true;
            }

            IgnoringThisCollision = false;
            SensorsRotation = SurfaceAngle;
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

            // There is nothing to do here but set variables. This information is used for crushers.
            if (leftCheck)
            {
                LeftCeilingHit = leftCheck;
                LeftCeiling = leftCheck.Transform;
            }

            if (rightCheck)
            {
                RightCeilingHit = rightCheck;
                RightCeiling = rightCheck.Transform;
            }

            if (leftCheck)
            {
                NotifyTriggers(leftCheck);
                IgnoringThisCollision = false;
                return true;
            }
            if (rightCheck)
            {
                NotifyTriggers(rightCheck);
                IgnoringThisCollision = false;
                return true;
            }

            IgnoringThisCollision = false;
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

            // Get the angles of their surfaces in degrees
            var leftAngle = left ? left.SurfaceAngle * Mathf.Rad2Deg : 0.0f;
            var rightAngle = right ? right.SurfaceAngle * Mathf.Rad2Deg : 0.0f;

            // Get the difference between the two surfaces
            var diff = DMath.ShortestArc_d(leftAngle, rightAngle);

            // Get the differences between the two surfaces and the angle of the last surface
            var leftDiff = Mathf.Abs(DMath.ShortestArc_d(SurfaceAngle, leftAngle));
            var rightDiff = Mathf.Abs(DMath.ShortestArc_d(SurfaceAngle, rightAngle));

            // Get easy checks out of the way

            // If we've found no surface, or they're all too steep, detach from the ground
            if ((!left || leftDiff > MaxClimbAngle) && (!right || rightDiff > MaxClimbAngle))
                goto detach;

            // If only one sensor found a surface we don't need any further checks
            if (!left && right)
                goto orientRight;

            if (left && !right)
                goto orientLeft;

            // Both feet have surfaces beneath them, things get complicated

            // If one surface is too steep, use the other (as long as that surface isn't too different from the current)
            const float SteepnessTolerance = 0.25f;
            if (diff > MaxClimbAngle || diff < -MaxClimbAngle)
            {
                if (leftDiff < rightDiff && leftDiff < MaxClimbAngle*SteepnessTolerance)
                    goto orientLeft;

                if (rightDiff < MaxClimbAngle*SteepnessTolerance)
                    goto orientRight;
            }

            // Propose an angle that would rotate the controller between both surfaces
            var overlap = DMath.Angle(right.Hit.point - left.Hit.point);

            // If it's between the angles of the two surfaces (+/- tolerance), it'll do
            if ((DMath.Equalsf(diff) && Mathf.Abs(DMath.ShortestArc(overlap, left.SurfaceAngle)) < SurfaceAngleTolerance) ||

                 (diff > 0.0f &&
                 DMath.AngleInRange(overlap,
                     left.SurfaceAngle - SurfaceAngleTolerance, right.SurfaceAngle + SurfaceAngleTolerance)) ||

                 (diff < 0.0f &&
                 DMath.AngleInRange(overlap,
                     right.SurfaceAngle - SurfaceAngleTolerance, left.SurfaceAngle + SurfaceAngleTolerance)))
            {
                // Angle closest to the current gets priority
                if (leftDiff < rightDiff)
                    goto orientLeftRight;

                goto orientRightLeft;
            }

            // Otherwise use only one surface, check for steepness again
            if (leftDiff < MaxClimbAngle || rightDiff < MaxClimbAngle)
            {
                // Angle closest to the current gets priority
                if (leftDiff < rightDiff)
                    goto orientLeft;

                goto orientRight;
            }

            // Usually shouldn't reach this point, but detaching from the ground is a good catch-all solution
            goto detach;

            #region Orientation Goto's
            orientLeft:
            // Use the left sensor as the ground
            NotifyTriggers(left);

            if (!IgnoringThisCollision)
            {
                Footing = Footing.Left;
                SensorsRotation = SurfaceAngle = left.SurfaceAngle * Mathf.Rad2Deg;
                transform.position += (Vector3)(left.Hit.point - (Vector2)Sensors.BottomLeft.position);

                SetSurface(left);
            }

            goto finish;

            orientRight:
            // Use the right sensor as the ground
            NotifyTriggers(right);

            if (!IgnoringThisCollision)
            {
                Footing = Footing.Right;
                SensorsRotation = SurfaceAngle = right.SurfaceAngle * Mathf.Rad2Deg;
                transform.position += (Vector3)(right.Hit.point - (Vector2)Sensors.BottomRight.position);

                SetSurface(right);
            }

            goto finish;

            orientLeftRight:
            // Use the left sensor as the ground, then rotate between the left and right sensors
            NotifyTriggers(left);
            NotifyTriggers(right);

            if (!IgnoringThisCollision)
            {
                Footing = Footing.Left;
                SensorsRotation = SurfaceAngle = DMath.Angle(right.Hit.point - left.Hit.point) * Mathf.Rad2Deg;
                transform.position += (Vector3)(left.Hit.point - (Vector2)Sensors.BottomLeft.position);

                SetSurface(left, right);
            }

            goto finish;

            orientRightLeft:
            // Use the right sensor as the ground, then rotate between the right and left sensors
            NotifyTriggers(right);
            NotifyTriggers(left);

            if (!IgnoringThisCollision)
            {
                Footing = Footing.Right;
                SensorsRotation = SurfaceAngle = DMath.Angle(right.Hit.point - left.Hit.point) * Mathf.Rad2Deg;
                transform.position += (Vector3)(right.Hit.point - (Vector2)Sensors.BottomRight.position);

                SetSurface(right, left);
            }

            goto finish;

            detach:
            IgnoringThisCollision = false;
            Detach();
            return false;
            #endregion

            finish:
            IgnoringThisCollision = false;
            return true;
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
            InterruptTimer = Mathf.Max(DMath.Epsilon, time);
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
        /// When called during a collision check, tells the controller to not respond to the collision.
        /// This can be called by platform triggers to prevent their effects being overwritten by
        /// the controller.
        /// 
        /// For example, the controller will normally land on any flat ground. However, when it hits a spring,
        /// IgnoreThisCollision is called, preventing this and allowing the controller to stay in the air.
        /// </summary>
        public void IgnoreThisCollision()
        {
            IgnoringThisCollision = true;
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
            GroundVelocity = DMath.ScalarProjectionAbs(velocity, SurfaceAngle * Mathf.Deg2Rad);

            var v = DMath.AngleToVector(SurfaceAngle * Mathf.Deg2Rad) * GroundVelocity;
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
            GroundVelocity += DMath.ScalarProjectionAbs(velocity, SurfaceAngle * Mathf.Deg2Rad);

            var v = DMath.AngleToVector(SurfaceAngle * Mathf.Deg2Rad) * GroundVelocity;
            Vx = v.x;
            Vy = v.y;

            return GroundVelocity;
        }

        /// <summary>
        /// Translates the controller by the specified amount. This function doesn't immediately move the controller
        /// and instead sets it to move in its next FixedUpdate, right before collision checks.
        /// </summary>
        /// <param name="amount"></param>
        public void Translate(Vector3 amount)
        {
            QueuedTranslation += amount;
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
        #endregion
        #region Surface Acquisition Functions
        /// <summary>
        /// Detach the player from whatever surface it is on. If the player is not grounded this has no effect
        /// other than setting lockUponLanding.
        /// <returns>Whether the controller detached successfully.</returns>
        /// </summary>
        public bool Detach()
        {
            if (DetachLock) return false;
            var wasGrounded = Grounded;

            Grounded = false;
            JustDetached = true;
            Footing = Footing.None;
            SensorsRotation = GravityDirection + 90.0f;
            SetSurface(null);

            // Don't invoke the event if the controller was already off the ground
            if (wasGrounded)
            {
                if (!DisableEvents) OnDetach.Invoke();
                if (Animator != null && DetachTriggerHash != 0)
                    Animator.SetTrigger(DetachTriggerHash);
            }

            return false;
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
            var angleDegrees = DMath.Modp(angleRadians * Mathf.Rad2Deg, 360.0f);
            GroundVelocity = groundSpeed;
            SurfaceAngle = angleDegrees;
            SensorsRotation = SurfaceAngle;
            Grounded = true;

            // If we already were on the ground, that's all we needed to do
            if (wasGrounded) return true;

            // Otherwise - quick check to make sure we belong on the surface
            GroundSurfaceCheck();

            // If it turns out we don't, we're outta here
            if (!Grounded) return false;

            // If it turns out we definitely do, invoke events and animations
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
        public ImpactResult CheckImpact(TerrainCastHit hit)
        {
            if (hit == null || hit.Hit.fraction == 0f) return default(ImpactResult);

            var surfaceDegrees = DMath.PositiveAngle_d(hit.SurfaceAngle * Mathf.Rad2Deg);

            // The player can't possibly land on something if he's traveling 90 degrees within the normal
            var playerAngle = DMath.Angle(Velocity)*Mathf.Rad2Deg;
            if (DMath.AngleInRange_d(playerAngle, surfaceDegrees, surfaceDegrees + 180f))
            {
                return default(ImpactResult);
            }

            var result = 0f;
            var shouldAttach = true;
            var fixedAngled = RelativeAngle(surfaceDegrees);
            if (RelativeVelocity.y <= 0.0f)
            {
                if (fixedAngled < 22.5f || fixedAngled > 337.5f)
                {
                    result = RelativeVelocity.x;
                }
                else if (fixedAngled < 45.0f)
                {
                    result = Mathf.Abs(RelativeVelocity.x) > -RelativeVelocity.y
                        ? RelativeVelocity.x
                        : 0.5f * RelativeVelocity.y;
                }
                else if (fixedAngled > 315.0f)
                {
                    result = Mathf.Abs(RelativeVelocity.x) > -RelativeVelocity.y
                        ? RelativeVelocity.x
                        : -0.5f * RelativeVelocity.y;
                }
                else if (fixedAngled < 90.0f)
                {
                    result = Mathf.Abs(RelativeVelocity.x) > -RelativeVelocity.y
                        ? RelativeVelocity.x
                        : RelativeVelocity.y;
                }
                else if (fixedAngled > 270.0f)
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
                if (fixedAngled > 90.0f && fixedAngled < 135.0f)
                {
                    result = RelativeVelocity.y;
                }
                else if (fixedAngled > 225.0f && fixedAngled < 270.0f)
                {
                    result = -RelativeVelocity.y;
                }
                else if (fixedAngled > 135.0f && fixedAngled < 225.0f)
                {
                    RelativeVelocity = new Vector2(RelativeVelocity.x, 0.0f);
                }
                else
                {
                    shouldAttach = false;
                }
            }

            // If we're gonna fall off because we're too slow, don't attach at all
            if (DetachWhenSlow && (Mathf.Abs(result) < DetachSpeed && fixedAngled > 90.0f && fixedAngled < 270.0f))
                return default(ImpactResult);

            return new ImpactResult {GroundSpeed = result, ShouldAttach = shouldAttach, SurfaceAngle = hit.SurfaceAngle};
        }

        /// <summary>
        /// Sets the controller's primary and secondary surfaces and triggers their platform events, if any.
        /// </summary>
        /// <param name="primarySurfaceHit">The new primary surface.</param>
        /// <param name="secondarySurfaceHit">The new secondary surface.</param>
        public void SetSurface(TerrainCastHit primarySurfaceHit, TerrainCastHit secondarySurfaceHit = null)
        {
            PrimarySurfaceHit = primarySurfaceHit;
            PrimarySurface = PrimarySurfaceHit ? PrimarySurfaceHit.Transform : null;

            SecondarySurfaceHit = secondarySurfaceHit;
            SecondarySurface = SecondarySurfaceHit ? SecondarySurfaceHit.Transform : null;

            if (DisableNotifyPlatforms)
                return;

            var primaryTrigger = PrimarySurface == null
                ? null : PrimarySurface.GetComponent<PlatformTrigger>();
            var secondaryTrigger = SecondarySurface == null
                ? null : SecondarySurface.GetComponent<PlatformTrigger>();

            if (primaryTrigger != null)
                primaryTrigger.NotifySurfaceCollision(primarySurfaceHit);
            if (secondaryTrigger != null)
                secondaryTrigger.NotifySurfaceCollision(secondarySurfaceHit);
        }

        /// <summary>
        /// Returns the specified absolute angle in degrees relative to the direction of gravity.
        /// For example, 0 (flat surface angle) returns 180 (upside-down surface) when gravity
        /// points upward and 0 when gravity points downward.
        /// </summary>
        /// <param name="absoluteAngle"></param>
        /// <returns></returns>
        public float RelativeAngle(float absoluteAngle)
        {
            return DMath.PositiveAngle_d(absoluteAngle - GravityDirection + 270.0f);
        }

        public float AbsoluteAngle(float relativeAngle)
        {
            return DMath.PositiveAngle_d(relativeAngle + GravityDirection - 270.0f);
        }
        #endregion
        #region Trigger Helpers
        /// <summary>
        /// Lets a platform's trigger know about a collision, if it has one.
        /// </summary>
        /// <param name="collision"></param>
        private void NotifyTriggers(TerrainCastHit collision)
        {
            if (!collision || collision.Transform == null)
                return;

            if (!DisableNotifyPlatforms)
            {
                var trigger = collision.Transform.GetComponent<PlatformTrigger>();
                if (trigger == null)
                {
                    if (!DisableEvents) OnCollide.Invoke(collision);
                    return;
                }

                trigger.NotifyCollision(collision);
                if (IgnoringThisCollision) return;
            }

            if (!DisableEvents)
                OnCollide.Invoke(collision);
        }

        #region Notify Reactive Methods
        public void NotifyAreaEnter(ReactiveArea area)
        {
            Reactives.Add(area);
            if (!DisableEvents) OnAreaEnter.Invoke(area);
        }

        public void NotifyAreaExit(ReactiveArea area)
        {
            Reactives.Remove(area);
            if (!DisableEvents) OnAreaExit.Invoke(area);
        }

        public void NotifyPlatformEnter(ReactivePlatform platform)
        {
            Reactives.Add(platform);
            if (!DisableEvents) OnPlatformCollisionEnter.Invoke(platform);
        }

        public void NotifyPlatformExit(ReactivePlatform platform)
        {
            Reactives.Remove(platform);
            if (!DisableEvents) OnPlatformCollisionExit.Invoke(platform);
        }

        public void NotifySurfaceEnter(ReactivePlatform platform)
        {
            if (!DisableEvents) OnPlatformSurfaceEnter.Invoke(platform);
        }

        public void NotifySurfaceExit(ReactivePlatform platform)
        {
            if (!DisableEvents) OnPlatformSurfaceExit.Invoke(platform);
        }

        public void NotifyActivateObject(ReactiveObject obj)
        {
            Reactives.Add(obj);
            if (!DisableEvents) OnObjectActivate.Invoke(obj);
        }

        public void NotifyDeactivateObject(ReactiveObject obj)
        {
            Reactives.Remove(obj);
            if (!DisableEvents) OnObjectDeactivate.Invoke(obj);
        }
        #endregion

        /// <summary>
        /// Finds the first reactive of the specified type the controller is interacting with, if any.
        /// </summary>
        /// <typeparam name="TReactive">The specified type.</typeparam>
        /// <returns></returns>
        public TReactive GetReactive<TReactive>() where TReactive : BaseReactive
        {
            return Reactives.FirstOrDefault(reactive => reactive is TReactive) as TReactive;
        }

        /// <summary>
        /// Returns whether the controller is interacting with a reactive of the specified type.
        /// </summary>
        /// <typeparam name="TReactive">The specified type.</typeparam>
        /// <returns></returns>
        public bool InteractingWith<TReactive>() where TReactive : BaseReactive
        {
            return Reactives.Any(reactive => reactive is TReactive);
        }

        /// <summary>
        /// Returns whether the controller is interacting with the specified reactive.
        /// </summary>
        /// <param name="reactive">The specified reactive.</param>
        /// <returns></returns>
        public bool InteractingWith(BaseReactive reactive)
        {
            return Reactives.Contains(reactive);
        }

        /// <summary>
        /// Returns whether the controller is inside a reactive area of the specified type.
        /// </summary>
        /// <typeparam name="TReactiveArea">The specified type.</typeparam>
        /// <returns></returns>
        public bool Inside<TReactiveArea>() where TReactiveArea : ReactiveArea
        {
            return Reactives.Any(reactive => reactive is TReactiveArea);
        }

        /// <summary>
        /// Returns whether the controller is inside the specified reactive area.
        /// </summary>
        /// <param name="area">The specified reactive area.</param>
        /// <returns></returns>
        public bool Inside(ReactiveArea area)
        {
            return area.AreaTrigger.HasController(this);
        }

        /// <summary>
        /// Returns whether the controller is inside the specified
        /// </summary>
        /// <typeparam name="TReactivePlatform"></typeparam>
        /// <returns></returns>
        public bool StandingOn<TReactivePlatform>() where TReactivePlatform : ReactivePlatform
        {
            return Reactives.Any(reactive =>
            {
                var platform = reactive as TReactivePlatform;
                if (platform == null) return false;
                if (!platform.PlatformTrigger.HasControllerOnSurface(this))return false;
                return true;
            });
        }

        /// <summary>
        /// Returns whether the controller is standing on the specified object.
        /// </summary>
        /// <param name="platform">The specified object.</param>
        /// <param name="checkParents">Whether to also check if the object is a parent of what the controller
        /// is standing on.</param>
        /// <returns></returns>
        public bool StandingOn(Transform platform, bool checkParents = false)
        {
            return Grounded && (checkParents
                ? (PrimarySurface && PrimarySurface.IsChildOf(platform) ||
                   (SecondarySurface && SecondarySurface.IsChildOf(platform)))
                : platform == PrimarySurface || platform == SecondarySurface);
        }

        /// <summary>
        /// Returns whether the controller is standing on the specified reactive platform.
        /// </summary>
        /// <param name="platform">The specified platform.</param>
        /// <returns></returns>
        public bool StandingOn(ReactivePlatform platform)
        {
            return StandingOn(platform.transform) || platform.PlatformTrigger.HasControllerOnSurface(this);
        }
        #endregion
    }
}