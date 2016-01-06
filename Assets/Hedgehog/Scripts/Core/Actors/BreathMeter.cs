using Hedgehog.Core.Triggers;
using Hedgehog.Level.Areas;
using UnityEngine;

namespace Hedgehog.Core.Actors
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
        public HedgehogHealth Health;

        /// <summary>
        /// How long the controller can stay underwater, in seconds.
        /// </summary>
        [Tooltip("How long the controller can stay underwater, in seconds.")]
        public float TotalAir;

        /// <summary>
        /// How quickly the controller gets air back, in seconds per second (lol).
        /// </summary>
        [Tooltip("How quickly the controller gets air back, in seconds per second (lol).")]
        public float ReplenishRate;

        /// <summary>
        /// How much air the controller has left, in seconds.
        /// </summary>
        [Tooltip("How much air the controller has left, in seconds.")]
        public float RemainingAir;

        /// <summary>
        /// Whether the controller is underwater.
        /// </summary>
        [Tooltip("Whether the controller is underwater.")]
        public bool Underwater;

        /// <summary>
        /// Whether the controller has some source of air. This can be set by outside sources, such as
        /// the Bubble Shield.
        /// </summary>
        [Tooltip("Whether the controller has some source of air. This can be set by outside sources, such " +
                 "as the Bubble Shield.")]
        public bool HasAir;

        /// <summary>
        /// The target animator.
        /// </summary>
        [Header("Animation")]
        [Tooltip("The target animator.")]
        public Animator Animator;

        /// <summary>
        /// Name of an Animator bool set to whether the controller is underwater.
        /// </summary>
        [Tooltip("Name of an Animator bool set to whether the controller is underwater.")]
        public string UnderwaterBool;
        protected int UnderwaterBoolHash;

        /// <summary>
        /// Name of an Animator float set to remaining air, in seconds.
        /// </summary>
        [Tooltip("Name of an Animator float set to remaining air, in seconds.")]
        public string AirFloat;
        protected int AirFloatHash;

        /// <summary>
        /// Name of an Animator float set to remaining air as a fraction of total air.
        /// </summary>
        [Tooltip("Name of an Animator float set to remaining air as a fraction of total air.")]
        public string AirPercentFloat;
        protected int AirPercentFloatHash;

        public void Reset()
        {
            TotalAir = 30.0f;
            ReplenishRate = 9001.0f;
            Controller = GetComponent<HedgehogController>();
            Health = Controller ? GetComponent<HedgehogHealth>() : null;
            Animator = Controller ? Controller.Animator : null;
        }

        public void Awake()
        {
            RemainingAir = 30.0f;
            Underwater = false;
            HasAir = false;
        }

        public void Start()
        {
            Controller = Controller ? Controller : GetComponent<HedgehogController>();
            Health = Health ? Health : Controller.GetComponent<HedgehogHealth>();
            Animator = Animator ? Animator : Controller.Animator;

            Controller.OnReactiveEnter.AddListener(OnReactiveEnter);
            Controller.OnReactiveExit.AddListener(OnReactiveExit);

            if (Animator == null) return;
            UnderwaterBoolHash = Animator.StringToHash(UnderwaterBool);
            AirFloatHash = Animator.StringToHash(AirFloat);
            AirPercentFloatHash = Animator.StringToHash(AirPercentFloat);
        }

        public void Update()
        {
            if (Animator != null)
            {
                if (AirFloatHash != 0) Animator.SetFloat(AirFloatHash, RemainingAir);
                if (AirPercentFloatHash != 0) Animator.SetFloat(AirPercentFloatHash, RemainingAir/TotalAir);
            }
            
            if (!Underwater || HasAir)
            {
                RemainingAir += ReplenishRate*Time.deltaTime;
                if (RemainingAir > TotalAir) RemainingAir = TotalAir;
                return;
            }

            RemainingAir -= Time.deltaTime;
            if (RemainingAir < 0.0f)
            {
                RemainingAir = 0.0f;
                Health.Kill();
            }
        }

        protected void OnReactiveEnter(BaseReactive reactive)
        {
            Underwater = Controller.Inside<Water>();
            if(Animator != null && UnderwaterBoolHash != 0)
                Animator.SetBool(UnderwaterBoolHash, Underwater);
        }

        protected void OnReactiveExit(BaseReactive reactive)
        {
            Underwater = Controller.Inside<Water>();
            if (Animator != null && UnderwaterBoolHash != 0)
                Animator.SetBool(UnderwaterBoolHash, Underwater);
        }
    }
}
