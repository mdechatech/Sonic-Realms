using UnityEditor;
using UnityEngine;

namespace Hedgehog.Core.Triggers.Editor
{
    [CustomEditor(typeof(BaseTrigger)), CanEditMultipleObjects]
    public class BaseTriggerEditor : UnityEditor.Editor
    {
        protected SerializedProperty
            TriggersFromChildrenProperty,
            OnEnterProperty,
            OnStayProperty,
            OnExitProperty;

        protected static bool ShowBaseEvents
        {
            get { return EditorPrefs.GetBool("BaseTriggerEditor.ShowBaseEvents", false); }
            set { EditorPrefs.SetBool("BaseTriggerEditor.ShowBaseEvents", value); }
        }

        public virtual void OnEnable()
        {
            TriggersFromChildrenProperty = serializedObject.FindProperty("TriggersFromChildren");
            OnEnterProperty = serializedObject.FindProperty("OnEnter");
            OnStayProperty = serializedObject.FindProperty("OnStay");
            OnExitProperty = serializedObject.FindProperty("OnExit");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(TriggersFromChildrenProperty, 
                new GUIContent("Trigger From Children", TriggersFromChildrenProperty.tooltip));

            ShowBaseEvents = EditorGUILayout.Foldout(ShowBaseEvents, "Base Events");
            if (ShowBaseEvents)
            {
                EditorGUILayout.PropertyField(OnEnterProperty);
                EditorGUILayout.PropertyField(OnStayProperty);
                EditorGUILayout.PropertyField(OnExitProperty);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
