using System.Collections.Generic;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Moves;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Objects
{
    /// <summary>
    /// Makes the platform bounce players out in the direction of the collision's normal.
    /// </summary>
    public class BouncyPlatform : ReactivePlatform
    {
        #region Public Fields & Properties

        /// <summary>
        /// <para>
        /// The center of the range of normal angles for which the platform will bounce the player, in degrees.
        /// </para>
        /// <para>
        /// Doesn't account for object rotation if <see cref="RelativeToRotation"/> is true.
        /// </para>
        /// </summary>
        public float BouncyArcCenter
        {
            get { return _bouncyAngles.x; }
            set
            {
                value = DMath.PositiveAngle_d(value);
                _bouncyAngles = new Vector2(value - BouncyArcLength*0.5f, value + BouncyArcLength*0.5f);
            }
        }

        /// <summary>
        /// <para>
        /// The length of the range of normal angles for which the platform will bounce the player, in degrees.
        /// </para>
        /// <para>
        /// Doesn't account for object rotation if <see cref="RelativeToRotation"/> is true.
        /// </para>
        /// </summary>
        public float BouncyArcLength
        {
            get { return _bouncyAngles.y - _bouncyAngles.x; }
            set
            {
                value = Mathf.Clamp(value, 0, 360);
                _bouncyAngles = new Vector2(BouncyArcCenter - value*0.5f, BouncyArcCenter + value*0.5f);
            }
        }

        /// <summary>
        /// <para>
        /// The minimum normal angle for which the platform will bounce the player, in degrees clamped to 0-360
        /// traveling counter-clockwise to <see cref="BouncyAngleMax"/>.
        /// </para>
        /// <para>
        /// Doesn't account for object rotation if <see cref="RelativeToRotation"/> is true.
        /// </para>
        /// </summary>
        public float BouncyAngleMin
        {
            get { return _bouncyAngles.x; }
            set { _bouncyAngles = new Vector2(Mathf.Clamp(value, 0, BouncyAngleMax), BouncyAngleMax); }
        }

        /// <summary>
        /// <para>
        /// The maximum normal angle for which the platform will bounce the player, in degrees clamped to 0-360
        /// traveling clockwise to <see cref="BouncyAngleMax"/>.
        /// </para>
        /// <para>
        /// Doesn't account for object rotation if <see cref="RelativeToRotation"/> is true.
        /// </para>
        /// </summary>
        public float BouncyAngleMax
        {
            get { return _bouncyAngles.y; }
            set { _bouncyAngles = new Vector2(BouncyAngleMin, Mathf.Clamp(value, BouncyAngleMin, 360)); }
        }

        /// <summary>
        /// How the a player's bounce angle is determined.
        /// </summary>
        public BouncyPlatformBounceAngleBehavior BounceAngleBehavior
        {
            get { return _bounceAngleBehavior; }
            set { _bounceAngleBehavior = value; }
        }

        /// <summary>
        /// If true, allows modification of outbound bounce angles based on inbound bounce angles
        /// through <see cref="RemapBounceAnglesCurve"/>.
        /// </summary>
        public bool RemapBounceAngles { get { return _remapBounceAngles; } set { _remapBounceAngles = value; } }

        /// <summary>
        /// Maps the x-axis to inbound bounce angle (0-360 degree value) and the y-axis to the outbound
        /// bounce angle (0-360 degree value). <see cref="RemapBounceAngles"/> must be true for this 
        /// to apply.
        /// </summary>
        public AnimationCurve RemapBounceAnglesCurve
        {
            get { return _remapBounceAnglesCurve; }
            set { _remapBounceAnglesCurve = value; }
        }

        /// <summary>
        /// If true, allows modification of bounce speed based on inbound bounce angle (not affected 
        /// by <see cref="RemapBounceAngles"/>).
        /// </summary>
        public bool MapBounceAngleToVelocity
        {
            get { return _mapBounceAngleToVelocity; }
            set { _mapBounceAngleToVelocity = value; }
        }

        /// <summary>
        /// Maps the x-axis to inbound bounce angle (0-360 degree value) and the y-axis to velocity 
        /// multiplier (any value). <see cref="MapBounceAngleToVelocity"/> must be true for this to 
        /// apply.</summary>
        public AnimationCurve BounceAngleToVelocityCurve
        {
            get { return _bounceAngleToVelocityCurve; }
            set { _bounceAngleToVelocityCurve = value; }
        }

        /// <summary>
        /// Whether the bouncy angles change with the object's current rotation. If true, all angle values are corrected
        /// by adding <c>transform.eulerAngles.z</c>.
        /// </summary>
        public bool RelativeToRotation { get { return _relativeToRotation; } set { _relativeToRotation = value; } }

        /// <summary>
        /// Velocity at which the player bounces off the platform in units per second.
        /// </summary>
        public float Velocity { get { return _velocity; } set { _velocity = value; } }

        /// <summary>
        /// What the platform should do to the player's velocity when it bounces.
        /// </summary>
        public BouncyPlatformVelocityEffect VelocityEffect { get { return _velocityEffect; } set { _velocityEffect = value; } }

        /// <summary>
        /// How many times the same player can bounce off the platform each second.
        /// </summary>
        public float MaxBouncesPerPlayerPerSecond
        {
            get { return _maxBouncesPerPlayerPerSecond; }
            set { _maxBouncesPerPlayerPerSecond = value; }
        }

        /// <summary>
        /// If nonzero, what the player's horizontal control lock timer is set to in seconds when it bounces.
        /// </summary>
        public float ControlLockTime { get { return _controlLockTime; } set { _controlLockTime = value; } }

        /// <summary>
        /// Whether to detach the player from the ground when it bounces off the platform.
        /// </summary>
        public bool DetachPlayer { get { return _detachPlayer; } set { _detachPlayer = value; } }

        /// <summary>
        /// Whether to end the player's rolling when it bounces off the platform, like a vertical spring would.
        /// </summary>
        public bool EndRoll { get { return _endRoll; } set { _endRoll = value; } }

        public Animator Animator { get { return _animator; } set { _animator = value; } }

        /// <summary>
        /// Hash value of an Animator trigger to set when a player bounces off the platform.
        /// </summary>
        public int BounceTriggerHash { get { return _bounceTriggerHash; } set { _bounceTriggerHash = value; } }

        /// <summary>
        /// Hash value of an Animator trigger on the player to set when it bounces off the platform.
        /// </summary>
        public int PlayerBounceTriggerHash
        {
            get { return _playerBounceTriggerHash; }
            set { _playerBounceTriggerHash = value; }
        }

        #endregion

        #region Inspector & Private Fields
        [SerializeField]
        [Foldout("Bouncing")]
        [MinMaxSlider(0, 360)]
        [Tooltip("Range of normal angles for which the platform is bouncy.")]
        private Vector2 _bouncyAngles;

        [SerializeField]
        [Foldout("Bouncing")]
        [Tooltip("How the a player's bounce angle is determined.")]
        private BouncyPlatformBounceAngleBehavior _bounceAngleBehavior;

        [SerializeField]
        [Foldout("Bouncing")]
        [Tooltip("Whether the bouncy angles change with the object's current rotation.")]
        private bool _relativeToRotation;

        [Space]
        [SerializeField]
        [Foldout("Bouncing")]
        [Tooltip("Velocity at which the player bounces off the platform in units per second.")]
        private float _velocity;

        [SerializeField]
        [Foldout("Bouncing")]
        [Tooltip("What the platform should do to the player's velocity when it bounces.")]
        private BouncyPlatformVelocityEffect _velocityEffect;

        [SerializeField]
        [Foldout("Bouncing")]
        [Tooltip("How many times the same player can bounce off the platform each second.")]
        private float _maxBouncesPerPlayerPerSecond;

        [Space]
        [SerializeField]
        [Foldout("Bouncing")]
        [Tooltip("If checked, allows modification of outbound bounce angles based on inbound bounce angles.")]
        private bool _remapBounceAngles;

        [SerializeField]
        [Foldout("Bouncing")]
        [ContextMenuItem("Reset", "ResetRemapBounceAnglesCurve")]
        [Tooltip("Maps the x-axis to inbound bounce angle (0-360 degree value) and the y-axis to the outbound " +
                 "bounce angle (0-360 degree value).")]
        private AnimationCurve _remapBounceAnglesCurve;

        [Space]
        [SerializeField]
        [Foldout("Bouncing")]
        [Tooltip("If checked, allows modification of bounce speed based on inbound bounce angle (not affected by " +
                 "remapped bounce angles).")]
        private bool _mapBounceAngleToVelocity;

        [SerializeField]
        [Foldout("Bouncing")]
        [ContextMenuItem("Reset", "ResetBounceAngleToVelocityCurve")]
        [Tooltip("Maps the x-axis to inbound bounce angle (0-360 degree value) and the y-axis to velocity " +
                 "multiplier (any value).")]
        private AnimationCurve _bounceAngleToVelocityCurve;

        [SerializeField]
        [Foldout("Player Stuff")]
        [Tooltip("If nonzero, what the player's horizontal control lock timer is set to in seconds when it bounces.")]
        private float _controlLockTime;

        [SerializeField]
        [Foldout("Player Stuff")]
        [Tooltip("Whether to detach the player from the ground when it bounces off the platform.")]
        private bool _detachPlayer;

        [SerializeField]
        [Foldout("Player Stuff")]
        [Tooltip("Whether to end the player's rolling when it bounces off the platform, like a vertical spring would.")]
        private bool _endRoll;

        [SerializeField]
        [Foldout("Animation")]
        private Animator _animator;

        [SerializeField]
        [Foldout("Animation")]
        [Tooltip("Name of an Animator trigger to set when a player bounces off the platform.")]
        private string _bounceTrigger;
        private int _bounceTriggerHash;

        [SerializeField]
        [Foldout("Player Animation")]
        [Tooltip("Name of an Animator trigger on the player to set when it bounces off the platform.")]
        private string _playerBounceTrigger;
        private int _playerBounceTriggerHash;

        private List<BounceTimer> _bounceTimers;

        private float MaxBouncesInterval
        {
            get { return _maxBouncesPerPlayerPerSecond == 0 ? 0 : 1/_maxBouncesPerPlayerPerSecond; }
        }

        #endregion

        #region Public Helper Functions

        /// <summary>
        /// Returns the given angle in degrees as a 0-360 value, corrected for the object's rotation if
        /// <see cref="RelativeToRotation"/> is true.
        /// </summary>
        public float RelativeAngle(float angle)
        {
            return DMath.PositiveAngle_d(angle - (RelativeToRotation ? transform.eulerAngles.z : 0));
        }

        /// <summary>
        /// Returns the given local angle in degrees as a global 0-360 value, corrected for the object's
        /// rotation if <see cref="RelativeToRotation"/> is ture.
        /// </summary>
        public float AbsoluteAngle(float angle)
        {
            return DMath.PositiveAngle_d(angle + (RelativeToRotation ? transform.eulerAngles.z : 0));
        }

        /// <summary>
        /// Remaps the given relative angle based on <see cref="RemapBounceAngles"/>.
        /// </summary>
        /// <param name="relativeAngle">The given relative angle in degrees.</param>
        public float RemapBounceAngle(float relativeAngle)
        {
            if (RemapBounceAngles)
                return RemapBounceAnglesCurve.Evaluate(DMath.PositiveAngle_d(relativeAngle));

            return DMath.PositiveAngle_d(relativeAngle);
        }

        /// <summary>
        /// Returns velocity multiplier for a player that hits the area at the given relative bounce angle.
        /// </summary>
        /// <param name="relativeAngle">The relative bounce angle in degrees.</param>
        public float GetVelocityMultiplier(float relativeAngle)
        {
            if (MapBounceAngleToVelocity)
                return BounceAngleToVelocityCurve.Evaluate(DMath.PositiveAngle_d(relativeAngle));

            return 1;
        }

        #endregion

        #region Lifecycle Functions

        public override void Reset()
        {
            base.Reset();

            BouncyArcCenter = 90;
            BouncyArcLength = 30;
            MaxBouncesPerPlayerPerSecond = 10;

            ResetRemapBounceAnglesCurve();
            ResetBounceAngleToVelocityCurve();

            _velocity = 9.6f;
            _detachPlayer = true;

            _animator = GetComponent<Animator>();
        }

        public override void Awake()
        {
            base.Awake();

#if UNITY_EDITOR
            if (transform.lossyScale.x < 0 || transform.lossyScale.y < 0)
            {
                Debug.LogWarningFormat(
                    "BouncyPlatform \"{0}\" has a negative scale - bouncy angle values don't change for " +
                    "flipped objects, use rotation instead!", name);
            }
#endif

            _bounceTimers = new List<BounceTimer>();

            Animator = Animator ?? GetComponent<Animator>();
            _bounceTriggerHash = Animator.StringToHash(_bounceTrigger);
            _playerBounceTriggerHash = Animator.StringToHash(_playerBounceTrigger);
        }

        protected void Update()
        {
            UpdateBounceTimers();
        }
#if UNITY_EDITOR
        protected void OnDrawGizmosSelected()
        {
            DrawBounceField();
        }

        private void DrawBounceField()
        {
            const float arrowSpace = 15;

            DrawArrowForBounceAngle(BouncyAngleMin);
            DrawArrowForBounceAngle(BouncyAngleMax);

            for (float i = BouncyAngleMin, iters = 0;
                i < BouncyAngleMax && iters < 100;
                i += arrowSpace, ++iters)
            {
                DrawArrowForBounceAngle(i);
            }
        }

        private static readonly Color inboundColor = Color.yellow;
        const float inboundLength = 0.15f;
        const float inboundTipLength = 0.02f;

        static readonly Color outboundColor = Color.cyan;
        const float outboundLength = 0.15f;
        const float outboundTipLength = 0.045f;

        private void DrawArrowForBounceAngle(float bounceAngle)
        {
            var inbound = AbsoluteAngle(bounceAngle);
            var outbound = AbsoluteAngle(RemapBounceAngle(bounceAngle));

            var multiplier = GetVelocityMultiplier(inbound);

            if (!RemapBounceAngles)
            {
                DrawArrow(transform.position, outbound, outboundColor,
                    (inboundLength + outboundLength) * multiplier, outboundTipLength);
            }
            else
            {
                DrawArrow(transform.position, inbound, inboundColor, inboundLength, inboundTipLength);

                var inboundPos = transform.position +
                                 (Vector3)DMath.UnitVector(inbound * Mathf.Deg2Rad) * inboundLength;

                DrawArrow(inboundPos, outbound, outboundColor,
                    outboundLength + (inboundLength + outboundLength) * (multiplier - 1),
                    outboundTipLength);
            }
        }

        private void DrawArrow(Vector3 position, float direction, Color color, float length = 0.3f, float tipLength = 0.1f)
        {
            var tipPosition = position + (Vector3)DMath.UnitVector(direction * Mathf.Deg2Rad) * length;

            Gizmos.color = color;

            Gizmos.DrawLine(position, tipPosition);

            if (length < 0)
                direction += 180;

            Gizmos.DrawLine(tipPosition,
                tipPosition + (Vector3)DMath.UnitVector((direction - 135) * Mathf.Deg2Rad) * tipLength);
            Gizmos.DrawLine(tipPosition,
                tipPosition + (Vector3)DMath.UnitVector((direction + 135) * Mathf.Deg2Rad) * tipLength);
        }
#endif

        protected void ResetRemapBounceAnglesCurve()
        {
            RemapBounceAnglesCurve = AnimationCurve.Linear(0, 0, 360, 360);
        }

        protected void ResetBounceAngleToVelocityCurve()
        {
            BounceAngleToVelocityCurve = AnimationCurve.Linear(0, 1, 360, 1);
        }


        #endregion

        #region Event Functions

        public override void OnPreCollide(PlatformCollision.Contact contact)
        {
            var data = contact.HitData;
            var player = contact.Controller;

            var hitNormal = 0f;

            if (BounceAngleBehavior == BouncyPlatformBounceAngleBehavior.UseNormal)
            {
                hitNormal = RelativeAngle(data.NormalAngle*Mathf.Rad2Deg);
            }
            else if (BounceAngleBehavior == BouncyPlatformBounceAngleBehavior.UseLineFromCenter)
            {
                hitNormal = RelativeAngle(DMath.Angle(player.transform.position - transform.position)*Mathf.Rad2Deg);
            }

            if (!(BouncyAngleMin == 0 && Mathf.Approximately(BouncyAngleMax, 360)) &&
                !DMath.AngleInRange_d(hitNormal, BouncyAngleMin, BouncyAngleMax))
                return;

            if (!CheckBounceTimer(player))
                return;

            if (ControlLockTime > 0)
            {
                var groundControl = player.GetMove<GroundControl>();

                if (groundControl)
                    groundControl.Lock(ControlLockTime);
            }

            if (DetachPlayer)
            {
                player.Detach();
                player.IgnoreThisCollision();
            }

            if (EndRoll)
            {
                player.EndMove<Roll>();
            }

            var normalRadians = AbsoluteAngle(RemapBounceAngle(hitNormal))*Mathf.Deg2Rad;
            var velocityMultiplier = GetVelocityMultiplier(hitNormal);

            if (VelocityEffect == BouncyPlatformVelocityEffect.Set)
            {
                player.Velocity = DMath.UnitVector(normalRadians)*Velocity*velocityMultiplier;
            }
            else if (VelocityEffect == BouncyPlatformVelocityEffect.StopAndAdd)
            {
                player.Velocity = new Vector2(
                    player.Velocity.x*Mathf.Abs(Mathf.Sin(normalRadians)),
                    player.Velocity.y*Mathf.Abs(Mathf.Cos(normalRadians)));

                player.Velocity += DMath.UnitVector(normalRadians)*Velocity*velocityMultiplier;
            }

            if (Animator)
            {
                if (BounceTriggerHash != 0)
                    Animator.SetTrigger(BounceTriggerHash);
            }

            RealmsAnimatorUtility.SilentSet(player, SetPlayerOnPreCollideParameters);

            BlinkEffectTrigger(player);

            StartBounceTimer(player);
        }

        private void StartBounceTimer(HedgehogController player)
        {
            for (var i = _bounceTimers.Count - 1; i >= 0; --i)
            {
                var timer = _bounceTimers[i];
                if (timer.Player == player)
                {
                    timer.Time = MaxBouncesInterval;
                    return;
                }
            }

            _bounceTimers.Add(new BounceTimer(player, 1/MaxBouncesPerPlayerPerSecond));
        }

        private void UpdateBounceTimers()
        {
            for (var i = _bounceTimers.Count - 1; i >= 0; --i)
            {
                var timer = _bounceTimers[i];

                if ((timer.Time -= Time.deltaTime) <= 0)
                {
                    _bounceTimers.RemoveAt(i);
                }
            }
        }

        private bool CheckBounceTimer(HedgehogController player)
        {
            for (var i = 0; i < _bounceTimers.Count; ++i)
            {
                var timer = _bounceTimers[i];
                if (timer.Player == player && timer.Time > 0)
                    return false;
            }

            return true;
        }

        private void SetPlayerOnPreCollideParameters(Animator animator)
        {
            if (PlayerBounceTriggerHash != 0)
                animator.SetTrigger(PlayerBounceTriggerHash);
        }

        #endregion

        private class BounceTimer
        {
            public HedgehogController Player;
            public float Time;

            public BounceTimer(HedgehogController player, float time)
            {
                Player = player;
                Time = time;
            }
        }
    }

    /// <summary>
    /// Controls how a <see cref="BouncyPlatform"/> affects the player's velocity.
    /// </summary>
    public enum BouncyPlatformVelocityEffect
    {
        /// <summary>
        /// The bouncy thing will set the player's velocity to its desired value. Used in bumpers and diagonal springs.
        /// </summary>
        Set,

        /// <summary>
        /// The bouncy thing will stop the component of the player's velocity traveling towards its normal then add
        /// its desired value. Used in vertical and horizontal springs.
        /// </summary>
        StopAndAdd,
    }

    /// <summary>
    /// Controls how a <see cref="BouncyPlatform"/> decides the angle at which to bounce a player.
    /// </summary>
    public enum BouncyPlatformBounceAngleBehavior
    {
        /// <summary>
        /// The bouncy thing will use the normal angle of the platform the player hit.
        /// </summary>
        UseNormal,

        /// <summary>
        /// The bouncy thing will use the angle of a line drawn from its center to the player's position.
        /// </summary>
        UseLineFromCenter,
    }
}
