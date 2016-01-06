using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Core.Actors
{
    [RequireComponent(typeof(HedgehogController))]
    public class MotobugAI : MonoBehaviour
    {
        public HedgehogController Controller;

        /// <summary>
        /// Whether the controller is facing right or left.
        /// </summary>
        [Foldout("Movement")]
        [Tooltip("Whether the controller is facing right or left.")]
        public bool FacingRight;

        /// <summary>
        /// If checked, the controller will attempt to flip the sprite when turning.
        /// </summary>
        [Foldout("Movement")]
        [Tooltip("If checked, the controller will attempt to flip the sprite when turning.")]
        public bool AutoFlip;

        /// <summary>
        /// Walk speed, in units per second.
        /// </summary>
        [Foldout("Movement")]
        [Tooltip("Walk speed, in units per second.")]
        public float Speed;

        /// <summary>
        /// Time it takes to turn around after hitting a turn signal.
        /// </summary>
        [Foldout("Movement")]
        [Tooltip("Time it takes to turn around after hitting a turn signal.")]
        public float TurnTime;

        /// <summary>
        /// This collider will be used for checking against the turning signals.
        /// </summary>
        [Foldout("Signals")]
        [Tooltip("This collider will be used for checking against the turning signals.")]
        public Collider2D SignalSearchArea;

        /// <summary>
        /// When a collider with this tag is hit, the AI will move left.
        /// </summary>
        [Foldout("Signals"), Tag]
        [Tooltip("When a collider with this tag is hit, the AI will move left.")]
        public string TurnLeftTag;

        /// <summary>
        /// When a collider with this tag is hit, the AI will move right.
        /// </summary>
        [Foldout("Signals"), Tag]
        [Tooltip("When a collider with this tag is hit, the AI will move right.")]
        public string TurnRightTag;

        /// <summary>
        /// If turning, how much time until the turn completes.
        /// </summary>
        [HideInInspector]
        public float TurnTimer;

        /// <summary>
        /// Name of an Animator bool set to whether the motobug is facing right.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator bool set to whether the motobug is facing right.")]
        public string FacingRightBool;
        protected int FacingRightBoolHash;

        /// <summary>
        /// Name of an Animator bool set to whether the motobug is turning.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator bool set to whether the motobug is turning.")]
        public string TurningBool;
        protected int TurningBoolHash;

        public void Reset()
        {
            Controller = GetComponent<HedgehogController>();
            SignalSearchArea = GetComponentInChildren<Collider2D>();

            TurnLeftTag = "Motobug Turn Left";
            TurnRightTag = "Motobug Turn Right";

            FacingRight = true;
            AutoFlip = true;

            Speed = 1f;
            TurnTime = 1f;

            FacingRightBool = "";
            TurningBool = "";
        }

        public void Awake()
        {
            TurnTimer = 0f;
        }

        public void Start()
        {
            SignalSearchArea.gameObject.AddComponent<TriggerCallback2D>().TriggerEnter2D.AddListener(OnTriggerEnter2D);
            Controller.AutoFacingForward = AutoFlip;

            if (Controller.Animator == null) return;
            FacingRightBoolHash = string.IsNullOrEmpty(FacingRightBool) ? 0 : Animator.StringToHash(FacingRightBool);
            TurningBoolHash = string.IsNullOrEmpty(TurningBool) ? 0 : Animator.StringToHash(TurningBool);
        }

        public void FixedUpdate()
        {
            Controller.GroundVelocity = Speed*(FacingRight ? 1f : -1f);
            if (TurnTimer != 0f)
            {
                Controller.GroundVelocity = 0f;
                if ((TurnTimer -= Time.deltaTime) < 0f)
                {
                    TurnTimer = 0f;
                    FacingRight = !FacingRight;
                }
            }

            if (AutoFlip) Controller.FacingForward = FacingRight;

            if (Controller.Animator == null) return;
            if(FacingRightBoolHash != 0) Controller.Animator.SetBool(FacingRightBoolHash, FacingRight);
            if(TurningBoolHash != 0) Controller.Animator.SetBool(TurningBoolHash, TurnTimer != 0f);
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if ((other.CompareTag(TurnLeftTag) && FacingRight) || (other.CompareTag(TurnRightTag) && !FacingRight))
                TurnTimer = TurnTime;
        }
    }
}
