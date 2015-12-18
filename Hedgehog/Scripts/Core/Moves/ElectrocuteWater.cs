using Hedgehog.Core.Triggers;
using Hedgehog.Level.Areas;
using UnityEngine;

namespace Hedgehog.Core.Moves
{
    /// <summary>
    /// Electrocutes water on contact, like the electric shield does.
    /// </summary>
    public class ElectrocuteWater : Move
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
            // Listen for when we interact with something, to check if it's water
            Controller.OnReactiveEnter.AddListener(OnReactiveEnter);

            // Electrocute immediately if already underwater
            if (Controller.Underwater)
            {
                _water = Controller.GetReactive<Water>();
                Perform();
            }
        }

        public override void OnManagerRemove()
        {
            Controller.OnReactiveEnter.RemoveListener(OnReactiveEnter);
        }

        public override void OnActiveEnter()
        {
            _water.Electrocute();
            if (DestroyOnContact) Remove();
        }

        public void OnReactiveEnter(BaseReactive reactive)
        {
            var water = reactive as Water;
            if (water != null)
            {
                _water = water;
                Perform();
            } 
        }

    }
}
