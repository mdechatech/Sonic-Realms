using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using UnityEngine;

namespace Hedgehog.Level.Effects
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

        public override void OnActivateEnter(HedgehogController controller)
        {
            controller.GetCamera().Camera.SetBounds(Min, Max);
        }
    }
}
