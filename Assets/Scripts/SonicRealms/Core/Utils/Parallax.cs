#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SonicRealms.Core.Utils
{
    /// <summary>
    /// 2D parallax.
    /// </summary>
    [ExecuteInEditMode]
    public class Parallax : MonoBehaviour
    {
        /// <summary>
        /// The camera to follow.
        /// </summary>
        [Tooltip("The camera to follow.")]
        public Camera Target;

        /// <summary>
        /// Interval at which to round the parallax, in units.
        /// </summary>
        [Tooltip("Interval at which to round the parallax, in units.")]
        public Vector2 SnapInterval;

        /// <summary>
        /// At this position, the parallax will always be centered on the camera.
        /// </summary>
        [Space]
        [Tooltip("At this position, the parallax will always be centered on the camera.")]
        public Vector2 Center;

        /// <summary>
        /// The parallax factor - the higher it is, the farther away it is from the camera. If factor is one, there
        /// is no parallax behavior. If Factor is zero, the parallax is glued to the camera (infinite depth).
        /// </summary>
        [Tooltip("The parallax factor - the higher it is, the farther away it is from the camera. If Factor is one, there " +
                 "is no parallax behavior. If Factor is zero, the parallax is glued to the camera (infinite depth).")]
        public Vector2 Factor;

        public void Reset()
        {
            Target = Camera.main;
            SnapInterval = new Vector2(0.01f, 0.01f);

            Center = Vector2.zero;
            Factor = new Vector2(1f, 1f);
        }

        public void Awake()
        {
            Target = Target ? Target : Camera.main;
        }

        public void Start()
        {
#if UNITY_EDITOR
            if (Application.isPlaying) return;
#endif
            if(Target != null) MoveTo(Target.transform.position);
        }

#if UNITY_EDITOR
        public void OnEnable()
        {
            if (Application.isPlaying) return;
            EditorApplication.update += Update;
        }

        public void OnDisable()
        {
            if (Application.isPlaying) return;
            EditorApplication.update -= Update;
        }
#endif

        public void FixedUpdate()
        {
            if (!Target) return;
            MoveTo(Target.transform.position);
        }
#if UNITY_EDITOR
        public void Update()
        {
            if (Application.isPlaying) return;
            if (SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.camera != null)
                MoveTo(SceneView.lastActiveSceneView.camera.transform.position);
        }
#endif
        public void MoveTo(Vector3 target)
        {
            float x = target.x, y = target.y;

            x += (target.x - Center.x)*GetRatio(Factor.x);
            y += (target.y - Center.y)*GetRatio(Factor.y);

            if (SnapInterval != default(Vector2))
            {
                x = DMath.Round(x, SnapInterval.x);
                y = DMath.Round(y, SnapInterval.y);
            }

            transform.position = new Vector2(x, y);
        }

        protected float GetRatio(float factor)
        {
            if (factor == 0f) return 0f;
            return -((1f - factor)/factor)-1f;
        }
    }
}
