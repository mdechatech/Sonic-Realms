using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Triggers.Editor
{
    [CustomEditor(typeof(BaseTrigger)), CanEditMultipleObjects]
    public class BaseTriggerEditor : UnityEditor.Editor
    {
        protected static bool ShowTriggers
        {
            get { return EditorPrefs.GetBool("ShowTriggers", false); }
            set { EditorPrefs.SetBool("ShowTriggers", value); }
        }

        [MenuItem("Hedgehog/Show Triggers")]
        public static void ShowTriggersMenuItem()
        {
            ShowTriggers = true;
        }

        [MenuItem("Hedgehog/Hide Triggers")]
        public static void HideTriggersMenuItem()
        {
            ShowTriggers = false;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "TriggerFromChildren");
            serializedObject.ApplyModifiedProperties();
        }
    }
}
