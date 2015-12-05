using Hedgehog.Core.Actors;
using Hedgehog.Core.Moves;
using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level.Areas
{
    /// <summary>
    /// Liquid that makes the controller slower or faster and has options for buoyancy and even running
    /// on top of it, if it's fast enough!
    /// </summary>
    [AddComponentMenu("Hedgehog/Areas/Water")]
    public class Water : ReactivePlatformArea
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

        private Collider2D _collider2D;

        public override void Reset()
        {
            Viscosity = 2.0f;
            Buoyancy = 0.0f;
            MinFloatSpeed = 5.0f;
        }

        public override bool IsInside(HedgehogController controller)
        {
            return base.IsInside(controller) && !PlatformTrigger.HasController(controller);
        }

        // The water is a surface if the player is upright, on top of it, grounded, not already submerged,
        // and running quickly enough.
        public override bool IsSolid(TerrainCastHit hit)
        {
            if (hit.Controller == null) return false;
            return base.IsSolid(hit) &&
                   (hit.Side & ControllerSide.Bottom) > 0 &&
                   hit.Hit.fraction > 0.0f &&
                   hit.Controller.Grounded &&
                   Mathf.Abs(hit.Controller.GroundVelocity) >= MinFloatSpeed;
        }
        
        // Apply new physics values based on viscosity
        public override void OnAreaEnter(HedgehogController controller)
        {
            controller.GroundControl.Acceleration /= Viscosity;
            controller.GroundControl.Deceleration /= Viscosity;
            controller.GroundControl.TopSpeed /= Viscosity;
            controller.GroundFriction /= Viscosity;
            controller.AirControl.Acceleration /= Viscosity;
            controller.AirGravity /= Viscosity;
            controller.Vx /= Viscosity;
            controller.Vy /= Viscosity*2.0f;
            controller.DetachSpeed /= Viscosity;

            var jump = controller.MoveManager.GetMove<Jump>();
            if (jump == null)
                return;
            jump.ActivateSpeed /= Viscosity;
            jump.ReleaseSpeed /= Viscosity;
        }
        
        // Constantly apply buoyancy.
        public override void OnAreaStay(HedgehogController controller)
        {
            if(!controller.Grounded) controller.Vy += Buoyancy*Time.fixedDeltaTime;
        }

        // Restore old physics values.
        public override void OnAreaExit(HedgehogController controller)
        {
            controller.GroundControl.Acceleration *= Viscosity;
            controller.GroundControl.Deceleration *= Viscosity;
            controller.GroundControl.TopSpeed *= Viscosity;
            controller.GroundFriction *= Viscosity;
            controller.AirControl.Acceleration *= Viscosity;
            controller.AirGravity *= Viscosity;
            controller.Vy *= Viscosity;
            controller.DetachSpeed *= Viscosity;

            var jump = controller.MoveManager.GetMove<Jump>();
            if (jump == null)
                return;
            jump.ActivateSpeed *= Viscosity;
            jump.ReleaseSpeed *= Viscosity;
        }
    }
}
