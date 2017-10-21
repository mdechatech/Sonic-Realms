using System.Collections;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Legacy.UI
{
    public class LinearItemCarouselArranger : ItemCarouselArranger
    {
        [SerializeField, Foldout("Placement")]
        private Transform _itemContainer;

        [SerializeField, Foldout("Placement")]
        private Transform _selectionCenter;

        [SerializeField, Foldout("Placement"), Range(-180, 180)]
        private float _direction;

        [SerializeField, Foldout("Placement")]
        private float _spacing;

        [SerializeField, Foldout("Placement")]
        private bool _worldPositionStays;

        [SerializeField, Foldout("Animation")]
        private float _transitionLength;

        [SerializeField, Foldout("Animation")]
        private AnimationCurve _easingCurve;

        private Transition _transition;

        private float _smoothSelectedIndex;

        public override void PlaceItem(GameObject item, int index)
        {
            item.transform.SetParent(_itemContainer, _worldPositionStays);
            Move(item, index);
        }

        public override void SnapSelection(int oldSelectedIndex, int newSelectedIndex)
        {
            MoveAll(newSelectedIndex);
        }

        public override IEnumerator ChangeSelection(int oldSelectedIndex, int newSelectedIndex)
        {
            if (_transitionLength <= 0)
            {
                SnapSelection(oldSelectedIndex, newSelectedIndex);
            }
            else
            {
                _transition = new Transition(oldSelectedIndex, newSelectedIndex, _transitionLength, _easingCurve);

                while (!_transition.IsDone)
                {
                    _transition.Time += Time.deltaTime;
                    MoveAll(_transition.Position);

                    yield return null;
                }
            }
        }

        protected void MoveAll(float smoothSelectedIndex)
        {
            var start = _selectionCenter.position -
                        (Vector3) SrMath.UnitVector(_direction*Mathf.Deg2Rad)*_spacing*smoothSelectedIndex;

            var end = _selectionCenter.position +
                      (Vector3) SrMath.UnitVector(_direction*Mathf.Deg2Rad)*_spacing*
                      (Carousel.ItemCount - smoothSelectedIndex - 1);

            for (var i = 0; i < Carousel.ItemCount; ++i)
            {
                var t = i/(float)(Mathf.Max(1, Carousel.ItemCount - 1));

                Carousel[i].transform.position = Vector3.Lerp(start, end, t);
            }

            _smoothSelectedIndex = smoothSelectedIndex;
        }

        protected void Move(GameObject gameObject, int index)
        {
            var start = _selectionCenter.position -
                        (Vector3) SrMath.UnitVector(_direction*Mathf.Deg2Rad)*_spacing*_smoothSelectedIndex;

            var end = _selectionCenter.position +
                      (Vector3) SrMath.UnitVector(_direction*Mathf.Deg2Rad)*_spacing*
                      (Carousel.ItemCount - _smoothSelectedIndex - 1);

            gameObject.transform.position = Vector3.Lerp(start, end,
                index/(float) (Mathf.Max(1, Carousel.ItemCount - 1)));
        }

        protected virtual void Reset()
        {
            _spacing = 1;

            _itemContainer = transform;
            _selectionCenter = transform;

            _transitionLength = 0.5f;
            _easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }

        protected virtual void Awake()
        {
            _itemContainer = _itemContainer ?? transform;
            _selectionCenter = _selectionCenter ?? transform;
        }

        protected override void Start()
        {
            base.Start();

            _smoothSelectedIndex = Carousel.SelectedIndex;
        }

        public class Transition
        {
            public readonly int From;
            public readonly int To;
            public readonly AnimationCurve EasingCurve;
            public readonly float Duration;

            public float Time { get { return _time; } set { _time = Mathf.Clamp(value, 0, Duration); } }

            public float Progress { get { return Time/Duration; } set { Time = value*Duration; } }

            public float Position { get { return Mathf.Lerp(From, To, EasingCurve.Evaluate(Progress)); } }

            public bool IsDone { get { return Progress >= 1; } }

            private float _time;

            public Transition(int from, int to, float duration, AnimationCurve easingCurve = null)
            {
                From = from;
                To = to;
                Duration = duration;

                EasingCurve = easingCurve ?? AnimationCurve.Linear(0, 0, 1, 1);
            }
        }
    }
}
