using SonicRealms.Core.Utils;
using SonicRealms.Core.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.Level.Platforms.Movers.Editor
{
    [CustomEditor(typeof(SwingPlatform))]
    public class SwingPlatformEditor : UnityEditor.Editor
    {
        private SwingPlatform _instance;
        public void OnEnable()
        {
            _instance = target as SwingPlatform;
        }

        public override void OnInspectorGUI()
        {
            if (_instance == null) return;

            serializedObject.Update();

            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                "Duration",
                "CurrentTime",
                "PositionCurve",
                "OnComplete",
                "Pivot",
                "Radius",
                "MidAngle",
                "Range");

            serializedObject.ApplyModifiedProperties();
        }

        public void OnSceneGUI()
        {
            Handles.color = Color.gray;
            Handles.DrawLine(_instance.Pivot,
                _instance.Pivot + DMath.AngleToVector(_instance.MidAngle * Mathf.Deg2Rad) * _instance.Radius);

            Handles.color = Color.white;
            Handles.DrawWireArc(_instance.Pivot,
                _instance.transform.forward,
                DMath.AngleToVector((_instance.MidAngle - _instance.Range/2.0f)*Mathf.Deg2Rad),
                _instance.Range,
                _instance.Radius);
        }
    }
}
