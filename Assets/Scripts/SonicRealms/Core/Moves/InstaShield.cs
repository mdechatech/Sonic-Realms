using System.IO;
using SonicRealms.Core.Actors;
using UnityEngine;

namespace SonicRealms.Core.Moves
{
    /// <summary>
    /// The insta-shield move from Sonic 3 & Knuckles. Widens the player hitbox and gives
    /// invincibility for a short time.
    /// </summary>
    public class InstaShield : DoubleJump
    {
        /// <summary>
        /// The controller's health system.
        /// </summary>
        [Tooltip("The controller's health system.")]
        public HedgehogHealth Health;

        /// <summary>
        /// The amount by which the controller's hitbox changes, in units.
        /// </summary>
        [Tooltip("The amount by which the controller's hitbox changes, in units.")]
        public Vector2 SizeChange;

        /// <summary>
        /// The collider whose size will be changed while the move is active.
        /// </summary>
        [Tooltip("The collider whose size will be changed while the move is active.")]
        public BoxCollider2D Target;

        /// <summary>
        /// The time during which the controller is invincible, in seconds.
        /// </summary>
        [Tooltip("The time during which the controller is invincible, in seconds.")]
        public float InvincibilityTime;

        /// <summary>
        /// Time left until the move's invincibility ends.
        /// </summary>
        [HideInInspector]
        public float InvincibilityTimer;

        /// <summary>
        /// Whether the controller has a shield. The insta-shield is disabled while it does.
        /// </summary>
        [HideInInspector]
        public bool HasShield;

        public override void Reset()
        {
            base.Reset();
            Health = GetComponentInParent<HedgehogController>()
                ? GetComponentInParent<HedgehogController>().GetComponent<HedgehogHealth>()
                : null;
            SizeChange = new Vector2(0.24f, 0.24f);
            Target = GetComponentInParent<HedgehogController>()
                ? GetComponentInParent<HedgehogController>().GetComponentInChildren<BoxCollider2D>()
                : null;
            InvincibilityTime = 0.1f;
        }

        public override void Awake()
        {
            base.Awake();
            InvincibilityTimer = 0.0f;
            HasShield = false;
        }

        public override void Start()
        {
            base.Start();
            Health = Health ? Health : Controller.GetComponent<HedgehogHealth>();
            Target.enabled = false;
        }

        public override void OnManagerAdd()
        {
            base.OnManagerAdd();
            
            // Listening for when the controller gets a shield, so we can disable ourselves
            PowerupManager.OnAdd.AddListener(OnManager);
            PowerupManager.OnRemove.AddListener(OnManager);
        }

        public override void OnManagerRemove()
        {
            base.OnManagerRemove();
            PowerupManager.OnAdd.RemoveListener(OnManager);
            PowerupManager.OnRemove.RemoveListener(OnManager);
        }

        protected void OnManager(Powerup powerup)
        {
            HasShield = Controller.HasPowerup<Shield>();
        }

        public override bool Available
        {
            get { return !HasShield && base.Available; }
        }

        public override void OnActiveEnter()
        {
            base.OnActiveEnter();
            InvincibilityTimer = InvincibilityTime;
            Target.enabled = true;
            Target.size += SizeChange;
            Health.Invincible = true;
        }

        public override void OnActiveUpdate()
        {
            base.OnActiveUpdate();

            if ((InvincibilityTimer -= Time.deltaTime) < 0.0f)
            {
                InvincibilityTimer = 0.0f;
                End();
            }
        }

        public override void OnActiveExit()
        {
            base.OnActiveExit();
            
            InvincibilityTimer = 0.0f;
            Target.enabled = false;
            Target.size -= SizeChange;

            if(!Controller.HasPowerup<Invincibility>())
                Health.Invincible = false;
        }
    }
}
