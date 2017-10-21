using SonicRealms.Core.Actors;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Areas
{
    /// <summary>
    /// Pushes the player back without allowing actions that would normally occur with walls, such as
    /// pushing and climbing. Does not support rotation.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class InvisibleWall : ReactiveArea
    {
        public enum Direction
        {
            Left, Right, Up, Down
        }

        protected BoxCollider2D Collider2D;

        /// <summary>
        /// The direction in which the wall pushes against the player.
        /// </summary>
        [Tooltip("The direction in which the wall pushes against the player.")]
        public Direction PushDirection;

        public override void Reset()
        {
            base.Reset();
            PushDirection = Direction.Left;
        }

        public override void Awake()
        {
            base.Awake();
            Collider2D = GetComponent<BoxCollider2D>();
        }

        public override bool CanTouch(AreaCollision.Contact contact)
        {
            return base.CanTouch(contact) && Collider2D.bounds.Intersects(contact.Hitbox.Collider.bounds);
        }

        public override void OnAreaEnter(AreaCollision collision)
        {
            var controller = collision.Controller;
            var collider2D = collision.Latest.Hitbox.Collider;

            float x = controller.transform.position.x,
                y = controller.transform.position.y;
            switch (PushDirection)
            {
                case Direction.Left:
                    x = Collider2D.bounds.min.x - collider2D.bounds.extents.x - SrMath.Epsilon;
                    if (collision.Controller.Vx > 0f) collision.Controller.Vx = collision.Controller.GroundVelocity = 0f;
                    break;

                case Direction.Right:
                    x = Collider2D.bounds.max.x + collider2D.bounds.extents.x + SrMath.Epsilon;
                    if (collision.Controller.Vx < 0f) collision.Controller.Vx = collision.Controller.GroundVelocity = 0f;
                    break;

                case Direction.Up:
                    y = Collider2D.bounds.max.y + collider2D.bounds.extents.y + SrMath.Epsilon;
                    if (collision.Controller.Vy < 0f) collision.Controller.Vy = collision.Controller.GroundVelocity = 0f;
                    break;

                case Direction.Down:
                    y = Collider2D.bounds.min.y - collider2D.bounds.extents.y - SrMath.Epsilon;
                    if (collision.Controller.Vy > 0f) collision.Controller.Vy = collision.Controller.GroundVelocity = 0f;
                    break;
            }

            collision.Controller.transform.position = new Vector3(x, y);
        }

        public override void OnAreaStay(AreaCollision collision)
        {
            OnAreaEnter(collision);
        }
    }
}
