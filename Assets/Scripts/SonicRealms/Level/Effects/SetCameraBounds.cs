using SonicRealms.Core.Actors;
using SonicRealms.Core.Triggers;
using UnityEngine;

namespace SonicRealms.Level.Effects
{
    /// <summary>
    /// Sets camera boundaries.
    /// </summary>
    public class SetCameraBounds : ReactiveObject
    {
        /// <summary>
        /// The minimum (bottom-left corner) position of the boundaries.
        /// </summary>
        [Tooltip("The minimum (bottom-left corner) position of the boundaries.")]
        public Transform Min;
        
        /// <summary>
        /// The maximum (top-right corner) position of the boundaries.
        /// </summary>
        [Tooltip("The maximum (top-right corner) position of the boundaries.")]
        public Transform Max;

        public override void OnActivate(HedgehogController controller)
        {
            controller.GetCamera().Camera.SetBounds(Min, Max);
        }
    }
}
