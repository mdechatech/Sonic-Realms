using SonicRealms.Core.Utils.Editor;
using UnityEditor;

namespace SonicRealms.Core.Triggers.Editor
{
    [CustomEditor(typeof(BaseTrigger), true), CanEditMultipleObjects]
    public class BaseTriggerEditor : BaseFoldoutEditor
    {
        private const string ShowTriggersMenu = "Realms/Show Triggers";

        protected static bool ShowTriggers
        {
            get { return EditorPrefs.GetBool("ShowTriggers", false); }
            set { EditorPrefs.SetBool("ShowTriggers", value); }
        }

        static BaseTriggerEditor()
        {
            EditorApplication.delayCall += () =>
            {
                Menu.SetChecked(ShowTriggersMenu, ShowTriggers);
            };
        }

        [MenuItem("Realms/Show Triggers", false, 50)]
        public static void ShowTriggersMenuItem()
        {
            ShowTriggers = !ShowTriggers;
            Menu.SetChecked(ShowTriggersMenu, ShowTriggers);
        }
    }
}
