using Hedgehog.Core.Triggers.Editor;
using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Level.Effects.Editor
{
    [CustomEditor(typeof(SwitchGravity))]
    [CanEditMultipleObjects]
    public class SwitchGravityEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ReactiveObjectEditor.DrawActivatorCheck(targets);

            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                "RestoreOnExit",
                "ModifyDirection");

            if (serializedObject.FindProperty("ModifyDirection").boolValue)
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject, "Direction");
            }

            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "ModifyStrength");
            if (serializedObject.FindProperty("ModifyStrength").boolValue)
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                    "AirStrength",
                    "GroundStrength");
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
