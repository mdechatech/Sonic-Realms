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

        public void Reset()
        {
            TransferMomentumX = TransferMomentumY = TransferMomentumGround = false;
        }

        public override void Awake()
        {
            base.Awake();

            _linkedControllers = new List<HedgehogController>();
            _linkedAnchors = new List<MovingPlatformAnchor>();
        }

        private MovingPlatformAnchor CreateAnchor(HedgehogController controller, TerrainCastHit hit)
        {
            var anchor = new GameObject().AddComponent<MovingPlatformAnchor>();
            anchor.LinkController(controller);
            anchor.name = controller.name + "'s Moving Platform Anchor";
            anchor.transform.SetParent(transform);
            // We will update the anchor manually and lazily
            anchor.enabled = false;

            return anchor;
        }

        // Attaches the controller to the platform through a MovingPlatformAnchor
        public override void OnSurfaceEnter(HedgehogController controller, TerrainCastHit hit)
        {
            _linkedControllers.Add(controller);
            _linkedAnchors.Add(CreateAnchor(controller, hit));
        }

        // Updates the anchor associated with the controller
        public override void OnSurfaceStay(HedgehogController controller, TerrainCastHit hit)
        {
            _linkedAnchors[_linkedControllers.IndexOf(controller)].TranslateController();
        }

        // Removes the anchor associated with the controller
        public override void OnSurfaceExit(HedgehogController controller, TerrainCastHit hit)
        {
            var velocity = (Vector2) _linkedAnchors[_linkedControllers.IndexOf(controller)].DeltaPosition
                           /Time.fixedDeltaTime;

            if (controller.Grounded)
            {
                if(TransferMomentumGround) controller.AddGroundVelocity(velocity);
            }
            else
            {
                controller.Velocity += new Vector2(
                    TransferMomentumX ? velocity.x : 0.0f,
                    TransferMomentumY ? velocity.y : 0.0f);
            }

            var index = _linkedControllers.IndexOf(controller);
            _linkedControllers.RemoveAt(index);
            Destroy(_linkedAnchors[index].gameObject);
            _linkedAnchors.RemoveAt(index);
        }
    }
}
