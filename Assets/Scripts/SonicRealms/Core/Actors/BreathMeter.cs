using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using SonicRealms.Level.Areas;
using UnityEngine;
using UnityEngine.Events;

namespace SonicRealms.Core.Actors
{
    /// <summary>
    /// Keeps track of air while underwater. Kills the controller if it runs out.
    /// </summary>
    public class BreathMeter : MonoBehaviour
    {
        /// <summary>
        /// The target controller.
        /// </summary>
        [Tooltip("The target controller.")]
        public HedgehogController Controller;

        /// <summary>
        /// The controller's health system.
        /// </summary>
        [Tooltip("The controller's health system")]
        public HealthSystem Health;

        /// <summary>
        /// How long the controller can stay underwater, in seconds.
        /// </summary>
        [Space]
        [Tooltip("How long the controller can stay underwater, in seconds.")]
        public float TotalAir;

        /// <summary>
        /// How quickly the controller gets air back, in seconds per second (lol).
        /// </summary>
        [Tooltip("How quickly the controller gets air back, in seconds per second (lol).")]
        public float ReplenishRate;

        [Space]
        [Tooltip("Gravity after drowning.")]
        public float DrownGravity;

        #region Animation
        /// <summary>
        /// The target animator for air and air percentage parameters.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("The target animator for air and air percentage parameters.")]
        public Animator MeterAnimator;

        /// <summary>
        /// Name of an Animator float set to remaining air, in seconds.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator float set to remaining air, in seconds.")]
        public string AirFloat;
        protected int AirFloatHash;

        /// <summary>
        /// Name of an Animator float set to remaining air as a fraction of total air.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator float set to remaining air as a fraction of total air.")]
        public string AirPercentFloat;
        protected int AirPercentFloatHash;

        /// <summary>
        /// The target animator for underwater and drowning parameters.
        /// </summary>
        [Space, Foldout("Animation")]
        [Tooltip("The target animator for underwater and drowning parameters.")]
        public Animator StatusAnimator;

        /// <summary>
        /// Name of an Animator bool set to whether the controller is underwater.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator bool set to whether the controller is underwater.")]
        public string UnderwaterBool;
        protected int UnderwaterBoolHash;

        /// <summary>
        /// Name of an Animator trigger set when the player drowns.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator trigger set when the player drowns.")]
        public string DrownTrigger;
        protected int DrownTriggerHash;
        #endregion
        #region Events
        /// <summary>
        /// Invoked when the player enters water.
        /// </summary>
        [Foldout("Events")]
        public UnityEvent OnEnterWater;

        /// <summary>
        /// Invoked when the player exits water.
        /// </summary>
        [Foldout("Events")]
        public UnityEvent OnExitWater;

        /// <summary>
        /// Invoked when the player finds an extra air source, by any means.
        /// </summary>
        [Foldout("Events")]
        public UnityEvent OnGotExtraAirSource;

        /// <summary>
        /// Invoked when the player loses its extra air source.
        /// </summary>
        [Foldout("Events")]
        public UnityEvent OnLostExtraAirSource;

        /// <summary>
        /// Invoked when the player gains the ability to breathe.
        /// </summary>
        [Foldout("Events")]
        public UnityEvent OnCanBreathe;

        /// <summary>
        /// Invoked when the player loses the ability to breathe.
        /// </summary>
        [Foldout("Events")]
        public UnityEvent OnCannotBreathe;

        /// <summary>
        /// Invoked when the player drowns.
        /// </summary>
        [Foldout("Events")]
        public UnityEvent OnDrown;
        #endregion

        /// <summary>
        /// How much air the controller has left, in seconds.
        /// </summary>
        [Foldout("Debug")]
        [Tooltip("How much air the controller has left, in seconds.")]
        public float RemainingAir;

        /// <summary>
        /// Whether the controller is underwater.
        /// </summary>
        [Foldout("Debug")]
        [Tooltip("Whether the controller is underwater.")]
        public bool Underwater;

        /// <summary>
        /// Whether the controller has some residual source of air. This can be set by outside sources, such as
        /// the Bubble Shield.
        /// </summary>
        [SerializeField, Foldout("Debug")]
        [Tooltip("Whether the controller has some residual source of air. This can be set by outside sources, such " +
                 "as the Bubble Shield.")]
        private bool _hasExtraAirSource;
        public bool HasExtraAirSource
        {
            get { return _hasExtraAirSource; }
            set
            {
                if (_hasExtraAirSource != value)
                {
                    if(_hasExtraAirSource && !value) OnLostExtraAirSource.Invoke();
                    else OnGotExtraAirSource.Invoke();

                    _hasExtraAirSource = value;
                    UpdateCanBreathe();
                }
            }
        }

