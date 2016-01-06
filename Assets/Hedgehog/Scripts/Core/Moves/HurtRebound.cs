using System;
using Hedgehog.Level.Areas;
using UnityEngine;

namespace Hedgehog.Core.Moves
{
    /// <summary>
    /// Locks player control and causes the controller to rebound opposite the direction of a threat.
    /// </summary>
    public class HurtRebound : Move
    {
        /// <summary>
        /// The position of the threat that caused the controller to rebound. Set this to control in which
        /// direction it rebounds.
        /// </summary>
        [NonSerialized]
        public Vector2 ThreatPosition;

        /// <summary>
        /// Positive speed at which the controller rebounds, in units per second.
        /// </summary>
        [Tooltip("Positive speed at which the controller rebounds, in units per second.")]
        public Vector2 ReboundSpeed;

        /// <summary>
        /// Positive speed at which the controller rebounds underwater, in units per second.
        /// </summary>
        [Tooltip("Positive speed at which the controller rebounds underwater, in units per second.")]
        public Vector2 UnderwaterReboundSpeed;

        public override void Reset()
        {
            base.Reset();
            ReboundSpeed = new Vector2(1.2f, 2.4f);
            UnderwaterReboundSpeed = new Vector2(1.2f, 0.8f);
        }

        public void OnAttach()
        {
            Controller.GroundVelocity = 0.0f;
            End();
        }

        public void OnPerformAirControl()
        {
            End();
        }

        public override void OnActiveEnter(State previousState)
        {
            Controller.OnAttach.AddListener(OnAttach);

            // Set speed based on underwater and position of the threat
            var speed = Controller.Inside<Water>() ? UnderwaterReboundSpeed : ReboundSpeed;
            if(Controller.transform.position.x < ThreatPosition.x)
                speed = new Vector2(-speed.x, speed.y);

            // Detach from ground and prevent landing back on it this frame
            Controller.Detach();
            Controller.IgnoreThisCollision();
            Controller.RelativeVelocity = speed;

            // Restrict control
            Controller.EndMove<AirControl>();

            // End this move if something else wants to restore control
            Controller.AirControl.OnActive.AddListener(OnPerformAirControl);
            
        }

        public override void OnActiveExit()
        {
            // End this move if something else wants to restore control
            Controller.OnAttach.RemoveListener(OnAttach);
            if (!Controller.Grounded && !Manager.IsActive<Death>())
                Controller.PerformMove<AirControl>();

            Controller.AirControl.OnActive.RemoveListener(OnPerformAirControl);
        }
    }
}
