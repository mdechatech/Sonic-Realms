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
        public GroundControl DefaultGroundState;

        /// <summary>
        /// The default control state to enter when in the air.
        /// </summary>
        [SerializeField]
        [Tooltip("The default control state to enter when in the air.")]
        public AirControl DefaultAirState;

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
        [SerializeField]
        public CollisionMode CollisionMode = CollisionMode.Layers;

        /// <summary>
        /// The layer mask which represents the ground the player checks for collision with.
        /// </summary>
        [HideInInspector]
        public LayerMask TerrainMask;

        [SerializeField]
        public List<string> TerrainTags = new List<string>();

        [SerializeField]
        public List<string> TerrainNames = new List<string>();
        #endregion
        #region Sensors
        /// <summary>
        /// Contains sensor data for hit detection.
        /// </summary>
        [SerializeField]
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
        [Range(0.0f, 0.25f)]
        [Tooltip("Ground friction in units per second squared.")]
        public float GroundFriction = 0.12f;

        /// <summary>
        /// The player's horizontal acceleration in the air in units per second per second.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 0.25f)]
        [Tooltip("Air acceleration in units per second squared.")]
        public float AirAcceleration = 0.16f;

        /// <summary>
        /// Minimum horizontal speed requirement for air drag.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 20.0f)]
        [Tooltip("Horizontal speed requirement for air drag.")]
        public float AirDragHorizontalSpeed = 0.25f;

        /// <summary>
        /// Minimum vertical speed requirement for air drag.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 20.0f)]
        [Tooltip("Vertical speed requirement for air drag.")]
        public float AirDragVerticalSpeed = 4.8f;

        /// <summary>
        /// This coefficient is applied to horizontal speed when horizontal and vertical speed
        /// requirements are met.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 1.0f)]
        [Tooltip("Air drag coefficient applied if horizontal speed is greater than air drag speed.")]
        public float AirDragCoefficient = 0.9738896f;

        /// <summary>
        /// The acceleration by gravity in units per second per second.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 1.0f)]
        [Tooltip("Acceleration in the air, in the downward direction, in units per second squared.")]
        public float AirGravity = 0.3f;

        /// <summary>
        /// The magnitude of the force applied to the player when going up or down slopes.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 1.0f)]
        [Tooltip("Acceleration on downward slopes, the maximum being this value, in units per second squared")]
        public float SlopeGravity = 0.3f;

        /// <summary>
        /// Direction of gravity. 0/360 is right, 90 is up, 180 is left, 270 is down.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 360.0f)]
        [Tooltip("Direction of gravity in degrees. 0/360 is right, 90 is up, 180 is left, 270 is down.")]
        public float GravityDirection = 270.0f;
        /// <summary>
        /// The player's maximum speed in units per second.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 100.0f)]
        [Tooltip("Maximum speed in units per second.")]
        public float MaxSpeed = 20.0f;

        [SerializeField]
        public float LedgeClimbHeight = 0.25f;

        /// <summary>
        /// The maximum depth of a surface below the player's feet that it can drop to without hindrance.
        /// </summary>
        [SerializeField]
        public float LedgeDropHeight = 0.25f;

        /// <summary>
        /// The minimum speed in units per second the player must be moving at to stagger each physics update,
        /// processing the movement in fractions.
        /// </summary>
        [SerializeField]
        public float AntiTunnelingSpeed = 5.0f;

        /// <summary>
        /// The minimum angle of an incline at which slope gravity is applied.
        /// </summary>
        [SerializeField]
        public float SlopeGravityBeginAngle = 10.0f;

        /// <summary>
        /// The speed in units per second below which the player must be traveling on a wall or ceiling to be
        /// detached from it.
        /// </summary>
        [SerializeField]
        public float DetachSpeed = 3.0f;

        /// <summary>
        /// The maximum change in angle between two surfaces that the player can walk in.
        /// </summary>
        [SerializeField]
        public float MaxSurfaceAngleDifference = 70.0f;

        /// <summary>
        /// The maximum surface angle difference from a vertical wall at which a player is able to detach from
        /// through use of the directional key opposite to the one in which it is traveling.
        /// </summary>
        [SerializeField]
        public float MaxVerticalDetachAngle = 5.0f;
        #endregion
        #region Events
        /// <summary>
        /// Invoked when the controller is touching something with both its top and ground sensors.
        /// </summary>
        [SerializeField]
        public UnityEvent OnCrush;

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
        /// The controller's velocity as a Vector2.
        /// </summary>
        public Vector2 Velocity
        {
            get { return new Vector2(Vx, Vy); }
            set
            {
                if (Grounded)
                {
                    SetGroundVelocity(Velocity);
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
        /// The controller's velocity on the ground; the faster it's running, the higher in magnitude
        /// this number is. If it's moving forward (counter-clockwise inside a loop), this is positive.
        /// If backwards (clockwise inside a loop), negative.
        /// </summary>
        public float GroundVelocity
        {
            get { return Vg; }
            set { Vg = value; }
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
        [HideInInspector]
        public float Vx;

        /// <summary>
        /// The player's vertical velocity in units per second.
        /// </summary>
        [HideInInspector]
        public float Vy;

        /// <summary>
        /// If grounded, the player's ground velocity in units per second.
        /// </summary>
        [HideInInspector]
        public float Vg;

        /// <summary>
        /// Whether the player is touching the ground.
        /// </summary>
        /// <value><c>true</c> if grounded; otherwise, <c>false</c>.</value>
        [HideInInspector]
        public bool Grounded;

        /// <summary>
        /// If grounded, the angle of incline the controller is walking on in degrees. Goes hand-in-hand
        /// with rotation.
        /// </summary>
        [HideInInspector]
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
            AutoFindMoves = true;
        }

        public void Awake()
        {
            Footing = Footing.None;
            Grounded = false;
            Vx = Vy = Vg = 0.0f;
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
            EnterControlState(DefaultAirState);
        }

        public void Update()
        {
            if(ControlState != null) ControlState.OnStateUpdate();
            for (var i = ActiveMoves.Count - 1; i >= 0; i--)
            {
                ActiveMoves[i].OnActiveUpdate();
            }

            HandleMoves();
        }

        public void FixedUpdate()
        {
            if(ControlState != null) ControlState.OnStateFixedUpdate();
            HandleForces();

            if (QueuedTranslation != default(Vector3))
            {
                transform.position += QueuedTranslation;
                QueuedTranslation = default(Vector3);
            }
            
            // Stagger routine - if the player's gotta go fast, move it in increments of AntiTunnelingSpeed
            // to prevent tunneling.
            var vt = Mathf.Sqrt(Vx * Vx + Vy * Vy);
            
            if (vt < AntiTunnelingSpeed)
            {
                transform.position = new Vector2(transform.position.x + (Vx * Time.fixedDeltaTime), transform.position.y + (Vy * Time.fixedDeltaTime));
                HandleCollisions();
            }
            else
            {
                var vc = vt;
                while (vc > 0.0f)
                {
                    if (vc > AntiTunnelingSpeed)
                    {
                        transform.position += (new Vector3(Vx * Time.fixedDeltaTime, Vy * Time.fixedDeltaTime)) * (AntiTunnelingSpeed / vt);
                        vc -= AntiTunnelingSpeed;
                    }
                    else
                    {
                        transform.position += (new Vector3(Vx * Time.fixedDeltaTime, Vy * Time.fixedDeltaTime)) * (vc / vt);
                        vc = 0.0f;
                    }

                    HandleCollisions();

                    // If the player's speed changes mid-stagger recalculate current velocity and total velocity
                    var vn = Mathf.Sqrt(Vx * Vx + Vy * Vy);
                    vc *= vn / vt;
                    vt *= vn / vt;
                }
            }

            for (var i = ActiveMoves.Count - 1; i >= 0; i--)
            {
                ActiveMoves[i].OnActiveFixedUpdate();
            }

            HandleDisplay(Time.fixedDeltaTime);
        }
        #endregion

        #region Lifecycle Subroutines
        public void HandleMoves()
        {
            for (var i = UnavailableMoves.Count - 1; i >= 0; i--)
            {
                var move = UnavailableMoves[i];
                if (move.Available())
                {
                    UnavailableMoves.RemoveAt(i);
                    AvailableMoves.Add(move);
                    move.ChangeState(Move.State.Available);
                }
            }

            for (var i = AvailableMoves.Count - 1; i >= 0; i--)
            {
                var move = AvailableMoves[i];

                if (!move.Available())
                {
                    AvailableMoves.RemoveAt(i);
                    UnavailableMoves.Add(move);
                    move.ChangeState(Move.State.Unavailable);
                }
                else if (move.InputActivate())
                {
                    AvailableMoves.RemoveAt(i);
                    ActiveMoves.Add(move);
                    move.ChangeState(Move.State.Active);
                }
            }

            for (var i = ActiveMoves.Count - 1; i >= 0; i--)
            {
                var move = ActiveMoves[i];
                if (move.InputDeactivate())
                {
                    ActiveMoves.RemoveAt(i);
                    AvailableMoves.Add(move);
                    move.ChangeState(Move.State.Available);
                }
            }
        }

        public void HandleDisplay()
        {
            HandleDisplay(Time.fixedDeltaTime);
        }

        public void HandleDisplay(float timestep)
        {
            if (AutoFlip)
            {
                FacingForward = Grounded ? GroundVelocity >= 0.0f : RelativeVelocity.x >= 0.0f;
            }

            if (!Grounded)
            {
                RendererObject.transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 
                    Mathf.LerpAngle(transform.eulerAngles.z, SensorsRotation, Time.fixedDeltaTime*20.0f));
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
                    Vg -= SlopeGravity*Mathf.Sin(RelativeSurfaceAngle*Mathf.Deg2Rad)*timestep;
                }

                // Ground friction
                if (ApplyGroundFriction)
                    Vg -= Mathf.Min(Mathf.Abs(Vg), GroundFriction*timestep)*Mathf.Sign(Vg);

                // Speed limit
                if (Vg > MaxSpeed) Vg = MaxSpeed;
                else if (Vg < -MaxSpeed) Vg = -MaxSpeed;

                // Detachment from walls if speed is too low
                if (DetachWhenSlow &&
                    Mathf.Abs(Vg) < DetachSpeed && 
                    DMath.AngleInRange_d(RelativeSurfaceAngle, 
                        90.0f - MaxVerticalDetachAngle,
                        270.0f + MaxVerticalDetachAngle))
                {
                    Detach();
                    OnSteepDetach.Invoke();
                }
            }
            else
            {
                if(ApplyAirGravity)
                    Velocity += DMath.AngleToVector(GravityDirection*Mathf.Deg2Rad)*AirGravity*timestep;

                if (ApplyAirDrag && 
                    Vy > AirDragVerticalSpeed && Mathf.Abs(Vx) > AirDragHorizontalSpeed)
                {
                    Vx *= Mathf.Pow(AirDragCoefficient, timestep);
                }
            }
        }

        /// <summary>
        /// Checks for collision with all sensors, changing position, velocity, and rotation if necessary. This should be called each time
        /// the player changes position. This method does not require a timestep because it only resolves overlaps in the player's collision.
        /// </summary>
        public void HandleCollisions()
        {
            var anyHit = false;

            if(CrushCheck()) OnCrush.Invoke();

            if (!Grounded)
            {
                anyHit = AirSideCheck() | AirCeilingCheck() | AirGroundCheck();
                SensorsRotation = GravityDirection + 90.0f;
            }

            if (Grounded)
            {
                anyHit = GroundSideCheck() | GroundCeilingCheck() | GroundSurfaceCheck();
                if (!SurfaceAngleCheck()) Detach();
            }

            if (!anyHit && JustDetached)
                JustDetached = false;
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
                transform.position += (Vector3) sideLeftCheck.Hit.point - Sensors.CenterLeft.position +
                                      ((Vector3) sideLeftCheck.Hit.point - Sensors.CenterLeft.position).normalized*
                                      DMath.Epsilon;

                if (Vx < 0.0f) Vx = 0.0f;

                NotifyCollision(sideLeftCheck);

                return true;
            }
            if (sideRightCheck)
            {
                transform.position += (Vector3)sideRightCheck.Hit.point - Sensors.CenterRight.position +
                                      ((Vector3)sideRightCheck.Hit.point - Sensors.CenterRight.position)
                                      .normalized * DMath.Epsilon;

                if (Vx > 0.0f) Vx = 0.0f;

                NotifyCollision(sideRightCheck);

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
            var leftCheck = this.TerrainCast(Sensors.CenterLeft.position, Sensors.TopLeft.position, ControllerSide.Top);

            if (leftCheck)
            {
                transform.position += (Vector3) leftCheck.Hit.point - Sensors.TopLeft.position;
                if((!JustDetached) && HandleImpact(leftCheck)) Footing = Footing.Left;

                NotifyCollision(leftCheck);

                return true;
            }

            var rightCheck = this.TerrainCast(Sensors.CenterRight.position, Sensors.TopRight.position, ControllerSide.Top);

            if (rightCheck)
            {
                transform.position += (Vector3) rightCheck.Hit.point - Sensors.TopRight.position;
                if((!JustDetached) && HandleImpact(rightCheck)) Footing = Footing.Right;

                NotifyCollision(rightCheck);

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
                            (GravityDirection + 360.0f)*Mathf.Deg2Rad) >= 0.0f)
                    {
                        if (HandleImpact(groundLeftCheck)) Footing = Footing.Left;
                        NotifyCollision(groundLeftCheck);
                    }
                    else
                    {
                        if (HandleImpact(groundRightCheck)) Footing = Footing.Right;
                        NotifyCollision(groundRightCheck);
                    }
                }
                else if (groundLeftCheck)
                {
                    if (HandleImpact(groundLeftCheck)) Footing = Footing.Left;
                    NotifyCollision(groundLeftCheck);
                }
                else
                {
                    if (HandleImpact(groundRightCheck)) Footing = Footing.Right;
                    NotifyCollision(groundRightCheck);
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
                if(Vg < 0.0f) Vg = 0.0f;
                transform.position += (Vector3) sideLeftCheck.Hit.point - Sensors.CenterLeft.position +
                                      ((Vector3) sideLeftCheck.Hit.point - Sensors.CenterLeft.position).normalized*
                                      DMath.Epsilon;

                // If running down a wall and hits the floor, orient the player onto the floor
                if (DMath.AngleInRange_d(RelativeSurfaceAngle,
                    MaxSurfaceAngleDifference, 180.0f - MaxSurfaceAngleDifference))
                {
                    SurfaceAngle -= 90.0f;
                }

                SensorsRotation = SurfaceAngle;
                NotifyCollision(sideLeftCheck);

                return true;
            }
            if (sideRightCheck)
            {
                if(Vg > 0.0f) Vg = 0.0f;
                transform.position += (Vector3)sideRightCheck.Hit.point - Sensors.CenterRight.position +
                                      ((Vector3)sideRightCheck.Hit.point - Sensors.CenterRight.position)
                                      .normalized * DMath.Epsilon;
                
                // If running down a wall and hits the floor, orient the player onto the floor
                if (DMath.AngleInRange_d(RelativeSurfaceAngle,
                    180.0f + MaxSurfaceAngleDifference, 360.0f - MaxSurfaceAngleDifference))
                {
                    SurfaceAngle += 90.0f;
                }

                SensorsRotation = SurfaceAngle;
                NotifyCollision(sideRightCheck);

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
                if(Vg < 0.0f) Vg = 0.0f;

                // Add epsilon to prevent sticky collisions
                transform.position += (Vector3) ceilLeftCheck.Hit.point - Sensors.TopLeft.position +
                                      ((Vector3) ceilLeftCheck.Hit.point - Sensors.TopLeft.position).normalized*
                                      DMath.Epsilon;

                NotifyCollision(ceilLeftCheck);

                return true;
            }
            if (ceilRightCheck)
            {
                if (Vg > 0.0f) Vg = 0.0f;
                transform.position += (Vector3) ceilRightCheck.Hit.point - Sensors.TopRight.position +
                                      ((Vector3) ceilRightCheck.Hit.point - Sensors.TopRight.position)
                                          .normalized*DMath.Epsilon;

                NotifyCollision(ceilRightCheck);

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
            if ((!left || leftDiff > MaxSurfaceAngleDifference) && (!right || rightDiff > MaxSurfaceAngleDifference))
                goto detach;

            // If only one sensor found a surface we don't need any further checks
            if (!left && right)
                goto orientRight;

            if (left && !right)
                goto orientLeft;

            // Both feet have surfaces beneath them, things get complicated

            // If a surface is too steep, use only one surface (as long as that surface isn't too different from the current)
            if (diff > MaxSurfaceAngleDifference || diff < -MaxSurfaceAngleDifference)
            {
                if (leftDiff < rightDiff && leftDiff < MaxSurfaceAngleDifference/4.0f)
                    goto orientLeft;

                if(rightDiff < MaxSurfaceAngleDifference/4.0f)
                    goto orientRight;
            }

            // Propose an angle that would rotate the controller between both surfaces
            var overlap = DMath.Angle(right.Hit.point - left.Hit.point);

            // If the proposed angle is between the angles of the two surfaces, it's works
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
            if (leftDiff < MaxSurfaceAngleDifference || rightDiff < MaxSurfaceAngleDifference)
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
            Footing = Footing.Left;
            SensorsRotation = SurfaceAngle = left.SurfaceAngle * Mathf.Rad2Deg;
            transform.position += (Vector3)left.Hit.point - Sensors.BottomLeft.position;

            SetSurface(left);
            NotifyCollision(left);

            goto finish;

            orientRight:
            Footing = Footing.Right;
            SensorsRotation = SurfaceAngle = right.SurfaceAngle * Mathf.Rad2Deg;
            transform.position += (Vector3)right.Hit.point - Sensors.BottomRight.position;

            SetSurface(right);
            NotifyCollision(right);

            goto finish;

            orientLeftBoth:
            Footing = Footing.Left;
            SensorsRotation = SurfaceAngle = DMath.Angle(right.Hit.point - left.Hit.point)*Mathf.Rad2Deg;
            transform.position += (Vector3)left.Hit.point - Sensors.BottomLeft.position;

            SetSurface(left, right);
            NotifyCollision(left);
            NotifyCollision(right);

            goto finish;

            orientRightBoth:
            Footing = Footing.Right;
            SensorsRotation = SurfaceAngle = DMath.Angle(right.Hit.point - left.Hit.point)*Mathf.Rad2Deg;
            transform.position += (Vector3)right.Hit.point - Sensors.BottomRight.position;

            SetSurface(right, left);
            NotifyCollision(right);
            NotifyCollision(left);

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
                Vx = Vg * Mathf.Cos(SurfaceAngle * Mathf.Deg2Rad);
                Vy = Vg * Mathf.Sin(SurfaceAngle * Mathf.Deg2Rad);
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

        #region Control Functions
        /// <summary>
        /// Enters the specified control state.
        /// </summary>
        /// <param name="state">The specified control state.</param>
        public void EnterControlState(ControlState state)
        {
            if (ControlState != null)
            {
                ControlState.OnStateExit(state);
                ControlState.IsCurrent = false;
            }

            state.OnStateEnter(ControlState);
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
        public bool IsAvailable<TMove>()
        {
            return AvailableMoves.Any(move => move is TMove);
        }

        /// <summary>
        /// Returns whether the first move of the specified type is active.
        /// </summary>
        /// <typeparam name="TMove">The specified type.</typeparam>
        /// <returns></returns>
        public bool IsActive<TMove>()
        {
            return ActiveMoves.Any(move => move is TMove);
        }

        /// <summary>
        /// Forces the controller to perform the first move of the specified type, even if it's unavailable.
        /// </summary>
        /// <typeparam name="TMove">The specified type.</typeparam>
        /// <returns>Whether the move was performed.</returns>
        public bool ForcePerformMove<TMove>()
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
                UnavailableMoves.Remove(move);
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
        public bool PerformMove<TMove>()
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

            AvailableMoves.Remove(move);
            ActiveMoves.Add(move);
            return move.ChangeState(Move.State.Active);
        }

        /// <summary>
        /// Ends the first move of the specified type, if it's active.
        /// </summary>
        /// <typeparam name="TMove">The specified type.</typeparam>
        /// <returns>Whether the move was ended.</returns>
        public bool EndMove<TMove>()
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

            ActiveMoves.Remove(move);
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
            return Vg = DMath.ScalarProjection(velocity, SurfaceAngle*Mathf.Deg2Rad);
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
            return Vg += DMath.ScalarProjection(velocity, SurfaceAngle * Mathf.Deg2Rad);
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
        /// </summary>
        public void Detach()
        {
            // Don't invoke the event if the controller was already off the ground
            if (Grounded)
                OnDetach.Invoke();

            Grounded = false;
            JustDetached = true;
            Footing = Footing.None;
            SensorsRotation = GravityDirection + 90.0f;
            SetSurface(null);
            EnterControlState(DefaultAirState);
        }

        /// <summary>
        /// Attaches the player to a surface within the reach of its surface sensors. The angle of attachment
        /// need not be perfect; the method works reliably for angles within 45 degrees of the one specified.
        /// </summary>
        /// <param name="groundSpeed">The ground speed of the player after attaching.</param>
        /// <param name="angleRadians">The angle of the surface, in radians.</param>
        public void Attach(float groundSpeed, float angleRadians)
        {
            var angleDegrees = DMath.Modp(angleRadians * Mathf.Rad2Deg, 360.0f);
            Vg = groundSpeed;
            SurfaceAngle = angleDegrees;
            SensorsRotation = SurfaceAngle;
            Grounded = true;
            EnterControlState(DefaultGroundState);
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