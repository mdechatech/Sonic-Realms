using UnityEngine;

namespace Hedgehog.Core.Actors
{
    /// <summary>
    /// Holds all of the sensors for HedgehogController.
    /// </summary>
    public class HedgehogSensors : MonoBehaviour
    {
        // These sensors are used for hit detection with ceilings.
        public Transform TopLeft;
        public Transform TopCenter;
        public Transform TopRight;

        // These sensors are used for hit detection with walls.
        public Transform CenterLeft;
        public Transform Center;
        public Transform CenterRight;

        /// These sensors are used for hit detection with the floor when in the air.
        public Transform BottomLeft;
        public Transform BottomCenter;
        public Transform BottomRight;

        // These sensors are used for hit detection with the floor when on the ground.
        public Transform LedgeClimbLeft;
        public Transform LedgeClimbRight;
        public Transform LedgeDropLeft;
        public Transform LedgeDropRight;
    }
}
