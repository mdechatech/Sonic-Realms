using System.Collections;
using UnityEngine;

namespace SonicRealms.Core.Utils
{
    public class SnapPosition : MonoBehaviour
    {
        public Vector2 Interval;

        private Vector3 _position;
        private Transform _transform;

        private Coroutine _snapper;
        private WaitForEndOfFrame _waiter;

        public void Reset()
        {
            Interval = new Vector2(0.01f, 0.01f);
        }

        public void Awake()
        {
            _transform = GetComponent<Transform>();
            _snapper = StartCoroutine(SnapUpdate());
        }

        public void OnEnable()
        {
            if (_snapper == null) _snapper = StartCoroutine(SnapUpdate());
        }

        public void OnDisable()
        {
            if (_snapper != null)
            {
                StopCoroutine(_snapper);
                _snapper = null;
            }
        }

        protected IEnumerator SnapUpdate()
        {
            _waiter = new WaitForEndOfFrame();
            while (true)
            {
                yield return _waiter;
                _transform.position = _position;
            }
        }
        
        public void LateUpdate()
        {
            _position = _transform.position;
            _transform.position = new Vector3(
                DMath.Round(_position.x, Interval.x),
                DMath.Round(_position.y, Interval.y),
                _position.z);
        }
    }
}
