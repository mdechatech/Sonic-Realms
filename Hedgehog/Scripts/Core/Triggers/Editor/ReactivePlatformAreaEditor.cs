using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Triggers.Editor
{
    [CustomEditor(typeof(ReactivePlatformArea), true)]
    public class ReactivePlatformAreaEditor : ReactivePlatformEditor
    {
        public override void OnInspectorGUI()
        {
            HedgehogEditorGUIUtility.DrawExcluding(serializedObject,
                "CollidingTrigger", "CollidingBool",
                "SurfaceTrigger", "SurfaceBool",
                "InsideTrigger", "InsideBool", HedgehogEditorGUIUtility.ScriptPropertyName);
            
            DrawAnimationFoldout();
            if(ShowAnimationFoldout)
                DrawAnimationProperties();
        }

        protected override void DrawAnimationProperties()
        {
            base.DrawAnimationProperties();
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "InsideTrigger", "InsideBool");
        }
    }
}
