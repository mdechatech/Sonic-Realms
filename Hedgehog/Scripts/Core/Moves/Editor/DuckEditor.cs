using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Moves.Editor
{
    [CustomEditor(typeof(Duck))]
    public class DuckEditor : MoveEditor
    {
        protected override void DrawControlProperties()
        {
            base.DrawControlProperties();
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "ActivateAxis", "RequireNegative",
                "MaxActivateSpeed");
        }
    }
}
