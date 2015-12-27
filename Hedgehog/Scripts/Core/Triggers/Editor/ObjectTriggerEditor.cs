using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Triggers.Editor
{
    [CustomEditor(typeof(ObjectTrigger))]
    [CanEditMultipleObjects]
    public class ObjectTriggerEditor : BaseTriggerEditor
    {
        protected static bool ShowObjectEvents
        {
            get { return EditorPrefs.GetBool("ObjectTriggerEditor.ShowObjectEvents", false); }
            set { EditorPrefs.SetBool("ObjectTriggerEditor.ShowObjectEvents", value); }
        }

        protected static bool ShowAnimation
        {
            get { return EditorPrefs.GetBool("ObjectTriggerEditor.ShowAnimation", false); }
            set { EditorPrefs.SetBool("ObjectTriggerEditor.ShowAnimation", value); }
        }

        protected static bool ShowSound
        {
            get { return EditorPrefs.GetBool("ObjectTriggerEditor.ShowSound", false); }
            set { EditorPrefs.SetBool("ObjectTriggerEditor.ShowSound", value); }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                "AllowReactivation");

            ShowObjectEvents = EditorGUILayout.Foldout(ShowObjectEvents, "Object Events");
            if (ShowObjectEvents)
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                    "OnActivateEnter",
                    "OnActivateStay",
                    "OnActivateExit");
            }

            ShowAnimation = EditorGUILayout.Foldout(ShowAnimation, "Animation");
            if (ShowAnimation)
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                    "Animator",
                    "ActivatedTrigger",
                    "ActivatedBool");
            }

            ShowSound = EditorGUILayout.Foldout(ShowSound, "Sound");
            if (ShowSound)
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                    "ActivateSound", "LoopSound", "DeactivateSound");
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
