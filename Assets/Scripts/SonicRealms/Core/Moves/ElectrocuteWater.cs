using SonicRealms.Core.Triggers;
using SonicRealms.Level.Areas;
using UnityEngine;

namespace SonicRealms.Core.Moves
{
    /// <summary>
    /// Electrocutes water on contact, like the electric shield does.
    /// </summary>
    public class ElectrocuteWater : Powerup
    {
        /// <summary>
        /// Whether to destroy the move's object on contact.
        /// </summary>
        [Tooltip("Whether to destroy the move's object on contact.")]
        public bool DestroyOnContact;

        private Water _water;

        public override void Reset()
        {
            base.Reset();
            DestroyOnContact = true;
        }

        public override void OnManagerAdd()
        {
            // Listen for when we hit an area, to check if it's water
            Controller.OnAreaEnter.AddListener(OnAreaEnter);

            // Electrocute immediately if already underwater
            if (Controller.Inside<Water>())
            {
                _water = Controller.GetReactive<Water>();
                Electrocute();
            }
        }

        public override void OnManagerRemove()
        {
            Controller.OnAreaEnter.RemoveListener(OnAreaEnter);
        }

        public void OnAreaEnter(ReactiveArea area)
        {
            var water = area as Water;
            if (water != null)
            {
                _water = water;
                Electrocute();
            } 
        }

        public void Electrocute()
        {
            if (_water == null) _water = Controller.GetReactive<Water>();
            if (_water == null) return;

            _water.Electrocute();
            if (DestroyOnContact) Remove();
        }
    }
}
