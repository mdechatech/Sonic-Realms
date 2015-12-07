using Hedgehog.Level.Objects;
using UnityEngine;

namespace Hedgehog.Core.Moves
{
    /// <summary>
    /// Attracts rings in a certain radius to follow the controller.
    /// </summary>
    public class MagnetizeRings : Move
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
        }

        public override void OnManagerAdd()
        {
            Perform(true);
        }
        
        public override void OnActiveFixedUpdate()
        {
            CheckTimer -= Time.fixedDeltaTime;
            if (CheckTimer < 0.0f)
            {
                CheckTimer = CheckTime;
                CheckRings();
            }
        }

        public void CheckRings()
        {
            var hits = Physics2D.OverlapCircleNonAlloc(transform.position, Radius, _results);
            for (var i = 0; i < hits; ++i)
            {
                var ring = _results[i].GetComponent<Ring>();
                if (!ring)
                    continue;

                if (ring.GetComponent<MagnetizedRing>() != null)
                    continue;

                var magnetized = ring.gameObject.AddComponent<MagnetizedRing>();
                magnetized.Acceleration = RingSpeed;
                magnetized.Target = Controller.transform;
            }
        }
    }
}
