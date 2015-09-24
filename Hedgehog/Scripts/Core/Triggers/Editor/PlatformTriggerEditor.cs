using UnityEditor;

namespace Hedgehog.Core.Triggers.Editor
{
    [CustomEditor(typeof(PlatformTrigger)), CanEditMultipleObjects]
    public class PlatformTriggerEditor : BaseTriggerEditor
    {
        protected SerializedProperty
            OnSurfaceEnterProperty,
            OnSurfaceStayProperty,
            OnSurfaceExitProperty;

        protected static bool ShowPlatformEvents
        {
            get { return EditorPrefs.GetBool("PlatformTriggerEditor.ShowPlatformEvents", false); }
            set { EditorPrefs.SetBool("PlatformTriggerEditor.ShowPlatformEvents", value); }
        }

        protected static bool ShowSurfaceEvents
        {
            get { return EditorPrefs.GetBool("PlatformTriggerEditor.ShowSurfaceEvents", false); }
            set { EditorPrefs.SetBool("PlatformTriggerEditor.ShowSurfaceEvents", value); }
        }

        public override void OnEnable()
        {
            base.OnEnable();

            OnSurfaceEnterProperty = serializedObject.FindProperty("OnSurfaceEnter");
            OnSurfaceStayProperty = serializedObject.FindProperty("OnSurfaceStay");
            OnSurfaceExitProperty = serializedObject.FindProperty("OnSurfaceExit");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            ShowPlatformEvents = EditorGUILayout.Foldout(ShowPlatformEvents, "Platform Events");
            if (ShowPlatformEvents)
            {
                ++EditorGUI.indentLevel;
                ShowSurfaceEvents = EditorGUILayout.Foldout(ShowSurfaceEvents, "Surface Events");
                if (ShowSurfaceEvents)
                {
                    EditorGUILayout.PropertyField(OnSurfaceEnterProperty);
                    EditorGUILayout.PropertyField(OnSurfaceStayProperty);
                    EditorGUILayout.PropertyField(OnSurfaceExitProperty);
                }
                --EditorGUI.indentLevel;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
