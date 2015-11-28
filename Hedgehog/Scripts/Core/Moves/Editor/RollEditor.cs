using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Moves.Editor
{
    [CustomEditor(typeof (Roll))]
    public class RollEditor : MoveEditor
    {
        protected override void DrawAnimationProperties()
        {
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "UphillBool");
            base.DrawAnimationProperties();
        }

        protected override void DrawControlProperties()
        {
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "ActivateAxis", "RequireNegative", 
                "MinActivateSpeed");
            base.DrawControlProperties();
        }

        protected override void DrawPhysicsProperties()
        {
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "HeightChange", "WidthChange",
                "UphillGravity", "DownhillGravity", "Deceleration", "Friction");
            base.DrawPhysicsProperties();
        }
    }
}
