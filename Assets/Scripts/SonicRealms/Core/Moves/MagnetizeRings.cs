using System.Collections.Generic;
using SonicRealms.Level.Objects;
using UnityEngine;

namespace SonicRealms.Core.Moves
{
    /// <summary>
    /// Attracts rings in a certain radius to follow the player.
    /// </summary>
    public class MagnetizeRings : Powerup
    {
        /// <summary>
        /// Distance from the controller at which rings are attracted.
        /// </summary>
        [Tooltip("Distance from the controller at which rings are attracted.")]
        public float Radius;

        /// <summary>
        /// Speed at which rings float toward the controller, in units per second squared.
        /// </summary>
        [Tooltip("Speed at which rings float toward the controller, in units per second squared.")]
        public float RingSpeed;

        /// <summary>
        /// How long to wait between checking for rings to attract, in seconds.
        /// </summary>
        [Tooltip("How long to wait between checking for rings to attract, in seconds.")]
        public float CheckTime;
        protected float CheckTimer;

        private const int MaxResults = 16;
        private Collider2D[] _results;

        private List<MagnetizedRing> _magnetizedRings;

        public override void Reset()
        {
            base.Reset();
            Radius = 0.64f;
            RingSpeed = 6.75f;
            CheckTime = 0.25f;
        }

        public override void Awake()
        {
            _results = new Collider2D[MaxResults];
            CheckTimer = CheckTime;

            _magnetizedRings = new List<MagnetizedRing>();
        }

        public override void OnAddedFixedUpdate()
        {
            CheckTimer -= Time.fixedDeltaTime;
            if (CheckTimer < 0.0f)
            {
                CheckTimer = CheckTime;
                CheckRings();
            }
        }

        /// <summary>
        /// Checks for rings in the radius and magnetizes them.
        /// </summary>
        public void CheckRings()
        {
            var hits = Physics2D.OverlapCircleNonAlloc(transform.position, Radius, _results);
            for (var i = 0; i < hits; ++i)
            {
                var ring = _results[i].GetComponent<Ring>();
                if (!ring) continue;

                Magnetize(ring);
            }
        }

        /// <summary>
        /// Magnetizes the given ring to follow the player.
        /// </summary>
        /// <param name="ring"></param>
        public void Magnetize(Ring ring)
        {
            if (ring.GetComponent<MagnetizedRing>() != null)
                return;

            var magnetized = ring.gameObject.AddComponent<MagnetizedRing>();
            magnetized.Acceleration = RingSpeed;
            magnetized.Target = Controller.transform;

            _magnetizedRings.Add(magnetized);
        }

        public override void OnManagerRemove()
        {
            // Make all magnetized rings float away and destroy themselves after a short time
            foreach (var ring in _magnetizedRings)
            {
                if (!ring) continue;
                Destroy(ring);
                Destroy(ring.gameObject, 3f);
            }
        }
    }
}
