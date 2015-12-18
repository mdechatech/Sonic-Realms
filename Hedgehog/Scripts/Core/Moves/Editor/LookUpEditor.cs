using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Moves.Editor
{
    [CustomEditor(typeof(LookUp))]
    public class LookUpEditor : MoveEditor
    {
        protected override void DrawControlProperties()
        {
            base.DrawControlProperties();
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "ActivateAxis");
        }
    }
}
