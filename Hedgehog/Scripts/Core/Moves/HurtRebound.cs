using System;
using UnityEngine;

namespace Hedgehog.Core.Moves
{
    public class HurtRebound : Move
    {
        [NonSerialized]
        public Vector2 ThreatPosition;

        public Vector2 ReboundSpeed;

        public override void Reset()
        {
            base.Reset();
            ReboundSpeed = new Vector2(1.2f, 2.4f);
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

            var speed = ReboundSpeed;
            if(Controller.transform.position.x < ThreatPosition.x)
                speed = new Vector2(-speed.x, speed.y);

            Controller.Detach();
            Controller.Velocity = speed;

            Controller.EndMove<AirControl>();
            Controller.AirControl.OnActive.AddListener(OnPerformAirControl);
        }

        public override void OnActiveExit()
        {
            Controller.OnAttach.RemoveListener(OnAttach);
            if (!Controller.Grounded)
                Controller.PerformMove<AirControl>();

            Controller.AirControl.OnActive.RemoveListener(OnPerformAirControl);
        }
    }
}
