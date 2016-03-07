using System.Collections.Generic;
using System.Linq;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Actors
{
    /// <summary>
    /// Controls the buzz bomber.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class BuzzBomberAI : MonoBehaviour
    {
        #region Movement
        /// <summary>
        /// Whether the buzz bomber is facing right or left.
        /// </summary>
        [Foldout("Movement")]
        [Tooltip("Whether the buzz bomber is facing right or left.")]
        public bool FacingRight;

        /// <summary>
        /// Movement speed, in units per second.
        /// </summary>
        [Foldout("Movement")]
        [Tooltip("Movement speed, in units per second.")]
        public Vector2 MoveSpeed;

        /// <summary>
        /// Movement speed while shooting, in units per second.
        /// </summary>
        [Foldout("Movement")]
        [Tooltip("Movement speed while shooting, in units per second.")]
        public Vector2 ShootMoveSpeed;

        /// <summary>
        /// Movement speed while turning, in units per second.
        /// </summary>
        [Foldout("Movement")]
        [Tooltip("Movement speed while turning, in units per second.")]
        public Vector2 TurnMoveSpeed;

        /// <summary>
        /// Time it takes to turn around, in seconds.
        /// </summary>
        [Foldout("Movement")]
        [Tooltip("Time it takes to turn around, in seconds.")]
        public float TurnTime;
        #endregion
        #region Attack
        /// <summary>
        /// The buzz bomber shoots when a player enters this area.
        /// </summary>
        [Foldout("Attack")]
        [Tooltip("The buzz bomber shoots when a player enters this area.")]
        public Collider2D PlayerSearchArea;

        /// <summary>
        /// When the field of view hits a collider with this tag, the buzz bomber will shoot.
        /// </summary>
        [Foldout("Attack"), Tag]
        [Tooltip("When the field of view hits a collider with this tag, the buzz bomber will shoot.")]
        public string PlayerTag;

        /// <summary>
        /// Time it takes to shoot, in seconds.
        /// </summary>
        [Foldout("Attack"), Space]
        [Tooltip("Time it takes to shoot, in seconds.")]
        public float ShotDuration;

        /// <summary>
        /// Time between shots, not including shot duration.
        /// </summary>
        [Foldout("Attack")]
        [Tooltip("Time between shots, not including shot duration.")]
        public float TimeBetweenShots;

        /// <summary>
        /// The projectile to shoot.
        /// </summary>
        [Foldout("Attack"), Space]
        [Tooltip("The projectile to shoot.")]
        public GameObject Projectile;

        /// <summary>
        /// Where the projectile is shot.
        /// </summary>
        [Foldout("Attack")]
        [Tooltip("Where the projectile is shot.")]
        public Transform ProjectilePosition;

        /// <summary>
        /// The speed the projectile is set to.
        /// </summary>
        [Foldout("Attack")]
        [Tooltip("The speed the projectile is set to.")]
        public Vector2 ProjectileSpeed;

        /// <summary>
        /// Whether to flip the projectile's speed based on the direction the buzz bomber is facing.
        /// </summary>
        [Foldout("Attack")]
        [Tooltip("Whether to flip the projectile's speed based on the direction the buzz bomber is firing.")]
        public bool FlipProjectileSpeed;
        #endregion
        #region Signals
        /// <summary>
        /// This collider is used for picking up signals.
        /// </summary>
        [Foldout("Signals")]
        [Tooltip("This collider is used for picking up signals.")]
        public Collider2D SignalSearchArea;

        /// <summary>
        /// When touching a collider with this tag, the buzz bomber will turn left.
        /// </summary>
        [Foldout("Signals"), Tag]
        [Tooltip("When touching a collider with this tag, the buzz bomber will turn left.")]
        public string TurnLeftTag;
        
        /// <summary>
        /// When touching a collider with this tag, the buzz bomber will turn right.
        /// </summary>
        [Foldout("Signals"), Tag]
        [Tooltip("When touching a collider with this tag, the buzz bomber will turn right.")]
        public string TurnRightTag;

        /// <summary>
        /// When touching a collider with this tag, the buzz bomber will shoot.
        /// </summary>
        [Foldout("Signals"), Tag]
        [Tooltip("When touching a collider with this tag, the buzz bomber will shoot.")]
        public string ShootTag;
        #endregion
        #region Animation
        [Foldout("Animation")]
        public Animator Animator;

        /// <summary>
        /// Name of an Animator bool set to whether the buzz bomber is shooting.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator bool set to whether the buzz bomber is shooting.")]
        public string ShootingBool;
        protected int ShootingBoolHash;

        /// <summary>
        /// Name of an Animator bool set to whther the buzz bomber is turning.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator bool set to whther the buzz bomber is turning.")]
        public string TurningBool;
        protected int TurningBoolHash;
        #endregion
        #region Debug
        /// <summary>
        /// Whether the buzz bomber is currently allowed to shoot.
        /// </summary>
        [Foldout("Debug")]
        [Tooltip("Whether the buzz bomber is currently allowed to shoot.")]
        public bool CanShoot;

        /// <summary>
        /// Whether the buzz bomber currently sees an enemy.
        /// </summary>
        [Foldout("Debug")]
        [Tooltip("Whether the buzz bomber currently sees an enemy.")]
        public bool EnemySighted;

        /// <summary>
        /// A list of all enemies currently sighted.
        /// </summary>
        public HashSet<Collider2D> EnemiesSighted;
            
        /// <summary>
        /// Counts down the time until the buzz bomber is ready to shoot again, in seconds.
        /// </summary>
        [Foldout("Debug")]
        [Tooltip("Counts down the time until the buzz bomber is ready to shoot again, in seconds.")]
        public float TimeBetweenShotsTimer;

        /// <summary>
        /// Counts down the time during which the buzz bomber is shooting. Once zero, the buzz bomber goes back
        /// to moving.
        /// </summary>
        [Foldout("Debug")]
        [Tooltip("Counts down the time during which the buzz bomber is shooting. Once zero, the buzz bomber goes back " +
                 "to moving.")]
        public float ShotTimer;

        /// <summary>
        /// Counts down the time during which the buzz bomber is turning. Once zero, the buzz bomber moves in
        /// the opposite direction.
        /// </summary>
        [Foldout("Debug")]
        [Tooltip("Counts down the time during which the buzz bomber is turning. Once zero, the buzz bomber moves in " +
                 "the opposite direction.")]
        public float TurnTimer;

        /// <summary>
        /// The buzz bomber's current state.
        /// </summary>
        [Foldout("Debug")]
        [Tooltip("The buzz bomber's current state.")]
        public State CurrentState;
        public enum State
        {
            Moving,
            Turning,
            Shooting,
        }
        #endregion
        protected Rigidbody2D Rigidbody2D;

        /// <summary>
        /// Turns to face left. No effect if already facing left.
        /// </summary>
        public void TurnLeft() { Turn(false); }

        /// <summary>
        /// Turns to face right. No effect if already facing right.
        /// </summary>
        public void TurnRight() { Turn(true); }
        
        /// <summary>
        /// Turns to the specified direction. No effect if already facing in that direction.
        /// </summary>
        /// <param name="right">The specified direction. True if right, false if left.</param>
        public virtual void Turn(bool right)
        {
            if (right == FacingRight) return;

            CurrentState = State.Turning;
            TurnTimer = TurnTime;
            Rigidbody2D.velocity = FacingRight ? TurnMoveSpeed : Vector2.Reflect(TurnMoveSpeed, Vector2.right);

            UpdateAnimator();
        }

        /// <summary>
        /// Moves left without having to turn. No effect if already moving left.
        /// </summary>
        public void MoveLeft() { Move(false); }

        /// <summary>
        /// Moves right without having to turn. No effect if already moving right.
        /// </summary>
        public void MoveRight() { Move(true); }

        /// <summary>
        /// Moves in the specified direction without having to turn. No effect if already moving in that direction.
        /// </summary>
        /// <param name="right">The specified direction. True if right, false if left.</param>
        public virtual void Move(bool right)
        {
            if (CurrentState == State.Moving && right == FacingRight) return;

            if (FacingRight != right) transform.localScale = Vector3.Reflect(transform.localScale, Vector3.right);
            FacingRight = right;
            Rigidbody2D.velocity = FacingRight
                ? MoveSpeed
                : Vector2.Reflect(MoveSpeed, Vector2.right);

            CurrentState = State.Moving;

            UpdateAnimator();
        }

        /// <summary>
        /// Triggers the shooting state. Doesn't actually shoot bullets! That is done by using the ShootProjectile method.
        /// It does, however, trigger animations, which may have ShootProjectile events.
        /// 
        /// No effect if the Time Between Shots Timer is still counting down.
        /// </summary>
        public void Shoot() { Shoot(false); }

        /// <summary>
        /// Triggers the shooting state. Doesn't actually shoot bullets! That is done by using the ShootProjectile method.
        /// It does, however, trigger animations, which may have ShootProjectile events.
        /// </summary>
        /// <param name="ignoreTimer">Whether to ignore the Time Between Shots Timer.</param>
        public virtual void Shoot(bool ignoreTimer)
        {
            if (CurrentState == State.Shooting || (!ignoreTimer && TimeBetweenShotsTimer > 0f)) return;

            CurrentState = State.Shooting;
            ShotTimer = ShotDuration;
            TimeBetweenShotsTimer = TimeBetweenShots;
            CanShoot = false;
            Rigidbody2D.velocity = FacingRight ? ShootMoveSpeed : Vector2.Reflect(ShootMoveSpeed, Vector2.right);

            UpdateAnimator();
        }

        /// <summary>
        /// Actually shoots the projectile.
        /// </summary>
        public void ShootProjectile()
        {
            var projectile = Instantiate(Projectile);
            projectile.transform.position = ProjectilePosition.position;

            var rigidbody2D = projectile.gameObject.GetComponent<Rigidbody2D>();
            if (rigidbody2D != null)
                rigidbody2D.velocity = (!FlipProjectileSpeed || FacingRight)
                    ? ProjectileSpeed
                    : Vector2.Reflect(ProjectileSpeed, Vector2.right);
        }

        /// <summary>
        /// Updates animator parameters.
        /// </summary>
        public void UpdateAnimator()
        {
            if (Animator == null) return;

            if (TurningBoolHash != 0) Animator.SetBool(TurningBoolHash, CurrentState == State.Turning);
            if (ShootingBoolHash != 0) Animator.SetBool(ShootingBoolHash, CurrentState == State.Shooting);
        }

        public void Reset()
        {
            MoveSpeed = new Vector2(2.4f, 0f);
            TurnTime = 0f;
            ShotDuration = 1.6f;
            TimeBetweenShots = 3f;

            PlayerSearchArea = GetComponentInChildren<Collider2D>();
            SignalSearchArea = GetComponentsInChildren<Collider2D>().FirstOrDefault(d => d != PlayerSearchArea);

            PlayerTag = Hitbox.MainHitboxTag;
            TurnLeftTag = "Buzz Bomber Turn Left";
            TurnRightTag = "Buzz Bomber Turn Right";
            ShootTag = "Buzz Bomber Shoot";

            Animator = GetComponent<Animator>();
            ShootingBool = "";
            TurningBool = "";
        }

        public void Awake()
        {
            CanShoot = true;
            EnemySighted = false;
            EnemiesSighted = new HashSet<Collider2D>();
            TimeBetweenShotsTimer = 0f;
            ShotTimer = 0f;
            TurnTimer = 0f;
            CurrentState = State.Moving;
        }

        public void Start()
        {
            Rigidbody2D = GetComponent<Rigidbody2D>();
            Rigidbody2D.velocity = MoveSpeed;

            if (PlayerSearchArea)
            {
                var fov = PlayerSearchArea.gameObject.AddComponent<TriggerCallback2D>();
                fov.TriggerEnter2D.AddListener(OnPlayerEnter);
                fov.TriggerExit2D.AddListener(OnPlayerExit);
            }

            if(SignalSearchArea)
                SignalSearchArea.gameObject.AddComponent<TriggerCallback2D>().TriggerEnter2D.AddListener(OnSignalEnter);

            if (Animator == null) return;
            ShootingBoolHash = string.IsNullOrEmpty(ShootingBool) ? 0 : Animator.StringToHash(ShootingBool);
            TurningBoolHash = string.IsNullOrEmpty(TurningBool) ? 0 : Animator.StringToHash(TurningBool);
        }

        public void Update()
        {
            if (!CanShoot)
            {
                TimeBetweenShotsTimer -= Time.deltaTime;
                if (TimeBetweenShotsTimer < 0f)
                {
                    TimeBetweenShotsTimer = 0f;
                    CanShoot = true;
                }
            }
            else
            {
                if(EnemySighted) Shoot(false);
            }

            if (CurrentState == State.Shooting)
            {
                ShotTimer -= Time.deltaTime;
                if (ShotTimer < 0f)
                {
                    ShotTimer = 0f;
                    Move(FacingRight);
                }
            }
            else if (CurrentState == State.Turning)
            {
                TurnTimer -= Time.deltaTime;
                if (TurnTimer < 0f)
                {
                    TurnTimer = 0f;
                    Move(!FacingRight);
                }
            }
        }

        public void OnPlayerEnter(Collider2D other)
        {
            if (!other.CompareTag(PlayerTag)) return;

            EnemySighted = true;
            EnemiesSighted.Add(other);
        }

        public void OnPlayerExit(Collider2D other)
        {
            if (!other.CompareTag(PlayerTag)) return;

            EnemiesSighted.Remove(other);
            EnemySighted = EnemiesSighted.Count > 0;
        }

        public void OnSignalEnter(Collider2D other)
        {
            if (other.CompareTag(TurnLeftTag)) Turn(false);
            else if (other.CompareTag(TurnRightTag)) Turn(true);
            else if (other.CompareTag(ShootTag)) Shoot(false);
        }
    }
}
