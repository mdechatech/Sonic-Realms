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
                    "InputAxisFloat", "GroundedBool", "AcceleratingBool", "BrakingBool",
                    "SpeedFloat", "AbsoluteSpeedFloat", "SurfaceAngleFloat", "ControlLockBool",
                    "ControlLockTimerFloat", "TopSpeedBool");
        }

        protected override void DrawControlProperties()
        {
            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                "MovementAxis", "InvertAxis");
        }

        protected override void DrawPhysicsProperties()
        {
            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                    "Acceleration", "Deceleration", "TopSpeed");
        }
    }
}