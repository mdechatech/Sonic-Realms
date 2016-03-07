using SonicRealms.Core.Utils.Editor;
using UnityEditor;

namespace SonicRealms.Core.Triggers.Editor
{
    [CustomEditor(typeof(BaseTrigger), true), CanEditMultipleObjects]
    public class BaseTriggerEditor : BaseFoldoutEditor
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
    }
}
