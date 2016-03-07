using System.Collections;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Actors
{
    public class BubbleMovement : MonoBehaviour
    {
        /// <summary>
        /// How quickly the bubble moves vertically in units per second.
        /// </summary>
        [Foldout("Movement")]
        [Tooltip("How quickly the bubble moves vertically in units per second.")]
        public float Buoyancy;

        /// <summary>
        /// How far to wiggle horizontally, in units.
        /// </summary>
        [Space, Foldout("Movement")]
        [Tooltip("How far to wiggle horizontally, in units.")]
        public float WiggleLength;

        /// <summary>
        /// How long it takes to wiggle once, in seconds.
        /// </summary>
        [Foldout("Movement")]
        [Tooltip("How long it takes to wiggle once, in seconds.")]
        public float WiggleTime;

        /// <summary>
        /// The layers on which water are located.
        /// </summary>
        [Foldout("Collision")]
        [Tooltip("The layers on which water are located.")]
        public LayerMask WaterLayer;

        /// <summary>
        /// How long to wait between checking to see if the bubble is underwater. The bubble will
        /// pop once it leaves the water.
        /// </summary>
        [Foldout("Collision")]
        [Tooltip("How long to wait between checking to see if the bubble is underwater. The bubble will " +
                 "pop once it leaves the water.")]
        public float WaterCheckInterval;

        /// <summary>
        /// Whether to destroy the game object once the bubble pops.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Whether to destroy the game object once the bubble pops.")]
        public bool DestroyOnPop;

        [Space, Foldout("Animation")]
        public Animator Animator;

        /// <summary>
        /// Name of an Animator trigger set when the bubble pops.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator trigger set when the bubble pops.")]
        public string PopTrigger;
        protected int PopTriggerHash;

        private Coroutine _waterChecker;
        protected float WiggleTimer;

        public void Reset()
        {
            Buoyancy = 0.5f;

            WiggleLength = 0.05f;
            WiggleTime = 0.5f;

            WaterCheckInterval = 0.5f;
            WaterLayer = 1 << LayerMask.NameToLayer("Water");

            Animator = GetComponent<Animator>();
        }

        public void Awake()
        {
            WiggleTimer = Random.Range(0, WiggleTime);
            Animator = Animator ? Animator : GetComponent<Animator>();
            PopTriggerHash = Animator.StringToHash(PopTrigger);
        }

        public void Start()
        {
            _waterChecker = StartCoroutine(CheckWater());
        }

        protected IEnumerator CheckWater()
        {
            yield return new WaitForSeconds(WaterCheckInterval);

            while (true)
            {
                if (!Physics2D.OverlapPoint(transform.position, WaterLayer)) Pop();
                yield return new WaitForSeconds(WaterCheckInterval);
            }
        }

        public void Pop()
        {
            if (_waterChecker != null) StopCoroutine(_waterChecker);
            if (Animator != null && PopTriggerHash != 0) Animator.SetTrigger(PopTriggerHash);
            if (DestroyOnPop) Destroy(gameObject);
        }

        public void OnEnable()
        {
            if (_waterChecker == null) _waterChecker = StartCoroutine(CheckWater());
        }

        public void OnDisable()
        {
            if (_waterChecker != null)
            {
                StopCoroutine(_waterChecker);
                _waterChecker = null;
            }
        }

        public void OnDestroy()
        {
            if (_waterChecker != null) StopCoroutine(_waterChecker);
        }

        public void FixedUpdate()
        {
            float x = transform.position.x, y = transform.position.y;

            WiggleTimer += Time.deltaTime;
            WiggleTimer %= WiggleTime;

            x += (WiggleTimer/WiggleTime < 0.5f ? WiggleLength : -WiggleLength)*Time.deltaTime/WiggleTime;

            y += Buoyancy*Time.deltaTime;

            transform.position = new Vector3(x, y);
        }
    }
}
