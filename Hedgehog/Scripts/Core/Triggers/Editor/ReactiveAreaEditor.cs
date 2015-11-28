using Hedgehog.Core.Utils.Editor;

namespace Hedgehog.Core.Triggers.Editor
{
    public class ReactiveAreaEditor : BaseReactiveEditor
    {
        public override void OnInspectorGUI()
        {
            HedgehogEditorGUIUtility.DrawExcluding(serializedObject,
                "InsideTrigger", "InsideBool", HedgehogEditorGUIUtility.ScriptPropertyName);
            base.OnInspectorGUI();
        }

        protected override void DrawAnimationProperties()
        {
            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                "InsideTrigger", "InsideBool");
        }
    }
}
