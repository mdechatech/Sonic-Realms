using SonicRealms.Core.Actors;
using UnityEngine;

namespace SonicRealms.Core.Moves
{
    /// <summary>
    /// Turns off collision and bounces the player off the map.
    /// </summary>
    public class Death : Move
    {
        /// <summary>
        /// How quickly the controller bounces off the map.
        /// </summary>
        [PhysicsFoldout]
        public float Velocity;

        /// <summary>
        /// How long to wait before ending the move, causing the zone manager to restart the game.
        /// </summary>
        [ControlFoldout]
        public float RestartDelay;

        [HideInInspector]
        public float RestartTimer;

        [HideInInspector]
        public bool Restarting;

        public override void Reset()
        {
            base.Reset();
            Velocity = 4.20f; // blaze it
            RestartDelay = 2.333333f;
        }

        public override void Awake()
        {
            base.Awake();
            RestartTimer = 0.0f;
            Restarting = false;
        }

        public override void OnActiveEnter(State previousState)
        {
            // Bounce up and ignore collisions
            Controller.Detach();

            // End all moves (except for this one)
            Manager.EndAll(move => move != this);

            Controller.IgnoreCollision = true;
            Controller.RelativeVelocity = new Vector2(0.0f, Velocity);

            // Become invincible to further damage
            var health = Controller.GetComponent<HedgehogHealth>();
            health.Invincible = true;

            // Bring the sprite to the front
            var sprite = Controller.GetComponentInChildren<SpriteRenderer>();
            sprite.sortingOrder = 10;

            // Start the death timer
            RestartTimer = RestartDelay;
            Restarting = true;
        }

        public override void OnActiveUpdate()
        {
            if (!Restarting) return;

            RestartTimer -= Time.deltaTime;
            if (RestartTimer < 0.0f)
            {
                RestartTimer = 0.0f;
                Restarting = false;
                End();
            }
        }
    }
}
