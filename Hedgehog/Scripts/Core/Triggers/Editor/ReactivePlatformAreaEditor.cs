using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Triggers.Editor
{
    [CustomEditor(typeof(ReactivePlatformArea), true)]
    [CanEditMultipleObjects]
    public class ReactivePlatformAreaEditor : ReactivePlatformEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            HedgehogEditorGUIUtility.DrawExcluding(serializedObject,
                "Animator", "ObjectTrigger",
                "CollidingTrigger", "CollidingBool",
                "SurfaceTrigger", "SurfaceBool",
                "InsideTrigger", "InsideBool", HedgehogEditorGUIUtility.ScriptPropertyName);

            serializedObject.ApplyModifiedProperties();

            DrawAnimationFoldout();
            if(ShowAnimationFoldout)
                DrawAnimationProperties();

            serializedObject.ApplyModifiedProperties();
        }

        protected override void DrawAnimationProperties()
        {
            base.DrawAnimationProperties();
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "InsideTrigger", "InsideBool");
        }
    }
}
