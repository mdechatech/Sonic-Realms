using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Moves.Editor
{
    [CustomEditor(typeof(GroundControl))]
    public class GroundControlEditor : MoveEditor
    {
        protected override void DrawAnimationProperties()
        {
            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                    "InputAxisFloat", "InputBool", "AcceleratingBool",
                    "BrakingBool", "ControlLockBool", "ControlLockTimerFloat",
                    "TopSpeedBool", "TopSpeedPercentFloat");
            base.DrawAnimationProperties();
        }

        protected override void DrawControlProperties()
        {
            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                "MovementAxis", "InvertAxis");
            base.DrawControlProperties();
        }

        protected override void DrawPhysicsProperties()
        {
            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                    "Acceleration", "Deceleration", "TopSpeed", "MinSlopeGravitySpeed");
            base.DrawPhysicsProperties();
        }
    }
}