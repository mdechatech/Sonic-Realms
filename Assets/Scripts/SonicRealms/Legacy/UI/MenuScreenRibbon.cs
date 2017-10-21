using System.Collections;
using System.Collections.Generic;
using SonicRealms.Core.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace SonicRealms.Legacy.UI
{
    public class MenuScreenRibbon : MonoBehaviour
    {
        [SerializeField]
        private Image _baseUnderlay;

        [SerializeField, Range(0.5f, 5)]
        private float _underlayScale;

        [SerializeField]
        private Gradient _imageGradient;

        [SerializeField]
        private Mask _mask;

        [SerializeField]
        private Transform _start;

        [SerializeField]
        private Transform _end;

        [SerializeField, MinMaxSlider(0, 3), Tooltip("test")]
        private Vector2 _spawnInterval;

        [SerializeField, MinMaxSlider(0, 2)]
        private Vector2 _speed;

        private List<Underlay> _underlays;

        private float _spawnTimer;

        private Coroutine _spawnTimerCoroutine;

        private void Reset()
        {
            _spawnInterval = new Vector2(0.1f, 0.2f);
            _speed = new Vector2(0.3f, 0.7f);
        }

        private void Awake()
        {
            _underlays = new List<Underlay>();
        }

        private void Start()
        {
            _spawnTimerCoroutine = StartCoroutine(RunSpawnTimer());
        }

        private void Update()
        {
            var difference = _end.position - _start.position;
            var length = difference.magnitude;

            for (var i = _underlays.Count - 1; i >= 0; --i)
            {
                var underlay = _underlays[i];

                var progress = underlay.Speed*Time.deltaTime;

                underlay.Progress += progress;
                underlay.Transform.position = Vector3.Lerp(_start.position, _end.position, underlay.Progress);

                if (underlay.Progress > 1)
                {
                    _underlays.RemoveAt(i);
                    Destroy(underlay.Transform.gameObject);
                }
            }
        }

        private void OnEnable()
        {
            if (_spawnTimerCoroutine == null)
                _spawnTimerCoroutine = StartCoroutine(RunSpawnTimer());
        }

        private void OnDisable()
        {
            if (_spawnTimerCoroutine != null)
            {
                StopCoroutine(_spawnTimerCoroutine);
                _spawnTimerCoroutine = null;
            }
        }

        private void SpawnUnderlay()
        {
            var underlayImage = Instantiate(_baseUnderlay);
            underlayImage.transform.SetParent(_mask.transform, false);
            underlayImage.transform.position = _start.position;
            underlayImage.transform.localScale = new Vector3(1, 1, 1)*_underlayScale;

            underlayImage.color = _imageGradient.Evaluate(Random.value);

            var underlay = new Underlay(underlayImage.transform, underlayImage);
            underlay.Speed = Random.Range(_speed.x, _speed.y);

            _underlays.Add(underlay);
        }

        private IEnumerator RunSpawnTimer()
        {
            while (true)
            {
                SpawnUnderlay();
                yield return new WaitForSeconds(Random.Range(_spawnInterval.x, _spawnInterval.y));
            }
        }

        private class Underlay
        {
            public readonly Transform Transform;
            public readonly Image Image;
            public float Speed;
            public float Progress;

            public Underlay(Transform transform, Image image, float speed = 0)
            {
                Transform = transform;
                Image = image;
                Speed = speed;
            }
        }
    }
}
