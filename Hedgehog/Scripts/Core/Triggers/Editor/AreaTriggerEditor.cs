using UnityEditor;

namespace Hedgehog.Core.Triggers.Editor
{
    [CustomEditor(typeof(AreaTrigger)), CanEditMultipleObjects]
    public class AreaTriggerEditor : BaseTriggerEditor
    {
        protected SerializedProperty
            IgnoreLayersProperty,
            OnAreaEnterProperty,
            OnAreaStayProperty,
            OnAreaExitProperty;

        protected static bool ShowAreaEvents
        {
            get { return EditorPrefs.GetBool("AreaTriggerEditor.ShowAreaEvents", false); }
            set { EditorPrefs.SetBool("AreaTriggerEditor.ShowAreaEvents", value); }
        }

        public override void OnEnable()
        {
            base.OnEnable();

            IgnoreLayersProperty = serializedObject.FindProperty("IgnoreLayers");
            OnAreaEnterProperty = serializedObject.FindProperty("OnAreaEnter");
            OnAreaStayProperty = serializedObject.FindProperty("OnAreaStay");
            OnAreaExitProperty = serializedObject.FindProperty("OnAreaExit");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(IgnoreLayersProperty);
            EditorGUILayout.PropertyField(TriggerFromChildrenProperty);

            ShowAreaEvents = EditorGUILayout.Foldout(ShowAreaEvents, "Area Events");
            if (ShowAreaEvents)
            {
                EditorGUILayout.PropertyField(OnAreaEnterProperty);
                EditorGUILayout.PropertyField(OnAreaStayProperty);
                EditorGUILayout.PropertyField(OnAreaExitProperty);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
