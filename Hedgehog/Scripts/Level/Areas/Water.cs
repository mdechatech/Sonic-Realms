using System.Linq;
using Hedgehog.Core.Actors;
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

        /// <summary>
        /// Name of an Animator trigger set when the water is electrocuted.
        /// </summary>
        [Tooltip("Name of an Animator trigger set when the water is electrocuted.")]
        public string ElectrocutedTrigger;
        protected int ElectrocutedTriggerHash;

        private Collider2D[] _colliders;

        public override void Reset()
        {
            base.Reset();
            Viscosity = 2.0f;
            Buoyancy = 0.0f;
            MinFloatSpeed = 5.0f;
            ElectrocutedTrigger = "";
        }

        public override void Start()
        {
            base.Start();
            _colliders = GetComponentsInChildren<Collider2D>(false);

            if (!Animator) return;
            ElectrocutedTriggerHash = string.IsNullOrEmpty(ElectrocutedTrigger)
                ? 0
                : Animator.StringToHash(ElectrocutedTrigger);
        }

        /// <summary>
        /// Electrocutes the water.
        /// </summary>
        public virtual void Electrocute()
        {
            // TODO destroy badniks?
            if(Animator && ElectrocutedTriggerHash != 0) Animator.SetTrigger(ElectrocutedTriggerHash);
        }

        public override bool IsInside(HedgehogController controller)
        {
            return base.IsInside(controller) && !PlatformTrigger.HasController(controller) &&
                   _colliders.Any(collider => collider.OverlapPoint(controller.Sensors.Center.position));
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
            controller.Vx /= Viscosity;
            controller.Vy /= Viscosity*2.0f;
        }
        
        // Constantly apply buoyancy.
        public override void OnAreaStay(HedgehogController controller)
        {
            if(!controller.Grounded) controller.Vy += Buoyancy*Time.fixedDeltaTime;
        }

        // Restore old physics values.
        public override void OnAreaExit(HedgehogController controller)
        {
            controller.Vy *= Viscosity;
        }
    }
}
