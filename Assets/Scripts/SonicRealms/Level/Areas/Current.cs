using SonicRealms.Core.Actors;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Areas
{
    /// <summary>
    /// Pushes the controller in a certain direction in the air and on the ground. It has finer control 
    /// than updraft, but it is also more complicated.
    /// </summary>
    public class Current : ReactiveArea
    {
        /// <summary>
        /// The acceleration of the controller toward the target veloicty.
        /// </summary>
        [SerializeField] public float Power;

        /// <summary>
        /// The velocity to accelerate the controller to.
        /// </summary>
        [SerializeField] public Vector2 TargetVelocity;

        /// <summary>
        /// Whether to push the controller on the ground. If true, the controller will move
        /// based on its surface angle vs. the current's target velocity.
        /// </summary>
        [SerializeField] public bool WorkOnGround;

        /// <summary>
        /// Whether to detach controllers from the ground when they enter the current.
        /// </summary>
        [SerializeField] public bool PullOffGround;

        /// <summary>
        /// Whether to push the controller while it is in the air.
        /// </summary>
        [SerializeField] public bool WorkInAir;

        /// <summary>
        /// Whether to automatically add controller gravity to the current's velocity.
        /// </summary>
        [SerializeField] public bool AccountForGravity;

        /// <summary>
        /// Whether to automatically add controller friction to the current's velocity.
        /// </summary>
        [SerializeField] public bool AccountForFriction;

        public override void Reset()
        {
            Power = 25.0f;
            TargetVelocity = new Vector2(2.5f, 2.5f);
            WorkOnGround = true;
            PullOffGround = false;
            WorkInAir = true;
            AccountForGravity = true;
            AccountForFriction = true;
        }

        public override void OnAreaStay(Hitbox hitbox)
        {
            var controller = hitbox.Controller;

            if(PullOffGround && controller.Grounded)
                controller.Detach();

            if (WorkOnGround && controller.Grounded)
            {
                var oldVelocity = controller.GroundVelocity;

                var surfaceAngle = controller.SurfaceAngle*Mathf.Deg2Rad;
                var targetAngle = surfaceAngle - DMath.Angle(TargetVelocity);
                var targetMagnitude = TargetVelocity.magnitude;

                var result = Mathf.Cos(targetAngle)*Power*Time.fixedDeltaTime;

                var limit = Mathf.Cos(targetAngle)*targetMagnitude;

                if (Mathf.Abs(oldVelocity) > Mathf.Abs(limit))
                {
                    return;
                }

                if (AccountForFriction && !DMath.Equalsf(result))
                    result += controller.GroundFriction*Mathf.Sign(result)*Time.fixedDeltaTime;

                controller.GroundVelocity += result;

                if (oldVelocity < limit && controller.GroundVelocity > limit)
                    controller.GroundVelocity = limit;

            } else if (WorkInAir && !controller.Grounded)
            {
                var oldVelocity = controller.Velocity;

                var targetAngle = DMath.Angle(TargetVelocity);
                var targetMagnitude = TargetVelocity.magnitude;
                var normalized = (TargetVelocity - oldVelocity).normalized;

                var result = normalized*Power*Time.fixedDeltaTime;
                if (AccountForGravity) result += Vector2.up*controller.AirGravity*Time.fixedDeltaTime;

                if (Mathf.Abs(DMath.ShortestArc(DMath.Angle(result), targetAngle)) > DMath.HalfPi)
                {
                    controller.Velocity = TargetVelocity;
                    return;
                }

                controller.Velocity += result;

                if (oldVelocity.magnitude < targetMagnitude && controller.Velocity.magnitude > targetMagnitude)
                    controller.Velocity = TargetVelocity;
            }
        }
    }
}
