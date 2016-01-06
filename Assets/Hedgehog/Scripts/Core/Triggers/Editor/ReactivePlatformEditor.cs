using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Triggers.Editor
{
    [CustomEditor(typeof(ReactivePlatform), true)]
    [CanEditMultipleObjects]
    public class ReactivePlatformEditor : BaseReactiveEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            HedgehogEditorGUIUtility.DrawExcluding(serializedObject,
                "Animator", "ObjectTrigger",
                "CollidingTrigger", "CollidingBool",
                "SurfaceTrigger", "SurfaceBool", HedgehogEditorGUIUtility.ScriptPropertyName);

            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }

        protected override void DrawAnimationProperties()
        {
            base.DrawAnimationProperties();
            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                "CollidingTrigger", "CollidingBool",
                "SurfaceTrigger", "SurfaceBool");
        }
    }
}
