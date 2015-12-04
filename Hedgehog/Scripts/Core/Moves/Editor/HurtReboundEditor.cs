using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Moves.Editor
{
    [CustomEditor(typeof(HurtRebound))]
    public class HurtReboundEditor : MoveEditor
    {
        protected override void DrawPhysicsProperties()
        {
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, 
                "ReboundSpeed");
        }
    }
}
