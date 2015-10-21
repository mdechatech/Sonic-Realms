using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Triggers.Editor
{
    [CustomEditor(typeof(ObjectTrigger))]
    [CanEditMultipleObjects]
    public class ObjectTriggerEditor : UnityEditor.Editor
    {
        protected static bool ShowObjectEvents
        {
            get { return EditorPrefs.GetBool("ObjectTriggerEditor.ShowObjectEvents", false); }
            set { EditorPrefs.SetBool("ObjectTriggerEditor.ShowObjectEvents", value); }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                "TriggerFromChildren",
                "AutoActivate",
                "AllowReactivation");

            ShowObjectEvents = EditorGUILayout.Foldout(ShowObjectEvents, "Object Events");
            if (ShowObjectEvents)
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                    "OnActivateEnter",
                    "OnActivateStay",
                    "OnActivateExit");
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
