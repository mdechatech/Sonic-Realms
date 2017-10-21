using System.Collections.Generic;
using System.Linq;
using SonicRealms.Core.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SonicRealms.Level
{
    [SelectionBase]
    [ExecuteInEditMode]
    [RequireComponent(typeof(BoxCollider2D))]
    public class TileObject : MonoBehaviour
    {
        [SerializeField]
        private GameObject _baseTile;

        [SerializeField, HideInInspector]
        private GameObject _prevBaseTile;

        [SerializeField]
        private Vector2 _tileSize;

        [SerializeField, HideInInspector]
        private Vector2 _prevTileSize;

        [SerializeField]
        private Vector2 _size;

        [SerializeField, HideInInspector]
        private Vector2 _prevSize;

        [SerializeField, HideInInspector]
        private Vector3 _prevScale;

        [SerializeField]
        private Vector2 _snapInterval;

        [SerializeField, HideInInspector]
        private Vector2 _prevSnapInterval;

        [SerializeField, HideInInspector]
        private List<GameObject> _tiles;

        [SerializeField]
        private bool _ignoreXScale;

        [SerializeField]
        private bool _ignoreYScale;

        protected void Reset()
        {
            for (var i = transform.childCount - 1; i >= 0; --i)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            _tileSize = new Vector2(0.16f, 0.16f);
            _size = new Vector3(0.641f, 0.641f);

            var box = GetComponent<BoxCollider2D>();
            box.enabled = false;
            box.hideFlags = HideFlags.HideInInspector;
        }

        protected void Awake()
        {

        }

#if UNITY_EDITOR
        protected void Update()
        {
            OnValidate();
        }
#endif

        protected void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider2D>();

            var bounds = new Bounds(box.offset, box.size);

            Gizmos.DrawLine(
                transform.TransformPoint(new Vector2(bounds.min.x, bounds.max.y)),
                transform.TransformPoint(bounds.max));

            Gizmos.DrawLine(
                transform.TransformPoint(bounds.max),
                transform.TransformPoint(new Vector2(bounds.max.x, bounds.min.y)));

            Gizmos.DrawLine(
                transform.TransformPoint(new Vector2(bounds.max.x, bounds.min.y)),
                transform.TransformPoint(bounds.min));

            Gizmos.DrawLine(
                transform.TransformPoint(bounds.min),
                transform.TransformPoint(new Vector2(bounds.min.x, bounds.max.y)));
        }

        protected void OnValidate()
        {
            if (_prevScale != transform.lossyScale)
            {
                _prevSize = transform.lossyScale;

                RearrangeTiles();
            }

            if (_prevBaseTile != _baseTile)
            {
                _prevBaseTile = _baseTile;

                RecreateTiles();
            }

            if (_prevTileSize != _tileSize)
            {
                _tileSize = new Vector2(Mathf.Max(0.1f, _tileSize.x), Mathf.Max(0.1f, _tileSize.y));

                _prevTileSize = _tileSize;

                RearrangeTiles();
            }

            if (_prevSize != _size)
            {
                _size = new Vector2(Mathf.Max(0.1f, _size.x), Mathf.Max(0.1f, _size.y));

                GetComponent<BoxCollider2D>().size = _prevSize = _size;

                RearrangeTiles();
            }

            if (_prevSnapInterval != _snapInterval)
            {
                _snapInterval = new Vector2(
                    Mathf.Max(0, _snapInterval.x),
                    Mathf.Max(0, _snapInterval.y));

                _prevSnapInterval = _snapInterval;

                RearrangeTiles();
            }

            CheckCorruption();
        }

        private void CheckCorruption()
        {
            if (_baseTile != null && _tiles.Count != transform.childCount)
                RecreateTiles();
        }

        [ContextMenu("Recreate Tiles")]
        private void RecreateTiles()
        {
            if (_tiles.Count > 0)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    var children = new Transform[transform.childCount];

                    for (var i = 0; i < children.Length; ++i)
                        children[i] = transform.GetChild(i);

                    EditorApplication.delayCall += () =>
                    {
                        for (var i = 0; i < children.Length; ++i)
                        {
                            if (children[i])
                                DestroyImmediate(children[i].gameObject);
                        }
                    };

                    _tiles.Clear();
                }
#endif
            }

            if (_baseTile)
            {
                RearrangeTiles();
            }
        }

        private void RearrangeTiles()
        {
            if (!_baseTile)
                return;

            var box = GetComponent<BoxCollider2D>();

            float xMin, xMax, yMin, yMax;

            if (_ignoreXScale)
            {
                xMin = (box.offset.x - box.size.x*0.5f) + _tileSize.x*0.5f;
                xMax = (box.offset.x + box.size.x*0.5f) - _tileSize.x*0.5f;
            } else
            {
                xMin = (box.offset.x - box.size.x*0.5f)*transform.lossyScale.x + _tileSize.x*0.5f;
                xMax = (box.offset.x + box.size.x*0.5f)*transform.lossyScale.x - _tileSize.x*0.5f;
            }

            if (_ignoreYScale)
            {
                yMin = (box.offset.y - box.size.y * 0.5f) + _tileSize.y * 0.5f;
                yMax = (box.offset.y + box.size.y * 0.5f) - _tileSize.y * 0.5f;
            }
            else
            {
                yMin = (box.offset.y - box.size.y * 0.5f) * transform.lossyScale.y + _tileSize.y * 0.5f;
                yMax = (box.offset.y + box.size.y * 0.5f) * transform.lossyScale.y - _tileSize.y * 0.5f;
            }

            var columns = Mathf.CeilToInt((xMax - xMin)/_tileSize.x);
            var rows = Mathf.CeilToInt((yMax - yMin)/_tileSize.y);

            var count = columns*rows;

            var i = 0;
            for ( ; i < _tiles.Count || i < count; ++i)
            {
                GameObject tile;

                if (i >= count && i < _tiles.Count || (i < _tiles.Count && !_tiles[i]))
                {
                    tile = _tiles[i];
#if UNITY_EDITOR
                    EditorApplication.delayCall += () =>
                    {
                        DestroyImmediate(tile);
                    };
#else
                    Destroy(tile);
#endif
                    _tiles.RemoveAt(i--);

                    continue;
                }
                else if (i < _tiles.Count)
                {
                    tile = _tiles[i];
                }
                else
                {
                    var scale = _baseTile.transform.localScale;
                    _tiles.Add(tile = (GameObject)Instantiate(_baseTile, transform, false));
                    tile.transform.localScale = scale;
                    tile.name = i.ToString();
                }

                var x = (xMin + _tileSize.x*(i/rows))/transform.lossyScale.x;
                var y = (yMin + _tileSize.y*(i%rows))/transform.lossyScale.y;

                if (_snapInterval.x > 0)
                {
                    x = SrMath.Round(x, _snapInterval.x);
                }

                if (_snapInterval.y > 0)
                {
                    y = SrMath.Round(y, _snapInterval.y);
                }

                tile.transform.localPosition = new Vector3(x, y);

                tile.transform.localScale = new Vector2(
                    1/transform.lossyScale.x,
                    1/transform.lossyScale.y);
            }
        }
    }
}
