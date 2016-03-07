using System.Linq;
using SonicRealms.Core.Utils.Editor;
using UnityEditor;

namespace SonicRealms.Core.Triggers.Editor
{
    [CustomEditor(typeof(ActivatePlatform))]
    public class ActivatePlatformEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                "WhenColliding",
                "WhenOnSurface",
                "LimitAngle");

            if (targets.Count() == 1)
            {
                var limitAngle = serializedObject.FindProperty("LimitAngle").boolValue;
                if (limitAngle)
                {
                    HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                        "RelativeToRotation",
                        "SurfaceAngleMin",
                        "SurfaceAngleMax");
                }
            }
            else
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                        "RelativeToRotation",
                        "SurfaceAngleMin",
                        "SurfaceAngleMax");
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
