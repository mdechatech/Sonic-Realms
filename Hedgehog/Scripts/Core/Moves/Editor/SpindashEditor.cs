using Hedgehog.Core.Moves;
using Hedgehog.Core.Moves.Editor;
using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Scripts.Core.Moves.Editor
{
    [CustomEditor(typeof(Spindash))]
    public class SpindashEditor : MoveEditor
    {
        protected override void DrawControlProperties()
        {
            base.DrawControlProperties();
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "ChargeButton");
        }

        protected override void DrawPhysicsProperties()
        {
            base.DrawPhysicsProperties();
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "BasePower", "ChargePower",
                "ChargePowerDecay", "MaxChargePower");
        }
    }
}
