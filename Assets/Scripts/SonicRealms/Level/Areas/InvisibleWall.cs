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

        public override bool IsInside(Hitbox hitbox)
        {
            return base.IsInside(hitbox) && Collider2D.bounds.Intersects(hitbox.Collider2D.bounds);
        }

        public override void OnAreaEnter(Hitbox hitbox)
        {
            float x = hitbox.Controller.transform.position.x,
                y = hitbox.Controller.transform.position.y;
            switch (PushDirection)
            {
                case Direction.Left:
                    x = Collider2D.bounds.min.x - hitbox.Collider2D.bounds.extents.x - DMath.Epsilon;
                    if (hitbox.Controller.Vx > 0f) hitbox.Controller.Vx = hitbox.Controller.GroundVelocity = 0f;
                    break;

                case Direction.Right:
                    x = Collider2D.bounds.max.x + hitbox.Collider2D.bounds.extents.x + DMath.Epsilon;
                    if (hitbox.Controller.Vx < 0f) hitbox.Controller.Vx = hitbox.Controller.GroundVelocity = 0f;
                    break;

                case Direction.Up:
                    y = Collider2D.bounds.max.y + hitbox.Collider2D.bounds.extents.y + DMath.Epsilon;
                    if (hitbox.Controller.Vy < 0f) hitbox.Controller.Vy = hitbox.Controller.GroundVelocity = 0f;
                    break;

                case Direction.Down:
                    y = Collider2D.bounds.min.y - hitbox.Collider2D.bounds.extents.y - DMath.Epsilon;
                    if (hitbox.Controller.Vy > 0f) hitbox.Controller.Vy = hitbox.Controller.GroundVelocity = 0f;
                    break;
            }

            hitbox.Controller.transform.position = new Vector3(x, y);
        }

        public override void OnAreaStay(Hitbox hitbox)
        {
            OnAreaEnter(hitbox);
        }
    }
}
