using System.Collections.Generic;
using SonicRealms.Core.Actors;

namespace SonicRealms.Core.Moves
{
    /// <summary>
    /// Powerup helpers that can be called straight from HedgehogController.
    /// </summary>
    public static class PowerupExtensions
    {
        public static void AddPowerup(this HedgehogController controller, Powerup powerup)
        {
            var powerupManager = controller.GetComponent<PowerupManager>();
            if (powerupManager == null) return;
            powerupManager.Add(powerup);
        }

        public static TPowerup HasPowerup<TPowerup>(this HedgehogController controller)
            where TPowerup : Powerup
        {
            var powerupManager = controller.GetComponent<PowerupManager>();
            if (powerupManager == null) return null;
            return powerupManager.Get<TPowerup>();
        }

        public static TPowerup GetPowerup<TPowerup>(this HedgehogController controller)
            where TPowerup : Powerup
        {
            var powerupManager = controller.GetComponent<PowerupManager>();
            if (powerupManager == null) return null;
            return powerupManager.Get<TPowerup>();
        }

        public static List<Powerup> GetPowerups(this HedgehogController controller)
        {
            var powerupManager = controller.GetComponent<PowerupManager>();
            if (powerupManager == null) return null;
            return powerupManager.Powerups;
        } 

        public static bool RemovePowerup<TPowerup>(this HedgehogController controller)
            where TPowerup : Powerup
        {
            var powerupManager = controller.GetComponent<PowerupManager>();
            if (powerupManager == null) return false;
            return powerupManager.Remove<TPowerup>();
        }
    }
}
