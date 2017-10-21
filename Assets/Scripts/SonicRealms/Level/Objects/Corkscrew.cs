using System.Collections.Generic;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Moves;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace SonicRealms.Level.Objects
{
    [SelectionBase]
    public class Corkscrew : MonoBehaviour, ISurfaceSolver
    {
        #region Public Variables
        #region Trigger Properties
        /// <summary>
        /// Whether to auto-position the triggers below based on the dimensions of the corkscrew.
        /// </summary>
        public bool AutoPositionTriggers { get { return _autoPositionTriggers; } set { _autoPositionTriggers = value; } }

        /// <summary>
        /// Where the player would enter the corkscrew if it were moving forward.
        /// </summary>
        public AreaTrigger Entrance { get { return _entrance; } }

        /// <summary>
        /// Where the player would exit the corkscrew if it were moving forward.
        /// </summary>
        public AreaTrigger Exit { get { return _exit; } }
        #endregion

        #region Waveform Properties
        /// <summary>
        /// Length of the corkscrew in units.
        /// </summary>
        public float Length { get { return _length; } set { _length = value; } }

        /// <summary>
        /// Length of one segment of the corkscrew in units.
        /// </summary>
        public float Period { get { return _period; } set { _period = value; } }

        /// <summary>
        /// Horizontal offset of the wave as a 0-1 value normalized from wavelength.
        /// </summary>
        public float PhaseShift { get { return _phaseShift; } set { _phaseShift = value; } }

        /// <summary>
        /// Height of the corkscrew measured from its midpoint in units.
        /// </summary>
        public float Amplitude { get { return _amplitude; } set { _amplitude = value; } }
        #endregion

        #region Surface Properties
        /// <summary>
        /// Minimum ground speed for the player to stay on the corkscrew.
        /// </summary>
        public float MinSpeed { get { return _minSpeed; } set { _minSpeed = value; } }

        /// <summary>
        /// If true, a player on the corkscrew will always have the same surface angle like in Sonic 2.
        /// </summary>
        public bool UseConstantSurfaceAngle
        {
            get { return _useConstantSurfaceAngle; }
            set { _useConstantSurfaceAngle = value; }
        }

        /// <summary>
        /// Surface angle of the entire corkscrew in degrees. This has no effect if 
        /// <see cref="UseConstantSurfaceAngle"/> is false.
        /// </summary>
        public float SurfaceAngle { get { return _surfaceAngle; } set { _surfaceAngle = value; } }

        /// <summary>
        /// Whether the surface angle of the corkscrew is relative to the object's rotation. This has no effect if
        /// <see cref="UseConstantSurfaceAngle"/> is false.
        /// </summary>
        public bool RelativeToRotation { get { return _relativeToRotation; } set { _relativeToRotation = value; } }
        #endregion

        #region Player Animation Properties
        /// <summary>
        /// Hash value of an Animator bool on the player to set to whether it is on the corkscrew.
        /// </summary>
        public int OnCorkscrewBoolHash { get { return _onCorkscrewBoolHash; } }

        /// <summary>
        /// Hash value of an Animator float on the player to set to a 0-1 value showing player progress through the corkscrew.
        /// </summary>
        public int CorkscrewProgressFloatHash { get { return _corkscrewProgressFloatHash; } }

        /// <summary>
        /// Hash value of an Animator float on the player to set to a 0-1 value showing player progress on a single period of
        /// the corkscrew. If the corkscrew's <see cref="Length"/> equal to its <see cref="Period"/>, this is the same as
        /// <see cref="CorkscrewProgressFloatHash"/>.
        /// </summary>
        public int PeriodProgressFloatHash { get { return _periodProgressFloatHash; } }

        /// <summary>
        /// The start position of the corkscrew.
        /// </summary>
        public Vector2 StartPosition { get { return GetPosition(0); } }

        /// <summary>
        /// The end position of the corkscrew.
        /// </summary>
        public Vector2 EndPosition { get { return GetPosition(1); } }
        #endregion
        #endregion

        #region Inspector Fields

        #region Triggers

        [SerializeField]
        [Foldout("Triggers")]
        [Tooltip("Whether to auto-position the triggers below based on the dimensions of the corkscrew.")]
        private bool _autoPositionTriggers;

        [SerializeField]
        [Foldout("Triggers")]
        [Tooltip("Where the player would enter the corkscrew if it were moving forward.")]
        private AreaTrigger _entrance;

        [SerializeField]
        [Foldout("Triggers")]
        [Tooltip("Where the player would exit the corkscrew if it were moving forward.")]
        private AreaTrigger _exit;

        #endregion

        #region Waveform

        [SerializeField]
        [Range(0.01f, 20)]
        [Foldout("Waveform")]
        [Tooltip("Length of the corkscrew in units.")]
        private float _length;

        [SerializeField]
        [Range(0.5f, 20)]
        [Foldout("Waveform")]
        [Tooltip("Length of one segment of the corkscrew in units.")]
        private float _period;

        [SerializeField]
        [Range(0, 1)]
        [Foldout("Waveform")]
        [Tooltip("Horizontal offset of the wave as a 0-1 value normalized from wavelength.")]
        private float _phaseShift;

        [SerializeField]
        [Range(0, 20)]
        [Foldout("Waveform")]
        [Tooltip("Height of the corkscrew measured from its midpoint in units.")]
        private float _amplitude;

        #endregion

        #region Surface

        [SerializeField]
        [Range(0.1f, 20)]
        [Foldout("Surface")]
        [Tooltip("Minimum ground speed for the player to stay on the corkscrew.")]
        private float _minSpeed;

        [SerializeField]
        [Foldout("Surface")]
        [Tooltip("If checked, a player on the corkscrew will always have the same surface angle like in Sonic 2.")]
        private bool _useConstantSurfaceAngle;

        [SerializeField]
        [Range(-180, 180)]
        [Foldout("Surface")]
        [Tooltip("Surface angle of the entire corkscrew in degrees.")]
        private float _surfaceAngle;

        [SerializeField]
        [Foldout("Surface")]
        [Tooltip("Whether the surface angle of the corkscrew is relative to the object's rotation.")]
        private bool _relativeToRotation;

        #endregion

        #region Animation

        [SerializeField]
        [Foldout("Animation")]
        [Tooltip("Name of an Animator bool on the player to set to whether it is on the corkscrew.")]
        private string _onCorkscrewBool;
        private int _onCorkscrewBoolHash;

        [SerializeField]
        [Foldout("Animation")]
        [Tooltip("Name of an Animator float on the player to set to a 0-1 value showing player progress " +
                 "through the corkscrew.")]
        private string _corkscrewProgressFloat;
        private int _corkscrewProgressFloatHash;

        [SerializeField]
        [Foldout("Animation")]
        [Tooltip("Name of an Animator float on the player to set to a 0-1 value showing player progress " +
                 "on a single period of the corkscrew. If the corkscrew is standard length, this is the " +
                 "same as Corkscrew Progress Float.")]
        private string _periodProgressFloat;
        private int _periodProgressFloatHash;

        #endregion

        #endregion

        // Keeps track of players on the corkscrew
        private Dictionary<HedgehogController, PassengerInfo> _passengers;

        #region Helper Functions
        /// <summary>
        /// If the given player is on the corkscrew, returns an object containing info about it. Otherwise, returns null.
        /// </summary>
        public PassengerInfo GetPassengerInfo(HedgehogController controller)
        {
            return _passengers.ContainsKey(controller) ? _passengers[controller] : null;
        }

        /// <summary>
        /// Returns where a player would be in the corkscrew with the given amount of progress.
        /// </summary>
        /// <param name="progress">Progress in the corkscrew as a 0-1 value where 0 is the start and 1 is the end.</param>
        public Vector2 GetPosition(float progress)
        {
            var x = transform.position.x + (progress - 0.5f) * Length;
            var y = transform.position.y +
                    Mathf.Sin(PhaseShift*Mathf.PI*2 + progress*(Length/Period)*Mathf.PI*2)*Amplitude;

            return SrMath.RotateBy(new Vector2(x, y), transform.eulerAngles.z*Mathf.Deg2Rad, transform.position);
        }

        /// <summary>
        /// Returns what the height of the player would be if it had the given amount of progress through the corkscrew,
        /// relative to the corkscrew's midpoint. The value is positive if above the midpoint, otherwise below.
        /// </summary>
        public float GetLocalHeight(float progress)
        {
            return SrMath.HeightDifference(GetPosition(progress), transform.position,
                (transform.eulerAngles.z + 90)*Mathf.Deg2Rad);
        }

        /// <summary>
        /// Returns what the surface angle would be if it had the given amount of progress through the corkscrew.
        /// </summary>
        /// <returns>The surface angle in degrees.</returns>
        public float GetSurfaceAngle(float progress)
        {
            if (UseConstantSurfaceAngle)
            {
                return RelativeToRotation ? transform.eulerAngles.z + SurfaceAngle : SurfaceAngle;
            }
            else
            {
                // I don't wanna derive so we'll use good ol' limits
                var slope = (GetPosition(progress + 0.001f) - GetPosition(progress))*1000;
                return Mathf.Atan2(slope.y, slope.x)*Mathf.Rad2Deg;
            }
        }
        #endregion

        #region Lifecycle Functions
        protected virtual void Reset()
        {
            _autoPositionTriggers = true;

            _length = 3.84f;
            _period = 3.84f;
            _amplitude = 0.29f;
            _phaseShift = 0.75f;

            _minSpeed = 3;

            _useConstantSurfaceAngle = true;
            _relativeToRotation = true;
        }

        protected virtual void Awake()
        {
            _passengers = new Dictionary<HedgehogController, PassengerInfo>();

            _onCorkscrewBoolHash = Animator.StringToHash(_onCorkscrewBool);
            _corkscrewProgressFloatHash = Animator.StringToHash(_corkscrewProgressFloat);
            _periodProgressFloatHash = Animator.StringToHash(_periodProgressFloat);
        }

        protected virtual void Start()
        {
            _entrance.OnAreaEnter.AddListener(OnEnterStart);
            _entrance.OnAreaStay.AddListener(OnEnterStart);

            _exit.OnAreaEnter.AddListener(OnEnterEnd);
            _exit.OnAreaStay.AddListener(OnEnterEnd);
        }

        protected virtual void FixedUpdate()
        {
            foreach (var passenger in _passengers)
            {
                RealmsAnimatorUtility.SilentSet(passenger.Key, passenger.Value, AnimatePassengerStay);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_entrance)
            {
                _entrance.OnAreaEnter.RemoveListener(OnEnterStart);
                _entrance.OnAreaStay.RemoveListener(OnEnterStart);
            }

            if (_exit)
            {
                _exit.OnAreaEnter.RemoveListener(OnEnterEnd);
                _exit.OnAreaStay.RemoveListener(OnEnterEnd);
            }
        }

        protected virtual void OnValidate()
        {
            if (AutoPositionTriggers)
            {
                if (Entrance)
                    Entrance.transform.position = StartPosition;

                if (Exit)
                    Exit.transform.position = EndPosition;
            }
        }
        #endregion

        public void Solve(ISurfaceSolverMediator mediator)
        {
            var info = GetPassengerInfo(mediator.Controller);

            if (info == null)
            {
                mediator.Detach();
                return;
            }
            
            if (info.EnteredAtStart)
            {
                info.Progress += mediator.ProposedTranslation.magnitude/Length;

                if (info.Progress >= 1 || info.Controller.GroundVelocity < MinSpeed)
                {
                    mediator.Detach();
                    return;
                }
            }
            else
            {
                info.Progress -= mediator.ProposedTranslation.magnitude/Length;

                if (info.Progress <= 0 || info.Controller.GroundVelocity > -MinSpeed)
                {
                    mediator.Detach();
                    return;
                }
            }

            var truePosition = GetPosition(info.Progress);
            var fixedPosition = GetFixedPosition(info);

            info.Controller.Sensors.TrueCenterOffset = truePosition - fixedPosition;

            var surfaceAngle = GetSurfaceAngle(info.Progress);

            mediator.SetAll(fixedPosition, transform, surfaceAngle);
        }

        public void OnSolverAdded(HedgehogController controller)
        {

        }

        public void OnSolverRemoved(HedgehogController controller)
        {
            RemovePassenger(controller);
        }

        private void AnimatePassengerEnter(Animator animator, PassengerInfo info)
        {
            if (OnCorkscrewBoolHash != 0)
                animator.SetBool(OnCorkscrewBoolHash, true);

            AnimatePassengerStay(animator, info);
        }

        private void AnimatePassengerStay(Animator animator, PassengerInfo info)
        {
            if (CorkscrewProgressFloatHash != 0)
                animator.SetFloat(CorkscrewProgressFloatHash, info.Progress);

            if (PeriodProgressFloatHash != 0)
                animator.SetFloat(PeriodProgressFloatHash, info.PeriodProgress);
        }

        private void AnimatePassengerExit(Animator animator, PassengerInfo info)
        {
            if (OnCorkscrewBoolHash != 0)
                animator.SetBool(OnCorkscrewBoolHash, false);

            AnimatePassengerStay(animator, info);
        }

        private void AddPassenger(HedgehogController controller, bool enteredAtStart)
        {
            if (GetPassengerInfo(controller) != null)
                return;

            controller.UseCustomSurface(this);

            var info = new PassengerInfo(controller, this, EstimateProgress(controller.transform.position),
                enteredAtStart);

            var roll = controller.GetMove<Roll>();
            if (roll != null)
            {
                roll.OnActive.AddListener(info.UpdatePositionAction);
                roll.OnEnd.AddListener(info.UpdatePositionAction);
            }

            _passengers[controller] = info;

            RealmsAnimatorUtility.SilentSet(controller, info, AnimatePassengerEnter);
        }
        
        private bool RemovePassenger(HedgehogController controller)
        {
            var info = GetPassengerInfo(controller);

            if (info == null)
                return false;

            var roll = controller.GetMove<Roll>();
            if (roll != null)
            {
                roll.OnActive.RemoveListener(info.UpdatePositionAction);
                roll.OnEnd.RemoveListener(info.UpdatePositionAction);
            }

            _passengers.Remove(controller);

            RealmsAnimatorUtility.SilentSet(controller, info, AnimatePassengerExit);

            return true;
        }

        private float EstimateProgress(Vector2 position)
        {
            var rotation = transform.eulerAngles.z*Mathf.Deg2Rad;

            var projected = SrMath.Projection(position, StartPosition, rotation);
            var difference = SrMath.HeightDifference(projected, StartPosition, rotation);

            return difference/Length;
        }

        private void OnEnterStart(AreaCollision collision)
        {
            var controller = collision.Controller;

            if (GetPassengerInfo(controller) != null)
                return;

            if (CanEnterStart(controller))
            {
                AddPassenger(controller, true);
            }
        }

        private void OnEnterEnd(AreaCollision collision)
        {
            var controller = collision.Controller;

            if (GetPassengerInfo(controller) != null)
                return;

            if (CanEnterEnd(controller))
            {
                AddPassenger(controller, false);
            }
        }

        private bool CanEnterStart(HedgehogController controller)
        {
            return controller.Grounded && controller.GroundVelocity >= MinSpeed;
        }

        private bool CanEnterEnd(HedgehogController controller)
        {
            return controller.Grounded && controller.GroundVelocity <= -MinSpeed;
        }

        private Vector2 GetFixedPosition(PassengerInfo info)
        {
            var roll = info.Controller.GetMove<Roll>();

            if (roll != null && roll.Active)
            {
                var normalizedHeight = Amplitude == 0 ? 0 : GetLocalHeight(info.Progress) / Amplitude;

                var dir = SrMath.UnitVector((info.Controller.SurfaceAngle - 90)*Mathf.Deg2Rad);

                return GetPosition(info.Progress)
                       + dir*roll.HeightChange*normalizedHeight*0.5f;
            }
            else
            {
                return GetPosition(info.Progress);
            }
        }

        private void Reposition(PassengerInfo passenger)
        {
            var truePosition = GetPosition(passenger.Progress);
            var fixedPosition = GetFixedPosition(passenger);

            passenger.Controller.Sensors.TrueCenterOffset = truePosition - fixedPosition;
            passenger.Controller.transform.position = fixedPosition;
        }

        public class PassengerInfo
        {
            public readonly HedgehogController Controller;
            public readonly Corkscrew Corkscrew;

            public float Progress;

            public readonly bool EnteredAtStart;

            public UnityAction UpdatePositionAction; 

            public float PeriodProgress { get { return SrMath.Modp(Progress*Corkscrew.Length/Corkscrew.Period, 1); } }

            public PassengerInfo(HedgehogController controller, Corkscrew corkscrew, float progress, bool enteredAtStart)
            {
                Controller = controller;
                Corkscrew = corkscrew;
                Progress = progress;
                EnteredAtStart = enteredAtStart;

                UpdatePositionAction = () => corkscrew.Reposition(this);
            }
        }
    }
}
