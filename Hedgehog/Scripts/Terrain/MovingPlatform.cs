using System.Collections.Generic;
using Hedgehog.Actors;
using UnityEditor.Events;
using UnityEngine;

namespace Hedgehog.Terrain
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

            _trigger = GetComponent<PlatformTrigger>();
            UnityEventTools.AddPersistentListener(_trigger.OnSurfaceEnter, Link);
            UnityEventTools.AddPersistentListener(_trigger.OnSurfaceStay, Translate);
            UnityEventTools.AddPersistentListener(_trigger.OnSurfaceExit, Unlink);
        }

        public void Awake()
        {
            _linkedControllers = new List<HedgehogController>();
            _linkedAnchors = new List<MovingPlatformAnchor>();
            _controllerRemoveQueue = new List<HedgehogController>();
        }

        public void Start()
        {
            _trigger = GetComponent<PlatformTrigger>();
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

        public void Link(HedgehogController controller, TerrainCastHit hit, SurfacePriority priority)
        {
            if (priority == SurfacePriority.Secondary || _linkedControllers.Contains(controller)) return;
           
            _linkedControllers.Add(controller);
            _linkedAnchors.Add(CreateAnchor(controller, hit));
        }

        public void Translate(HedgehogController controller, TerrainCastHit hit, SurfacePriority priority)
        {
            if (priority == SurfacePriority.Secondary) return;

            var index = _linkedControllers.IndexOf(controller);
            if (index < 0) return;
            _linkedAnchors[index].TranslateController();
        }

        public void Unlink(HedgehogController controller, TerrainCastHit hit, SurfacePriority priority)
        {
            if (!_linkedControllers.Contains(controller) || 
                (controller.PrimarySurface == transform || controller.SecondarySurface == transform)) return;

            var perSecondVelocity = (Vector2) Velocity/Time.fixedDeltaTime;
            controller.Velocity += new Vector2(TransferMomentumX ? perSecondVelocity.x : 0.0f,
                TransferMomentumY ? perSecondVelocity.y : 0.0f);

            _controllerRemoveQueue.Add(controller);
        }
    }
}
