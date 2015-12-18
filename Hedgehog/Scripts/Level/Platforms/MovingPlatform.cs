using System.Collections.Generic;
using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level.Platforms
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

        private List<HedgehogController> _linkedControllers;
        private List<MovingPlatformAnchor> _linkedAnchors;

        public override void Reset()
        {
            base.Reset();
            TransferMomentumX = TransferMomentumY = TransferMomentumGround = false;
        }

        public override void Awake()
        {
            base.Awake();

            _linkedControllers = new List<HedgehogController>();
            _linkedAnchors = new List<MovingPlatformAnchor>();
        }

        private MovingPlatformAnchor CreateAnchor(TerrainCastHit hit)
        {
            var anchor = new GameObject().AddComponent<MovingPlatformAnchor>();
            anchor.LinkController(hit.Controller);
            anchor.name = hit.Controller.name + "'s Moving Platform Anchor";
            anchor.transform.SetParent(hit.Hit.transform);
            // We will update the anchor manually and lazily
            anchor.enabled = false;

            return anchor;
        }

        // Attaches the hit.Source to the platform through a MovingPlatformAnchor
        public override void OnSurfaceEnter(TerrainCastHit hit)
        {
            _linkedControllers.Add(hit.Controller);
            _linkedAnchors.Add(CreateAnchor(hit));
        }

        // Updates the anchor associated with the hit.Source
        public override void OnSurfaceStay(TerrainCastHit hit)
        {
            var anchor = _linkedAnchors[_linkedControllers.IndexOf(hit.Controller)];
            if(anchor.transform.parent != hit.Hit.transform)
                anchor.transform.SetParent(hit.Hit.transform);
            anchor.TranslateController();
        }

        // Removes the anchor associated with the hit.Source
        public override void OnSurfaceExit(TerrainCastHit hit)
        {
            var velocity = (Vector2) _linkedAnchors[_linkedControllers.IndexOf(hit.Controller)].DeltaPosition
                           /Time.fixedDeltaTime;

            if (hit.Controller.Grounded)
            {
                if(TransferMomentumGround) hit.Controller.PushOnGround(velocity);
            }
            else
            {
                hit.Controller.Velocity += new Vector2(
                    TransferMomentumX ? velocity.x : 0.0f,
                    TransferMomentumY ? velocity.y : 0.0f);
            }

            var index = _linkedControllers.IndexOf(hit.Controller);
            _linkedControllers.RemoveAt(index);
            Destroy(_linkedAnchors[index].gameObject);
            _linkedAnchors.RemoveAt(index);
        }
    }
}
