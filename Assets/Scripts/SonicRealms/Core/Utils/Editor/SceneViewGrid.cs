using UnityEditor;
using UnityEngine;

namespace SonicRealms.Core.Utils.Editor
{
    public class SceneViewGrid : EditorWindow
    {
        public static Color DivisionColor = Color.white;
        public static float DivisionWidth = 1.28f;
        public static float DivisionHeight = 1.28f;

        public static Color SubdivisionColor = Color.gray;
        public static int Subdivisions = 2;

        private static Camera _sceneCamera;

        [MenuItem("Hedgehog/Show Grid")]
        public static void ShowWindow()
        {
            GetWindow(typeof (SceneViewGrid), false, "Grid", true);
        }

        private void OnFocus()
        {
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
        }

        private void OnDestroy()
        {
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            var bounds = GetCameraBounds(sceneView.camera);

            var divisions = bounds.size.x/DivisionHeight + bounds.size.y/DivisionWidth;
            if (!(divisions > 0.0f && divisions < 500.0f)) return;

            var divisionColor = new Color(DivisionColor.r, DivisionColor.g, DivisionColor.b,
                    DivisionColor.a * (1.0f - Mathf.Clamp01(divisions / 500.0f)));

            var subdivisionColor = new Color(SubdivisionColor.r, SubdivisionColor.g, SubdivisionColor.b,
                    DivisionColor.a * (1.0f - Mathf.Clamp01(divisions / 500.0f)));

            for (var x = Mathf.Floor(bounds.min.x/DivisionWidth)*DivisionWidth; x < bounds.max.x; x += DivisionWidth)
            {
                Handles.color = divisionColor;

                Handles.DrawLine(new Vector3(x, bounds.max.y), new Vector3(x, bounds.min.y));

                if (Subdivisions < 2)
                    continue;

                Handles.color = subdivisionColor;

                for (var x2 = x - DivisionWidth/Subdivisions; x2 < x; x2 += DivisionWidth / Subdivisions)
                {
                    Handles.DrawLine(new Vector3(x2, bounds.max.y), new Vector3(x2, bounds.min.y));
                }
            }

            for (var y = Mathf.Floor(bounds.min.y / DivisionHeight) * DivisionHeight; y < bounds.max.y; y += DivisionWidth)
            {
                Handles.color = divisionColor;

                Handles.DrawLine(new Vector3(bounds.min.x, y), new Vector3(bounds.max.x, y));

                if (Subdivisions < 2)
                    continue;

                Handles.color = subdivisionColor;

                for (var y2 = y - DivisionHeight/Subdivisions; y2 < y; y2 += DivisionHeight / Subdivisions)
                {
                    Handles.DrawLine(new Vector3(bounds.min.x, y2), new Vector3(bounds.max.x, y2));
                }
            }
        }

        public static Bounds GetCameraBounds(Camera camera)
        {
            var min = camera.ViewportToWorldPoint(Vector3.zero);
            min = new Vector3(min.x, min.y, 0.0f);

            var max = camera.ViewportToWorldPoint(new Vector3(1.0f, 1.0f));
            max = new Vector3(max.x, max.y, 0.0f);

            return new Bounds((min + max)/2.0f, max - min);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Divisions", EditorStyles.boldLabel);
            DivisionColor = EditorGUILayout.ColorField("Color", DivisionColor);
            DivisionWidth = EditorGUILayout.FloatField("Width", DivisionWidth);
            DivisionHeight = EditorGUILayout.FloatField("Height", DivisionHeight);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Subdivisions", EditorStyles.boldLabel);
            Subdivisions = EditorGUILayout.IntField("Subdivisions", Subdivisions);
            SubdivisionColor = EditorGUILayout.ColorField("Color", SubdivisionColor);
            
        }
    }
}
