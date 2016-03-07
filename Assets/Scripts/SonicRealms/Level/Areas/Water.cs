using System.Linq;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Areas
{
    /// <summary>
    /// Liquid that makes the controller slower or faster. Physics are configured in the player's animator.
    /// </summary>
    [AddComponentMenu("Hedgehog/Areas/Water")]
    public class Water : ReactiveArea
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
        /// Name of an Animator trigger set when the water is electrocuted.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator trigger set when the water is electrocuted.")]
        public string ElectrocutedTrigger;
        protected int ElectrocutedTriggerHash;

        private Collider2D[] _colliders;

        public override void Reset()
        {
            base.Reset();
            Viscosity = 2.0f;
            Buoyancy = 0.0f;
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

        public override bool IsInside(Hitbox hitbox)
        {
            return base.IsInside(hitbox) &&
                   _colliders.Any(collider => collider.OverlapPoint(hitbox.Controller.Sensors.Center.position));
        }
        
        // Apply new physics values based on viscosity
        public override void OnAreaEnter(Hitbox hitbox)
        {
            var controller = hitbox.Controller;
            controller.Vx /= Viscosity;
            controller.Vy /= Viscosity*2.0f;
        }
        
        // Constantly apply buoyancy.
        public override void OnAreaStay(Hitbox hitbox)
        {
            var controller = hitbox.Controller;
            if (!controller.Grounded) controller.Vy += Buoyancy*Time.fixedDeltaTime;
        }

        // Restore old physics values.
        public override void OnAreaExit(Hitbox hitbox)
        {
            var controller = hitbox.Controller;
            controller.Vy *= Viscosity;
        }
    }
}
