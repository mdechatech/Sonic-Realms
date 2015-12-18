using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Moves.Editor
{
    [CustomEditor(typeof(BubbleSpecial))]
    public class BubbleSpecialEditor : MoveEditor
    {
        protected override void DrawAnimationProperties()
        {
            base.DrawAnimationProperties();
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "BounceTrigger");
        }

        protected override void DrawControlProperties()
        {
            base.DrawControlProperties();
            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                "DiveVelocity", "BounceSpeed", "BounceReleaseSpeed",
                "UnderwaterBounceSpeed", "UnderwaterBounceReleaseSpeed");
        }
    }
}
