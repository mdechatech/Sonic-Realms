using System.Collections.Generic;
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
    [AddComponentMenu("Hedgehog/Platforms/Moving Platform Tracker")]
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

        public override bool IsSolid(TerrainCastHit hit)
        {
            // Prevent the player from sinking down onto moving platforms
            if (hit.Side != ControllerSide.Bottom) return true;
            if (!hit.Controller.Grounded) return true;
            if (hit.Controller.StandingOn(transform, true)) return true;
            return hit.Hit.fraction <
                   hit.Controller.LedgeClimbHeight/(hit.Controller.LedgeClimbHeight + hit.Controller.LedgeDropHeight);
        }

        // Links the player to an object .
        public override void OnSurfaceEnter(TerrainCastHit hit)
        {
            if (hit.Controller == null) return;
            CreateAnchor(hit);
        }

        // Updates the anchor associated with the hit.Source
        public override void OnSurfaceStay(TerrainCastHit hit)
        {
            var anchor = GetAnchor(hit.Controller);
            if (anchor == null) return;

            var delta = anchor.Transform.position - anchor.PreviousPosition;
            anchor.PreviousPosition = anchor.Transform.position;

            if (!anchor.CalculatingPlayerDelta && anchor.PlayerDelta != Vector2.zero)
                delta -= (Vector3)anchor.PlayerDelta;
            if (delta == Vector3.zero) return;
            
            hit.Controller.transform.position += delta;
        }

        // Removes the anchor associated with the hit.Source
        public override void OnSurfaceExit(TerrainCastHit hit)
        {
            DestroyAnchor(hit);
        }

        protected Anchor CreateAnchor(TerrainCastHit hit)
        {
            DestroyAnchor(hit);

            var anchor = new Anchor {Transform = new GameObject().transform};
            anchor.Transform.position = hit.Hit.point;
            anchor.PreviousPosition = anchor.Transform.position;
            anchor.Transform.SetParent(transform);

            Anchors.Add(hit.Controller, anchor);

            hit.Controller.AddCommandBuffer(StoreDelta, HedgehogController.BufferEvent.BeforeMovement);
            hit.Controller.AddCommandBuffer(ApplyDelta, HedgehogController.BufferEvent.AfterMovement);

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

        protected void DestroyAnchor(TerrainCastHit hit)
        {
            Anchor anchor;
            if (!Anchors.TryGetValue(hit.Controller, out anchor)) return;

            Anchors.Remove(hit.Controller);
            Destroy(anchor.Transform.gameObject);

            hit.Controller.RemoveCommandBuffer(StoreDelta, HedgehogController.BufferEvent.BeforeMovement);
            hit.Controller.RemoveCommandBuffer(ApplyDelta, HedgehogController.BufferEvent.AfterMovement);
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
