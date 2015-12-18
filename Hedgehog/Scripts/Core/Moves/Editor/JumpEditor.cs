using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Moves.Editor
{
    [CustomEditor(typeof(Jump))]
    public class JumpEditor : MoveEditor
    {
        protected override void DrawControlProperties()
        {
            base.DrawControlProperties();
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "ActivateButton", "ClearanceHeight");
        }

        protected override void DrawPhysicsProperties()
        {
            base.DrawPhysicsProperties();
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "ActivateSpeed", "ReleaseSpeed");
        }
    }
}
