using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Triggers.Editor
{
    [CustomEditor(typeof(ReactiveArea), true)]
    [CanEditMultipleObjects]
    public class ReactiveAreaEditor : BaseReactiveEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            HedgehogEditorGUIUtility.DrawExcluding(serializedObject,
                "Animator", "ObjectTrigger", "InsideTrigger",
                "InsideBool", HedgehogEditorGUIUtility.ScriptPropertyName);

            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();

            serializedObject.ApplyModifiedProperties();
        }

        protected override void DrawAnimationProperties()
        {
            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                "InsideTrigger", "InsideBool");
        }
    }
}
