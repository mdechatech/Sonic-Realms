using Hedgehog.Core.Utils.Editor;
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

        protected static bool ShowEvents
        {
            get { return EditorPrefs.GetBool("AreaTriggerEditor.ShowEvents", false); }
            set { EditorPrefs.SetBool("AreaTriggerEditor.ShowEvents", value); }
        }

        protected static bool ShowSound
        {
            get { return EditorPrefs.GetBool("AreaTriggerEditor.ShowSound", false); }
            set { EditorPrefs.SetBool("AreaTriggerEditor.ShowSound", value); }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "AlwaysCollide");

            ShowEvents = EditorGUILayout.Foldout(ShowEvents, "Events");
            if (ShowEvents)
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                    "OnAreaEnter",
                    "OnAreaStay",
                    "OnAreaExit");
            }

            ShowSound = EditorGUILayout.Foldout(ShowSound, "Sound");
            if (ShowSound)
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                    "AreaEnterSound",
                    "AreaLoopSound",
                    "AreaExitSound");
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
