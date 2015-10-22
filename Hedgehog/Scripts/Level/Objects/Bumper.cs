using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level.Objects
{
    /// <summary>
    /// Bumpers like the ones in Spring Yard and Carnival Night.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class Bumper : ReactivePlatform
    {
        /// <summary>
        /// The velocity at which the controller bounces off.
        /// </summary>
        [SerializeField]
        [Tooltip("The velocity at which the controller bounces off.")]
        public float Velocity;

        /// <summary>
        /// Whether to make the controller launch straight in the direction of the spring. For example, a spring
        /// facing up with this set to true will launch you straight up, no horizontal movement.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to make the controller launch straight in the direction of the spring. For example, " +
                 "a spring facing up with this checked will launch you straight up, no horizontal movement.")]
        public bool ForceDirection;

        /// <summary>
        /// Whether to lock the controller's horizontal controller after being hit.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to lock the controller's horizontal control after being hit.")]
        public bool LockControl;

        private CircleCollider2D _circleCollider2D;

        public void Reset()
        {
            Velocity = 10.0f;
            ForceDirection = false;
            LockControl = true;
        }

        public override void Start()
        {
            base.Start();
            _circleCollider2D = GetComponent<CircleCollider2D>();
            AutoActivate = false;
        }

        public override void OnPlatformEnter(HedgehogController controller, TerrainCastHit hit)
        {
            controller.Detach();
            if(LockControl) controller.LockHorizontal();
           
            var normal = DMath.Angle(hit.Hit.point - (Vector2)_circleCollider2D.bounds.center);

            if (ForceDirection)
            {
                controller.Velocity = DMath.AngleToVector(normal) * Velocity;
            }
            else
            {
                controller.Velocity = new Vector2(controller.Velocity.x*Mathf.Abs(Mathf.Sin(normal)), 
                    controller.Velocity.y*Mathf.Abs(Mathf.Cos(normal)));
                controller.Velocity += DMath.AngleToVector(normal)*Velocity;
            }

            TriggerObject();
        }
    }
}
