using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Triggers.Editor
{
    [CustomEditor(typeof(ReactivePlatform), true)]
    public class ReactivePlatformEditor : BaseReactiveEditor
    {
        public override void OnInspectorGUI()
        {
            HedgehogEditorGUIUtility.DrawExcluding(serializedObject,
                "CollidingTrigger", "CollidingBool",
                "SurfaceTrigger", "SurfaceBool", HedgehogEditorGUIUtility.ScriptPropertyName);
            base.OnInspectorGUI();
        }

        protected override void DrawAnimationProperties()
        {
            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                "CollidingTrigger", "CollidingBool",
                "SurfaceTrigger", "SurfaceBool");
        }
    }
}
