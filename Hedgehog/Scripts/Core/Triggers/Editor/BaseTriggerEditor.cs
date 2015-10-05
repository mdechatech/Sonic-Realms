using UnityEditor;
using UnityEngine;

namespace Hedgehog.Core.Triggers.Editor
{
    [CustomEditor(typeof(BaseTrigger)), CanEditMultipleObjects]
    public class BaseTriggerEditor : UnityEditor.Editor
    {
        protected SerializedProperty
            TriggerFromChildrenProperty,
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
            TriggerFromChildrenProperty = serializedObject.FindProperty("TriggerFromChildren");
            OnEnterProperty = serializedObject.FindProperty("OnEnter");
            OnStayProperty = serializedObject.FindProperty("OnStay");
            OnExitProperty = serializedObject.FindProperty("OnExit");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(TriggerFromChildrenProperty, 
                new GUIContent("Trigger From Children", TriggerFromChildrenProperty.tooltip));

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
