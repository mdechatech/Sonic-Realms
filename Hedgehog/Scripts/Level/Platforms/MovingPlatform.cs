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
    public class MovingPlatform : MonoBehaviour
    {
        /// <summary>
        /// Whether the player gets the horizontal speed it had on the platform after leaving it.
        /// </summary>
        [SerializeField, Tooltip("Whether the player gets the horizontal speed it had on the platform after leaving it.")]
        public bool TransferMomentumX;

        /// <summary>
        /// Whether the player gets the vertical speed it had on the platform after leaving it.
        /// </summary>
        [SerializeField, Tooltip("Whether the player gets the horizontal speed it had on the platform after leaving it.")]
        public bool TransferMomentumY;

        [HideInInspector]
        public Vector3 Velocity;

        private PlatformTrigger _trigger;

        private List<HedgehogController> _linkedControllers;
        private List<MovingPlatformAnchor> _linkedAnchors; 
        private List<HedgehogController> _controllerRemoveQueue;

        public void Reset()
        {
            TransferMomentumX = TransferMomentumY = false;
        }

        public void Awake()
        {
            _linkedControllers = new List<HedgehogController>();
            _linkedAnchors = new List<MovingPlatformAnchor>();
            _controllerRemoveQueue = new List<HedgehogController>();
            _trigger = GetComponent<PlatformTrigger>();
        }

        public void OnEnable()
        {
            _trigger.OnSurfaceEnter.AddListener(Link);
            _trigger.OnSurfaceStay.AddListener(Translate);
            _trigger.OnSurfaceExit.AddListener(Unlink);
        }

        public void OnDisable()
        {
            _trigger.OnSurfaceEnter.RemoveListener(Link);
            _trigger.OnSurfaceStay.RemoveListener(Translate);
            _trigger.OnSurfaceExit.RemoveListener(Unlink);
        }

        public void FixedUpdate()
        {
            foreach (var controller in _controllerRemoveQueue)
            {
                var index = _linkedControllers.IndexOf(controller);
                if (index < 0) continue;

                _linkedControllers.RemoveAt(index);
                Destroy(_linkedAnchors[index].gameObject);
                _linkedAnchors.RemoveAt(index);
            }

            _controllerRemoveQueue.Clear();
        }

        public void OnDestroy()
        {
            _trigger.OnSurfaceEnter.RemoveListener(Link);
            _trigger.OnSurfaceStay.RemoveListener(Translate);
            _trigger.OnSurfaceExit.RemoveListener(Unlink);
        }

        private MovingPlatformAnchor CreateAnchor(HedgehogController controller, TerrainCastHit hit)
        {
            var anchor = new GameObject().AddComponent<MovingPlatformAnchor>();
            anchor.LinkController(controller, hit.Hit.point);
            anchor.name = controller.name + "'s Moving Platform Anchor";
            anchor.transform.SetParent(transform);
            // We will update the anchor manually and lazily
            anchor.enabled = false;

            return anchor;
        }

        // Attaches the controller to the platform through a MovingPlatformAnchor
        public void Link(HedgehogController controller, TerrainCastHit hit, SurfacePriority priority)
        {
            if (priority == SurfacePriority.Secondary || _linkedControllers.Contains(controller)) return;
           
            _linkedControllers.Add(controller);
            _linkedAnchors.Add(CreateAnchor(controller, hit));
        }

        // Updates the anchor associated with the controller
        public void Translate(HedgehogController controller, TerrainCastHit hit, SurfacePriority priority)
        {
            if (priority == SurfacePriority.Secondary) return;

            var index = _linkedControllers.IndexOf(controller);
            if (index < 0) return;
            _linkedAnchors[index].TranslateController();
        }

        // Removes the anchor associated with the controller
        public void Unlink(HedgehogController controller, TerrainCastHit hit, SurfacePriority priority)
        {
            if (!_linkedControllers.Contains(controller) || 
                (controller.PrimarySurface == transform || controller.SecondarySurface == transform)) return;

            var anchor = _linkedAnchors[_linkedControllers.IndexOf(controller)];
            var velocity = (Vector2) anchor.DeltaPosition/Time.fixedDeltaTime;
            
            controller.Velocity += new Vector2(
                TransferMomentumX ? velocity.x : 0.0f,
                TransferMomentumY ? velocity.y : 0.0f);
            
            _controllerRemoveQueue.Add(controller);
        }
    }
}
