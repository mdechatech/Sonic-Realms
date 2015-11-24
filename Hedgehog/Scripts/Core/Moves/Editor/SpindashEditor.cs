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
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "ChargeButton");
            base.DrawControlProperties();
        }

        protected override void DrawPhysicsProperties()
        {
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "BasePower", "ChargePower",
                "ChargePowerDecay", "MaxChargePower");
            base.DrawPhysicsProperties();
        }
    }
}
