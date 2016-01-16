#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Hedgehog.Core.Utils
{
    [ExecuteInEditMode]
    public class Parallax : MonoBehaviour
    {
        public bool PixelSnap;
        public Camera Target;

        public Vector2 Center;

        public Vector2 BottomLeftWorld;
        public Vector2 BottomLeftParallax;

        public Vector2 TopRightWorld;
        public Vector2 TopRightParallax;

        public void Reset()
        {
            PixelSnap = true;
            Target = Camera.main;
            BottomLeftWorld = new Vector2(0.0f, 0.0f);
            BottomLeftParallax = new Vector2(0.0f, 0.0f);

            TopRightWorld = new Vector2(1.0f, 1.0f);
            TopRightParallax = new Vector2(0.1f, 1.0f);
        }

        public void Awake()
        {
            Target = Target ? Target : Camera.main;
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
            var worldSize = new Vector2(TopRightWorld.x - BottomLeftWorld.x, TopRightWorld.y - BottomLeftWorld.y);
            var ratioX = worldSize.x == 0.0f
                ? 0.0f
                : (target.x - BottomLeftWorld.x) /
                  (TopRightWorld.x - BottomLeftWorld.x);
            var ratioY = worldSize.y == 0.0f
                ? 0.0f
                : (target.y - BottomLeftWorld.y) /
                  (TopRightWorld.y - BottomLeftWorld.y);

            var offsetX = ratioX * (TopRightParallax.x - BottomLeftParallax.x) + BottomLeftParallax.x;
            var offsetY = ratioY * (TopRightParallax.y - BottomLeftParallax.y) + BottomLeftParallax.y;

            transform.position = new Vector2(
                DMath.Flip(target.x + offsetX, target.x),
                DMath.Flip(target.y + offsetY, target.y));

            if (PixelSnap)
                transform.position = new Vector3(
                    DMath.Round(transform.position.x, 0.01f),
                    DMath.Round(transform.position.y, 0.01f));
        }
    }
}
