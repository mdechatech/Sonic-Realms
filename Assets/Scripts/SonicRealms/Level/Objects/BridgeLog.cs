using SonicRealms.Core.Triggers;
using UnityEngine;

namespace SonicRealms.Level.Objects
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class BridgeLog : ReactivePlatform
    {
        private Tween _tween;

        private float _tweenTimer;

        private BoxCollider2D _collider;

        public bool RotateToNeighbors;

        public BridgeLog LeftSegment;

        public BridgeLog RightSegment;

        public float Depression
        {
            get { return -transform.localPosition.y; }
            set
            {
                transform.localPosition = new Vector3(
                    transform.localPosition.x,
                    -value,
                    transform.localPosition.z);
            }
        }

        public float Rotation
        {
            get { return transform.localEulerAngles.z; }
            set
            {
                transform.localEulerAngles = new Vector3(
                    transform.localEulerAngles.x,
                    transform.localEulerAngles.y,
                    value);
            }
        }

        public float Length
        {
            get
            {
#if UNITY_EDITOR
                if(!Application.isPlaying)
                    return GetComponent<BoxCollider2D>().size.x;
#endif
                if (_collider == null)
                    _collider = GetComponent<BoxCollider2D>();

                return _collider.size.x;
            }

            set
            {
#if UNITY_EDITOR
                if(!Application.isPlaying)
                {
                    GetComponent<BoxCollider2D>().size = new Vector2(
                        value,
                        GetComponent<BoxCollider2D>().size.y);

                    return;
                }
#endif
                if (_collider == null)
                    _collider = GetComponent<BoxCollider2D>();

                _collider.size = new Vector2(value, _collider.size.y);
            }
        }

        public void Depress(float amount, float time, AnimationCurve curve)
        {
            _tween = new Tween
            {
                DepressionStart = Depression,
                DepressionEnd = amount,
                MoveTime = time,
                Curve = curve
            };

            _tweenTimer = 0;
        }

        public override void Awake()
        {
            base.Awake();
            _collider = GetComponent<BoxCollider2D>();
        }

        protected void FixedUpdate()
        {
            if (_tween != null && _tweenTimer < _tween.MoveTime)
            {
                _tweenTimer += Time.fixedDeltaTime;
                Depression = _tween.GetDepression(_tweenTimer);

                UpdateRotation();
            }
        }

        public void UpdateRotation()
        {
            if (RotateToNeighbors)
            {
                Vector3 vector;

                if (RightSegment && LeftSegment)
                    vector = RightSegment.transform.localPosition - LeftSegment.transform.localPosition;
                else if (RightSegment)
                    vector = RightSegment.transform.localPosition - transform.localPosition;
                else if (LeftSegment)
                    vector = transform.localPosition - LeftSegment.transform.localPosition;
                else
                    vector = Vector3.right;

                Rotation = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
            }
            else
            {
                Rotation = 0;
            }
        }

        public class Tween
        {
            public float DepressionStart;
            public float DepressionEnd;

            public float MoveTime;
            public AnimationCurve Curve;

            public float GetDepression(float time)
            {
                return Mathf.Lerp(DepressionStart, DepressionEnd, Curve.Evaluate(time/MoveTime));
            }
        }
    }
}
