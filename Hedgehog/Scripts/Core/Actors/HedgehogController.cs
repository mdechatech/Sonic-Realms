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
        /// Game object that contains the renderer.
        /// </summary>
        [SerializeField]
        public GameObject RendererObject;

        /// <summary>
        /// The controller's animator, if any.
        /// </summary>
        [SerializeField]
        public Animator Animator;
        #endregion
        #region Moves and Control
        /// <summary>
        /// The default control state to enter when on the ground.
        /// </summary>
        [SerializeField]
        [Tooltip("The default control state to enter when on the ground.")]
        public GroundControl GroundControl;

        /// <summary>
        /// The default control state to enter when in the air.
        /// </summary>
        [SerializeField]
        [Tooltip("The default control state to enter when in the air.")]
        public AirControl AirControl;

        /// <summary>
        /// Whether to find moves on initialization. Searches through all children and itself.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to find moves on initialization. Searches through all children and itself.")]
        public bool AutoFindMoves;

        /// <summary>
        /// All of the controller's moves.
        /// </summary>
        [SerializeField]
        [Tooltip("All of the controller's moves.")]
        public List<Move> Moves;
        #endregion
        #region Collision
        /// <summary>
        /// What paths the controller collides with. An object is on a path if it or one of its parents
        /// has that path's name.
        /// </summary>
        [SerializeField]
        [Tooltip("What paths the controller is on. An object is on a path if it or one of its parents " +
                 "has that path's name.")]
        public List<string> Paths;
        #endregion
        #region Sensors
        /// <summary>
        /// Contains sensor data for hit detection.
        /// </summary>
        [SerializeField]
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
        [SerializeField]
        [Tooltip("Ground friction in units per second squared.")]
        public float GroundFriction;

        /// <summary>
        /// This coefficient is applied to horizontal speed when horizontal and vertical speed
        /// requirements are met.
        /// </summary>
        [SerializeField]
        [Tooltip("Air drag coefficient applied if horizontal speed is greater than air drag speed.")]
        public float AirDrag;

        /// <summary>
        /// The speed above which air drag begins.
        /// </summary>
        [SerializeField]
        [Tooltip("The speed above which air drag begins.")]
        public Vector2 AirDragRequiredSpeed;

        /// <summary>
        /// The acceleration by gravity in units per second per second.
        /// </summary>
        [SerializeField]
        [Tooltip("Acceleration in the air, in the downward direction, in units per second squared.")]
        public float AirGravity;

        /// <summary>
        /// The magnitude of the force applied to the player when going up or down slopes.
        /// </summary>
        [SerializeField]
        [Tooltip("Acceleration on downward slopes, the maximum being this value, in units per second squared")]
        public float SlopeGravity;

        /// <summary>
        /// Direction of gravity. 0/360 is right, 90 is up, 180 is left, 270 is down.
        /// </summary>
        [SerializeField]
        [Tooltip("Direction of gravity in degrees. 0/360 is right, 90 is up, 180 is left, 270 is down.")]
        public float GravityDirection;

        /// <summary>
        /// Maximum speed in units per second. The controller can NEVER go faster than this.
        /// </summary>
        [SerializeField]
        [Tooltip("Maximum speed in units per second. The controller can NEVER go faster than this.")]
        public float MaxSpeed;

        /// <summary>
        /// The maximum height of a surface above the player's feet that it can snap onto without hindrance.
        /// </summary>
        [SerializeField]
        [Tooltip("The maximum height of a surface above the player's feet that it can snap onto without hindrance.")]
        public float LedgeClimbHeight;

        /// <summary>
        /// The maximum depth of a surface below the player's feet that it can drop to without hindrance.
        /// </summary>
        [SerializeField]
        [Tooltip("The maximum depth of a surface below the player's feet that it can drop to without hindrance.")]
        public float LedgeDropHeight;

        /// <summary>
        /// The minimum speed in units per second the player must be moving at to stagger each physics update,
        /// processing the movement in fractions.
        /// </summary>
        [SerializeField]
        [Tooltip("The minimum speed in units per second the player must be moving at to stagger each physics update, " +
                 "processing the movement in fractions.")]
        public float AntiTunnelingSpeed;

        /// <summary>
        /// The minimum angle of an incline at which slope gravity is applied.
        /// </summary>
        [SerializeField]
        [Tooltip("The minimum angle of an incline at which slope gravity is applied.")]
        public float SlopeGravityBeginAngle;

        /// <summary>
        /// The controller falls off walls and ceilings if moving below this speed.
        /// </summary>
        [SerializeField]
        [Tooltip("The controller falls off walls and ceilings if moving below this speed.")]
        public float DetachSpeed;

        /// <summary>
        /// The controller can't latch onto walls if they're this much steeper than the floor it's on, in degrees.
        /// </summary>
        [SerializeField]
        [Tooltip("The controller can't latch onto walls if they're this much steeper than the floor it's on, in degrees.")]
        public float MaxClimbAngle;
        #endregion
        #region Events
        /// <summary>
        /// Invoked when the controller is touching something with both its top and ground sensors.
        /// </summary>
        [SerializeField]
        public UnityEvent OnCrush;

        /// <summary>
        /// Invoked when the controller lands on something.
        /// </summary>
        [SerializeField]
        public UnityEvent OnAttach;

        /// <summary>
        /// Invoked when the controller collides with a platform.
        /// </summary>
        [SerializeField]
        public PlatformCollisionEvent OnCollide;

        /// <summary>
        /// Invoked when the controller detaches from the ground for any reason.
        /// </summary>
        [SerializeField]
        public UnityEvent OnDetach;

        /// <summary>
        /// Invoked when the controller detaches from the ground because it was moving too slowly on a steep
        /// surface.
        /// </summary>
        [SerializeField]
        public UnityEvent OnSteepDetach;
        #endregion
        #endregion
        #region Control Variables
        /// <summary>
        /// Whether the controller has collisions and physics turned off. Often used for set pieces.
        /// </summary>
        [Tooltip("Whether the controller has collisions and physics turned off. Often used for set pieces.")]
        public bool Interrupted;

        /// <summary>
        /// The current control state.
        /// </summary>
        [SerializeField]
        [Tooltip("The current control state.")]
        public ControlState ControlState;

        /// <summary>
        /// The moves currently being performed.
        /// </summary>
        [SerializeField]
        [Tooltip("The moves currently being performed.")]
        public List<Move> ActiveMoves;

        /// <summary>
        /// The moves which can be performed given the controller's current state.
        /// </summary>
        [SerializeField]
        [Tooltip("The moves which can be performed given the controller's current state.")]
        public List<Move> AvailableMoves;

        /// <summary>
        /// The moves which cannot be performed given the controller's current state.
        /// </summary>
        [SerializeField]
        [Tooltip("The moves which cannot be performed given the controller's current state.")]
        public List<Move> UnavailableMoves; 
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
        [SerializeField]
        [Tooltip("The player's horizontal velocity in units per second.")]
        public float Vx;

        /// <summary>
        /// The player's vertical velocity in units per second.
        /// </summary>
        [SerializeField]
        [Tooltip("The player's vertical velocity in units per second.")]
        public float Vy;

        /// <summary>
        /// The controller's velocity on the ground in units per second. Positive if forward, negative if backward.
        /// </summary>
        [SerializeField]
        [Tooltip("The controller's velocity on the ground in units per second. Positive if forward, negative if backward.")]
        public float GroundVelocity;

        /// <summary>
        /// Whether the player is touching the ground.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether the player is touching the ground.")]
        public bool Grounded;

        /// <summary>
        /// If grounded, the angle of incline the controller is walking on in degrees. Goes hand-in-hand
        /// with rotation.
        /// </summary>
        [SerializeField]
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
        /// The controller will move this much in its next FixedUpdate. It is set using the Translate function,
        /// which guarantees that movement from outside sources is applied before collision checks.
        /// </summary>
        public Vector3 QueuedTranslation;
        #endregion

        #region Lifecycle Functions
        public virtual void Reset()
        {
            RendererObject = GetComponentInChildren<Renderer>().gameObject;
            Animator = RendererObject.GetComponent<Animator>() ?? GetComponentInChildren<Animator>();

            GroundControl = GetComponent<GroundControl>() ?? gameObject.AddComponent<GroundControl>();
            AirControl = GetComponent<AirControl>() ?? gameObject.AddComponent<AirControl>();
            AutoFindMoves = true;

            Paths = new List<string> {"Always Collide", "Path 1"};

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
        }

        public void Awake()
        {
            Footing = Footing.None;
            Grounded = false;
            Vx = Vy = GroundVelocity = 0.0f;
            JustDetached = false;
            QueuedTranslation = default(Vector3);

            if (AutoFindMoves)
            {
                Moves = GetComponentsInChildren<Move>().ToList();
            }

            ActiveMoves = new List<Move>();
            AvailableMoves = new List<Move>();
            UnavailableMoves = new List<Move>(Moves);

            ApplyAirDrag = ApplyAirGravity = ApplyGroundFriction = ApplySlopeGravity = DetachWhenSlow = true;
            AutoFlip = AutoRotate = FacingForward = true;
            AttachLock = DetachLock = false;
            EnterControlState(AirControl);
        }

        public void Update()
        {
            if (Interrupted)
                return;

            if(ControlState != null)
                ControlState.OnControlStateUpdate();

            for (var i = ActiveMoves.Count - 1; i >= 0; i--)
            {
                ActiveMoves[i].OnActiveUpdate();
            }

            HandleMoves();
        }

        public void FixedUpdate()
        {
            if (Interrupted)
                return;

            if(ControlState != null)
                ControlState.OnStateFixedUpdate();

            HandleForces();
            HandleQueuedTranslation();
            HandleMovement();

            for (var i = ActiveMoves.Count - 1; i >= 0; i--)
            {
                ActiveMoves[i].OnActiveFixedUpdate();
            }

            HandleDisplay();
        }
        #endregion

        #region Lifecycle Subroutines
        /// <summary>
        /// Handles movement caused by velocity.
        /// </summary>
        public void HandleMovement()
        {
            var vt = Mathf.Sqrt(Vx * Vx + Vy * Vy);

            if (vt < AntiTunnelingSpeed)
            {
                transform.position += new Vector3(Vx*Time.fixedDeltaTime, Vy*Time.fixedDeltaTime);
                HandleCollisions();
            }
            else
            {
                // If the controller's gotta go fast, split up collision into steps. The speed limit becomes however many steps
                // your computer can handle!
                var vc = vt;
                var steps = 0;
                while (vc > 0.0f)
                {
                    if (vc > AntiTunnelingSpeed)
                    {
                        transform.position += 
                            (new Vector3(Vx * Time.fixedDeltaTime, Vy * Time.fixedDeltaTime)) * (AntiTunnelingSpeed / vt);
                        vc -= AntiTunnelingSpeed;
                    }
                    else
                    {
                        transform.position += 
                            (new Vector3(Vx * Time.fixedDeltaTime, Vy * Time.fixedDeltaTime)) * (vc / vt);
                        vc = 0.0f;
                    }

                    HandleCollisions();
                    
                    // If the player's speed changes mid-stagger recalculate current velocity and total velocity
                    var vn = Mathf.Sqrt(Vx * Vx + Vy * Vy);
                    vc *= vn / vt;
                    vt *= vn / vt;

                    if (++steps > CollisionStepLimit) break;
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
        /// Handles move activation/deactivation and availability.
        /// </summary>
        public void HandleMoves()
        {
            if (UnavailableMoves.Count > 0)
            {
                foreach (var move in new List<Move>(UnavailableMoves))
                {
                    if (!move.Available())
                        continue;

                    if (!UnavailableMoves.Remove(move))
                        continue;

                    AvailableMoves.Add(move);
                    move.ChangeState(Move.State.Available);
                }
            }

            if (AvailableMoves.Count > 0)
            {
                foreach (var move in new List<Move>(AvailableMoves))
                {
                    if (!move.Available())
                    {
                        if (!AvailableMoves.Remove(move))
                            continue;

                        UnavailableMoves.Add(move);
                        move.ChangeState(Move.State.Unavailable);
                    }
                    else if (move.InputActivate())
                    {
                        if (!AvailableMoves.Remove(move))
                            continue;

                        ActiveMoves.Add(move);
                        move.ChangeState(Move.State.Active);
                    }
                }
            }

            if (ActiveMoves.Count > 0)
            {
                foreach (var move in new List<Move>(ActiveMoves))
                {
                    if (move.InputDeactivate())
                    {
                        if (!ActiveMoves.Remove(move))
                            continue;

                        AvailableMoves.Add(move);
                        move.ChangeState(Move.State.Available);
                    }
                }
            }
        }

        /// <summary>
        /// Uses Time.fixedDeltaTime as the timestep. Handles sprite rotation and other effects.
        /// </summary>
        public void HandleDisplay()
        {
            HandleDisplay(Time.fixedDeltaTime);
        }

        /// <summary>
        /// Handles sprite rotation and other effects.
        /// </summary>
        /// <param name="timestep">The specified timestep, in seconds.</param>
        public void HandleDisplay(float timestep)
        {
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

            if ((FacingForward && RendererObject.transform.localScale.x < 0.0f) || 
                (!FacingForward && RendererObject.transform.localScale.x > 0.0f))
            {
                RendererObject.transform.localScale = new Vector3(
                    -RendererObject.transform.localScale.x,
                    RendererObject.transform.localScale.y,
                    RendererObject.transform.localScale.z);
            }
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
                GroundSurfaceCheck();
                if (!SurfaceAngleCheck()) Detach();
            }
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
                NotifyCollision(sideLeftCheck);

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
                NotifyCollision(sideRightCheck);

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
                NotifyCollision(leftCheck);

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
                NotifyCollision(rightCheck);

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
                        NotifyCollision(groundLeftCheck);

                        if (!IgnoreNextCollision && HandleImpact(groundLeftCheck))
                            Footing = Footing.Left;

                        IgnoreNextCollision = false;
                    }
                    else
                    {
                        NotifyCollision(groundRightCheck);

                        if (!IgnoreNextCollision && HandleImpact(groundRightCheck))
                            Footing = Footing.Right;

                        IgnoreNextCollision = false;
                    }
                }
                else if (groundLeftCheck)
                {
                    NotifyCollision(groundLeftCheck);

                    if (!IgnoreNextCollision && HandleImpact(groundLeftCheck))
                        Footing = Footing.Left;

                    IgnoreNextCollision = false;
                }
                else
                {
                    NotifyCollision(groundRightCheck);

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
                NotifyCollision(sideLeftCheck);

                if (!IgnoreNextCollision)
                {
                    if (GroundVelocity < 0.0f)
                        GroundVelocity = 0.0f;

                    var push = (Vector3)(sideLeftCheck.Hit.point - (Vector2)Sensors.CenterLeft.position);
                    transform.position += push + push.normalized * DMath.Epsilon;

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
                NotifyCollision(sideRightCheck);

                if (!IgnoreNextCollision)
                {
                    if (GroundVelocity > 0.0f)
                        GroundVelocity = 0.0f;

                    var push = (Vector3)(sideRightCheck.Hit.point - (Vector2)Sensors.CenterRight.position);
                    transform.position += push + push.normalized * DMath.Epsilon;

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
                NotifyCollision(ceilLeftCheck);

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
                NotifyCollision(ceilRightCheck);

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
            if ((DMath.Equalsf(diff) && 
                    DMath.Equalsf(overlap, left.SurfaceAngle) && DMath.Equalsf(overlap, right.SurfaceAngle)) ||
                (diff > 0.0f && DMath.AngleInRange(overlap, left.SurfaceAngle, right.SurfaceAngle)) || 
                (diff < 0.0f && DMath.AngleInRange(overlap, right.SurfaceAngle, left.SurfaceAngle)))
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
            NotifyCollision(left);

            if (!IgnoreNextCollision)
            {
                Footing = Footing.Left;
                SensorsRotation = SurfaceAngle = left.SurfaceAngle * Mathf.Rad2Deg;
                transform.position += (Vector3)(left.Hit.point - (Vector2)Sensors.BottomLeft.position);

                SetSurface(left);
            }

            goto finish;

            orientRight:
            NotifyCollision(right);

            if (!IgnoreNextCollision)
            {
                Footing = Footing.Right;
                SensorsRotation = SurfaceAngle = right.SurfaceAngle * Mathf.Rad2Deg;
                transform.position += (Vector3)(right.Hit.point - (Vector2)Sensors.BottomRight.position);

                SetSurface(right);
            }

            goto finish;

            orientLeftBoth:
            NotifyCollision(left);
            NotifyCollision(right);

            if (!IgnoreNextCollision)
            {
                Footing = Footing.Left;
                SensorsRotation = SurfaceAngle = DMath.Angle(right.Hit.point - left.Hit.point) * Mathf.Rad2Deg;
                transform.position += (Vector3)(left.Hit.point - (Vector2)Sensors.BottomLeft.position);

                SetSurface(left, right);
            }

            goto finish;

            orientRightBoth:
            NotifyCollision(right);
            NotifyCollision(left);

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
        /// Check for changes in angle of incline for when player is on the ground.
        /// </summary>
        /// <returns><c>true</c> if the angle of incline is tolerable, <c>false</c> otherwise.</returns>
        private bool SurfaceAngleCheck()
        {
            // Can only stay on the surface if angle difference is low enough
            if (Grounded)
            {
                Vx = GroundVelocity * Mathf.Cos(SurfaceAngle * Mathf.Deg2Rad);
                Vy = GroundVelocity * Mathf.Sin(SurfaceAngle * Mathf.Deg2Rad);
                return true;
            }

            Detach();
            return false;
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
        /// <param name="endMoves">Whether to stop whatever moves the controller is performing.</param>
        public void Interrupt(bool endMoves = false)
        {
            Detach();
            Interrupted = true;

            if (!endMoves) return;
            foreach (var move in new List<Move>(ActiveMoves))
            {
                EndMove(move);
            }
        }

        /// <summary>
        /// Turns on the controller's physics, usually after leaving a set piece.
        /// </summary>
        public void Resume()
        {
            Interrupted = false;
        }

        /// <summary>
        /// Enters the specified control state.
        /// </summary>
        /// <param name="state">The specified control state.</param>
        public void EnterControlState(ControlState state)
        {
            if (ControlState != null)
            {
                ControlState.OnControlStateExit(state);
                ControlState.IsCurrent = false;
            }

            state.OnControlStateEnter(ControlState);
            ControlState = state;
            ControlState.IsCurrent = true;
        }

        /// <summary>
        /// Returns the first move of the specified type.
        /// </summary>
        /// <typeparam name="TMove">The specified type.</typeparam>
        /// <returns></returns>
        public TMove GetMove<TMove>() where TMove : Move
        {
            return Moves.FirstOrDefault(move => move is TMove) as TMove;
        }

        /// <summary>
        /// Returns whether the first move of the specified type is available.
        /// </summary>
        /// <typeparam name="TMove">The specified type.</typeparam>
        /// <returns></returns>
        public bool IsAvailable<TMove>() where TMove : Move
        {
            return AvailableMoves.Any(move => move is TMove);
        }

        /// <summary>
        /// Returns whether the first move of the specified type is active.
        /// </summary>
        /// <typeparam name="TMove">The specified type.</typeparam>
        /// <returns></returns>
        public bool IsActive<TMove>() where TMove : Move
        {
            return ActiveMoves.Any(move => move is TMove);
        }

        /// <summary>
        /// Forces the controller to perform the first move of the specified type, even if it's unavailable.
        /// </summary>
        /// <typeparam name="TMove">The specified type.</typeparam>
        /// <returns>Whether the move was performed.</returns>
        public bool ForcePerformMove<TMove>() where TMove : Move
        {
            return ForcePerformMove(Moves.FirstOrDefault(m => m is TMove));
        }

        /// <summary>
        /// Forces the controller to perform the specified move, even if it's unavailable.
        /// </summary>
        /// <param name="move">The specified move.</param>
        /// <returns>Whether the move was performed.</returns>
        public bool ForcePerformMove(Move move)
        {
            if (move == null || move.CurrentState == Move.State.Active)
                return false;

            if (move.CurrentState == Move.State.Unavailable)
            {
                if (!UnavailableMoves.Remove(move))
                    return false;

                ActiveMoves.Add(move);
                return move.ChangeState(Move.State.Active);
            }
            else if (move.CurrentState == Move.State.Available)
            {
                return PerformMove(move);
            }

            return false;
        }

        /// <summary>
        /// Performs the first move of the specified type, if it's available.
        /// </summary>
        /// <typeparam name="TMove">The specified type.</typeparam>
        /// <returns>Whether the move was performed.</returns>
        public bool PerformMove<TMove>() where TMove : Move
        {
            return PerformMove(AvailableMoves.FirstOrDefault(m => m is TMove));
        }

        /// <summary>
        /// Performs the specified move, if it's available.
        /// </summary>
        /// <param name="move">The specified move.</param>
        /// <returns>Whether the move was performed.</returns>
        public bool PerformMove(Move move)
        {
            if (move == null || move.CurrentState != Move.State.Available)
                return false;

            if (!AvailableMoves.Remove(move))
                return false;

            ActiveMoves.Add(move);
            return move.ChangeState(Move.State.Active);
        }

        /// <summary>
        /// Ends the first move of the specified type, if it's active.
        /// </summary>
        /// <typeparam name="TMove">The specified type.</typeparam>
        /// <returns>Whether the move was ended.</returns>
        public bool EndMove<TMove>() where TMove : Move
        {
            return EndMove(ActiveMoves.FirstOrDefault(m => m is TMove));
        }

        /// <summary>
        /// Ends the specified move, if it's active.
        /// </summary>
        /// <param name="move">The specified move.</param>
        /// <returns>Whether the move was ended.</returns>
        public bool EndMove(Move move)
        {
            if (move == null || move.CurrentState != Move.State.Active)
                return false;

            if (!ActiveMoves.Remove(move)) return false;
            if (move.Available())
            {
                AvailableMoves.Add(move);
                move.ChangeState(Move.State.Available);
            }
            else
            {
                UnavailableMoves.Add(move);
                move.ChangeState(Move.State.Unavailable);
            }

            return true;
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
                OnDetach.Invoke();

            Grounded = false;
            JustDetached = true;
            Footing = Footing.None;
            SensorsRotation = GravityDirection + 90.0f;
            SetSurface(null);
            EnterControlState(AirControl);

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
                OnAttach.Invoke();

            var angleDegrees = DMath.Modp(angleRadians * Mathf.Rad2Deg, 360.0f);
            GroundVelocity = groundSpeed;
            SurfaceAngle = angleDegrees;
            SensorsRotation = SurfaceAngle;
            Grounded = true;
            EnterControlState(GroundControl);

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
        /// Lets a platform's trigger know about a collision, if it has one.
        /// </summary>
        /// <param name="collision"></param>
        /// <returns>Whether a platform trigger was notified.</returns>
        private bool NotifyCollision(TerrainCastHit collision)
        {
            if (!collision || collision.Hit.transform == null) return false;
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
        #endregion
        #region Surface Calculation Functions
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
                return DMath.Equalsf(cast.Hit.fraction, 0.0f) ? default(TerrainCastHit) : cast;
            }
            cast = this.TerrainCast(Sensors.LedgeClimbRight.position, Sensors.LedgeDropRight.position,
                ControllerSide.Bottom);

            if (!cast)
            {
                return default(TerrainCastHit);
            }
            if (DMath.Equalsf(cast.Hit.fraction, 0.0f))
            {
                return default(TerrainCastHit);
            }

            return cast;
        }
        #endregion
    }
}