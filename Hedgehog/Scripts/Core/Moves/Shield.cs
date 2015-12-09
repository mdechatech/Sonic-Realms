using System.Linq;
using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using Hedgehog.Level.Areas;
using UnityEngine;

namespace Hedgehog.Core.Moves
{
    public class Shield : Move
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
        }

        public void OnHurt()
        {
            Destroy(gameObject);
        }

        public override void OnManagerAdd()
        {
            var health = Controller.GetComponent<HedgehogHealth>();
            health.OnHurt.AddListener(OnHurt);

            // Replace any previous shields
            var shield = Manager.GetMoves<Shield>().FirstOrDefault(move => move != this);
            if (shield != null) Destroy(shield.gameObject);

            if (DestroyInWater && Controller.Inside<Water>())
            {
                Destroy(gameObject);
            }
            else if (DestroyInWater)
            {
                Controller.OnReactiveEnter.AddListener(OnReactiveEnter);
            }

            Perform(true);
        }

        public override void OnManagerRemove()
        {
            var health = Controller.GetComponent<HedgehogHealth>();
            health.OnHurt.RemoveListener(OnHurt);

            if (!DestroyInWater)
                return;

            Controller.OnReactiveExit.RemoveListener(OnReactiveEnter);
        }

        public void OnReactiveEnter(BaseReactive reactive)
        {
            if (DestroyInWater && Controller.Inside<Water>())
                Destroy(gameObject);
        }

        public override void OnActiveUpdate()
        {
            if (RotateToGravity)
                transform.eulerAngles = new Vector3(
                    transform.eulerAngles.x,
                    transform.eulerAngles.y,
                    Controller.GravityDirection + 90.0f);
        }
    }
}
