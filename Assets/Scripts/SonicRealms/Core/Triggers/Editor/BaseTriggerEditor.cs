using SonicRealms.Core.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.Core.Triggers.Editor
{
    [CustomEditor(typeof(BaseTrigger), true), CanEditMultipleObjects]
    public class BaseTriggerEditor : BaseFoldoutEditor
    {
        private const string ShowTriggersMenu = "Realms/Show Triggers";

        protected static bool ShowTriggers
        {
            get { return EditorPrefs.GetBool("ShowTriggers", true); }
            set { EditorPrefs.SetBool("ShowTriggers", value); }
        }

        static BaseTriggerEditor()
        {
            EditorApplication.delayCall += () =>
            {
                Menu.SetChecked(ShowTriggersMenu, ShowTriggers);
            };
        }

        [MenuItem(ShowTriggersMenu, false, 50)]
        public static void ShowTriggersMenuItem()
        {
            ShowTriggers = !ShowTriggers;
            Menu.SetChecked(ShowTriggersMenu, ShowTriggers);

            foreach (var trigger in FindObjectsOfType<BaseTrigger>())
            {
                trigger.Update();
            }

            SceneView.RepaintAll();
        }
    }
}
