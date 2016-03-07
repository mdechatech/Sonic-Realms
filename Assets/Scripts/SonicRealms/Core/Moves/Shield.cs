using System.Linq;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Triggers;
using SonicRealms.Level.Areas;
using UnityEngine;

namespace SonicRealms.Core.Moves
{
    public class Shield : Powerup
    {
        /// <summary>
        /// Whether to destroy the powerup when underwater.
        /// </summary>
        [Tooltip("Whether to destroy the powerup when underwater.")]
        public bool DestroyInWater;

        /// <summary>
        /// Whether to rotate the shield with the direction of gravity.
        /// </summary>
        [Tooltip("Whether to rotate the shield with the direction of gravity.")]
        public bool RotateToGravity;

        public override void Reset()
        {
            base.Reset();
            DestroyInWater = false;
            RotateToGravity = true;
        }

        public void OnHurt(HealthEventArgs e)
        {
            Remove();
        }

        public override void OnManagerAdd()
        {
            var health = Controller.GetComponent<HedgehogHealth>();
            if (health == null)
            {
                Debug.LogWarning("Tried to add shield, but the controller has no health system!");
                enabled = false;
                return;
            }

            health.OnHurt.AddListener(OnHurt);

            // Replace any previous shields
            var shield = Manager.GetAll<Shield>().FirstOrDefault(move => move != this);
            if (shield != null) shield.Remove();

            if (DestroyInWater && Controller.Inside<Water>())
            {
                Remove();
            }
            else if (DestroyInWater)
            {
                Controller.OnAreaEnter.AddListener(OnAreaEnter);
            }
        }

        public override void OnManagerRemove()
        {
            var health = Controller.GetComponent<HedgehogHealth>();
            if (health != null)
            {
                health.OnHurt.RemoveListener(OnHurt);
            }

            if (!DestroyInWater)
                return;

            Controller.OnAreaEnter.RemoveListener(OnAreaEnter);
        }

        public void OnAreaEnter(ReactiveArea area)
        {
            if (DestroyInWater && Controller.Inside<Water>())
                Remove();
        }

        public override void OnAddedUpdate()
        {
            if (RotateToGravity)
                transform.eulerAngles = new Vector3(
                    transform.eulerAngles.x,
                    transform.eulerAngles.y,
                    Controller.GravityDirection + 90.0f);
        }
    }
}
