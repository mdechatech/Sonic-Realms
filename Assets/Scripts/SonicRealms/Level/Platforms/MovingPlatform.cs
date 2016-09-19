using System.Collections.Generic;
using System.Linq;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Platforms
{
    /// <summary>
    /// Moves the player with the transform while the player is on it.
    /// </summary>
    [RequireComponent(typeof(PlatformTrigger))]
    public class MovingPlatform : ReactivePlatform
    {
        /// <summary>
        /// Whether the controller gets the horizontal speed it had on the platform after leaving it.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether the controller gets the horizontal speed it had on the platform after leaving it.")]
        public bool TransferMomentumX;

        /// <summary>
        /// Whether the controller gets the vertical speed it had on the platform after leaving it.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether the controller gets the vertical speed it had on the platform after leaving it.")]
        public bool TransferMomentumY;

        /// <summary>
        /// Whether the controller gets the ground speed it had on the platform after leaving it.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether the controller gets the speed it had on the platform after leaving it for another surface.")]
        public bool TransferMomentumGround;

        [HideInInspector]
        public Vector3 Velocity;

        public Dictionary<HedgehogController, Anchor> Anchors;

        public override void Awake()
        {
            base.Awake();
            Anchors = new Dictionary<HedgehogController, Anchor>();
        }
        /*
        public override bool IsSolid(TerrainCastHit hit)
        {
            // Prevent the player from sinking down onto moving platforms
            if (hit.Side != ControllerSide.Bottom) return true;
            if (!hit.Controller.Grounded) return true;
            if (hit.Controller.StandingOn(transform, true)) return true;
            return hit.Hit.fraction <
                   hit.Controller.LedgeClimbHeight/(hit.Controller.LedgeClimbHeight + hit.Controller.LedgeDropHeight);
        }
        */

        public override bool IsOnSurface(SurfaceCollision.Contact contact)
        {
            if (!base.IsOnSurface(contact))
                return false;

            // Make sure a player can only be on one moving platform at a time
            for (var i = 0; i < contact.Controller.Reactives.Count; ++i)
            {
                var reactive = contact.Controller.Reactives[i];

                if (reactive is MovingPlatform)
                    return reactive == this;
            }

            return true;
        }

        // Links the player to an object .
        public override void OnSurfaceEnter(SurfaceCollision collision)
        {
            if (collision.Controller == null) return;
            CreateAnchor(collision);
        }

        protected void FixedUpdate()
        {
            foreach (var pair in Anchors)
            {
                var anchor = pair.Value;
                var controller = pair.Key;

                var delta = anchor.Transform.position - anchor.PreviousPosition;
                anchor.PreviousPosition = anchor.Transform.position;

                if (!anchor.CalculatingPlayerDelta && anchor.PlayerDelta != Vector2.zero)
                {
                    delta -= (Vector3)anchor.PlayerDelta;
                }

                if (delta == Vector3.zero)
                    return;

                controller.transform.position += delta;
            }
        }
        
        // Removes the anchor associated with the hit.Source
        public override void OnSurfaceExit(SurfaceCollision collision)
        {
            var anchor = GetAnchor(collision.Controller);
            if (anchor == null)
                return;

            if (collision.Controller.Grounded && TransferMomentumGround)
            {
                collision.Controller.GroundVelocity += DMath.ScalarProjectionAbs(
                    (anchor.Transform.position - anchor.PreviousPosition)/Time.fixedDeltaTime,
                    collision.Latest.HitData.SurfaceAngle);
            }
            
            DestroyAnchor(collision);
        }

        protected Anchor CreateAnchor(SurfaceCollision info)
        {
            DestroyAnchor(info);

            var anchor = new Anchor {Transform = new GameObject().transform};
            anchor.Transform.position = info.Latest.HitData.Raycast.point;
            anchor.PreviousPosition = anchor.Transform.position;
            anchor.Transform.SetParent(info.Latest.HitData.Transform);

            Anchors.Add(info.Controller, anchor);

            info.Controller.AddCommandBuffer(StoreDelta, HedgehogController.BufferEvent.BeforeMovement);
            info.Controller.AddCommandBuffer(ApplyDelta, HedgehogController.BufferEvent.AfterMovement);

            return anchor;
        }

        protected void StoreDelta(HedgehogController player)
        {
            var anchor = GetAnchor(player);
            if (anchor == null)
            {
                player.RemoveCommandBuffer(StoreDelta, HedgehogController.BufferEvent.BeforeMovement);
                return;
            }

            anchor.PlayerDelta = player.transform.position;
            anchor.CalculatingPlayerDelta = true;
        }

        protected void ApplyDelta(HedgehogController player)
        {
            var anchor = GetAnchor(player);
            if (anchor == null)
            {
                player.RemoveCommandBuffer(ApplyDelta, HedgehogController.BufferEvent.AfterMovement);
                return;
            }

            if (!anchor.CalculatingPlayerDelta)
            {
                anchor.PlayerDelta = Vector2.zero;
                return;
            }

            anchor.PlayerDelta = (Vector2)player.transform.position - anchor.PlayerDelta;
            anchor.Transform.position += (Vector3) anchor.PlayerDelta;
            anchor.CalculatingPlayerDelta = false;
        }

        protected Anchor GetAnchor(HedgehogController player)
        {
            return Anchors.ContainsKey(player) ? Anchors[player] : null;
        }

        protected void DestroyAnchor(SurfaceCollision info)
        {
            Anchor anchor;
            if (!Anchors.TryGetValue(info.Controller, out anchor)) return;

            Anchors.Remove(info.Controller);
            Destroy(anchor.Transform.gameObject);

            info.Controller.RemoveCommandBuffer(StoreDelta, HedgehogController.BufferEvent.BeforeMovement);
            info.Controller.RemoveCommandBuffer(ApplyDelta, HedgehogController.BufferEvent.AfterMovement);
        }

        public class Anchor
        {
            public Transform Transform;
            public Vector3 PreviousPosition;
            public Vector2 PlayerDelta;
            public bool CalculatingPlayerDelta;
        }
    }
}
