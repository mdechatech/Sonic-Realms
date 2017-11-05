using System.Linq;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Internal;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Areas
{
    /// <summary>
    /// Liquid that makes the controller slower or faster. Physics are configured in the player's animator.
    /// </summary>
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

        [SrFoldout("Animation")]
        public Animator Animator;

        /// <summary>
        /// Name of an Animator trigger set when the water is electrocuted.
        /// </summary>
        [SrFoldout("Animation")]
        [Tooltip("Name of an Animator trigger set when the water is electrocuted.")]
        public string ElectrocutedTrigger;
        protected int ElectrocutedTriggerHash;

        private Collider2D[] _colliders;

        public override void Reset()
        {
            base.Reset();

            Viscosity = 2.0f;

            Animator = GetComponent<Animator>();
        }

        public override void Start()
        {
            base.Start();

            _colliders = GetComponentsInChildren<Collider2D>(false);

            Animator = Animator ?? GetComponent<Animator>();
            ElectrocutedTriggerHash = Animator.StringToHash(ElectrocutedTrigger);
        }

        /// <summary>
        /// Electrocutes the water.
        /// </summary>
        public virtual void Electrocute()
        {
            // TODO destroy badniks?
            if(Animator && ElectrocutedTriggerHash != 0)
                Animator.SetTrigger(ElectrocutedTriggerHash);
        }

        public override bool CanTouch(AreaCollision.Contact contact)
        {
            return base.CanTouch(contact) &&
                   _colliders.Any(collider => collider.OverlapPoint(contact.Controller.Sensors.Center.position));
        }
        
        // Apply new physics values based on viscosity
        public override void OnAreaEnter(AreaCollision collision)
        {
            var controller = collision.Controller;
            controller.Vx /= Viscosity;
            controller.Vy /= Viscosity*2.0f;
        }
        
        // Constantly apply buoyancy.
        public override void OnAreaStay(AreaCollision collision)
        {
            var controller = collision.Controller;
            if (!controller.Grounded) controller.Vy += Buoyancy*Time.fixedDeltaTime;
        }

        // Restore old physics values.
        public override void OnAreaExit(AreaCollision collision)
        {
            var controller = collision.Controller;
            controller.Vy *= Viscosity;
        }
    }
}
