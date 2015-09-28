using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level.Areas
{
    /// <summary>
    /// Pushes the controller upward in the air. Emulates the fans in Hydrocity Act 2.
    /// </summary>
    public class Updraft : ReactiveArea
    {
        /// <summary>
        /// The velocity at which the controller is pushed upward.
        /// </summary>
        [SerializeField] public float Velocity;

        /// <summary>
        /// Whether to detach controllers from the ground when they enter the current.
        /// </summary>
        [SerializeField]
        public bool PullOffGround;

        /// <summary>
        /// Whether to automatically add controller gravity to the updraft's velocity.
        /// </summary>
        [SerializeField] public bool AccountForGravity;

        private Collider2D _collider2D;

        public void Reset()
        {
            Velocity = 2.5f;
            AccountForGravity = true;
            PullOffGround = true;
        }

        public override void Awake()
        {
            base.Awake();
            _collider2D = GetComponent<Collider2D>();
        }

        public override void OnAreaStay(HedgehogController controller)
        {
            if (controller.Grounded && PullOffGround)
                controller.Detach();

            if (!controller.Grounded)
            {
                if ((!_collider2D.OverlapPoint(controller.SensorMiddleLeft.position) &&
                    _collider2D.OverlapPoint(controller.SensorBottomLeft.position)) ||
                    (!_collider2D.OverlapPoint(controller.SensorMiddleRight.position)) &&
                    _collider2D.OverlapPoint(controller.SensorBottomRight.position))
                {
                    controller.Velocity = new Vector2(controller.Velocity.x,
                        (AccountForGravity ? controller.AirGravity * Time.fixedDeltaTime : 0.0f));
                }
                else
                {
                    controller.Velocity = new Vector2(controller.Velocity.x,
                    (AccountForGravity ? controller.AirGravity * Time.fixedDeltaTime : 0.0f) + Velocity);
                }
            }
        }

        public override void OnAreaExit(HedgehogController controller)
        {
            if (!controller.Grounded && controller.Velocity.y <
                (Velocity + (AccountForGravity ? controller.AirGravity*Time.fixedDeltaTime : 0.0f))*2.0f)
            {
                controller.Velocity = new Vector2(controller.Velocity.x, 0.0f);
            }
        }
    }
}
