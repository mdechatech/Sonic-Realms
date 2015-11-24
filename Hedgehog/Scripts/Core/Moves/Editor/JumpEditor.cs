using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Moves.Editor
{
    [CustomEditor(typeof(Jump))]
    public class JumpEditor : MoveEditor
    {
        protected override void DrawControlProperties()
        {
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "ActivateButton", "ClearanceHeight");
            base.DrawControlProperties();
        }

        protected override void DrawPhysicsProperties()
        {
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "ActivateSpeed", "ReleaseSpeed");
            base.DrawPhysicsProperties();
        }
    }
}
