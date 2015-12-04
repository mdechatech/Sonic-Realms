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

        protected static bool ShowAreaEvents
        {
            get { return EditorPrefs.GetBool("AreaTriggerEditor.ShowAreaEvents", false); }
            set { EditorPrefs.SetBool("AreaTriggerEditor.ShowAreaEvents", value); }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "TriggerFromChildren", "AlwaysCollide");

            ShowAreaEvents = EditorGUILayout.Foldout(ShowAreaEvents, "Area Events");
            if (ShowAreaEvents)
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                    "OnAreaEnter",
                    "OnAreaStay",
                    "OnAreaExit");
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
