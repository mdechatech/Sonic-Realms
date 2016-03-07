using System.Linq;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Moves
{
    /// <summary>
    /// Sonic invincibility. Unable to be harmed, and also makes Sonic hurt badniks on touch.
    /// </summary>
    public class Invincibility : Powerup
    {
        /// <summary>
        /// If true, the powerup will disable shields and their moves while active.
        /// </summary>
        [Tooltip("If checked, the powerup will disable shields and their moves while active.")]
        public bool DisableShields;

        /// <summary>
        /// The player's health system.
        /// </summary>
        [Foldout("Debug")]
        [Tooltip("The player's health system.")]
        public HealthSystem Health;

        /// <summary>
        /// The player's non-attack hitboxes. Stored for the purpose of making them harmful.
        /// </summary>
        [Foldout("Debug")]
        [Tooltip("The player's non-attack hitboxes. Stored for the purpose of making them harmful.")]
        public SonicHitbox[] Hitboxes;

        public override void OnManagerAdd()
        {
            // Make the health system invulnerable
            Health = Controller.GetComponent<HealthSystem>();
            if (Health != null)
                Health.Invincible = true;

            // Disable shields for the duration of the powerup
            if (DisableShields)
            {
                foreach (var shield in Manager.GetAll<Shield>())
                    shield.gameObject.SetActive(false);

                MoveManager.Disable<InstaShield>();

                // Listen for when the player gets powerups to disable shields on addition
                Manager.OnAdd.AddListener(OnAddPowerup);
            }

            // Make non-attack hitboxes to make them harmful
            Hitboxes = Controller.GetComponentsInChildren<SonicHitbox>().Where(
                hitbox => !hitbox.IsAttackHitbox).ToArray();
            foreach (var hitbox in Hitboxes) hitbox.Harmful = true;
        }

        public override void OnAddedUpdate()
        {
            if (Health != null)
                Health.Invincible = true;

            foreach (var hitbox in Hitboxes)
                hitbox.Harmful = true;
        }

        public override void OnManagerRemove()
        {
            // Make the player vulnerable again
            if (Health != null)
            {
                Health.Invincible = false;
                Health = null;
            }

            // If we disabled shields before, reenable them
            if (DisableShields)
            {
                foreach (var shield in Manager.GetAll<Shield>()) shield.gameObject.SetActive(true);

                MoveManager.Enable<InstaShield>();

                Manager.OnAdd.RemoveListener(OnAddPowerup);
            }
            
            // Make non-attack hitboxes harmless
            foreach (var hitbox in Hitboxes)
                hitbox.Harmful = false;
        }

        protected void OnAddPowerup(Powerup powerup)
        {
            // Any shields gained during invincibility are deactivated for the duration
            if (powerup is Shield) powerup.gameObject.SetActive(false);
        }
    }
}
