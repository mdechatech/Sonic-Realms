using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Moves.Editor
{
    [CustomEditor(typeof (Roll))]
    public class RollEditor : MoveEditor
    {
        protected override void DrawControlProperties()
        {
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "ActivateInput", "RequireNegative", 
                "MinActivateSpeed");
        }

        protected override void DrawPhysicsProperties()
        {
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "HeightChange", "WidthChange",
                "UphillGravity", "DownhillGravity", "Deceleration", "Friction");
        }
    }
}
