using System.Linq;
using SonicRealms.Core.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.Core.Triggers.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ActivatePlatform))]
    public class ActivatePlatformEditor : UnityEditor.Editor
    {
        private static readonly string[] LimiterNames =
        {
            "SurfaceAngle",
            "Velocity",
            "GroundSpeed",
            "AirSpeed",
            "Moves",
            "Powerups"
        };

        private static Limiter[] Limiters;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            RealmsEditorUtility.DrawProperties(serializedObject,
                "WhenColliding",
                "WhenOnSurface");

            for (var i = 0; i < Limiters.Length; ++i)
            {
                var limiter = Limiters[i];

                EditorGUILayout.PropertyField(limiter.Boolean);
                if (targets.Length > 1 || limiter.Boolean.boolValue)
                {
                    EditorGUILayout.PropertyField(limiter.Details, new GUIContent("Details"), true);
                }
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        protected void OnEnable()
        {
            Limiters = new Limiter[LimiterNames.Length];
            for (var i = 0; i < LimiterNames.Length; ++i)
            {
                Limiters[i] = new Limiter
                {
                    Boolean = serializedObject.FindProperty("Limit" + LimiterNames[i]),
                    Details = serializedObject.FindProperty("Limit" + LimiterNames[i] + "Details")
                };
            }
        }

        class Limiter
        {
            public SerializedProperty Boolean;
            public SerializedProperty Details;
        }
    }
}
