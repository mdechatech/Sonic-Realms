using UnityEditor;
using UnityEngine;

namespace SonicRealms.Core.Triggers.Editor
{
    public abstract class LimiterEditorBase : UnityEditor.Editor
    {
        protected virtual string LimiterBoolName { get { return "Limit{0}"; } }
        protected virtual string LimiterDetailsName { get { return "{0}Limiter"; } }
        
        protected virtual int BoolIndentLevel { get { return 0; } }
        protected virtual int DetailsIndentLevel { get { return 1; } }

        protected abstract string[] LimiterNames { get; }

        protected Limiter[] Limiters;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            for (var i = 0; i < Limiters.Length; ++i)
            {
                var limiter = Limiters[i];

                EditorGUI.indentLevel = BoolIndentLevel;
                EditorGUILayout.PropertyField(limiter.Boolean);

                if (targets.Length > 1 || limiter.Boolean.boolValue)
                {
                    EditorGUI.indentLevel = DetailsIndentLevel;
                    EditorGUILayout.PropertyField(limiter.Details, GetDetailsGUIContent(limiter.Details), true);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual GUIContent GetDetailsGUIContent(SerializedProperty detailsProperty)
        {
            return new GUIContent("Details");
        }

        protected virtual void OnEnable()
        {
            Limiters = new Limiter[LimiterNames.Length];
            for (var i = 0; i < LimiterNames.Length; ++i)
            {
                var limiterName = LimiterNames[i];

                Limiters[i] = new Limiter
                {
                    Boolean = serializedObject.FindProperty(string.Format(LimiterBoolName, limiterName)),
                    Details = serializedObject.FindProperty(string.Format(LimiterDetailsName, limiterName)),
                    BaseName = limiterName
                };
            }
        }

        protected class Limiter
        {
            public SerializedProperty Boolean;
            public SerializedProperty Details;
            public string BaseName;
        }
    }
}
