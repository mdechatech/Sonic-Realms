using System;
using SonicRealms.Core.Utils;
using SonicRealms.Level.Objects;
using UnityEngine;
using UnityEngine.Events;

namespace SonicRealms.Core.Actors
{
    /// <summary>
    /// Allows ring collection and spilling.
    /// </summary>
    public class RingCounter : MonoBehaviour
    {
        public HedgehogController Controller;
        public int Rings
        {
            get { return _rings; }
            set
            {
                if (_rings == value) return;
                _rings = value;
                OnValueChange.Invoke();
            }
        }
        private int _rings;

        /// <summary>
        /// Invoked when the ring total changes.
        /// </summary>
        public UnityEvent OnValueChange;

        protected Animator Animator;

        /// <summary>
        /// Name of an Animator int set to the current ring amount.
        /// </summary>
        [Tooltip("Name of an Animator int set to the current ring amount.")]
        public string AmountInt;
        protected int AmountIntHash;

        /// <summary>
        /// Whether the component is allowed to collect rings.
        /// </summary>
        [Tooltip("Whether the component is allowed to collect rings.")]
        public bool CanCollect;

        /// <summary>
        /// Time until component is allowed to collect rings, in seconds.
        /// </summary>
        [NonSerialized]
        public float CanCollectTimer;

        /// <summary>
        /// Base spilled ring to make copies of. Velocity is set automatically when copied.
        /// </summary>
        [Header("Ring Spilling")]
        [Tooltip("Base spilled ring to make copes if. Velocity is set automatically when copied.")]
        public SpilledRing SpilledRingBase;

        /// <summary>
        /// How long after spilling rings the controller must wait to pick them back up, in seconds.
        /// </summary>
        [Tooltip("How long after spilling rings the controller must wait to pick them back up, in seconds.")]
        public float SpillCollectDisableTime;

        /// <summary>
        /// Maximum possible rings that can be spilled in a single hit.
        /// </summary>
        [Tooltip("Maximum possible rings that can be spilled in a single hit.")]
        public int MaxSpilledRings;

        /// <summary>
        /// Spilled rings are spawned in concentric circles with varying speeds.
        /// </summary>
        [Tooltip("Spilled rings are spawned in concentric circles with varying speeds.")]
        public int RingsPerCircle;

        /// <summary>
        /// The speed of each circle of rings, starting from the first, in units per second.
        /// </summary>
        [Tooltip("The speed of each circle of rings, starting from the first, in units per second.")]
        public float[] CircleSpeeds;

        public void Reset()
        {
            Controller = GetComponentInParent<HedgehogController>();
            Rings = 0;
            AmountInt = "";

            CanCollect = true;

            // Default values from https://info.sonicretro.org/SPG:Ring_Loss
            SpillCollectDisableTime = 1.066667f;
            MaxSpilledRings = 32;
            RingsPerCircle = 16;
            CircleSpeeds = new[] {4.0f, 2.0f};
        }

        public void Awake()
        {
            Animator = Controller.Animator;
            CanCollectTimer = 0.0f;

            if (Animator == null) return;
            AmountIntHash = string.IsNullOrEmpty(AmountInt) ? 0 : Animator.StringToHash(AmountInt);
        }

        public void Update()
        {
            // If can't collect, update the timer and check for completion
            if (!CanCollect)
            {
                CanCollectTimer -= Time.deltaTime;
                if (CanCollectTimer < 0.0f)
                {
                    CanCollect = true;
                    CanCollectTimer = 0.0f;
                }
            }
            else
            {
                CanCollectTimer = 0.0f;
            }
        }

        /// <summary>
        /// Disables ring collection for the value specified by SpillCollectDisableTime.
        /// </summary>
        public void DisableCollection()
        {
            DisableCollection(SpillCollectDisableTime);
        }

        /// <summary>
        /// Disables ring collection for the specified duration.
        /// </summary>
        /// <param name="duration">The specified duration, in seconds.</param>
        public void DisableCollection(float duration)
        {
            CanCollectTimer = duration;
            CanCollect = false;
        }

        public void Spill(int amount)
        {
            if (SpilledRingBase == null)
            {
                Debug.LogError("Can't spill rings because there's no SpilledRingBase to make copies of!");
                return;
            }

            // So that the controller doesn't instantly pick them back up
            DisableCollection();

            // Calculate the rings to spill and the amount to deduct from the total
            var toSpill = Mathf.Min(Mathf.Min(amount, Rings), MaxSpilledRings);
            Rings = Mathf.Max(Rings - amount, 0);

            // Ring spilling algorithm from https://info.sonicretro.org/SPG:Ring_Loss
            var angle = 101.25f;
            var angleDelta = 360.0f/RingsPerCircle;
            var flip = false;
            var circle = 0;
            var speed = 2.0f;
            for (var i = 0; i < toSpill; ++i)
            {
                if (i % RingsPerCircle == 0)
                {
                    angle = 101.25f;
                    circle = i/RingsPerCircle;
                    speed = CircleSpeeds[circle];
                }

                var ring = Instantiate(SpilledRingBase);
                ring.transform.position = transform.position;
                ring.Velocity = DMath.AngleToVector(angle*Mathf.Deg2Rad)*speed;

                if (flip)
                {
                    ring.Velocity = new Vector2(-ring.Velocity.x, ring.Velocity.y);
                    angle += angleDelta;
                }

                flip = !flip;
            }
        }

        /// <summary>
        /// Returns the ring amount in the ring collector.
        /// </summary>
        /// <param name="ringCounter">The ring collector to cast.</param>
        public static implicit operator int (RingCounter ringCounter)
        {
            return ringCounter ? ringCounter.Rings : 0;
        }
    }
}
