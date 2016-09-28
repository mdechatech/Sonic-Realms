using System;
using System.Collections.Generic;
using System.Linq;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace SonicRealms.Level.Objects
{
    /// <summary>
    /// Manipulates a line of blocks based on the things standing on it. In addition to being able to emulate the bridge from Sonic,
    /// it provides various curves that can be changed to create more exotic behavior.
    /// </summary>
    [SelectionBase]
    public class Bridge : ReactivePlatform
    {
        [SerializeField]
        [FormerlySerializedAs("_baseBridgeLog")]
        [Tooltip("The prefab used for bridge segments. The prefab should have BoxCollider2D and MovingPlatform components " +
                 "attached.")]
        private BridgeLog _baseSegment;

        [Header("Size")]
        [Range(0, 255)]
        [SerializeField]
        [Tooltip("The number of segments in the bridge.")]
        private int _segmentCount;
#if UNITY_EDITOR
        private int _prevSegmentCount;
#endif
        [SerializeField]
        [Tooltip("The length of a bridge segment in units.")]
        private float _segmentLength;
#if UNITY_EDITOR
        private float _prevSegmentLength;
#endif
        [Header("Tension")]
        [SerializeField]
        [Tooltip("The time it takes for the bridge to move to its desired state, in seconds, as well as a curve for easing " +
                 "the animation")]
        private ScaledCurve _moveTime;

        [SerializeField]
        [Tooltip("The depression value that each segment can move to in units. This defines how much more the bridge will " +
                 "depress when a player is standing in the middle vs on an edge.")]
        private ScaledCurve _maxDepression;

        [SerializeField]
        [Tooltip("How much the depression of each segment falls off based on how far it is from the segment a player " +
                 "is standing on. This defines the shape of the bridge.")]
        private AnimationCurve _distanceFalloff;

        [SerializeField]
        [Tooltip("If false, distance falloff is calculated based using the current player's position as the midpoint.")]
        private bool _useAbsoluteMidpoint;

        [Header("Transformations")]
        [SerializeField]
        [Tooltip("Experimental. Whether bridge segments should rotate based on their depression amount.")]
        private bool _rotateSegments;

        [SerializeField, HideInInspector]
        private BridgeLog[] _segments;

        [SerializeField, HideInInspector]
        private BridgeLog[] _allSegments;

        private TensorMap _tensorMap;
        private Dictionary<HedgehogController, int> _players;

        /// <summary>
        /// The number of segments in the bridge. Can't be set in play mode.
        /// </summary>
        public int SegmentCount
        {
            get { return _segmentCount; }
            set
            {
                if (Application.isPlaying)
                {
                    Debug.LogError("Can't set SegmentCount in play mode.");
                    return;
                }

                _segmentCount = value;
#if UNITY_EDITOR
                UpdateSegmentCount(value);
                UpdateSegmentLength();
                UpdateSegmentPositions();
#endif
            }
        }

        /// <summary>
        /// The length of a bridge segment in units.
        /// </summary>
        public float SegmentLength
        {
            get { return _segmentLength; }
            set
            {
                _segmentLength = value;
                UpdateSegmentLength();
                UpdateSegmentPositions();
            }
        }

        /// <summary>
        /// The time it takes for the bridge to move to its desired state, in seconds, as well as a curve for easing
        /// the animation.
        /// </summary>
        public ScaledCurve MoveTime
        {
            get { return _moveTime; }
            set { _moveTime = value; }
        }

        /// <summary>
        /// The depression value that each segment can move to in units. This defines how much more the bridge will
        /// depress when a player is standing in the middle vs on an edge.
        /// </summary>
        public ScaledCurve MaxDepression
        {
            get { return _maxDepression; }
            set
            {
                _maxDepression = value;
                UpdateSegmentDepressions();
            }
        }

        /// <summary>
        /// How much the depression of each segment falls off based on how far it is from the segment a player
        /// is standing on. This defines the shape of the bridge.
        /// </summary>
        public AnimationCurve DistanceFalloff
        {
            get { return _distanceFalloff; }
            set
            {
                _distanceFalloff = value;
                UpdateSegmentDepressions();
            }
        }

        /// <summary>
        /// If false, distance falloff is calculated based using the current player's position as the midpoint.
        /// </summary>
        public bool UseAbsoluteMidpoint
        {
            get { return _useAbsoluteMidpoint; }
            set
            {
                _useAbsoluteMidpoint = value;
                UpdateSegmentDepressions();
            }
        }

        /// <summary>
        /// Experimental. Whether bridge segments should rotate based on their depression amount.
        /// </summary>
        public bool RotateSegments
        {
            get { return _rotateSegments;}
            set
            {
                _rotateSegments = value;
                UpdateSegmentRotations();
                UpdateSegmentDepressions();
            }
        }

        public override void Reset()
        {
            base.Reset();

            _segmentLength = 0.16f;
#if UNITY_EDITOR
            ClearLogs();

            _prevSegmentCount = 0;

            UpdateSegmentCount(12);
            UpdateSegmentLength();
            UpdateSegmentPositions();
#else
            _segmentCount = 12;
#endif
            
            _moveTime = new ScaledCurve
            {
                Scale = 0.5f,

                Curve = new AnimationCurve(
                    new Keyframe(0, 0, 0, 2),
                    new Keyframe(1, 1, 0, 0)
                    )
            };

            _maxDepression = new ScaledCurve
            {
                Scale = 0.12f,

                Curve = new AnimationCurve(
                    new Keyframe(0, 0, 0, 2),
                    new Keyframe(0.5f, 1, 2, -2),
                    new Keyframe(1, 0, -2, 0)
                    )
            };

            _distanceFalloff = new AnimationCurve(
                new Keyframe(0, 1, 0, -4),
                new Keyframe(0.5f, 0, 0, 0),
                new Keyframe(1, 1, 4, 0)
                );
        }

        public override void Awake()
        {
            base.Awake();
            
            _tensorMap = new TensorMap(_segments);
            _tensorMap.StateChanged += TensorMapStateChanged;

            _players = new Dictionary<HedgehogController, int>();

            for (var i = 0; i < _segments.Length; ++i)
            {
                var segment = _segments[i];

                segment.RotateToNeighbors = _rotateSegments;

                if (i != 0)
                    segment.LeftSegment = _segments[i - 1];

                if (i != _segments.Length - 1)
                    segment.RightSegment = _segments[i + 1];
            }
        }

        public override void OnSurfaceEnter(SurfaceCollision collision)
        {
            if (_players.ContainsKey(collision.Controller))
                return;

            var segment = GetPlayerSegment(collision);

            _players.Add(collision.Controller, segment);
            _tensorMap.Increment(segment);
        }

        public override void OnSurfaceStay(SurfaceCollision collision)
        {
            if (!_players.ContainsKey(collision.Controller))
                return;

            var oldSegment = _players[collision.Controller];
            var newSegment = GetPlayerSegment(collision);
            if (oldSegment != newSegment)
            {
                _tensorMap.Decrement(oldSegment);
                _tensorMap.Increment(newSegment);

                _players[collision.Controller] = newSegment;
            }
        }

        public override void OnSurfaceExit(SurfaceCollision collision)
        {
            if (!_players.ContainsKey(collision.Controller))
                return;

            _tensorMap.Decrement(_players[collision.Controller]);
            _players.Remove(collision.Controller);
        }

        private int GetPlayerSegment(SurfaceCollision info)
        {
            var pos = transform.InverseTransformPoint(info.Controller.transform.position);
            /*
            if (pos.x > 0)
            {
                pos = transform.InverseTransformPoint(info.Controller.Sensors.BottomLeft.position);
            }
            else
            {
                pos = transform.InverseTransformPoint(info.Controller.Sensors.BottomRight.position);
            }
            */
            var totalLength = _segmentCount*_segmentLength;
            var halfSegment = _segmentLength*0.5f;
            
            return Mathf.Clamp((int) ((pos.x + totalLength*0.5f + halfSegment)/_segmentLength), 0, _segmentCount - 1);
        }

        private void TensorMapStateChanged()
        {
            UpdateSegmentDepressions();
        }

        private void UpdateSegmentDepressions()
        {
            if (!Application.isPlaying || _tensorMap == null || _segments == null)
                return;

            var targetDepressions = _tensorMap.GetDepressions(_maxDepression, _distanceFalloff, _useAbsoluteMidpoint);

            for (var i = 0; i < _segments.Length; ++i)
            {
                _segments[i].Depress(targetDepressions[i], _moveTime.Scale, _moveTime.Curve);
            }
        }

        private void UpdateSegmentRotations()
        {
            if (!Application.isPlaying || _segments == null)
                return;

            for (var i = 0; i < _segments.Length; ++i)
            {
                _segments[i].RotateToNeighbors = _rotateSegments;
                _segments[i].UpdateRotation();
            }
        }

        private void UpdateSegmentLength()
        {
            for (var i = 0; i < _segments.Length; ++i)
            {
                _segments[i].Length = _segmentLength;
            }
        }

        private void UpdateSegmentPositions()
        {
            for (var i = 0; i < _segments.Length; ++i)
            {
                var segment = _segments[i];

                var x = i * _segmentLength - (_segments.Length * _segmentLength * 0.5f);

                segment.transform.localPosition = new Vector3(
                    x,
                    0,
                    segment.transform.localPosition.z);
            }
        }

        #region Editor Helpers
#if UNITY_EDITOR
        protected void OnValidate()
        {
            if (_segmentCount < 0)
                _segmentCount = 0;

            if (_prevSegmentCount != _segmentCount)
            {
                if(!Application.isPlaying)
                {
                    UpdateSegmentCount(_segmentCount);
                    UpdateSegmentLength();
                    UpdateSegmentPositions();
                }
            }

            if (_prevSegmentLength != _segmentLength)
            {
                _prevSegmentLength = _segmentLength;
                UpdateSegmentLength();
                UpdateSegmentPositions();
            }

            if (Application.isPlaying)
            {
                UpdateSegmentRotations();
                UpdateSegmentDepressions();
            }
        }

        private BridgeLog CreateLog(int number)
        {
            var log = Instantiate(_baseSegment);
            log.name = "Log " + number;
            log.transform.SetParent(transform);
            log.transform.localPosition = Vector3.zero;
            return log;
        }

        private void ClearLogs()
        {
            for (var i = transform.childCount - 1; i >= 0; --i)
            {
                var childTransform = transform.GetChild(i);

                if (childTransform.GetComponent<BridgeLog>())
                    DestroyImmediate(childTransform.gameObject);
            }
        }

        private void ShowSegment(BridgeLog segment)
        {
            segment.gameObject.SetActive(true);
        }

        private void HideSegment(BridgeLog segment)
        {
            segment.gameObject.SetActive(false);
        }

        private void UpdateSegmentCount(int newSegments)
        {
            if (newSegments == _prevSegmentCount)
                return;

            if (newSegments > _prevSegmentCount)
            {
                Array.Resize(ref _segments, newSegments);

                var i = _prevSegmentCount;

                for (; i < newSegments && i < _allSegments.Length; ++i)
                {
                    var inactiveLog = _allSegments[i];
                    ShowSegment(inactiveLog);
                    _segments[i] = inactiveLog;
                }

                if (newSegments > _allSegments.Length)
                {
                    Array.Resize(ref _allSegments, newSegments);

                    for (; i < newSegments; ++i)
                    {
                        _segments[i] = _allSegments[i] = CreateLog(i);
                    }
                }
            }
            else if (newSegments < _prevSegmentCount)
            {
                for (var i = newSegments; i < _prevSegmentCount && i < _segments.Length; ++i)
                {
                    HideSegment(_segments[i]);
                }

                Array.Resize(ref _segments, newSegments);
            }
            
            _segmentCount = _prevSegmentCount = newSegments;
        }
#endif
        #endregion

        public class TensorMap
        {
            private readonly int[] _tensors;

            public event Action StateChanged; 

            public TensorMap(IList<BridgeLog> segments)
            {
                _tensors = new int[segments.Count];
            }

            public void Increment(int segment)
            {
                if (segment < 0 || segment >= _tensors.Length)
                    return;

                ++_tensors[segment];

                if (StateChanged != null)
                    StateChanged();
            }

            public void Decrement(int segment)
            {
                if (segment < 0 || segment >= _tensors.Length)
                    return;

                --_tensors[segment];

                if (StateChanged != null)
                    StateChanged();
            }

            public float[] GetDepressions(ScaledCurve maxDepressions, AnimationCurve distanceFalloff, bool useAbsoluteMidpoint)
            {
                var farLeftIndex = -1;
                
                for (var i = 0; i < _tensors.Length; ++i)
                {
                    if (_tensors[i] > 0)
                    {
                        farLeftIndex = i;
                        break;
                    }
                }

                var farRightIndex = -1;

                for (var i = _tensors.Length - 1; i >= 0; --i)
                {
                    if (_tensors[i] > 0)
                    {
                        farRightIndex = i;
                        break;
                    }
                }

                var result = new float[_tensors.Length];

                if (farLeftIndex == -1)
                    return result;

                var leftDepression = maxDepressions.Scale*
                                     (maxDepressions.Curve.Evaluate((float) (farLeftIndex + 1)/(_tensors.Length + 1)));

                for (var i = farLeftIndex; i >= 0; --i)
                {
                    float t = 0;

                    if (useAbsoluteMidpoint)
                    {
                        t = (float) (i + 1)/(_tensors.Length + 1);
                    }
                    else
                    {
                        if (farLeftIndex > 0)
                            t = (i + 1)/(float) (farLeftIndex + 1)*0.5f;
                    }

                    result[i] = leftDepression*(1 - distanceFalloff.Evaluate(t));
                }

                var rightDepression = maxDepressions.Scale*
                                      (maxDepressions.Curve.Evaluate((float) (farRightIndex + 1)/(_tensors.Length + 1)));

                for (var i = farRightIndex; i < _tensors.Length; ++i)
                {
                    float t;

                    if (useAbsoluteMidpoint)
                        t = (float) (i + 1)/(_tensors.Length + 1);
                    else
                        t = 0.5f + (i - farRightIndex + 1)/(float) (_tensors.Length - farRightIndex + 1)*0.5f;

                    result[i] = rightDepression*(1 - distanceFalloff.Evaluate(t));
                }

                return result;
            }

            public override string ToString()
            {
                return string.Format("[{0}]", string.Join(", ", _tensors.Select(t => t.ToString()).ToArray()));
            }
        }
    }
}
