using System.Collections;
using System.Collections.Generic;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace SonicRealms.Level.Platforms
{
    /// <summary>
    /// Moves the player with the transform while the player is on it.
    /// </summary>
    [RequireComponent(typeof(PlatformTrigger))]
    public class MovingPlatform : ReactivePlatform
    {
        public enum MovementMode
        {
            FromAnimator, // Animator keyframes
            NotFromAnimator // Everything else - unity events, scripts
        }

        /// <summary>
        /// What causes the platform to move. One may use the Animator or a combination of other sources
        /// (scripts, events, animator events) - but never both at the same time.
        /// </summary>
        public MovementMode MovementSource { get { return _movementSource; } set { _movementSource = value; } }

        /// <summary>
        /// If checked, the platform's momentum is transferred to the player when it falls off.
        /// </summary>
        public bool TransferAirborneMomentum { get { return _transferAirborneMomentum; } }

        /// <summary>
        /// If checked, the platform's momentum is transferred to the player if it moves to a new platform
        /// without becoming airborne.
        /// </summary>
        public bool TransferGroundedMomentum { get { return _transferGroundedMomentum; } }
        
        [SerializeField, EnumSelectionGrid]
        [Tooltip("What causes the platform to move. One may use either the Animator or a combination of " +
                 "other sources (scripts, events) - but never both at the same " +
                 "time.")]
        private MovementMode _movementSource;

        [SerializeField, FormerlySerializedAs("TransferMomentumX")]
        [Tooltip("If checked, the platform's momentum is transferred to the player when it falls off.")]
        private bool _transferAirborneMomentum;

        [SerializeField, FormerlySerializedAs("TransferMomentumGround")]
        [Tooltip("If checked, the platform's momentum is transferred to the player when it moves to a new platform " +
                 "without becoming airborne.")]
        private bool _transferGroundedMomentum;

        public Dictionary<HedgehogController, Anchor> Anchors;

        public override void Awake()
        {
            base.Awake();

            Anchors = new Dictionary<HedgehogController, Anchor>();
            StartCoroutine(Coroutine_LateFixedUpdate());
        }

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

        // Links the player to an object.
        public override void OnSurfaceEnter(SurfaceCollision collision)
        {
            if (collision.Controller != null)
                CreateAnchor(collision);
        }

        protected void ApplyNonAnimatorPositionChanges()
        {
            ApplyAnimatorPositionChanges(); // Same behavior for now
        }
        
        protected void ApplyAnimatorPositionChanges()
        {
            foreach (var pair in Anchors)
            {
                var anchor = pair.Value;
                var controller = pair.Key;

                var delta = anchor.Transform.position - anchor.PreviousAnimatorPosition;
                anchor.PreviousAnimatorPosition = anchor.Transform.position;

                if (!anchor.CalculatingPlayerDelta && anchor.PlayerDelta != Vector2.zero)
                {
                    delta -= (Vector3)anchor.PlayerDelta;
                }

                if (delta == Vector3.zero)
                    return;

                controller.transform.position += delta;
                anchor.PreviousDelta = delta;
            }
        }

        private IEnumerator Coroutine_LateFixedUpdate()
        {
            var wait = new WaitForFixedUpdate();

            while (true)
            {
                yield return wait;

                if (_movementSource == MovementMode.FromAnimator)
                    ApplyNonAnimatorPositionChanges();
            }
        }

        // Removes the anchor associated with the hit.Source
        public override void OnSurfaceExit(SurfaceCollision collision)
        {
            var anchor = GetAnchor(collision.Controller);
            if (anchor == null)
                return;

            if (_transferGroundedMomentum && collision.Controller.Grounded)
            {
                collision.Controller.GroundVelocity += SrMath.ScalarProjectionAbs(
                    anchor.PreviousDelta/Time.fixedDeltaTime,
                    collision.Latest.HitData.SurfaceAngle);
            }
            else if (_transferAirborneMomentum && !collision.Controller.Grounded)
            {
                collision.Controller.Velocity += anchor.PreviousDelta/Time.fixedDeltaTime;
            }

            DestroyAnchor(collision);
        }

        protected Anchor CreateAnchor(SurfaceCollision info)
        {
            DestroyAnchor(info);

            var anchor = new Anchor {Transform = new GameObject().transform};
            anchor.Transform.position = info.Latest.HitData.Raycast.point;
            anchor.PreviousAnimatorPosition = anchor.Transform.position;
            anchor.Transform.SetParent(info.Latest.HitData.Transform);

            Anchors.Add(info.Controller, anchor);

            // Account for player movement in order to separate it from platform movement
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

            if (_movementSource == MovementMode.NotFromAnimator)
                ApplyNonAnimatorPositionChanges();

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
            public Vector3 PreviousAnimatorPosition;
            public Vector2 PreviousDelta;
            public Vector2 PlayerDelta;
            public bool CalculatingPlayerDelta;
        }
    }
}
