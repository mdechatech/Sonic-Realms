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
