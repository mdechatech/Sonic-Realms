using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Moves.Editor
{
    [CustomEditor(typeof(AirControl))]
    public class AirControlEditor : MoveEditor
    {
        protected override void DrawAnimationProperties()
        {
            base.DrawAnimationProperties();
            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                "HorizontalSpeedFloat", "VerticalSpeedFloat");
        }

        protected override void DrawControlProperties()
        {
            base.DrawControlProperties();
            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                "MovementAxis", "InvertAxis");
        }

        protected override void DrawPhysicsProperties()
        {
            base.DrawPhysicsProperties();
            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                "Acceleration", "Deceleration", "TopSpeed");
        }
    }
}