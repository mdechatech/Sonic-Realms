using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using UnityEngine;

namespace Hedgehog.Level.Areas
{
    /// <summary>
    /// Pushes the controller in a certain direction in the air.
    /// </summary>
    public class Current : ReactiveArea
    {
        /// <summary>
        /// The velocity at which the controller is pushed.
        /// </summary>
        [SerializeField] public Vector2 Velocity;

        /// <summary>
        /// Whether to automatically add controller gravity to the current's velocity.
        /// </summary>
        [SerializeField] public bool AccountForGravity;

        /// <summary>
        /// Whether to detach controllers from the ground when they enter the current.
        /// </summary>
        [SerializeField] public bool DetachControllers;

        public void Reset()
        {
            Velocity = Vector2.up;
            AccountForGravity = true;
            DetachControllers = true;
        }

        public override void OnAreaStay(HedgehogController controller)
        {
            if (controller.Grounded && DetachControllers)
                controller.Detach();

            if (!controller.Grounded)
            {
                if (AccountForGravity)
                {
                    controller.Velocity += (Velocity + new Vector2(0.0f, controller.AirGravity)) * Time.fixedDeltaTime;
                }
                else
                {
                    controller.Velocity += Velocity * Time.fixedDeltaTime;
                }
            }
        }
    }
}
