using System.Collections;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Actors
{
    /// <summary>
    /// Spawns bubbles while Sonic is underwater.
    /// </summary>
    public class SonicBubbleGenerator : MonoBehaviour
    {
        /// <summary>
        /// The player's breath meter.
        /// </summary>
        [Tooltip("The player's breath meter.")]
        public BreathMeter BreathMeter;

        /// <summary>
        /// The bubble to make copies of.
        /// </summary>
        [Tooltip("The bubble to make copies of.")]
        public GameObject Bubble;

        /// <summary>
        /// How many bubbles to spawn each second.
        /// </summary>
        [Tooltip("How many bubbles to spawn each second.")]
        public float SpawnRate;

        /// <summary>
        /// The bubble to spawn when drowned.
        /// </summary>
        [Foldout("Drowned Bubbles")]
        [Tooltip("The bubble to spawn when drowned.")]
        public GameObject MediumBubble;

        /// <summary>
        /// How often to spawn bubbles while drowned.
        /// </summary>
        [Foldout("Drowned Bubbles")]
        [Tooltip("How often to spawn bubbles while drowned.")]
        public float DrownedSpawnRate;

        /// <summary>
        /// How often to spawn medium bubbles while drowned.
        /// </summary>
        [Foldout("Drowned Bubbles")]
        [Tooltip("How often to spawn medium bubbles while drowned.")]
        public float MediumBubbleSpawnRate;

        private Coroutine _spawner;
        private Coroutine _drownedSpawner;

        public void Reset()
        {
            BreathMeter = GetComponentInParent<BreathMeter>();
            SpawnRate = 1f;
            DrownedSpawnRate = 7f;
            MediumBubbleSpawnRate = 2f;
        }

        public void Start()
        {
            BreathMeter.OnCanBreathe.AddListener(OnCanBreathe);
            BreathMeter.OnCannotBreathe.AddListener(OnCannotBreathe);
            BreathMeter.OnDrown.AddListener(OnDrown);
        }

        protected void OnCanBreathe()
        {
            if (_spawner != null)
            {
                StopCoroutine(_spawner);
                _spawner = null;
            }
        }

        protected void OnCannotBreathe()
        {
            if (_spawner == null) _spawner = StartCoroutine(SpawnBubbles());
        }

        protected void OnDrown()
        {
            SpawnRate = DrownedSpawnRate;
            if (_spawner != null) StopCoroutine(_spawner);
            _spawner = StartCoroutine(SpawnBubbles());
            if (_drownedSpawner == null) _drownedSpawner = StartCoroutine(SpawnDrownedBubbles());
        }

        protected IEnumerator SpawnBubbles()
        {
            yield return new WaitForSeconds(Random.Range(0f, 1f/SpawnRate));
            while (true)
            {
                Instantiate(Bubble).transform.position = transform.position;
                yield return new WaitForSeconds(Random.Range(0.5f/SpawnRate, 1.5f/SpawnRate));
            }
        }

        protected IEnumerator SpawnDrownedBubbles()
        {
            yield return new WaitForSeconds(Random.Range(0f, 1f/MediumBubbleSpawnRate));
            while (true)
            {
                Instantiate(MediumBubble).transform.position = transform.position;
                yield return new WaitForSeconds(Random.Range(0.5f/MediumBubbleSpawnRate, 1.5f/MediumBubbleSpawnRate));
            }
        }
    }
}
