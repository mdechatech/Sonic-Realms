using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Moves.Editor
{
    [CustomEditor(typeof(LookUp))]
    public class LookUpEditor : MoveEditor
    {
        protected override void DrawControlProperties()
        {
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "ActivateAxis");
        }

        protected override void DrawPhysicsFoldout()
        {
            // do nothing
        }

        protected override void DrawPhysicsProperties()
        {
            // do nothing
        }
    }
}
