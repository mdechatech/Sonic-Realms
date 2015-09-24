using Hedgehog.Actors;
using UnityEngine;

namespace Hedgehog.Terrain
{
    /// <summary>
    /// Liquid that makes the controller slower or faster and has options for buoyancy and even running
    /// on top of it, if it's fast enough!
    /// </summary>
    [RequireComponent(typeof(PlatformTrigger), typeof(AreaTrigger))]
    public class Water : MonoBehaviour
    {
        /// <summary>
        /// The viscosity of the liquid. The player's physics values are essentially divided by
        /// this value.
        /// </summary>
        [SerializeField]
        public float Viscosity;

        /// <summary>
        /// The buoyancy of the liquid, in units per second of upward acceleration.
        /// </summary>
        [SerializeField]
        public float Buoyancy;

        /// <summary>
        /// The minimum speed at which the player must be running to be able to run on top of the water.
        /// </summary>
        [SerializeField]
        public float MinFloatSpeed;

        private PlatformTrigger _platformTrigger;
        private AreaTrigger _areaTrigger;

        private Collider2D _collider2D;

        public void Reset()
        {
            Viscosity = 2.0f;
            Buoyancy = 0.0f;
            MinFloatSpeed = 100.0f;
        }

        public void Start()
        {
            _platformTrigger = GetComponent<PlatformTrigger>();
            _platformTrigger.CollisionPredicates.Add(FloatSelector);

            _areaTrigger = GetComponent<AreaTrigger>();
            _areaTrigger.CollisionPredicates.Add(SubmersionPredicate);
            _areaTrigger.OnAreaEnter.AddListener(Apply);
            _areaTrigger.OnAreaStay.AddListener(Stay);
            _areaTrigger.OnAreaExit.AddListener(Revert);

            _collider2D = GetComponent<Collider2D>();
        }

        // The controller must be at least half submerged underwater to apply new physics values.
        public bool SubmersionPredicate(HedgehogController controller)
        {
            return _collider2D.OverlapPoint(controller.SensorMiddleMiddle.position);
        }

        // The water is a surface if the player is upright, on top of it, grounded, and running quickly enough.
        public bool FloatSelector(TerrainCastHit hit)
        {
            if (hit.Source == null || MinFloatSpeed <= 0.0f) return false;
            return
                (hit.Side & TerrainSide.Bottom) > 0 &&
                hit.Hit.fraction > 0.0f && 
                hit.Source.Grounded &&
                Mathf.Abs(hit.Source.GroundVelocity) >= MinFloatSpeed;
        }
        
        // Apply new physics values based on viscosity.
        public void Apply(HedgehogController controller)
        {
            controller.GroundAcceleration /= Viscosity;
            controller.GroundBrake /= Viscosity;
            controller.GroundDeceleration /= Viscosity;
            controller.TopSpeed /= Viscosity;
            controller.AirAcceleration /= Viscosity;
            controller.JumpSpeed /= Viscosity;
            controller.ReleaseJumpSpeed /= Viscosity;
            controller.AirGravity /= Viscosity;
            controller.SlopeGravity /= Viscosity;
            controller.Vx /= Viscosity;
            controller.Vy /= Viscosity*2.0f;
            controller.DetachSpeed /= Viscosity;
        }
        
        // Constantly apply buoyancy.
        public void Stay(HedgehogController controller)
        {
            if(!controller.Grounded) controller.Vy += Buoyancy*Time.fixedDeltaTime;
        }

        // Restore old physics values.
        public void Revert(HedgehogController controller)
        {
            controller.GroundAcceleration *= Viscosity;
            controller.GroundBrake *= Viscosity;
            controller.GroundDeceleration *= Viscosity;
            controller.TopSpeed *= Viscosity;
            controller.AirAcceleration *= Viscosity;
            controller.JumpSpeed *= Viscosity;
            controller.ReleaseJumpSpeed *= Viscosity;
            controller.AirGravity *= Viscosity;
            controller.SlopeGravity *= Viscosity;
            controller.Vy *= Viscosity;
            controller.DetachSpeed *= Viscosity;
        }

        public void OnDestroy()
        {
            _platformTrigger.CollisionPredicates.Remove(FloatSelector);
            _areaTrigger.CollisionPredicates.Remove(SubmersionPredicate);
            _areaTrigger.OnAreaEnter.RemoveListener(Apply);
            _areaTrigger.OnAreaExit.RemoveListener(Revert);
        }
    }
}
