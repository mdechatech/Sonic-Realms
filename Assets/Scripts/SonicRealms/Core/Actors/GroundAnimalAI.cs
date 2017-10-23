using SonicRealms.Core.Internal;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Actors
{
    /// <summary>
    /// For animals broken out of badniks. Simply makes them jump when they hit the ground.
    /// </summary>
    public class GroundAnimalAI : MonoBehaviour
    {
        public HedgehogController Controller;
        protected bool HitGround;

        /// <summary>
        /// Whether the controller is facing forward.
        /// </summary>
        [SrFoldout("Physics")]
        [Tooltip("Whether the controller is facing forward.")]
        public bool FacingRight;

        /// <summary>
        /// Initial speed, in units per second. Usually straight vertical.
        /// </summary>
        [SrFoldout("Physics")]
        [Tooltip("Initial speed, in units per second. Usually straight vertical.")]
        public Vector2 InitialSpeed;

        /// <summary>
        /// Run speed, in units per second.
        /// </summary>
        [SrFoldout("Physics")]
        [Tooltip("Run speed, in units per second.")]
        public float RunSpeed;

        /// <summary>
        /// Jump speed, in units per second.
        /// </summary>
        [SrFoldout("Physics")]
        [Tooltip("Jump speed, in units per second.")]
        public float JumpSpeed;

        [SrFoldout("Animation")]
        public Animator Animator;

        /// <summary>
        /// Name of an Animator trigger set when the controller hits the ground for the first time.
        /// </summary>
        [SrFoldout("Animation")]
        [Tooltip("Name of an Animator trigger set when the controller hits the ground for the first time.")]
        public string HitGroundTrigger;
        protected int HitGroundTriggerHash;

        public void Reset()
        {
            Controller = GetComponent<HedgehogController>();
            Animator = Controller ? Controller.Animator : GetComponent<Animator>();

            FacingRight = Random.value < 0.5f;
            RunSpeed = 3.6f;
            JumpSpeed = 2.1f;

            HitGroundTrigger = "";
        }

        public void Awake()
        {
            HitGround = false;
        }

        public void Start()
        {
            Controller.OnCollide.AddListener(OnCollide);
            Controller.RelativeVelocity = InitialSpeed;
            Controller.IsFacingForward = FacingRight;

            if (Animator != null && !string.IsNullOrEmpty(HitGroundTrigger))
                HitGroundTriggerHash = Animator.StringToHash(HitGroundTrigger);
        }

        public void OnCollide(PlatformCollision.Contact contact)
        {
            if (contact.HitData.Side != ControllerSide.Bottom) return;

            if (!HitGround)
            {
                HitGround = true;
                if (HitGroundTriggerHash != 0) Animator.SetTrigger(HitGroundTriggerHash);
            }

            // When we hit the ground, ignore it and jump right back up
            Controller.IsFacingForward = FacingRight;
            Controller.Detach();
            Controller.RelativeVelocity = new Vector2(RunSpeed * (FacingRight ? 1f : -1f), JumpSpeed);
        }
    }
}
