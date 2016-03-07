using SonicRealms.Core.Actors;
using UnityEngine;

namespace SonicRealms.Level.Platforms
{
    /// <summary>
    /// Used by MovingPlatform to track and apply changes in position to a controller.
    /// </summary>
    [AddComponentMenu("")]
    public class MovingPlatformAnchor : MonoBehaviour
    {
        [SerializeField]
        public HedgehogController Controller;

        private Vector3 _previousPosition;

        /// <summary>
        /// Use this to change the anchor's positon without applying the change to
        /// the controller.
        /// </summary>
        public Vector3 PositionOverride
        {
            get { return transform.position; }
            set
            {
                _previousPosition += value - transform.position;
                transform.position = value;
            }
        }

        /// <summary>
        /// Use this to change the anchor's local positon without applying the change to
        /// the controller.
        /// </summary>
        public Vector3 LocalPositionOverride
        {
            get { return transform.localPosition; }
            set
            {
                transform.localPosition = value;
                ResetDeltaPosition();
            }
        }

        public Vector3 DeltaPosition;

        public void FixedUpdate()
        {
            if(Controller != null) TranslateController();
        }

        public void TranslateController()
        {
            PositionOverride += (Vector3)Controller.Velocity * Time.fixedDeltaTime + Controller.QueuedTranslation;
            
            if (transform.position != _previousPosition)
            {
                DeltaPosition = transform.position - _previousPosition;
                Controller.transform.position += DeltaPosition;
                _previousPosition = transform.position;
            }
        }

        public void LinkController(HedgehogController controller, Vector2 contactPoint)
        {
            Controller = controller;
            transform.position = contactPoint;
            ResetDeltaPosition();
        }

        public void LinkController(HedgehogController controller)
        {
            LinkController(controller, controller.transform.position);
        }

        public void UnlinkController(Transform controller)
        {
            Controller = null;
            transform.SetParent(transform.root);
        }

        public void ResetDeltaPosition()
        {
            _previousPosition = transform.position;
        }
    }
}
