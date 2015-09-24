using Hedgehog.Core.Actors;
using UnityEngine;

namespace Hedgehog.Level.Platforms
{
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
                transform.position = value;
                ResetDeltaPosition();
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
            if (transform.position != _previousPosition)
            {
                DeltaPosition = transform.position - _previousPosition;
                // TODO: Fix bug where controller rocks side-to-side on sloped platforms
                Controller.Translate(DeltaPosition);
                _previousPosition = transform.position;
            }

            if(Controller.Velocity != default(Vector2))
                PositionOverride += (Vector3)Controller.Velocity * Time.fixedDeltaTime;
        }

        public void LinkController(HedgehogController controller, Vector2 contactPoint)
        {
            Controller = controller;
            transform.position = contactPoint;
            ResetDeltaPosition();
        }

        public void LinkController(HedgehogController controller)
        {
            Controller = controller;
            transform.position = controller.transform.position;
            ResetDeltaPosition();
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
