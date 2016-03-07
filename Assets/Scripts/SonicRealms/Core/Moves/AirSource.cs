using SonicRealms.Core.Actors;
using UnityEngine;

namespace SonicRealms.Core.Moves
{
    /// <summary>
    /// Prevents the controller's breath meter from decreasing while underwater.
    /// </summary>
    public class AirSource : Powerup
    {
        protected BreathMeter BreathMeter;

        public override void OnManagerAdd()
        {
            BreathMeter = Controller.GetComponent<BreathMeter>();
            if (!BreathMeter)
            {
                Debug.LogWarning("Tried to give air, but the controller has no breath meter!");
                return;
            }

            BreathMeter.HasExtraAirSource = true;
        }

        public override void OnManagerRemove()
        {
            BreathMeter.HasExtraAirSource = false;
        }
    }
}
