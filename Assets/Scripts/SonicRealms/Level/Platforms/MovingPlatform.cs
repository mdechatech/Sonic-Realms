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

        private Dictionary<HedgehogController, MovingPlatformAnchor> _anchors; 

        public override void Reset()
        {
            base.Reset();
            TransferMomentumX = TransferMomentumY = TransferMomentumGround = false;
        }

        public override void Awake()
        {
            base.Awake();
            _anchors = new Dictionary<HedgehogController, MovingPlatformAnchor>();
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
            _anchors.Add(hit.Controller, CreateAnchor(hit));
        }

        // Updates the anchor associated with the hit.Source
        public override void OnSurfaceStay(TerrainCastHit hit)
        {
            if (!_anchors.ContainsKey(hit.Controller)) return;
            var anchor = _anchors[hit.Controller];
            if(anchor.transform.parent != hit.Hit.transform)
                anchor.transform.SetParent(hit.Hit.transform);
            anchor.TranslateController();
        }

        // Removes the anchor associated with the hit.Source
        public override void OnSurfaceExit(TerrainCastHit hit)
        {
            if (!_anchors.ContainsKey(hit.Controller))
                return;

            var anchor = _anchors[hit.Controller];
            var velocity = (Vector2)anchor.DeltaPosition/Time.fixedDeltaTime;

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

            _anchors.Remove(hit.Controller);
            Destroy(anchor.gameObject);
        }
    }
}
