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
                "Acceleration", "Deceleration", "TopSpeed");
            base.DrawPhysicsProperties();
        }
    }
}