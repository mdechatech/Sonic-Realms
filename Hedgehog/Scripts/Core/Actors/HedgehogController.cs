using System;
using System.Collections.Generic;
using System.Linq;
using Hedgehog.Core.Moves;
using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Hedgehog.Core.Actors
{
    /// <summary>
    /// Controls the player.
    /// </summary>
    [AddComponentMenu("Hedgehog/Actors/Hedgehog")]
    public class HedgehogController : MonoBehaviour
    {
        #region Inspector Fields
        #region Components
        /// <summary>
        /// Component that handles the controller's moves.
        /// </summary>
        [Tooltip("Component that handles the controller's moves.")]
        public MoveManager MoveManager;

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
        /// What paths the controller collides with. An object is on a path if it or one of its parents
        /// has that path's name.
        /// </summary>
        [Tooltip("What paths the controller is on. An object is on a path if it or one of its parents " +
                 "has that path's name.")]
        public List<string> Paths;

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
            get { return Sensors.transform.eulerAngles.z; }
            set { Sensors.transform.eulerAngles = new Vector3(
                Sensors.transform.eulerAngles.x,
                Sensors.transform.eulerAngles.y,
                value); }
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
        #region Events
        /// <summary>
        /// Invoked when the controller is touching something with both its top and ground sensors.
        /// </summary>
        public UnityEvent OnCrush;

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

        // Events with reactives. Exposing them to the UI is unnecessary, they are only useful in scripts which can check
        // whether the reactive is a spring, button, body of water, etc.

        /// <summary>
        /// Invoked when the controller enters any reactive thing.
        /// </summary>
        public ReactiveEvent OnReactiveEnter;
        
        /// <summary>
        /// Invoked when the controller exits any reactive thing.
        /// </summary>
        public ReactiveEvent OnReactiveExit;
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

        /// <summary>
        /// The controller's ground control move, if any.
        /// </summary>
        [Tooltip("The controller's ground control move, if any.")]
        public GroundControl GroundControl;

        /// <summary>
        /// The controller's air control move, if any.
        /// </summary>
        [Tooltip("The controller's air control move, if any.")]
        public AirControl AirControl;

        /// <summary>
        /// The move manager's list of moves. Does not perform null checks.
        /// </summary>
        public List<Move> Moves
        {
            get { return MoveManager.Moves; }
            set { MoveManager.Moves = value; }
        }

        /// <summary>
        /// The moves currently being performed.
        /// </summary>
        public List<Move> ActiveMoves
        {
            get { return MoveManager.ActiveMoves; }
            set { MoveManager.ActiveMoves = value; }
        }

        /// <summary>
        /// The moves which can be performed given the controller's current state.
        /// </summary>
        public List<Move> AvailableMoves
        {
            get { return MoveManager.AvailableMoves; }
            set { MoveManager.AvailableMoves = value; }
        }

        /// <summary>
        /// The moves which cannot be performed given the controller's current state.
        /// </summary>
        public List<Move> UnavailableMoves
        {
            get { return MoveManager.UnavailableMoves; }
            set { MoveManager.UnavailableMoves = value; }
        }
        #endregion
        #region Physics Variables
        /// <summary>
        /// Used for sanity check during surface rotation.
        /// </summary>
        private const float SurfaceReversionThreshold = 0.05f;

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
        public bool AttachLock;

        /// <summary>
        /// Whether to apply air gravity on the controller when it's in the air.
        /// </summary>
        public bool ApplyAirGravity;

        /// <summary>
        /// Whether to apply air drag on the controller when it's in the air.
        /// </summary>
        public bool ApplyAirDrag;

        /// <summary>
        /// Whether to flip the controller horizontally when traveling backwards on the ground, and vice versa.
        /// </summary>
        public bool AutoFlip;

        /// <summary>
        /// Whether to rotate the controller to the angle of its surface.
        /// </summary>
        public bool AutoRotate;

        /// <summary>
        /// Whether the controller is facing forward or backward.
        /// </summary>
        public bool FacingForward;

        /// <summary>
        /// Makes the controller keep going when it finds something in its way. Often used by objects that
        /// destroy themselves and don't want the controller to stop, such as item boxes.
        /// </summary>
        public bool IgnoreNextCollision;

        /// <summary>
        /// The controller's velocity as a Vector2.
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
            get { return DMath.RotateBy(Velocity, (DefaultGravityDirection - GravityDirection)*Mathf.Deg2Rad); }
            set { Velocity = DMath.RotateBy(value, (GravityDirection - DefaultGravityDirection) *Mathf.Deg2Rad); }
        }

        /// <summary>
        /// A unit vector pointing in the "downward" direction based on the controller's GravityDirection.
        /// </summary>
        public Vector2 GravityDown
        {
            get { return DMath.AngleToVector(GravityDirection*Mathf.Deg2Rad); }
            set { GravityDirection = DMath.Modp(DMath.Angle(value)*Mathf.Rad2Deg, 360.0f); }
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
        /// If grounded, the angle of incline the controller is walking on
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

            Paths = new List<string> {"Always Collide", "Path 1"};
            Reactives = new List<BaseReactive>();

            Sensors = GetComponentInChildren<HedgehogSensors>();

            MaxSpeed = 9001.0f;
            GroundFriction = 1.6785f;
            GravityDirection = 270.0f;
            SlopeGravity = 4.5f;
            AirGravity = 7.857f;
            AirDrag = 0.1488343f;
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
            AttachLock = true;
            DetachLock = true;
            DetachWhenSlow = true;

            AutoFlip = true;
            AutoRotate = true;

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

            AirControl = GetMove<AirControl>();
            GroundControl = GetMove<GroundControl>();

            ApplyAirDrag = ApplyAirGravity = ApplyGroundFriction = ApplySlopeGravity = DetachWhenSlow = true;
            AutoFlip = AutoRotate = FacingForward = true;
            AttachLock = DetachLock = false;

            OnCrush = OnCrush ?? new UnityEvent();
            OnAttach = OnAttach ?? new UnityEvent();
            OnCollide = OnCollide ?? new PlatformCollisionEvent();
            OnDetach = OnDetach ?? new UnityEvent();
            OnSteepDetach = OnSteepDetach ?? new UnityEvent();
            OnReactiveEnter = OnReactiveEnter ?? new ReactiveEvent();
            OnReactiveExit = OnReactiveExit ?? new ReactiveEvent();

            if (Animator == null) return;

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
            if (Interrupted)
            {
                HandleInterrupt();
                return;
            }

            HandleDisplay();
        }

        public void FixedUpdate()
        {
            LeftWall = RightWall = null;
            LeftWallHit = RightWallHit = null;

            if (Interrupted)
                return;

            // Forces, then translations, then movement and collision, updating velocity each time ground velocity changes
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
        /// Handles movement caused by velocity.
        /// </summary>
        public void HandleMovement(float timestep)
        {
            var vt = Mathf.Sqrt(Vx * Vx + Vy * Vy);
            
            if (vt < AntiTunnelingSpeed)
            {
                transform.position += new Vector3(Vx*timestep, Vy*timestep);
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

                    if (DMath.Equalsf(Velocity.magnitude) || Interrupted || ++steps > CollisionStepLimit)
                        break;

                    // If the player's speed changes mid-stagger recalculate current velocity and total velocity
                    var vn = Mathf.Sqrt(Vx * Vx + Vy * Vy);
                    vc *= vn / vt;
                    vt *= vn / vt;
                }
            }
        }

        /// <summary>
        /// Handles translations requested from outside sources.
        /// </summary>
        public void HandleQueuedTranslation()
        {
            if (QueuedTranslation == Vector3.zero) return;

            transform.position += QueuedTranslation;
            QueuedTranslation = Vector3.zero;
        }

        /// <summary>
        /// Uses Time.fixedDeltaTime as the timestep. Handles sprite rotation, animation, and other effects.
        /// </summary>
        public void HandleDisplay()
        {
            HandleDisplay(Time.fixedDeltaTime);
        }

        /// <summary>
        /// Handles sprite rotation, animation, and other effects.
        /// </summary>
        /// <param name="timestep">The specified timestep, in seconds.</param>
        public void HandleDisplay(float timestep)
        {
            // Set FacingForward
            if (AutoFlip)
            {
                if (Grounded && !DMath.Equalsf(GroundVelocity))
                {
                    FacingForward = GroundVelocity >= 0.0f;
                }
                else if (!Grounded && !DMath.Equalsf(RelativeVelocity.x))
                {
                    FacingForward = RelativeVelocity.x >= 0.0f;
                }
            }

            // Rotate to surface angle
            if (!Grounded)
            {
                RendererObject.transform.eulerAngles = new Vector3(RendererObject.transform.eulerAngles.x, 
                    RendererObject.transform.eulerAngles.y, 
                    Mathf.LerpAngle(RendererObject.transform.eulerAngles.z, SensorsRotation, Time.fixedDeltaTime*10.0f));
            }
            else if(AutoRotate)
            {
                RendererObject.transform.eulerAngles = 
                    new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, SensorsRotation);
            }

            // Flip based on FacingForward
            if ((FacingForward && RendererObject.transform.localScale.x < 0.0f) || 
                (!FacingForward && RendererObject.transform.localScale.x > 0.0f))
            {
                RendererObject.transform.localScale = new Vector3(
                    -RendererObject.transform.localScale.x,
                    RendererObject.transform.localScale.y,
                    RendererObject.transform.localScale.z);
            }

            // Set animator parameters
            if (Animator == null)
                return;

            if(FacingForwardBoolHash != 0)
                Animator.SetBool(FacingForwardBoolHash, FacingForward);

            if(AirSpeedXFloatHash != 0)
                Animator.SetFloat(AirSpeedXFloatHash, Velocity.x);

            if(AirSpeedYFloatHash != 0)
                Animator.SetFloat(AirSpeedYFloatHash, Velocity.y);

            if(GroundedBoolHash != 0)
                Animator.SetBool(GroundedBoolHash, Grounded);

            if(GroundSpeedFloatHash != 0)
                Animator.SetFloat(GroundSpeedFloatHash, GroundVelocity);

            if(GroundSpeedBoolHash != 0)
                Animator.SetBool(GroundSpeedBool, GroundVelocity != 0.0f);

            if(AbsGroundSpeedFloatHash != 0)
                Animator.SetFloat(AbsGroundSpeedFloatHash, Mathf.Abs(GroundVelocity));

            if(SurfaceAngleFloatHash != 0)
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
                    GroundVelocity -= SlopeGravity*Mathf.Sin(RelativeSurfaceAngle*Mathf.Deg2Rad)*timestep;
                }

                // Ground friction
                if (ApplyGroundFriction)
                    GroundVelocity -= Mathf.Min(Mathf.Abs(GroundVelocity), GroundFriction*timestep)*Mathf.Sign(GroundVelocity);

                // Speed limit
                if (GroundVelocity > MaxSpeed) GroundVelocity = MaxSpeed;
                else if (GroundVelocity < -MaxSpeed) GroundVelocity = -MaxSpeed;

                // Detachment from walls if speed is too low
                if (DetachWhenSlow &&
                    Mathf.Abs(GroundVelocity) < DetachSpeed &&
                    DMath.AngleInRange_d(RelativeSurfaceAngle, 85.0f, 275.0f))
                {
                    if (Detach())
                        OnSteepDetach.Invoke();
                }
            }
            else
            {
                // Rotate sensors to direction of gravity
                SensorsRotation = GravityDirection + 90.0f;

                // Air gravity
                if (ApplyAirGravity)
                    Velocity += DMath.AngleToVector(GravityDirection*Mathf.Deg2Rad)*AirGravity*timestep;

                // Air drag
                if (ApplyAirDrag && 
                    Vy > AirDragRequiredSpeed.y && Mathf.Abs(Vx) > AirDragRequiredSpeed.x)
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
            if(CrushCheck()) OnCrush.Invoke();

            if (!Grounded)
            {
                AirSideCheck();
                AirCeilingCheck();
                AirGroundCheck();
                SensorsRotation = GravityDirection + 90.0f;
            }

            if (Grounded)
            {
                GroundSideCheck();
                UpdateGroundVelocity();

                GroundSurfaceCheck();
                UpdateGroundVelocity();
            }
        }

        /// <summary>
        /// If grounded, sets x and y velocity based on ground velocity.
        /// </summary>
        public void UpdateGroundVelocity()
        {
            if (!Grounded) return;

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
            var sideLeftCheck = this.TerrainCast(Sensors.Center.position, Sensors.CenterLeft.position,
                ControllerSide.Left);
            var sideRightCheck = this.TerrainCast(Sensors.Center.position, Sensors.CenterRight.position,
                ControllerSide.Right);

            if (sideLeftCheck)
            {
                NotifyTriggers(sideLeftCheck);

                if (!IgnoreNextCollision)
                {
                    var push = (Vector3)(sideLeftCheck.Hit.point - (Vector2)Sensors.CenterLeft.position);
                    transform.position += push + push.normalized * DMath.Epsilon;

                    if(RelativeVelocity.x < 0.0f)
                        RelativeVelocity = new Vector2(0.0f, RelativeVelocity.y);
                }

                IgnoreNextCollision = false;
                return true;
            }

            if (sideRightCheck)
            {
                NotifyTriggers(sideRightCheck);

                if (!IgnoreNextCollision)
                {
                    var push = (Vector3)(sideRightCheck.Hit.point - (Vector2)Sensors.CenterRight.position);
                    transform.position += push + push.normalized * DMath.Epsilon;

                    if(RelativeVelocity.x > 0.0f)
                        RelativeVelocity = new Vector2(0.0f, RelativeVelocity.y);
                }

                IgnoreNextCollision = false;
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
            if (leftCheck)
            {
                NotifyTriggers(leftCheck);

                if (!IgnoreNextCollision && leftCheck.Hit.fraction > 0.0f)
                {
                    transform.position += (Vector3)(leftCheck.Hit.point - (Vector2)Sensors.TopLeft.position);
                    if (HandleImpact(leftCheck))
                        Footing = Footing.Left;
                }
                
                IgnoreNextCollision = false;
                return true;
            }

            var rightCheck = this.TerrainCast(Sensors.TopRightStart.position, Sensors.TopRight.position, ControllerSide.Top);
            if (rightCheck)
            {
                NotifyTriggers(rightCheck);

                if (!IgnoreNextCollision && rightCheck.Hit.fraction > 0.0f)
                {
                    transform.position += (Vector3)(rightCheck.Hit.point - (Vector2)Sensors.TopRight.position);
                    if (HandleImpact(rightCheck))
                        Footing = Footing.Right;
                }

                IgnoreNextCollision = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Collision check with ground sensors for when player is in the air.
        /// </summary>
        /// <returns><c>true</c>, if a collision was found, <c>false</c> otherwise.</returns>
        private bool AirGroundCheck()
        {
            var groundLeftCheck = Groundcast(Footing.Left);
            var groundRightCheck = Groundcast(Footing.Right);

            if (groundLeftCheck || groundRightCheck)
            {
                if (groundLeftCheck && groundRightCheck)
                {
                    if (DMath.Highest(groundLeftCheck.Hit.point, groundRightCheck.Hit.point,
                            GravityDirection*Mathf.Deg2Rad) >= 0.0f)
                    {
                        NotifyTriggers(groundLeftCheck);

                        if (!IgnoreNextCollision && HandleImpact(groundLeftCheck))
                            Footing = Footing.Left;

                        IgnoreNextCollision = false;
                    }
                    else
                    {
                        NotifyTriggers(groundRightCheck);

                        if (!IgnoreNextCollision && HandleImpact(groundRightCheck))
                            Footing = Footing.Right;

                        IgnoreNextCollision = false;
                    }
                }
                else if (groundLeftCheck)
                {
                    NotifyTriggers(groundLeftCheck);

                    if (!IgnoreNextCollision && HandleImpact(groundLeftCheck))
                        Footing = Footing.Left;

                    IgnoreNextCollision = false;
                }
                else
                {
                    NotifyTriggers(groundRightCheck);

                    if (!IgnoreNextCollision && HandleImpact(groundRightCheck))
                        Footing = Footing.Right;

                    IgnoreNextCollision = false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Collision check with side sensors for when player is on the ground.
        /// </summary>
        /// <returns><c>true</c>, if a collision was found, <c>false</c> otherwise.</returns>
        private bool GroundSideCheck()
        {
            var sideLeftCheck = this.TerrainCast(Sensors.Center.position, Sensors.CenterLeft.position, ControllerSide.Left);
            var sideRightCheck = this.TerrainCast(Sensors.Center.position, Sensors.CenterRight.position,
                ControllerSide.Right);

            if (sideLeftCheck)
            {
                NotifyTriggers(sideLeftCheck);

                if (!IgnoreNextCollision)
                {
                    if (GroundVelocity < 0.0f)
                        GroundVelocity = 0.0f;

                    var push = (Vector3)(sideLeftCheck.Hit.point - (Vector2)Sensors.CenterLeft.position);
                    transform.position += push + push.normalized*DMath.Epsilon;

                    // If running down a wall and hits the floor, orient the player onto the floor
                    if (DMath.AngleInRange_d(RelativeSurfaceAngle,
                        MaxClimbAngle, 180.0f - MaxClimbAngle))
                    {
                        SurfaceAngle -= 90.0f;
                    }

                    SensorsRotation = SurfaceAngle;
                }

                IgnoreNextCollision = false;
                return true;
            }
            if (sideRightCheck)
            {
                NotifyTriggers(sideRightCheck);

                if (!IgnoreNextCollision)
                {
                    if (GroundVelocity > 0.0f)
                        GroundVelocity = 0.0f;

                    var push = (Vector3)(sideRightCheck.Hit.point - (Vector2)Sensors.CenterRight.position);
                    transform.position += push + push.normalized*DMath.Epsilon;

                    // If running down a wall and hits the floor, orient the player onto the floor
                    if (DMath.AngleInRange_d(RelativeSurfaceAngle,
                        180.0f + MaxClimbAngle, 360.0f - MaxClimbAngle))
                    {
                        SurfaceAngle += 90.0f;
                    }

                    SensorsRotation = SurfaceAngle;
                }

                IgnoreNextCollision = false;
                return true;
            }

            SensorsRotation = SurfaceAngle;
            return false;
        }

        /// <summary>
        /// Collision check with ceiling sensors for when player is on the ground.
        /// </summary>
        /// <returns><c>true</c>, if a collision was found, <c>false</c> otherwise.</returns>
        private bool GroundCeilingCheck()
        {
            var ceilLeftCheck = this.TerrainCast(Sensors.TopCenter.position, Sensors.TopLeft.position, ControllerSide.Left);
            var ceilRightCheck = this.TerrainCast(Sensors.TopCenter.position, Sensors.TopRight.position,
                ControllerSide.Right);

            if (ceilLeftCheck)
            {
                NotifyTriggers(ceilLeftCheck);

                if (!IgnoreNextCollision)
                {
                    if (GroundVelocity < 0.0f)
                        GroundVelocity = 0.0f;

                    var push = (Vector3)(ceilLeftCheck.Hit.point - (Vector2)Sensors.TopLeft.position);
                    transform.position += push + push.normalized * DMath.Epsilon;
                }

                IgnoreNextCollision = false;
                return true;
            }
            if (ceilRightCheck)
            {
                NotifyTriggers(ceilRightCheck);

                if (!IgnoreNextCollision)
                {
                    if (GroundVelocity > 0.0f)
                        GroundVelocity = 0.0f;

                    var push = (Vector3)(ceilRightCheck.Hit.point - (Vector2)Sensors.TopRight.position);
                    transform.position += push + push.normalized * DMath.Epsilon;
                }

                IgnoreNextCollision = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Collision check with ground sensors for when player is on the ground.
        /// </summary>
        /// <returns><c>true</c>, if a collision was found, <c>false</c> otherwise.</returns>
        private bool GroundSurfaceCheck()
        {
            var left = Surfacecast(Footing.Left);
            var right = Surfacecast(Footing.Right);

            var originalPosition = transform.position;
            var originalRotation = transform.eulerAngles;
            var originalPrimary = PrimarySurfaceHit;
            var originalSecondary = SecondarySurfaceHit;
            TerrainCastHit recheck;

            var leftAngle = left ? left.SurfaceAngle*Mathf.Rad2Deg : 0.0f;
            var rightAngle = right ? right.SurfaceAngle*Mathf.Rad2Deg : 0.0f;
            var diff = DMath.ShortestArc_d(leftAngle, rightAngle);

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

            // If a surface is too steep, use only one surface (as long as that surface isn't too different from the current)
            if (diff > MaxClimbAngle || diff < -MaxClimbAngle)
            {
                if (leftDiff < rightDiff && leftDiff < MaxClimbAngle/4.0f)
                    goto orientLeft;

                if(rightDiff < MaxClimbAngle/4.0f)
                    goto orientRight;
            }

            // Propose an angle that would rotate the controller between both surfaces
            var overlap = DMath.Angle(right.Hit.point - left.Hit.point);

            // If the proposed angle is between the angles of the two surfaces, it'll do
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
                    goto orientLeftBoth;

                goto orientRightBoth;
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

            // Goto's actually seem to improve the code's readability here, so please don't sue me
            #region Orientation Goto's
            orientLeft:
            NotifyTriggers(left);

            if (!IgnoreNextCollision)
            {
                Footing = Footing.Left;
                SensorsRotation = SurfaceAngle = left.SurfaceAngle * Mathf.Rad2Deg;
                transform.position += (Vector3)(left.Hit.point - (Vector2)Sensors.BottomLeft.position);

                SetSurface(left);
            }

            goto finish;

            orientRight:
            NotifyTriggers(right);

            if (!IgnoreNextCollision)
            {
                Footing = Footing.Right;
                SensorsRotation = SurfaceAngle = right.SurfaceAngle * Mathf.Rad2Deg;
                transform.position += (Vector3)(right.Hit.point - (Vector2)Sensors.BottomRight.position);

                SetSurface(right);
            }

            goto finish;

            orientLeftBoth:
            NotifyTriggers(left);
            NotifyTriggers(right);

            if (!IgnoreNextCollision)
            {
                Footing = Footing.Left;
                SensorsRotation = SurfaceAngle = DMath.Angle(right.Hit.point - left.Hit.point) * Mathf.Rad2Deg;
                transform.position += (Vector3)(left.Hit.point - (Vector2)Sensors.BottomLeft.position);

                SetSurface(left, right);
            }

            goto finish;

            orientRightBoth:
            NotifyTriggers(right);
            NotifyTriggers(left);

            if (!IgnoreNextCollision)
            {
                Footing = Footing.Right;
                SensorsRotation = SurfaceAngle = DMath.Angle(right.Hit.point - left.Hit.point) * Mathf.Rad2Deg;
                transform.position += (Vector3)(right.Hit.point - (Vector2)Sensors.BottomRight.position);

                SetSurface(right, left);
            }

            goto finish;

            detach:
            Detach();
            return false;
            #endregion
            // It's possible to come up short after surface checks because the sensors shift
            // during rotation. If we check after rotation and the sensor can't find the ground
            // directly beneath it as we expect, we have to undo everything.
            finish:
            recheck = Footing == Footing.Left
                ? this.TerrainCast(Sensors.LedgeClimbRight.position, Sensors.LedgeDropRight.position,
                    ControllerSide.Bottom)
                : this.TerrainCast(Sensors.LedgeClimbLeft.position, Sensors.LedgeDropLeft.position,
                    ControllerSide.Bottom);

            if (recheck && Vector2.Distance(recheck.Hit.point, Footing == Footing.Left
                ? Sensors.BottomRight.position
                : Sensors.BottomLeft.position) > SurfaceReversionThreshold)
            {
                SetSurface(originalPrimary, originalSecondary);
                transform.position = originalPosition;
                transform.eulerAngles = originalRotation;
            }

            IgnoreNextCollision = false;
            return true;
        }

        /// <summary>
        /// Checks for terrain hitting both horizontal or both vertical sides of a player, aka crushing.
        /// </summary>
        /// <returns></returns>
        private bool CrushCheck()
        {
            return (this.TerrainCast(Sensors.Center.position, Sensors.CenterLeft.position,
                        ControllerSide.Left) &&
                    this.TerrainCast(Sensors.Center.position, Sensors.CenterRight.position,
                        ControllerSide.Right)) ||

                   (this.TerrainCast(Sensors.Center.position, Sensors.TopCenter.position,
                       ControllerSide.Top) &&
                    this.TerrainCast(Sensors.Center.position, Sensors.BottomCenter.position,
                        ControllerSide.Bottom));
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

            if (!endMoves) return;
            MoveManager.EndAll();
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
        /// If grounded, sets the controller's ground velocity based on how much the specified velocity
        /// "fits" with its current surface angle (scalar projection). For example, if the controller
        /// is on a flat horizontal surface and the specified velocity points straight up, the resulting
        /// ground velocity will be zero.
        /// </summary>
        /// <param name="velocity">The specified velocity as a vector with x and y components.</param>
        /// <returns>The resulting ground velocity.</returns>
        public float SetGroundVelocity(Vector2 velocity)
        {
            if (!Grounded) return 0.0f;
            GroundVelocity = DMath.ScalarProjectionAbs(velocity, SurfaceAngle*Mathf.Deg2Rad);

            var v = DMath.AngleToVector(SurfaceAngle * Mathf.Deg2Rad) * GroundVelocity;
            Vx = v.x;
            Vy = v.y;

            return GroundVelocity;
        }

        /// <summary>
        /// If grounded, adds to the controller's ground velocity based on how much the specified velocity
        /// "fits" with its current surface angle (scalar projection). For example, if the controller
        /// is on a flat horizontal surface and the specified velocity points straight up, there will be
        /// no change in ground velocity.
        /// </summary>
        /// <param name="velocity">The specified velocity as a vector with x and y components.</param>
        /// <returns>The resulting change in ground velocity.</returns>
        public float AddGroundVelocity(Vector2 velocity)
        {
            if (!Grounded) return 0.0f;
            GroundVelocity += DMath.ScalarProjectionAbs(velocity, SurfaceAngle * Mathf.Deg2Rad);

            var v = DMath.AngleToVector(SurfaceAngle*Mathf.Deg2Rad)*GroundVelocity;
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

        #region Move Manager Helpers
        // Aliases that call the same function in the controller's move manager. They do not perform null-checks.

        /// <summary>
        /// Returns the first move of the specified type.
        /// </summary>
        /// <typeparam name="TMove">The specified type.</typeparam>
        /// <returns></returns>
        public TMove GetMove<TMove>() where TMove : Move { return MoveManager.Get<TMove>(); }

        /// <summary>
        /// Returns whether the first move of the specified type is available.
        /// </summary>
        /// <typeparam name="TMove">The specified type.</typeparam>
        /// <returns></returns>
        public bool IsAvailableMove<TMove>() where TMove : Move { return MoveManager.IsAvailable<TMove>(); }

        /// <summary>
        /// Returns whether the first move of the specified type is active.
        /// </summary>
        /// <typeparam name="TMove">The specified type.</typeparam>
        /// <returns></returns>
        public bool IsActiveMove<TMove>() where TMove : Move { return MoveManager.IsActive<TMove>(); }

        /// <summary>
        /// Performs the first move of the specified type, if it's available.
        /// </summary>
        /// <typeparam name="TMove">The specified type.</typeparam>
        /// <param name="force">Whether to perform the move even if it's unavailable.</param>
        /// <returns>Whether the move was performed.</returns>
        public bool PerformMove<TMove>(bool force = false) where TMove : Move { return MoveManager.Perform<TMove>(force); }

        /// <summary>
        /// Performs the specified move, if it's available.
        /// </summary>
        /// <param name="move">The specified move.</param>
        /// <param name="force">Whether to perform the move even if it's unavailable.</param>
        /// <returns>Whether the move was performed.</returns>
        public bool PerformMove(Move move, bool force = false) { return MoveManager.Perform(move, force); }

        /// <summary>
        /// Ends the first move of the specified type, if it's active.
        /// </summary>
        /// <typeparam name="TMove">The specified type.</typeparam>
        /// <returns>Whether the move was ended.</returns>
        public bool EndMove<TMove>() where TMove : Move { return MoveManager.End<TMove>(); }

        /// <summary>
        /// Ends the specified move if it's active.
        /// </summary>
        /// <param name="move">The specified move.</param>
        /// <returns>Whether the move was ended.</returns>
        public bool EndMove(Move move) { return MoveManager.End(move); }

        /// <summary>
        /// Ends all moves.
        /// </summary>
        /// <param name="predicate">A move will be ended only if this predicate is true, if any.</param>
        /// <returns>If there were no active moves, always returns true. Otherwise, returns whether all
        /// moves were successfully ended.</returns>
        public bool EndAllMoves(Predicate<Move> predicate = null) { return MoveManager.EndAll(predicate); }
        #endregion
        #endregion
        #region Trigger Helpers
        /// <summary>
        /// Lets a platform's trigger know about a collision, if it has one.
        /// </summary>
        /// <param name="collision"></param>
        /// <returns>Whether a platform trigger was notified.</returns>
        private bool NotifyTriggers(TerrainCastHit collision)
        {
            if (!collision || collision.Hit.transform == null)
                return false;

            if (collision.Side == ControllerSide.Left)
            {
                LeftWall = collision.Hit.transform;
                LeftWallHit = collision;
            }
            else if (collision.Side == ControllerSide.Right)
            {
                RightWall = collision.Hit.transform;
                RightWallHit = collision;
            }

            OnCollide.Invoke(collision);
            return TriggerUtility.NotifyPlatformCollision(collision.Hit.transform, collision);
        }

        /// <summary>
        /// Sets the controller's primary and secondary surfaces and triggers their platform events, if any.
        /// </summary>
        /// <param name="primarySurfaceHit">The new primary surface.</param>
        /// <param name="secondarySurfaceHit">The new secondary surface.</param>
        public bool SetSurface(TerrainCastHit primarySurfaceHit, TerrainCastHit secondarySurfaceHit = null)
        {
            PrimarySurfaceHit = primarySurfaceHit;
            PrimarySurface = PrimarySurfaceHit ? PrimarySurfaceHit.Hit.transform : null;

            SecondarySurfaceHit = secondarySurfaceHit;
            SecondarySurface = SecondarySurfaceHit ? SecondarySurfaceHit.Hit.transform : null;

            var result = false;
            result = TriggerUtility.NotifySurfaceCollision(PrimarySurface, primarySurfaceHit);
            TriggerUtility.NotifySurfaceCollision(SecondarySurface, secondarySurfaceHit);

            return result;
        }

        public void NotifyReactiveEnter(BaseReactive reactive)
        {
            Reactives.Add(reactive);
            OnReactiveEnter.Invoke(reactive);
        }

        public void NotifyReactiveExit(BaseReactive reactive)
        {
            Reactives.Remove(reactive);
            OnReactiveExit.Invoke(reactive);
        }

        public bool InteractingWith<TReactive>() where TReactive : BaseReactive
        {
            return Reactives.Any(reactive => reactive is TReactive);
        }

        public bool InteractingWith(BaseReactive reactive)
        {
            return Reactives.Contains(reactive);
        }

        public bool Inside<TReactiveArea>()
        {
            if (typeof(TReactiveArea).IsSubclassOf(typeof(ReactiveArea)))
            {
                return Reactives.Any(reactive =>
                    reactive is TReactiveArea &&
                    ((ReactiveArea) reactive).AreaTrigger.HasController(this));
            }

            if (typeof(TReactiveArea).IsSubclassOf(typeof(ReactivePlatformArea)))
            {
                return Reactives.Any(reactive =>
                    reactive is TReactiveArea &&
                    ((ReactivePlatformArea)reactive).AreaTrigger.HasController(this));
            }

            return false;
        }

        public bool Inside(ReactiveArea area)
        {
            return area.AreaTrigger.HasController(this);
        }

        public bool Inside(ReactivePlatformArea area)
        {
            return area.AreaTrigger.HasController(this);
        }

        public bool StandingOn<TReactivePlatform>() where TReactivePlatform : ReactivePlatform
        {
            return Reactives.Any(reactive => reactive is TReactivePlatform);
        }

        public bool StandingOn(Transform platform)
        {
            return Grounded && (platform == PrimarySurface || platform == SecondarySurface);
        }

        public bool StandingOn(ReactivePlatform platform)
        {
            return StandingOn(platform.transform) || platform.PlatformTrigger.HasController(this);
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

            // Don't invoke the event if the controller was already off the ground
            if (Grounded)
            {
                OnDetach.Invoke();
                if (Animator != null && DetachTriggerHash != 0)
                    Animator.SetTrigger(DetachTriggerHash);
            } 

            Grounded = false;
            JustDetached = true;
            Footing = Footing.None;
            SensorsRotation = GravityDirection + 90.0f;
            SetSurface(null);

            return false;
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
            if (AttachLock) return false;

            // Don't invoke the event if the controller was already on the ground
            if (!Grounded)
            {
                OnAttach.Invoke();
                if (Animator != null && AttachTriggerHash != 0)
                    Animator.SetTrigger(AttachTriggerHash);
            }

            var angleDegrees = DMath.Modp(angleRadians * Mathf.Rad2Deg, 360.0f);
            GroundVelocity = groundSpeed;
            SurfaceAngle = angleDegrees;
            SensorsRotation = SurfaceAngle;
            Grounded = true;

            GroundSurfaceCheck();

            return true;
        }

        /// <summary>
        /// Calculates the ground velocity as the result of an impact on the specified surface angle.
        /// 
        /// Uses the algorithm here: https://info.sonicretro.org/SPG:Solid_Tiles#Reacquisition_Of_The_Ground
        /// adapted for 360° gravity.
        /// </summary>
        /// <returns>Whether the player should attach to the specified incline.</returns>
        /// <param name="impact">The impact data as th result of a terrain cast.</param>
        public bool HandleImpact(TerrainCastHit impact)
        {
            var sAngled = DMath.PositiveAngle_d(impact.SurfaceAngle * Mathf.Rad2Deg);
            var sAngler = sAngled * Mathf.Deg2Rad;

            // The player can't possibly land on something if he's traveling 90 degrees
            // within the normal
            var surfaceNormal = DMath.Modp(sAngled + 90.0f, 360.0f);
            var playerAngle = DMath.Angle(new Vector2(Vx, Vy)) * Mathf.Rad2Deg;
            var surfaceDifference = DMath.ShortestArc_d(playerAngle, surfaceNormal);
            if (Mathf.Abs(surfaceDifference) < 90.0f)
            {
                return false;
            }

            float? result = null;
            var fixedAngled = RelativeAngle(sAngled);
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
                        : 0.5f*RelativeVelocity.y;
                }
                else if (fixedAngled > 315.0f)
                {
                    result = Mathf.Abs(RelativeVelocity.x) > -RelativeVelocity.y
                        ? RelativeVelocity.x
                        : -0.5f*RelativeVelocity.y;
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
            }

            if (result == null) return false;

            // If we're gonna fall off because we're too slow, don't attach at all
            if (DetachWhenSlow && (Mathf.Abs(result.Value) < DetachSpeed && fixedAngled > 90.0f && fixedAngled < 270.0f))
                return false;
    
            Attach(result.Value, sAngler);
            return true;
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
        
        /// <summary>
        /// Casts from LedgeClimbHeight to the player's geet.
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        private TerrainCastHit Groundcast(Footing side)
        {
            TerrainCastHit cast;
            if (side == Footing.Left)
            {
                cast = this.TerrainCast(
                    Sensors.LedgeClimbLeft.position, Sensors.BottomLeft.position, ControllerSide.Bottom);
            }
            else
            {
                cast = this.TerrainCast(Sensors.LedgeClimbRight.position, Sensors.BottomRight.position,
                    ControllerSide.Bottom);
            }
            
            return cast;
        }

        /// <summary>
        /// Returns the result of a linecast from the ClimbLedgeHeight to Su
        /// </summary>
        /// <returns>The result of the linecast.</returns>
        /// <param name="footing">The side to linecast from.</param>
        private TerrainCastHit Surfacecast(Footing footing)
        {
            TerrainCastHit cast;
            if (footing == Footing.Left)
            {
                // Cast from the player's side to below the player's feet based on its wall mode (Wallmode)
                cast = this.TerrainCast(Sensors.LedgeClimbLeft.position, Sensors.LedgeDropLeft.position,
                    ControllerSide.Bottom);

                if (!cast)
                {
                    return default(TerrainCastHit);
                }
                //return DMath.Equalsf(cast.Hit.fraction, 0.0f) ? default(TerrainCastHit) : cast;
                return cast;
            }
            cast = this.TerrainCast(Sensors.LedgeClimbRight.position, Sensors.LedgeDropRight.position,
                ControllerSide.Bottom);

            if (!cast)
            {
                return default(TerrainCastHit);
            }
            /*
            if (DMath.Equalsf(cast.Hit.fraction, 0.0f))
            {
                return default(TerrainCastHit);
            }
            */
            return cast;
        }
        #endregion
    }
}