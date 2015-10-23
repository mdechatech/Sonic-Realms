using System.Collections.Generic;
using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Hedgehog.Core.Actors
{
    /// <summary>
    /// Controls the player.
    /// </summary>
    public class HedgehogController : MonoBehaviour
    {
        #region Inspector Fields
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
        #region Controls
        [SerializeField]
        public KeyCode JumpKey = KeyCode.W;

        [SerializeField]
        public KeyCode LeftKey = KeyCode.A;

        [SerializeField]
        public KeyCode RightKey = KeyCode.D;

        [SerializeField]
        public KeyCode DebugSpindashKey = KeyCode.Space;
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
        /// The player's ground acceleration in units per second per second.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 0.25f)]
        [Tooltip("Ground acceleration in units per second squared.")]
        public float GroundAcceleration = 0.08f;

        /// <summary>
        /// The player's friction on the ground in units per second per second.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 0.25f)]
        [Tooltip("Ground deceleration in units per second squared.")]
        public float GroundDeceleration = 0.12f;

        /// <summary>
        /// The player's braking speed in units per second per second.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 1.00f)]
        [Tooltip("Ground braking speed in units per second squared.")]
        public float GroundBrake = 0.5f;

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
        /// The speed of the player's jump in units per second.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 25.0f)]
        [Tooltip("Jump speed in units per second.")]
        public float JumpSpeed = 8.0f;

        [SerializeField]
        [Range(0.0f, 25.0f)]
        [Tooltip("The maximum vertical speed the player can have after releasing the jump button.")]
        public float ReleaseJumpSpeed = 5.0f;

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
        /// The player's top speed, the maximum it can attain by running, it units per second.
        /// </summary>
        [SerializeField]
        [Range(0.0f, 100.0f)]
        [Tooltip("Maximum speed achieved through running in units per second.")]
        public float TopSpeed = 20.0f;

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
        /// The duration in seconds of the horizontal lock.
        /// </summary>
        [SerializeField]
        public float HorizontalLockTime = 0.25f;

        /// <summary>
        /// If the player is moving very quickly and jumps, normally it will not make much of a difference
        /// and the player will usually end up re-attaching to the surface.
        /// 
        /// With this constant, the player is forced to leave the ground by at least the specified angle
        /// difference, in degrees.
        /// </summary>
        [SerializeField]
        public float ForceJumpAngleDifference = 30.0f;

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
        [SerializeField]
        public UnityEvent OnCrush;
        #endregion
        #endregion
        #region Input Variables
        /// <summary>
        /// Whether the left key was held down since the last update. Key is determined by LeftKey.
        /// </summary>
        [HideInInspector]
        public bool LeftKeyDown;

        /// <summary>
        /// Whether the right key was held down since the last update. Key is determined by RightKey.
        /// </summary>
        [HideInInspector]
        public bool RightKeyDown;

        /// <summary>
        /// Whether the jump key was pressed since the last update. Key is determined by JumpKey.
        /// </summary>
        [HideInInspector]
        public bool JumpKeyPressed;

        /// <summary>
        /// Whether the jump key was held down since the last update. Key is determined by JumpKey.
        /// </summary>
        [HideInInspector]
        public bool JumpKeyDown;

        /// <summary>
        /// Whether the jump key was released since the last update.
        /// </summary>
        [HideInInspector]
        public bool JumpKeyReleased;

        /// <summary>
        /// Temporary. Whether the debug spindash key was pressed since the last update. Key is
        /// determined by DebugSpindashKey.
        /// </summary>
        [HideInInspector]
        public bool DebugSpindashKeyDown;
        #endregion
        #region Physics Variables
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
            get { return DMath.RotateBy(Velocity, (270.0f - GravityDirection)*Mathf.Deg2Rad); }
            set { Velocity = DMath.RotateBy(value, (GravityDirection - 270.0f)*Mathf.Deg2Rad); }
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
        /// Whether the player has just landed on the ground. Is used to ignore surface angle
        /// once right after.
        /// </summary>
        [HideInInspector]
        private bool _justLanded;

        /// <summary>
        /// If grounded and hasn't just landed, the controller of incline in degrees the player walked on
        /// one FixedUpdate ago.
        /// </summary>
        [HideInInspector]
        public float LastSurfaceAngle;

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
        /// Whether the player has control of horizontal ground movement.
        /// </summary>
        [HideInInspector]
        public bool HorizontalLock;

        /// <summary>
        /// If horizontally locked, the time in seconds left on it.
        /// </summary>
        [HideInInspector]
        public float HorizontalLockTimer;

        /// <summary>
        /// If not grounded, whether to activate the horizontal control lock when the player lands.
        /// </summary>
        [HideInInspector]
        public bool LockUponLanding;

        /// <summary>
        /// Whether the player has just jumped. Is used to avoid collisions right after.
        /// </summary>
        [HideInInspector]
        public bool JustJumped;

        /// <summary>
        /// Whether the player can release the jump key to reduce vertical speed.
        /// </summary>
        [HideInInspector]
        public bool CanReleaseJump;

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

        public Vector3 QueuedTranslation;
        #endregion

        #region Lifecycle Functions
        public void Awake()
        {
            Footing = Footing.None;
            Grounded = false;
            Vx = Vy = Vg = 0.0f;
            LastSurfaceAngle = 0.0f;
            LeftKeyDown = RightKeyDown = JumpKeyPressed = DebugSpindashKeyDown = false;
            JustJumped = _justLanded = JustDetached = false;
            QueuedTranslation = default(Vector3);
        }

        public void Update()
        {
            GetInput();
        }

        public void FixedUpdate()
        {
            HandleForces();
            HandleInput();
            
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

            HandleDisplay(Time.fixedDeltaTime);
        }
        #endregion

        #region Lifecycle Subroutines
        /// <summary>
        /// Stores keyboard input for the next fixed update (and HandleInput).
        /// </summary>
        private void GetInput()
        {
            LeftKeyDown = Input.GetKey(LeftKey);
            RightKeyDown = Input.GetKey(RightKey);
            
            if(!JumpKeyPressed) JumpKeyPressed = Input.GetKeyDown(JumpKey);

            if (Grounded)
                JumpKeyReleased = false;
            else if (!JumpKeyReleased && JumpKeyDown && !Input.GetKey(JumpKey))
                JumpKeyReleased = true;

            JumpKeyDown = Input.GetKey(JumpKey);

            if (Grounded) DebugSpindashKeyDown = Input.GetKey(DebugSpindashKey);
        }

        /// <summary>
        /// Handles the input from the previous update using Time.fixedDeltaTime as the timestep.
        /// </summary>
        private void HandleInput()
        {
            HandleInput(Time.fixedDeltaTime);
        }

        /// <summary>
        /// Handles the input from the previous update.
        /// </summary>
        private void HandleInput(float timestep)
        {
            if (Grounded)
            {
                // Debug spindash
                if (DebugSpindashKeyDown)
                {
                    GroundVelocity = MaxSpeed*Mathf.Sign(GroundVelocity);
                    DebugSpindashKeyDown = false;
                }
                
                // Horizontal lock timer update
                if (HorizontalLock)
                {
                    HorizontalLockTimer -= timestep;
                    if (HorizontalLockTimer < 0.0f)
                    {
                        HorizontalLock = false;
                        HorizontalLockTimer = 0.0f;
                    }
                }

                // Jump
                if (JumpKeyPressed)
                {
                    Jump();
                    JumpKeyPressed = false;
                }

                // Ground movement
                if (!HorizontalLock)
                {
                    if (LeftKeyDown)
                    {
                        if (Vg > 0.0f)
                        {
                            Vg -= GroundBrake * timestep;
                        }
                        else if (Vg > -TopSpeed)
                        {
                            Vg -= GroundAcceleration * timestep;
                        }
                    }
                    else if (RightKeyDown)
                    {
                        if (Vg < 0.0f)
                        {
                            Vg += GroundBrake * timestep;
                        }
                        else if (Vg < TopSpeed)
                        {
                            Vg += GroundAcceleration * timestep;
                        }
                    }
                    else
                    {
                        // Ground friction
                        Vg -= Mathf.Min(Mathf.Abs(Vg), GroundDeceleration * timestep) * Mathf.Sign(Vg);
                    }
                }
                
            }
            else
            {
                if (LeftKeyDown)
                   RelativeVelocity += Vector2.left*AirAcceleration*timestep;
                else if (RightKeyDown)
                   RelativeVelocity += Vector2.right*AirAcceleration*timestep;

                if (JumpKeyPressed) JumpKeyPressed = false;
                if (JumpKeyReleased)
                {
                    ReleaseJump();
                    JumpKeyReleased = false;
                }
            }
        }

        public void HandleDisplay()
        {
            HandleDisplay(Time.fixedDeltaTime);
        }

        public void HandleDisplay(float timestep)
        {
            // Do a little smoothness later
            var globalRotation = SensorsRotation;
            if (!Grounded)
            {
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 
                    Mathf.LerpAngle(transform.eulerAngles.z, SensorsRotation, Time.fixedDeltaTime*20.0f));
            }
            else
            {
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, SensorsRotation);
            }
            
            SensorsRotation = globalRotation;
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
                if (!DMath.AngleInRange_d(RelativeSurfaceAngle, -SlopeGravityBeginAngle, SlopeGravityBeginAngle))
                {
                    Vg -= SlopeGravity*Mathf.Sin(RelativeSurfaceAngle*Mathf.Deg2Rad)*timestep;
                }

                // Speed limit
                if (Vg > MaxSpeed) Vg = MaxSpeed;
                else if (Vg < -MaxSpeed) Vg = -MaxSpeed;

                // Detachment from walls if speed is too low
                if (Mathf.Abs(Vg) < DetachSpeed && DMath.AngleInRange_d(RelativeSurfaceAngle, 45.0f, 315.0f))
                {
                    if (DMath.AngleInRange_d(RelativeSurfaceAngle, 
                        90.0f - MaxVerticalDetachAngle,
                        270.0f + MaxVerticalDetachAngle))
                    {
                        Detach(true);
                    }
                    else
                    {
                        LockHorizontal();
                    }
                }
            }
            else
            {
                Velocity += DMath.AngleToVector(GravityDirection*Mathf.Deg2Rad)*AirGravity*timestep;
                if (Vy > AirDragVerticalSpeed && Mathf.Abs(Vx) > AirDragHorizontalSpeed)
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
            var jumpedPreviousCheck = JustJumped;

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

            if (JustJumped && jumpedPreviousCheck) JustJumped = false;
            
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
                if (!JustJumped)
                {
                    if (RelativeVelocity.x < 0.0f) RelativeVelocity = new Vector2(0.0f, RelativeVelocity.y);
                }

                transform.position += (Vector3) sideLeftCheck.Hit.point - Sensors.CenterLeft.position +
                                      ((Vector3) sideLeftCheck.Hit.point - Sensors.CenterLeft.position).normalized*
                                      DMath.Epsilon;

                NotifyCollision(sideLeftCheck);

                return true;
            }
            if (sideRightCheck)
            {
                if (!JustJumped)
                {
                    if (RelativeVelocity.x > 0.0f) RelativeVelocity = new Vector2(0.0f, RelativeVelocity.y);
                }

                transform.position += (Vector3)sideRightCheck.Hit.point - Sensors.CenterRight.position +
                                      ((Vector3)sideRightCheck.Hit.point - Sensors.CenterRight.position)
                                      .normalized * DMath.Epsilon;

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
                if((!JustDetached || JustJumped) && HandleImpact(leftCheck)) Footing = Footing.Left;

                NotifyCollision(leftCheck);

                return true;
            }

            var rightCheck = this.TerrainCast(Sensors.CenterRight.position, Sensors.TopRight.position, ControllerSide.Top);

            if (rightCheck)
            {
                transform.position += (Vector3) rightCheck.Hit.point - Sensors.TopRight.position;
                if((!JustDetached || JustJumped) && HandleImpact(rightCheck)) Footing = Footing.Right;

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
                if (JustJumped)
                {
                    if (groundLeftCheck)
                    {
                        transform.position += (Vector3)groundLeftCheck.Hit.point - Sensors.BottomLeft.position;
                        NotifyCollision(groundLeftCheck);
                    }
                    if (groundRightCheck)
                    {
                        transform.position += (Vector3)groundRightCheck.Hit.point - Sensors.BottomRight.position;
                        NotifyCollision(groundRightCheck);
                    }
                }
                else
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
                    Debug.Log(RelativeSurfaceAngle);
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

            // Get easy checks out of the way
            if (!left && !right)
                goto detach;

            if (!left && right)
                goto orientRight;

            if (left && !right)
                goto orientLeft;

            // Both feet have surfaces beneath them, things get complicated
            var leftAngle = left.SurfaceAngle * Mathf.Rad2Deg;
            var rightAngle = right.SurfaceAngle * Mathf.Rad2Deg;
            var diff = DMath.ShortestArc_d(leftAngle, rightAngle);

            var leftDiff = Mathf.Abs(DMath.ShortestArc_d(SurfaceAngle, leftAngle));
            var rightDiff = Mathf.Abs(DMath.ShortestArc_d(SurfaceAngle, rightAngle));
            
            // If a surface is too steep, use only one surface 
            // (as long as that surface isn't too different from the current)
            if (diff > MaxSurfaceAngleDifference || diff < -MaxSurfaceAngleDifference)
            {
                if (leftDiff < rightDiff && leftDiff < MaxSurfaceAngleDifference/4.0f)
                    goto orientLeft;

                if(rightDiff < MaxSurfaceAngleDifference/4.0f)
                    goto orientRight;
            }

            // If the proposed angle is between the angles of the two surfaces, we can go ahead and use both sensors
            var overlap = DMath.Angle(right.Hit.point - left.Hit.point);
            if ((diff >= 0.0f && DMath.AngleInRange(overlap, left.SurfaceAngle, right.SurfaceAngle)) || 
                (diff <  0.0f && DMath.AngleInRange(overlap, right.SurfaceAngle, left.SurfaceAngle)))
            {
                // Angle closest to the current gets priority
                if (leftDiff < rightDiff)
                    goto orientLeftBoth;

                goto orientRightBoth;
            }

            // Otherwise use only one surface, angle closest to the current gets priority
            if (leftDiff < rightDiff)
                goto orientLeft;

            goto orientRight;

            // gotos don't seem that bad here please don't sue me
            orientLeft:
            Footing = Footing.Left;
            SurfaceAngle = left.SurfaceAngle * Mathf.Rad2Deg;
            SensorsRotation = SurfaceAngle;
            transform.position += (Vector3)left.Hit.point - Sensors.BottomLeft.position;
            SetSurface(left);
            NotifyCollision(left);
            goto finish;

            orientRight:
            Footing = Footing.Right;
            SurfaceAngle = right.SurfaceAngle * Mathf.Rad2Deg;
            SensorsRotation = SurfaceAngle;
            transform.position += (Vector3)right.Hit.point - Sensors.BottomRight.position;
            SetSurface(right);
            NotifyCollision(right);
            goto finish;

            // It's possible to come up short when using both surfaces since the sensors shift
            // during rotation. If we check after rotation and the sensor can't find the ground
            // directly beneath it as we expect, we have to undo everything.
            orientLeftBoth:
            Footing = Footing.Left;
            SurfaceAngle = DMath.Angle(right.Hit.point - left.Hit.point)*Mathf.Rad2Deg;
            SensorsRotation = SurfaceAngle;
            transform.position += (Vector3)left.Hit.point - Sensors.BottomLeft.position;
            SetSurface(left, right);
            NotifyCollision(left);
            NotifyCollision(right);
            goto finish;

            orientRightBoth:
            Footing = Footing.Right;
            SurfaceAngle = DMath.Angle(right.Hit.point - left.Hit.point)*Mathf.Rad2Deg;
            SensorsRotation = SurfaceAngle;
            transform.position += (Vector3)right.Hit.point - Sensors.BottomRight.position;
            SetSurface(right, left);
            NotifyCollision(right);
            NotifyCollision(left);
            goto finish;

            detach:
            Detach(false);
            return false;

            finish:
            recheck = Footing == Footing.Left
                ? this.TerrainCast(Sensors.LedgeClimbRight.position, Sensors.LedgeDropRight.position,
                    ControllerSide.Bottom)
                : this.TerrainCast(Sensors.LedgeClimbLeft.position, Sensors.LedgeDropLeft.position,
                    ControllerSide.Bottom);

            if (recheck && Vector2.Distance(recheck.Hit.point, Footing == Footing.Left
                ? Sensors.BottomRight.position
                : Sensors.BottomLeft.position) > 0.05f)
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
            LastSurfaceAngle = SurfaceAngle;

            // Can only stay on the surface if angle difference is low enough
            if (Grounded && (_justLanded ||
                             Mathf.Abs(DMath.ShortestArc_d(LastSurfaceAngle, SurfaceAngle)) < MaxSurfaceAngleDifference))
            {
                Vx = Vg * Mathf.Cos(SurfaceAngle * Mathf.Deg2Rad);
                Vy = Vg * Mathf.Sin(SurfaceAngle * Mathf.Deg2Rad);

                _justLanded = false;
                
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
        /// Locks the player's horizontal control on the ground for the time specified by HorizontalLockTime.
        /// </summary>
        public void LockHorizontal()
        {
            LockHorizontal(HorizontalLockTime);
        }

        /// <summary>
        /// Locks the player's horizontal control on the ground for the specified time.
        /// </summary>
        /// <param name="time">The specified time, in seconds</param>
        public void LockHorizontal(float time)
        {
            HorizontalLock = true;
            HorizontalLockTimer = HorizontalLockTime;
        }

        public void Jump()
        {
            JustJumped = true;

            // Forces the player to leave the ground using the constant ForceJumpAngleDifference.
            // Helps prevent sticking to surfaces when the player's gotta go fast.
            var originalAngle = DMath.Modp(DMath.Angle(new Vector2(Vx, Vy)) * Mathf.Rad2Deg, 360.0f);

            var surfaceNormal = (SurfaceAngle + 90.0f) * Mathf.Deg2Rad;
            Vx += JumpSpeed * Mathf.Cos(surfaceNormal);
            Vy += JumpSpeed * Mathf.Sin(surfaceNormal);

            if (DMath.Equalsf(Vg))
            {
                Detach();
                return;
            }

            var newAngle = DMath.Modp(DMath.Angle(new Vector2(Vx, Vy)) * Mathf.Rad2Deg, 360.0f);
            var angleDifference = DMath.ShortestArc_d(originalAngle, newAngle);

            if (Mathf.Abs(angleDifference) < ForceJumpAngleDifference)
            {
                var targetAngle = originalAngle + ForceJumpAngleDifference * Mathf.Sign(angleDifference);
                var magnitude = new Vector2(Vx, Vy).magnitude;

                var targetAngleRadians = targetAngle * Mathf.Deg2Rad;
                var newVelocity = new Vector2(magnitude * Mathf.Cos(targetAngleRadians),
                    magnitude * Mathf.Sin(targetAngleRadians));

                Vx = newVelocity.x;
                Vy = newVelocity.y;
            }

            Detach();
        }

        public void ReleaseJump()
        {
            if (RelativeVelocity.y > JumpSpeed)
                RelativeVelocity += Vector2.down*(JumpSpeed - ReleaseJumpSpeed);
            else if (RelativeVelocity.y > ReleaseJumpSpeed)
                RelativeVelocity += Vector2.down*(RelativeVelocity.y - ReleaseJumpSpeed);
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
        #endregion
        #region Surface Acquisition Functions
        /// <summary>
        /// Detach the player from whatever surface it is on. If the player is not grounded this has no effect
        /// other than setting lockUponLanding.
        /// </summary>
        /// <param name="lockUponLanding">If set to <c>true</c> lock horizontal control when the player attaches.</param>
        public void Detach(bool lockUponLanding = false)
        {
            Vg = 0.0f;
            LastSurfaceAngle = SurfaceAngle = RelativeAngle(0.0f);
            Grounded = false;
            JustDetached = true;
            Footing = Footing.None;
            LockUponLanding = lockUponLanding;
            SensorsRotation = GravityDirection + 90.0f;
            SetSurface(null);
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
            SurfaceAngle = LastSurfaceAngle = angleDegrees;
            SensorsRotation = SurfaceAngle;
            Grounded = _justLanded = true;

            if (LockUponLanding)
            {
                LockUponLanding = false;
                LockHorizontal();
            }
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

            if (result != null)
            {
                Attach(result.Value, sAngler);
                return true;
            }
            else
            {
                return false;
            }
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

        private void NotifyCollision(TerrainCastHit collision)
        {
            if (!collision) return;

            var trigger = collision.Hit.transform.GetComponent<PlatformTrigger>();
            if (trigger == null)
                return;

            trigger.NotifyCollision(this, collision);
        }

        /// <summary>
        /// Sets the controller's primary and secondary surfaces and triggers their platform events, if any.
        /// </summary>
        /// <param name="primarySurfaceHit">The new primary surface.</param>
        /// <param name="secondarySurfaceHit">The new secondary surface.</param>
        public void SetSurface(TerrainCastHit primarySurfaceHit, TerrainCastHit secondarySurfaceHit = null)
        {
            PrimarySurfaceHit = primarySurfaceHit;
            PrimarySurface = PrimarySurfaceHit ? PrimarySurfaceHit.Hit.transform : null;

            SecondarySurfaceHit = secondarySurfaceHit;
            SecondarySurface = SecondarySurfaceHit ? SecondarySurfaceHit.Hit.transform : null;

            if (PrimarySurfaceHit)
            {
                PlatformTrigger primaryTrigger;
                if (primarySurfaceHit && (primaryTrigger = primarySurfaceHit.Hit.transform.GetComponent<PlatformTrigger>()) != null)
                    primaryTrigger.NotifySurfaceCollision(this, primarySurfaceHit);
            }

            if(SecondarySurfaceHit)
            {
                PlatformTrigger secondaryTrigger;
                if (secondarySurfaceHit && (secondaryTrigger = secondarySurfaceHit.Hit.transform.GetComponent<PlatformTrigger>()) != null)
                    secondaryTrigger.NotifySurfaceCollision(this, secondarySurfaceHit);
            }
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