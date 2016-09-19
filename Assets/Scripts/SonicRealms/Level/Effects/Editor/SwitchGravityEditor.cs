using SonicRealms.Core.Triggers.Editor;
using SonicRealms.Core.Utils.Editor;
using UnityEditor;

namespace SonicRealms.Level.Effects.Editor
{
    [CustomEditor(typeof(SwitchGravity))]
    [CanEditMultipleObjects]
    public class SwitchGravityEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ReactiveObjectEditor.DrawActivatorCheck(targets);

            RealmsEditorUtility.DrawProperties(serializedObject,
                "RestoreOnExit",
                "ModifyDirection");

            if (serializedObject.FindProperty("ModifyDirection").boolValue)
            {
                RealmsEditorUtility.DrawProperties(serializedObject, "Direction");
            }

            RealmsEditorUtility.DrawProperties(serializedObject, "ModifyStrength");
            if (serializedObject.FindProperty("ModifyStrength").boolValue)
            {
                RealmsEditorUtility.DrawProperties(serializedObject,
                    "AirStrength",
                    "GroundStrength");
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
