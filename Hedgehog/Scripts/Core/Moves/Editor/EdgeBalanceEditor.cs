using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Moves.Editor
{
    [CustomEditor(typeof(EdgeBalance))]
    public class EdgeBalanceEditor : MoveEditor
    {
        protected override void DrawAnimationProperties()
        {
            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                "DistanceFloat");
            base.DrawAnimationProperties();
        }

        protected override void DrawControlProperties()
        {
            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                "MaxDistance", "AllowDuck", "AllowLookUp");
            base.DrawControlProperties();
        }

        protected override void DrawPhysicsFoldout()
        {
            // do nothing
        }
    }
}
