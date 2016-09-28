using System;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Moves;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// On a call to Allows(), this object returns true or false based on powerup related criteria.
    /// </summary>
    [Serializable]
    public class PowerupsLimiter : ITriggerLimiter<HedgehogController>
    {
        /// <summary>
        /// If not empty, the name of the powerup the player must have in its Powerup Manager. The name is
        /// case-sensitive and without spaces, for example the Magnetize Rings powerup would be referred
        /// to as "MagnetizeRings".
        /// </summary>
        [Tooltip("If not empty, the name of the powerup the player must have in its Powerup Manager. The name is " +
                 "case-sensitive and without spaces, for example the Magnetize Rings powerup would be referred " +
                 "to as \"MagnetizeRings\".")]
        public string MustHavePowerup;

        public bool Allows(HedgehogController controller)
        {
            if (string.IsNullOrEmpty(MustHavePowerup))
                return true;

            return controller.HasPowerup(MustHavePowerup);
        }
    }
}
