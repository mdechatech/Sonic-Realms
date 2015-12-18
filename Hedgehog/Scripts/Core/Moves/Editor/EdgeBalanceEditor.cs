using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Moves.Editor
{
    [CustomEditor(typeof(EdgeBalance))]
    public class EdgeBalanceEditor : MoveEditor
    {
        protected override void DrawAnimationProperties()
        {
            base.DrawAnimationProperties();
            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                "DistanceFloat");
        }

        protected override void DrawControlProperties()
        {
            base.DrawControlProperties();
            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                "MaxDistance", "AllowDuck", "AllowLookUp");
        }
    }
}
