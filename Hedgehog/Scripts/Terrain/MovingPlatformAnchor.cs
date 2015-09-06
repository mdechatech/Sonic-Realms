using System;
using Hedgehog.Actors;
using UnityEngine;

namespace Hedgehog.Terrain
{
    public class MovingPlatformAnchor : MonoBehaviour
    {
        public event EventHandler<EventArgs> Destroyed;
        public event EventHandler<EventArgs> Moved; 

        [HideInInspector]
        public HedgehogController Controller;

        public Transform Platform;

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

        public Vector3 DeltaPosition
        {
            get
            {
                if (Platform == null) return default(Vector3);
                Debug.Log(transform.position - _previousPosition);
                return transform.position - _previousPosition;
            }
        }

        public void FixedUpdate()
        {
            if (Platform != null)
            {
                if (transform.position != _previousPosition)
                {
                    if (Moved != null) Moved(this, EventArgs.Empty);
                    _previousPosition = transform.position;
                }
            }
        }

        public void LinkController(HedgehogController controller)
        {
            Controller = controller;
            ResetDeltaPosition();
        }

        public void UnlinkController(HedgehogController controller)
        {
            Controller = null;
            transform.SetParent(transform.root);
        }

        public void LinkPlatform(Vector2 position, Transform terrain)
        {
            transform.SetParent(terrain);
            transform.position = position;

            Platform = terrain;
            ResetDeltaPosition();
        }

        public void UnlinkPlatform()
        {
            Platform = null;
            if (Controller != null)
            {
                transform.SetParent(Controller.transform);
            }
            else
            {
                transform.SetParent(transform.root);
            }

            ResetDeltaPosition();
        }

        public void ResetDeltaPosition()
        {
            if (Platform != null) _previousPosition = transform.position;
            else _previousPosition = default(Vector3);
        }

        public void OnDestroy()
        {
            if (Destroyed != null) Destroyed(this, EventArgs.Empty);
        }
    }
}