        /// <summary>
        /// Whether the player can breathe, based on being underwater and having an extra air source.
        /// </summary>
        [SerializeField, Foldout("Debug")]
        [Tooltip("Whether the player can breathe, based on being underwater and having an extra air source.")]
        private bool _canBreathe;
        public bool CanBreathe
        {
            get { return _canBreathe; }
        }

        /// <summary>
        /// Whether the player has drowned.
        /// </summary>
        [Foldout("Debug")]
        [Tooltip("Whether the player has drowned.")]
        public bool Drowned;

        public virtual void Reset()
        {
            TotalAir = 30.0f;
            ReplenishRate = 9001.0f;

            Controller = GetComponent<HedgehogController>();
            Health = Controller ? GetComponent<HedgehogHealth>() : null;

            DrownGravity = 2.25f;
        }

        public virtual void Awake()
        {
            RemainingAir = 30.0f;
            Underwater = false;
            _hasExtraAirSource = false;
            _canBreathe = true;
            Drowned = false;

            OnEnterWater = OnEnterWater ?? new UnityEvent();
            OnExitWater = OnExitWater ?? new UnityEvent();
            OnGotExtraAirSource = OnGotExtraAirSource ?? new UnityEvent();
            OnLostExtraAirSource = OnLostExtraAirSource ?? new UnityEvent();
            OnCanBreathe = OnCanBreathe ?? new UnityEvent();
            OnCannotBreathe = OnCannotBreathe ?? new UnityEvent();
        }

        public virtual void Start()
        {
            Controller = Controller ? Controller : GetComponent<HedgehogController>();
            Health = Health ? Health : Controller.GetComponent<HedgehogHealth>();
            MeterAnimator = MeterAnimator ? MeterAnimator : Controller.Animator;

            Controller.OnAreaEnter.AddListener(OnAreaEnter);
            Controller.OnAreaExit.AddListener(OnAreaExit);

            UnderwaterBoolHash = Animator.StringToHash(UnderwaterBool);
            AirFloatHash = Animator.StringToHash(AirFloat);
            AirPercentFloatHash = Animator.StringToHash(AirPercentFloat);
            DrownTriggerHash = Animator.StringToHash(DrownTrigger);
        }

        public virtual void Update()
        {
            if (MeterAnimator != null)
            {
                if (AirFloatHash != 0) MeterAnimator.SetFloat(AirFloatHash, RemainingAir);
                if (AirPercentFloatHash != 0) MeterAnimator.SetFloat(AirPercentFloatHash, RemainingAir/TotalAir);
            }

            if (CanBreathe)
            {
                RemainingAir += ReplenishRate*Time.deltaTime;
                if (RemainingAir > TotalAir)
                    RemainingAir = TotalAir;

                return;
            }

            RemainingAir -= Time.deltaTime;
            if (RemainingAir < 0.0f)
            {
                RemainingAir = 0.0f;
                Drown();
            }
        }

        public virtual void UpdateCanBreathe()
        {
            var newCanBreathe = !Underwater || HasExtraAirSource;

            if(_canBreathe && !newCanBreathe) OnCannotBreathe.Invoke();
            else if(!_canBreathe && newCanBreathe) OnCanBreathe.Invoke();

            _canBreathe = !Underwater || HasExtraAirSource;
        }

        private void OnAreaEnter(ReactiveArea area)
        {
            if (!Underwater && Controller.Inside<Water>())
            {
                Underwater = true;
                OnEnterWater.Invoke();
                UpdateCanBreathe();
            }

            if(StatusAnimator != null && UnderwaterBoolHash != 0)
                StatusAnimator.SetBool(UnderwaterBoolHash, Underwater);
        }

        private void OnAreaExit(ReactiveArea area)
        {
            if (Underwater && !Controller.Inside<Water>())
            {
                Underwater = false;
                OnExitWater.Invoke();
                UpdateCanBreathe();
            }

            if (StatusAnimator != null && UnderwaterBoolHash != 0)
                StatusAnimator.SetBool(UnderwaterBoolHash, Underwater);
        }

        public virtual void Drown()
        {
            if (Drowned) return;

            Drowned = true;

            Controller.OnAreaEnter.RemoveListener(OnAreaEnter);
            Controller.OnAreaExit.RemoveListener(OnAreaExit);

            Health.Kill();
            Controller.Vy = 0f;

            OnDrown.Invoke();

            if (StatusAnimator != null && DrownTriggerHash != 0)
                StatusAnimator.SetTrigger(DrownTriggerHash);
        }
    }
}
